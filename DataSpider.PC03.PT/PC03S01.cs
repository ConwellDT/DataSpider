﻿using System;
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
using OSIsoft.AF.Analysis;
using static System.Runtime.CompilerServices.RuntimeHelpers;
using System.Linq.Expressions;
using System.Text.Json;
using OSIsoft.AF.EventFrame;

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

        private static AFDatabase _AFDatabase = null;
        private static object objLock = new object();

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

        private PIInfo m_clsPIInfo;

        public PC03S01() : base()
        {
        }
                
        public PC03S01(PC03F01 pOwner, string strEquipType, string equipTypeName, string strEquipName, int nCurNo, bool bAutoRun, PIInfo clsPIInfo) : base(pOwner, strEquipType, equipTypeName, strEquipName, nCurNo, bAutoRun, clsPIInfo)
        {
            m_Logger = new FileLog(m_strEName);
            m_Logger.SetDbLogger(m_strEName);

            #region PI CONNECTION INFO
            m_clsPIInfo = clsPIInfo;
            serverName = m_clsPIInfo.strPI_Server;
            dbName = m_clsPIInfo.strPI_DB;
            user = m_clsPIInfo.strPI_USER;
            password = m_clsPIInfo.strPI_PWD;

            try
            {
                if (!CheckPIConnection(out string piErrText))
                {
                    mOwner.listViewMsg(m_strEName, $"PI Server Connection Error ({piErrText})", false, m_nCurNo, 1, true, PC00D01.MSGTERR);
                    m_Logger.WriteLog("PI Server Connection Error", PC00D01.MSGTERR, m_strEName);
                }

                if (!CheckAFDatabase(out string errString))
                {
                    mOwner.listViewMsg(m_strEName, $"GetAFDatabase ({errString})", true, m_nCurNo, 3, true, PC00D01.MSGTERR);
                    m_Logger.WriteLog("CheckAFDatabase Error", PC00D01.MSGTERR, m_strEName);
                }

                if (bAutoRun == true)
                {
                    m_Thd = new Thread(ThreadJob);
                    m_Thd.Start();
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

        private bool ProcessPIPoint()
        {
            bool result = true;

            try
            {
                if (!CheckPIConnection(out string piErrText))
                {
                    mOwner.listViewMsg(m_strEName, $"PI Server Connection Error ({piErrText})", false, m_nCurNo, 1, true, PC00D01.MSGTERR);
                    m_Logger.WriteLog("PI Server Connection Error", PC00D01.MSGTERR, m_strEName);
                    ThreadStatus = IF_STATUS.Disconnected;
                    //Thread.Sleep(1000);
                    return false;
                }

                string errCode = null;
                string errText = null;
                string pierrText = null;

                DataTable dtResult = m_sqlBiz.GetMeasureResult(m_strEType, ref errCode, ref errText);

                if (dtResult != null)
                {
                    ThreadStatus = IF_STATUS.Normal;

                    if (dtResult.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtResult.Rows.Count; i++)
                        {
                            int strSeq = int.Parse(dtResult.Rows[i]["HI_SEQ"].ToString());
                            int.TryParse(dtResult.Rows[i]["IF_COUNT"].ToString(), out int ifCount);
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
                                    m_Logger.WriteLog(string.Format(PC00D01.SucceededtoPI, pointName, pointValue), PC00D01.MSGTINF, m_strEName);
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
                }
                else
                {
                    ThreadStatus = IF_STATUS.InternalError;
                    result = false;
                }
            }
            catch (Exception ex)
            {
                mOwner.listViewMsg(m_strEName, ex.ToString(), true, m_nCurNo, 3, true, PC00D01.MSGTERR);
                m_Logger.WriteLog($"ProcessPIPoint - {ex} ", PC00D01.MSGTERR, m_strEName);
                ThreadStatus = IF_STATUS.InternalError;
                result = false;
            }
            return result;
        }

        private bool ProcessEventFrame()
        {
            bool result = true;

            try
            {
                // AF 연결 
                if (!CheckAFDatabase(out string errString))
                {
                    mOwner.listViewMsg(m_strEName, $"GetAFDatabase ({errString})", true, m_nCurNo, 3, true, PC00D01.MSGTERR);
                    return false;
                }

                string errCode = null;
                string errText = null;
                string pierrText = null;

                DataTable dtResult = m_sqlBiz.GetMeasureEventFrameResult(m_strEType, ref errCode, ref errText);

                if (dtResult != null)
                {
                    ThreadStatus = IF_STATUS.Normal;

                    if (dtResult.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtResult.Rows.Count; i++)
                        {
                            int strSeq = int.Parse(dtResult.Rows[i]["HI_SEQ"].ToString());
                            int.TryParse(dtResult.Rows[i]["AF_IF_COUNT"].ToString(), out int ifCount);
                            string equipName = dtResult.Rows[i]["EQUIP_NM"].ToString();
                            string eventFrameName = dtResult.Rows[i]["EVENTFRAME_NAME"].ToString();
                            int.TryParse(dtResult.Rows[i]["MSG_TYPE"].ToString(), out int msgType);
                            string startTime = dtResult.Rows[i]["START_TIME"].ToString();
                            string endTime = dtResult.Rows[i]["END_TIME"].ToString();
                            string attributes = dtResult.Rows[i]["ATTRIBUTES"].ToString();
                            string afIFTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                            List<EventFrameAttributeData> listAttributes = JsonSerializer.Deserialize<List<EventFrameAttributeData>>(attributes);
                            List<string> listAttributeNames = listAttributes.Select(x => x.Name).ToList();

                            AFElementTemplate efTemplate = GetEventFrameTemplate(_AFDatabase, m_strETypeName, msgType, listAttributeNames);

                            var (afIFFlag, afIFRemark) = SaveEventFrame(eventFrameName, efTemplate, startTime, endTime, listAttributes);

                            if (i % 10 == 9) Thread.Sleep(1);

                            // 20230406, SHS, SetPIValue 결과에 따른 로그 추가, 에러 로그 추가
                            if (afIFFlag.Equals("Y"))
                            { 
                                mOwner.listViewMsg(m_strEName, string.Format(PC00D01.SucceededtoEF, eventFrameName), true, m_nCurNo, 3, true, PC00D01.MSGTINF);
                                m_Logger.WriteLog(string.Format(PC00D01.SucceededtoEF, eventFrameName), PC00D01.MSGTINF, m_strEName);
                                ThreadStatus = IF_STATUS.Normal;
                            }
                            else
                            {
                                mOwner.listViewMsg(m_strEName, string.Format(PC00D01.FailedtoEF, eventFrameName, afIFRemark), true, m_nCurNo, 3, true, PC00D01.MSGTERR);
                                m_Logger.WriteLog(string.Format(PC00D01.FailedtoEF, eventFrameName, afIFRemark), PC00D01.MSGTERR, m_strEName);
                                ThreadStatus = IF_STATUS.InternalError;
                            }

                            result = m_sqlBiz.UpdateMeasureEventFrameResult(strSeq, afIFFlag, ifCount, afIFRemark, ref errCode, ref errText);

                            if (!result)
                            {
                                mOwner.listViewMsg(m_strEName, $"UpdateMeasureEventFrameResult Failed. [{eventFrameName}] - {errText}", true, m_nCurNo, 3, true, PC00D01.MSGTERR);
                                m_Logger.WriteLog($"UpdateMeasureEventFrameResult Failed. [{eventFrameName}] - {errText}", PC00D01.MSGTERR, m_strEName);
                                ThreadStatus = IF_STATUS.InternalError;
                            }
                        }
                    }
                }
                else
                {
                    ThreadStatus = IF_STATUS.InternalError;
                    result = false;
                }
            }
            catch (Exception ex)
            {
                mOwner.listViewMsg(m_strEName, ex.ToString(), true, m_nCurNo, 3, true, PC00D01.MSGTERR);
                m_Logger.WriteLog($"ProcessEventFrame - {ex} ", PC00D01.MSGTERR, m_strEName);
                ThreadStatus = IF_STATUS.InternalError;
                result = false;
            }
            return result;
        }
        private (string afIFFlag, string afIFRemark) SaveEventFrame(string eventFrameName, AFElementTemplate eventFrameTemplate, string startTime, string endTime, List<EventFrameAttributeData> listAttributes)
        {
            try
            {
                // AF 연결 
                if (!CheckAFDatabase(out string errString))
                {
                    mOwner.listViewMsg(m_strEName, $"{errString}", true, m_nCurNo, 3, true, PC00D01.MSGTERR);
                    return ("E", errString);
                }

                // EF 만들기
                AFEventFrame ef = new AFEventFrame(_AFDatabase, eventFrameName, eventFrameTemplate);
                ef.SetStartTime(startTime);
                ef.SetEndTime(endTime);

                listAttributes.ForEach(attrib => ef.Attributes[attrib.Name]?.SetValue(new AFValue(attrib.Value)));
                ef.CheckIn();
            }
            catch (Exception ex)
            {
                mOwner.listViewMsg(m_strEName, $"Exception in SaveEventFrame - ({ex})", true, m_nCurNo, 3, true, PC00D01.MSGTERR);
                return ("E", $"Exception in SaveEventFrame - ({ex})");
            }
            return ("Y", string.Empty);
        }

        private bool CheckAFDatabase(out string errText)
        {
            errText = string.Empty;
            lock (objLock)
            {
                // AF 연결 
                if (_AFDatabase == null || !_AFDatabase.PISystem.ConnectionInfo.IsConnected)
                {
                    _AFDatabase = GetAFDatabase(m_clsPIInfo.strPI_Server, m_clsPIInfo.AF_DB, m_clsPIInfo.AF_USER, m_clsPIInfo.AF_PWD, m_clsPIInfo.AF_DOMAIN, out string errString);
                    if (_AFDatabase == null)
                    {
                        mOwner.listViewMsg(m_strEName, $"AF not connected.", true, m_nCurNo, 3, true, PC00D01.MSGTINF);
                        errText = $"AF not connected. {(string.IsNullOrWhiteSpace(errString) ? "" : errString)}";
                        return false;
                    }
                }
                if (!_AFDatabase.PISystem.ConnectionInfo.IsConnected)
                {
                    mOwner.listViewMsg(m_strEName, $"PISystem not connected.", true, m_nCurNo, 3, true, PC00D01.MSGTINF);
                    errText = "PISystem not connected.";
                    return false;
                }
            }
            return true;
        }

        public AFDatabase GetAFDatabase(string serverName, string databaseName, string user, string pwd, string domain, out string errString)
        {
            try
            {
                errString = string.Empty;

                PISystems systems = new PISystems();
                PISystem assetServer;

                if (!string.IsNullOrEmpty(serverName))
                    assetServer = systems[serverName];
                else
                    assetServer = systems.DefaultPISystem;

                NetworkCredential credential = new NetworkCredential(user, pwd, domain);

                assetServer.Connect(credential);
                if (assetServer.ConnectionInfo.IsConnected)
                {
                    if (!string.IsNullOrEmpty(databaseName))
                        return assetServer.Databases[databaseName];
                    else
                        return assetServer.Databases.DefaultDatabase;
                }
            }
            catch (Exception ex)
            {
                errString = ex.Message;
            }
            return null;
        }

        private AFElementTemplate GetEventFrameTemplate(AFDatabase afDB, string templateName)
        {
            AFElementTemplate result = afDB?.ElementTemplates?.Where(x => x.InstanceType.Equals(typeof(AFEventFrame)) && x.Name.Equals(templateName))?.FirstOrDefault();
            //return afDB?.ElementTemplates?.Where(x => x.InstanceType.Equals(typeof(AFEventFrame)) && x.Name.Equals(templateName)).First();
            return result;
        }

        // PC03 은 무조건 기본 템플릿으로 가능한지 확인 후 안될 경우 다른 템플릿으로 확인 후 다 안되면 새로 생성
        private AFElementTemplate GetEventFrameTemplate(AFDatabase afDB, string equipTypeName, int msgType, List<string> listAttributeNames)
        {
            AFElementTemplate efTemp = null;

            //if (afDB == null) throw new ArgumentNullException(nameof(afDB));
            string efTemplateName = m_sqlBiz.ReadSTCommon(equipTypeName, "EventFrameTemplateName");

            // 최초 afDB 로드 후 다른 인스턴스에서 변경된 내용은 refresh 해야 한다
            afDB.Refresh();

            if (!string.IsNullOrWhiteSpace(efTemplateName))
            {
                // 설정된 템플릿 조회
                efTemp = GetEventFrameTemplate(afDB, efTemplateName);

                if (efTemp != null)
                {
                    List<string> omittedAttributeNames = efTemp.AttributeTemplates.GetOmittedAttributes(listAttributeNames);
                    // Template 의 Attribute 에 저장할 데이터의 Attribute 가 다 있으면 사용 
                    if (omittedAttributeNames.Count == 0)
                    {
                        return efTemp;
                    }
                    mOwner.listViewMsg(m_strEName, $"EventFrameTemplate({efTemplateName}) is not matches. Omitted attribute names : {string.Join(", ", omittedAttributeNames)}.", true, m_nCurNo, 3, true, PC00D01.MSGTINF);
                }
            }

            // 기본 템플릿 이름으로 시작하는 템플릿 모두 조회
            //AFElementTemplates eventFrameTemplate = afDB.ElementTemplates?.Where(x => x.InstanceType.Equals(typeof(AFEventFrame)) && x.Name.StartsWith(efTemplateBaseName)).OrderBy(x => x.Name);
            IEnumerable<AFElementTemplate> eventFrameTemplates = from temp in afDB.ElementTemplates 
                                                                 where temp.InstanceType.Equals(typeof(AFEventFrame)) && temp.Name.StartsWith($"{equipTypeName}_{msgType:00}") && !temp.Name.Equals(efTemplateName)
                                                                 orderby temp.Name descending select temp;

            efTemp = eventFrameTemplates.FirstOrDefault(x => x.AttributeTemplates.GetOmittedAttributes(listAttributeNames).Count == 0);

            // 저장가능한 템플릿이 없으면 새로 생성
            if (efTemp == null)
            {
                string eventFrameTemplateName = $"{equipTypeName}_{msgType:00}_{DateTime.Now:yyyyMMddHHmmssfff}";
                mOwner.listViewMsg(m_strEName, $"There are no matched EventFrameTemplates. Create new EventFrameTemplate Name : {eventFrameTemplateName}, AttributeTemplates : {string.Join(", ", listAttributeNames)}.", true, m_nCurNo, 3, true, PC00D01.MSGTINF);
                efTemp = CreateEventFrameTemplate(afDB, eventFrameTemplateName, listAttributeNames);
            }
            return efTemp;
            //return efTemp ?? CreateEventFrameTemplate(afDB, $"{efTemplateBaseName}_{DateTime.Now:yyyyMMddHHmmssfff}", listAttributeNames);
        }


        private AFElementTemplate CreateEventFrameTemplate(AFDatabase database, string eventFrameTemplateName, List<string> listAttributeNames)
        {
            AFElementTemplate eventFrameTemplate = null;

            if (database == null) throw new ArgumentNullException(nameof(database));
            
            eventFrameTemplate = database.ElementTemplates.Add(eventFrameTemplateName);
            eventFrameTemplate.InstanceType = typeof(AFEventFrame);
            eventFrameTemplate.Description = $"{eventFrameTemplateName}";

            mOwner.listViewMsg(m_strEName, $"CreateEventFrameTemplate ({eventFrameTemplateName}).", true, m_nCurNo, 3, true, PC00D01.MSGTINF);

            AFAttributeTemplate afAttrib;

            foreach (var attribName in listAttributeNames)
            {
                afAttrib = eventFrameTemplate.AttributeTemplates.Add(attribName);
                afAttrib.Type = typeof(string);
            }

            eventFrameTemplate.CheckIn();

            return eventFrameTemplate;
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
                    // PI POINT
                    ProcessPIPoint();
                    // EVENTFRAME
                    ProcessEventFrame();
                }
                catch (Exception ex)
                {
                    mOwner.listViewMsg(m_strEName, ex.ToString(), true, m_nCurNo, 3, true, PC00D01.MSGTERR);
                    m_Logger.WriteLog($"ThreadJob - {ex} ", PC00D01.MSGTERR, m_strEName);
                    ThreadStatus = IF_STATUS.InternalError;
                }
                finally
                {
                }
                Thread.Sleep(100);
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
