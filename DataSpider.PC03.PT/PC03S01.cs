using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Collections;
using System.Threading;
using System.Reflection;
using CFW.Common;
using System.IO;
using System.Text.RegularExpressions;
using System.Data;
using DataSpider.PC00.PT;
using OSIsoft.AF;
using System.Net;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Search;
using OSIsoft.AF.PI;
using OSIsoft.AF.Time;

namespace DataSpider.PC03.PT
{
    public class MyClsLog : FileLog
    {


        public MyClsLog(string fileName = "") : base(fileName)
        {
        }
        public void LogToFile(string FileType, string FileName, string p_strStat, string p_strExplain, string p_strLogMsg)
        {
            base.WriteLog(p_strLogMsg, p_strStat, logFileName);
        }
    }


    public class PC03S01 : PC03B01
    {
        static PISystem _PISystem;
        static PIServer _PIServer;

        public string serverName = "";
        public string dbName = "";
        public string user = "";
        public string password = "";
        
        FileLog m_Logger = null;

        DateTime dtLastThreadStatus = DateTime.MinValue;
        IF_STATUS lastThreadStatus = IF_STATUS.Normal;
        public IF_STATUS ThreadStatus
        {
            get
            {
                if (DateTime.Now.Subtract(dtLastThreadStatus).TotalSeconds > 60)
                {
                    lastThreadStatus = IF_STATUS.Unknown;
                }
                return lastThreadStatus;
            }
            set
            {
                lastThreadStatus = value;
                dtLastThreadStatus = DateTime.Now;
            }
        }


        public PC03S01() : base()
        {
        }
                
        public PC03S01(PC03F01 pOwner, string strEquipType, string strEquipName, int nCurNo, bool bAutoRun, PIInfo m_clsPIInfo) : base(pOwner, strEquipType, strEquipName, nCurNo, bAutoRun, m_clsPIInfo)
        {
            m_Logger = new FileLog(m_strEName);
            m_Logger.SetDbLogger(m_strEName);

            #region PI CONNECTION INFO

            serverName = m_clsPIInfo.strPI_Server;
            dbName = m_clsPIInfo.strPI_DB;
            user = m_clsPIInfo.strPI_USER;
            password = m_clsPIInfo.strPI_PWD;

            //if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(password))
            //{
            //    try
            //    {
            //        NetworkCredential credential = new NetworkCredential(user, password);
            //        _PISystem = (new PISystems()[serverName]);
            //        _PISystem.Connect(credential);
            //    }
            //    catch (Exception ex)
            //    {
            //        mOwner.listViewMsg(m_strEName, $"PI System Connection ({serverName}) - {ex.ToString()} ", false, m_nCurNo, 1, true, PC00D01.MSGTINF);
            //        m_Logger.WriteLog($"PI System Connection ({serverName}) - {ex.ToString()} ", PC00D01.MSGTINF, m_strEName);
            //    }
            //}
            //_AFDB = _PIStstem.Databases[dbName];
            try
            {
                //_PIserver = PIServer.FindPIServer(_PIStstem, serverName);
                //_PIserver?.Connect();
                if (!CheckPIConnection(out string piErrText))
                {
                    mOwner.listViewMsg(m_strEName, $"PI Server Connection Error ({piErrText})", false, m_nCurNo, 1, true, PC00D01.MSGTERR);
                    m_Logger.WriteLog("PI Server Connection Error", PC00D01.MSGTERR, m_strEName);
                }
            }
            catch (Exception ex)
            {
                mOwner.listViewMsg(m_strEName,$"PI Server Connection - {ex.ToString()} ", false, m_nCurNo, 1, true, PC00D01.MSGTINF);
                m_Logger.WriteLog($"PI Server Connection - {ex.ToString()} ", PC00D01.MSGTINF, m_strEName);
                                    
            }
            #endregion           
        }

        private bool CheckPIConnection(out string errText)
        {
            errText = string.Empty;

            if (string.IsNullOrWhiteSpace(serverName))
            {
                errText = "No PI Server info.";
                return false;
            }

            if (_PIServer == null)
            {
                _PIServer = PIServer.FindPIServer(_PISystem, serverName);
                if (_PIServer == null)
                {
                    errText = "FindPIServer Returned Null.";
                    return false;
                }
            }
            if (!_PIServer.ConnectionInfo.IsConnected)
            {
                try
                {
                    _PIServer.Connect();
                }
                catch (Exception ex)
                {
                    errText = $"Exception PI Server Connection ({serverName})- {ex}";
                    return false;
                }
                if (!_PIServer.ConnectionInfo.IsConnected)
                {
                    errText = "PI Not Connected.";
                    return false;
                }
            }
            return true;
        }

