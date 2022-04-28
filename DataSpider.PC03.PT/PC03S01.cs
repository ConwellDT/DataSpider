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

    public class PC03S01 : PC03B01
    {
        static PISystem _PIStstem;
        static PIServer _PIserver;
        static AFDatabase _AFDB;

        public string serverName = "";
        public string dbName = "";
        public string user = "";
        public string password = "";
        private DateTime dtLastUpdateProgDateTime = DateTime.MinValue;
        protected int UpdateInterval = 30;
        protected IF_STATUS lastStatus = IF_STATUS.Stop;

        public PC03S01() : base()
        {
        }
                
        public PC03S01(PC03F01 pOwner, string strEquipType, string strEquipName, int nCurNo, bool bAutoRun, PIInfo m_clsPIInfo) : base(pOwner, strEquipType, strEquipName, nCurNo, bAutoRun, m_clsPIInfo)
        {
            #region PI CONNECTION INFO

            serverName = m_clsPIInfo.strPI_Server;
            dbName = m_clsPIInfo.strPI_DB;
            user = m_clsPIInfo.strPI_USER;
            password = m_clsPIInfo.strPI_PWD;

            //NetworkCredential credential = new NetworkCredential(user, password);
            //_PIStstem = (new PISystems()[serverName]);
            //_PIStstem.Connect(credential);
            //_AFDB = _PIStstem.Databases[dbName];
            try
            {
                _PIserver = PIServer.FindPIServer(_PIStstem, serverName);
                _PIserver?.Connect();
            }
            catch(Exception ex)
            {
                mOwner.listViewMsg(m_strEName,$"PI Server Connection - {ex.ToString()} ", false, m_nCurNo, 1, true, PC00D01.MSGTINF);
            }
            #endregion           
        }

        protected override void ThreadJob()
        {
            string errCode = string.Empty;
            string errText = string.Empty;
            string pierrText = string.Empty;
            bool result = true;

            string CurDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            mOwner.listViewMsg(m_strEName, PC00D01.ON, true, m_nCurNo, 1, false, PC00D01.MSGTINF);
            mOwner.listViewMsg(m_strEName, "Thread Started", false, m_nCurNo, 1, true, PC00D01.MSGTINF);

            while (!bTerminal)
            {
                try
                {
            
                    if(m_nCurNo == 0)
                        UpdateEquipmentProgDateTime(IF_STATUS.Normal);

                    //데이타 조회
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


                                    result = m_sqlBiz.UpdateMeasureResult(strSeq, strFlag, ifCount, errMsg, ref errCode, ref errText);
                                    if (!result)
                                    {
                                        break;
                                    }

                                    if (result)
                                    {
                                        mOwner.listViewMsg(m_strEName, string.Format(PC00D01.SucceededtoPI, pointName, pointValue), true, m_nCurNo, 3, true, PC00D01.MSGTINF);
                                    }
                                    else
                                    {
                                        mOwner.listViewMsg(m_strEName, string.Format(PC00D01.FailedtoPI, $"{errText} - {pointName}", pointValue), true, m_nCurNo, 3, true, PC00D01.MSGTERR);

                                        UpdateEquipmentProgDateTime(IF_STATUS.InternalError);
                                    }
                                }
                                else
                                {
                                    string errMsg = "매핑된 PI 태그명이 없습니다.";
                                    mOwner.listViewMsg(m_strEName, string.Format(PC00D01.FailedtoPI, $"{errMsg} - {tagName}", pointValue), true, m_nCurNo, 3, true, PC00D01.MSGTERR);

                                    UpdateEquipmentProgDateTime(IF_STATUS.NoData);
                                }
                            }
                        }
                        else
                            m_Thd.Join(1000);
                    }
                    else
                        m_Thd.Join(1000);
                }
                catch (Exception ex)
                {
                    mOwner.listViewMsg(m_strEName, ex.ToString(), true, m_nCurNo, 3, true, PC00D01.MSGTERR);
                    UpdateEquipmentProgDateTime(IF_STATUS.InternalError);
                }
                finally
                {
                    //_PIserver?.Disconnect();

                    //if (m_strPgmPara.Trim() == "M")
                    //    m_Thd.Join(5000);
                    //else
                    //m_Thd.Join(1000)
                }
            }
            UpdateEquipmentProgDateTime(IF_STATUS.Unknown);
            mOwner.listViewMsg(m_strEName, PC00D01.OFF, true, m_nCurNo, 1, false, PC00D01.MSGTINF);
        }


        public bool SetPIValue(string pointName, object pointValue, DateTime mTime, ref string _strErrText)
        {
            bool rtnval = false;
            
            try
            {
                PIPoint point = PIPoint.FindPIPoint(_PIserver, pointName);

                AFTime aTime = new AFTime(mTime.ToUniversalTime());

                AFValue value = new AFValue(pointValue, aTime);

                //AFValue value = new AFValue(pointValue, aTime, null, AFValueStatus.Bad);

                //2021.05.20 변경요청 [김현지 프로]
                //point.UpdateValue(value, OSIsoft.AF.Data.AFUpdateOption.Replace);
                //point.UpdateValue(value, OSIsoft.AF.Data.AFUpdateOption.Insert);
                // 버퍼 미사용 옵션
                point.UpdateValue(value, OSIsoft.AF.Data.AFUpdateOption.Insert, OSIsoft.AF.Data.AFBufferOption.DoNotBuffer);

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
            PIPoint point = PIPoint.FindPIPoint(_PIserver, pointName);
            AFTime st = new AFTime(startTime.ToUniversalTime());
            AFTime et = new AFTime(endTime.ToUniversalTime());

            AFTimeRange range = new AFTimeRange(st, et);
            AFValues values = point.RecordedValues(range, OSIsoft.AF.Data.AFBoundaryType.Inside, null, false);

            foreach (AFValue value in values)
            {
                point.UpdateValue(value, OSIsoft.AF.Data.AFUpdateOption.Remove);
            }
        }

        protected bool UpdateEquipmentProgDateTime(IF_STATUS status = IF_STATUS.Normal)
        {
            string errCode = string.Empty;
            string errText = string.Empty;
            DateTime dtNow = DateTime.Now;
            if (dtNow.Subtract(dtLastUpdateProgDateTime).TotalSeconds > UpdateInterval || !lastStatus.Equals(status))
            {
                dtLastUpdateProgDateTime = dtNow;
                lastStatus = status;
                return m_sqlBiz.UpdateEquipmentProgDateTimePC02($"{System.Windows.Forms.Application.ProductName}", (int)status, ref errCode, ref errText);
            }
            return true;
        }
    }

}