        protected override void ThreadJob()
        {
            string errCode = string.Empty;
            string errText = string.Empty;
            string pierrText = string.Empty;
            bool result = true;

            string CurDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            mOwner.listViewMsg(m_strEName, PC00D01.ON, true, m_nCurNo, 1, false, PC00D01.MSGTINF);
            m_Logger.WriteLog(PC00D01.ON, PC00D01.MSGTINF, m_strEName);
            mOwner.listViewMsg(m_strEName, "Thread Started", false, m_nCurNo, 1, true, PC00D01.MSGTINF);
            m_Logger.WriteLog("Thread Started", PC00D01.MSGTINF, m_strEName);

            while (!bTerminal)
            {
                try
                {
                    if (!CheckPIConnection(out string piErrText))
                    {
                        mOwner.listViewMsg(m_strEName, $"PI Server Connection Error ({piErrText})", false, m_nCurNo, 1, true, PC00D01.MSGTERR);
                        m_Logger.WriteLog("PI Server Connection Error", PC00D01.MSGTERR, m_strEName);
                        ThreadStatus = IF_STATUS.Disconnected;
                        Thread.Sleep(1000);
                        continue;
                    }

                    DataTable dtResult = m_sqlBiz.GetMeasureResult(m_strEType, ref errCode, ref errText);

                    if (dtResult != null)
                    {
                        if (dtResult.Rows.Count > 0)
                        {
                            for (int i = 0; i < dtResult.Rows.Count; i++)
                            {
                                int strSeq = int.Parse(dtResult.Rows[i]["HI_SEQ"].ToString());
                                int ifCount = 0; int.TryParse(dtResult.Rows[i]["IF_COUNT"].ToString(), out ifCount);
                                string pointName = dtResult.Rows[i]["PI_TAG_NM"].ToString();
                                object pointValue = dtResult.Rows[i]["MEASURE_VALUE"].ToString();

                                //DateTime mTime = DateTime.Parse(dtResult.Rows[i]["MEASURE_DATE"].ToString("yyyyMMddHHmmss.fff"));

                                DateTime mTime = Convert.ToDateTime(dtResult.Rows[i]["MEASURE_DATE"]);

                                string tagName = dtResult.Rows[i]["TAG_NM"].ToString();

                                if (pointName.Trim() != "")
                                {
                                    //PI서버 업로드
                                    bool rVal = SetPIValue(pointName, pointValue, mTime, ref pierrText);

                                    if (i % 10 == 9) Thread.Sleep(1);

                                    //데이타 전송 플래그 업데이트
                                    string strFlag = "Y";
                                    string errMsg = "";

                                    if (rVal == false)
                                    {
                                        strFlag = "E";
                                        errMsg = pierrText.Replace("\\", "").Replace("\r\n", "").Replace("'", "");
                                    }

                                    // 20230406, SHS, SetPIValue 결과에 따른 로그 추가, 에러 로그 추가
                                    if (rVal)
                                    {
                                        mOwner.listViewMsg(m_strEName, string.Format(PC00D01.SucceededtoPI, pointName, pointValue), true, m_nCurNo, 3, true, PC00D01.MSGTINF);
                                        m_Logger.WriteLog(string.Format(PC00D01.SucceededtoPI, pointName, pointValue), PC00D01.MSGTERR, m_strEName);
                                        ThreadStatus = IF_STATUS.Normal;
                                    }
                                    else
                                    {
                                        mOwner.listViewMsg(m_strEName, string.Format(PC00D01.FailedtoPI, $"{errMsg} - {pointName}", pointValue), true, m_nCurNo, 3, true, PC00D01.MSGTERR);
                                        m_Logger.WriteLog(string.Format(PC00D01.FailedtoPI, $"{errMsg} - {pointName}", pointValue), PC00D01.MSGTERR, m_strEName);
                                        ThreadStatus = IF_STATUS.InternalError;
                                    }


                                    result = m_sqlBiz.UpdateMeasureResult(strSeq, strFlag, ifCount, errMsg, ref errCode, ref errText);

                                    // 20230406, SHS, SetPIValue 결과 DB UPDATE 결과 따른 로그 내용 변경, 성공이면 로그는 필요 없음
                                    //if (result)
                                    //{
                                    //    //mOwner.listViewMsg(m_strEName, string.Format(PC00D01.SucceededtoPI, pointName, pointValue), true, m_nCurNo, 3, true, PC00D01.MSGTINF);
                                    //    //m_Logger.WriteLog(string.Format(PC00D01.SucceededtoPI, pointName, pointValue), PC00D01.MSGTERR, m_strEName);
                                    //    //ThreadStatus = IF_STATUS.Normal;
                                    //}
                                    if (!result)
                                    {
                                        mOwner.listViewMsg(m_strEName, string.Format("UpdateMeasureResult Failed.", $"{errText} - {pointName}", pointValue), true, m_nCurNo, 3, true, PC00D01.MSGTERR);
                                        m_Logger.WriteLog(string.Format("UpdateMeasureResult Failed.", $"{errText} - {pointName}", pointValue), PC00D01.MSGTERR, m_strEName);
                                        ThreadStatus = IF_STATUS.InternalError;
                                    }

                                }
                                else
                                {
                                    string errMsg = "매핑된 PI 태그명이 없습니다.";
                                    mOwner.listViewMsg(m_strEName, string.Format(PC00D01.FailedtoPI, $"{errMsg} - {tagName}", pointValue), true, m_nCurNo, 3, true, PC00D01.MSGTERR);
                                    m_Logger.WriteLog(string.Format(PC00D01.FailedtoPI, $"{errMsg} - {tagName}", pointValue), PC00D01.MSGTERR, m_strEName);
                                    ThreadStatus = IF_STATUS.NoData;
                                }
                            }
                        }
                        ThreadStatus = IF_STATUS.Normal;
                    }
                    else
                    {
                        ThreadStatus = IF_STATUS.InternalError;
                    }

                }
                catch (Exception ex)
                {
                    mOwner.listViewMsg(m_strEName, ex.ToString(), true, m_nCurNo, 3, true, PC00D01.MSGTERR);
                    m_Logger.WriteLog($"ThreadJob - {ex.ToString()} ", PC00D01.MSGTERR, m_strEName);
                    ThreadStatus = IF_STATUS.InternalError;
                }
                finally
                {
                    //_PIserver?.Disconnect();

                    //if (m_strPgmPara.Trim() == "M")
                    //    m_Thd.Join(5000);
                    //else
                    //m_Thd.Join(1000)
                }
                Thread.Sleep(1000);
            }
            m_Logger.WriteLog(PC00D01.OFF, PC00D01.MSGTINF, m_strEName);
            //mOwner.listViewMsg(m_strEName, PC00D01.OFF, true, m_nCurNo, 1, false, PC00D01.MSGTINF);
            
        }


        public bool SetPIValue(string pointName, object pointValue, DateTime mTime, ref string _strErrText)
        {
            bool rtnval = false;
            
            try
            {
                PIPoint point = PIPoint.FindPIPoint(_PIServer, pointName);

                AFTime aTime = new AFTime(mTime.ToUniversalTime());

                AFValue value = new AFValue(pointValue, aTime);

                //AFValue value = new AFValue(pointValue, aTime, null, AFValueStatus.Bad);

                //2021.05.20 변경요청 [김현지 프로]
                //point.UpdateValue(value, OSIsoft.AF.Data.AFUpdateOption.Replace);
                //point.UpdateValue(value, OSIsoft.AF.Data.AFUpdateOption.Insert);
                // 버퍼 미사용 옵션
                point.UpdateValue(value, OSIsoft.AF.Data.AFUpdateOption.Insert, OSIsoft.AF.Data.AFBufferOption.Buffer);

                return true;
            }
            catch (Exception ex)
            {
                string ee = ex.ToString();

                _strErrText = ee;
                return false;
            }
        }

        static void DeleteFromRange(string pointName, DateTime startTime, DateTime endTime)
        {
            PIPoint point = PIPoint.FindPIPoint(_PIServer, pointName);
            AFTime st = new AFTime(startTime.ToUniversalTime());
            AFTime et = new AFTime(endTime.ToUniversalTime());

            AFTimeRange range = new AFTimeRange(st, et);
            AFValues values = point.RecordedValues(range, OSIsoft.AF.Data.AFBoundaryType.Inside, null, false);

            foreach (AFValue value in values)
            {
                point.UpdateValue(value, OSIsoft.AF.Data.AFUpdateOption.Remove);
            }
        }
    }

}
