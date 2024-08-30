using DataSpider.PC00.PT;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Threading;

namespace DataSpider.PC01.PT
{
    /// <summary>
    /// CEDEX_HIRES SQL SERVER INTERFACE
    /// 
    /// </summary>
    public class PC01S17 : PC00B01
    {
        protected SqlConnection mCon;
        string p_strErrCode;
        string p_strErrText;

        protected DateTime dtLastWriteTime = DateTime.MinValue;
        private DateTime m_dtLastProcessTime = DateTime.MinValue;

        // SoftwareVersion  삭제하기로 협의됨. 20220609 165800
        //private string m_SoftwareVersion = string.Empty;

        public PC01S17() : base()
        {
        }

        public PC01S17(PC01F01 pOwner, DataRow dr, string equipType, string equipName, string connectionInfo, string extraInfo, int nCurNo, bool bAutoRun = false) : base(pOwner, dr, equipType, equipName, connectionInfo, extraInfo, nCurNo, bAutoRun)
        {
        }

        public PC01S17(PC01F01 pOwner, DataRow dr, int nCurNo, bool bAutoRun = false) : this(pOwner, dr, dr["EQUIP_TYPE_NM"].ToString(), dr["EQUIP_NM"].ToString(), dr["CONNECTION_INFO"].ToString(), dr["EXTRA_INFO"].ToString(), nCurNo, bAutoRun)
        {
            if (bAutoRun == true)
            {
                m_Thd = new Thread(ThreadJob);
                m_Thd.Start();
            }
        }
        private string GetSoftwareVersion()
        {
            string softwareVersion = m_sqlBiz.ReadSTCommon(m_Name, "SoftwareVersion"); //PC00U01.ReadConfigValue("SoftwareVersion", m_Name, $@".\CFG\{m_Type}.ini");
            listViewMsg.UpdateMsg($"Read SoftwareVersion  : {softwareVersion}", false, true);
            listViewMsg.UpdateMsg($"SoftwareVersion :{softwareVersion}  !", false, true, true, PC00D01.MSGTINF);
            return softwareVersion;
        }
        private DateTime GetLastProcessTime()
        {
            DateTime LastProcessTime = DateTime.MinValue;
            string strLastProcessTime = m_sqlBiz.ReadSTCommon(m_Name, "LastProcessTime"); //PC00U01.ReadConfigValue("LastProcessTime", m_Name, $@".\CFG\{m_Type}.ini");
            DateTime.TryParseExact(strLastProcessTime, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal | DateTimeStyles.AllowInnerWhite, out LastProcessTime);
            if (LastProcessTime == DateTime.MinValue) LastProcessTime = DateTime.Now;
            listViewMsg.UpdateMsg($"Read last Process Time  : {LastProcessTime:yyyy-MM-dd HH:mm:ss.fff}", false, true);
            listViewMsg.UpdateMsg($"LastProcessTime :{LastProcessTime:yyyy-MM-dd HH:mm:ss.fff} , {strLastProcessTime}  !", false, true, true, PC00D01.MSGTINF);
            return LastProcessTime;
        }
        private bool SetLastProcessTime(DateTime LastProcessTime)
        {
            //if (!PC00U01.WriteConfigValue("LastProcessTime", m_Name, $@".\CFG\{m_Type}.ini", $"{LastProcessTime:yyyy-MM-dd HH:mm:ss.fff}"))
            if (!m_sqlBiz.WriteSTCommon(m_Name, "LastProcessTime", $"{LastProcessTime:yyyy-MM-dd HH:mm:ss.fff}"))
            {
                listViewMsg.UpdateMsg($"Error to write LastProcessTime to INI file", false, true);
                return false;
            }
            listViewMsg.UpdateMsg($"Write last LastEnqueuedDate : {LastProcessTime:yyyy-MM-dd HH:mm:ss.fff}", false, true);
            return true;
        }

        private void ThreadJob()
        {
            listViewMsg.UpdateStatus(true);
            listViewMsg.UpdateMsg($"{m_Name} Thread started");
            //m_SoftwareVersion = GetSoftwareVersion();
            //listViewMsg.UpdateMsg($"Read From Ini File m_SoftwareVersion :{m_SoftwareVersion}", false, true, true, PC00D01.MSGTINF);
            m_dtLastProcessTime = GetLastProcessTime();
            listViewMsg.UpdateMsg($"Read From Ini File m_dtLastProcessTime :{m_dtLastProcessTime:yyyy-MM-dd HH:mm:ss.fff}", false, true, true, PC00D01.MSGTINF);
            string data = string.Empty;
            while (!bTerminal)
            {
                try
                {
                    if (mCon == null || mCon.State != ConnectionState.Open)
                    {
                        if (DBConnect(m_ConnectionInfo, ref p_strErrCode, ref p_strErrText))
                        {
                            UpdateEquipmentProgDateTime(IF_STATUS.Normal);
                            listViewMsg.UpdateMsg("DB Connected", true, true, true);
                        }
                        else
                        {
                            UpdateEquipmentProgDateTime(IF_STATUS.Disconnected);
                            listViewMsg.UpdateMsg($"DB Disconnected", true, true, true);
                            Thread.Sleep(10000);
                        }
                    }
                    else
                    {
                        if ((data = ResultProcess()) != string.Empty)
                        {
                            UpdateEquipmentProgDateTime(IF_STATUS.Normal);
                            //data = ResultProcess();
                            EnQueue(MSGTYPE.MEASURE, data);
                            listViewMsg.UpdateMsg($"{m_Name}({MSGTYPE.MEASURE}) Data has been enqueued", true, true);
                            fileLog.WriteData(data, "EnQ", $"{m_Name}({MSGTYPE.MEASURE})");
                            Thread.Sleep(1000);
                        }
                        else
                        {
                            if (mCon != null && string.IsNullOrWhiteSpace(data))
                            {
                                UpdateEquipmentProgDateTime(IF_STATUS.Normal);
                                listViewMsg.UpdateMsg("No data in DB", true, false);
                                Thread.Sleep(1000);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    UpdateEquipmentProgDateTime(IF_STATUS.InternalError);
                    listViewMsg.UpdateMsg($"Exception in ThreadJob - ({ex})", false, true, true, PC00D01.MSGTERR);
                }
                finally
                {
                }
                Thread.Sleep(10);
            }
            UpdateEquipmentProgDateTime(IF_STATUS.Stop);
            listViewMsg.UpdateStatus(false);
            listViewMsg.UpdateMsg($"{m_Name} Thread finished");
        }

        public bool DBConnect(string p_strConnectionString, ref string p_strErrCode, ref string p_strErrText)
        {
            bool bReturn = false;
            p_strErrCode = "";
            p_strErrText = "";
            try
            {
                mCon = new SqlConnection(p_strConnectionString);
                mCon.Open();
                bReturn = true;
            }
            catch (Exception ex)
            {
                p_strErrText = ex.Message;
                if (mCon != null)
                {
                    if (mCon.State == ConnectionState.Open)
                        mCon.Close();
                    mCon.Dispose();
                    mCon = null;
                }

            }
            finally
            {
            }
            return bReturn;
        }

        public DataTable GetDataTable(string strQuery, ref string _strErrCode, ref string _strErrText)
        {
            DataTable result = null;
            _strErrCode = "";
            _strErrText = "";
            try
            {
                DataSet ds = GetDataSet(strQuery, null, CommandType.Text, ref _strErrCode, ref _strErrText);
                if (ds != null && ds.Tables[0] != null)
                {
                    result = ds.Tables[0];
                }
            }
            catch (Exception ex)
            {
                _strErrText = ex.ToString();
            }
            return result;
        }

        public DataSet GetDataSet(string commandText, SqlParameter[] oraParameters, CommandType commandType, ref string p_strErrCode, ref string p_strErrText)
        {
//            SqlConnection mCon = null;
            SqlCommand mCommand = null;
            SqlDataAdapter da = null;
            DataSet dsReturn = new DataSet();
            p_strErrCode = "";
            p_strErrText = "";
            try
            {
                mCommand = new SqlCommand();
                mCommand.CommandType = commandType;
                mCommand.CommandText = commandText;
                //mCommand.BindByName = true;

                if (oraParameters != null)
                {
                    foreach (SqlParameter param in oraParameters)
                    {
                        mCommand.Parameters.Add(param);
                    }
                }
                if (mCon==null || mCon.State != ConnectionState.Open)
                {
                    mCon = new SqlConnection(m_ConnectionInfo);
                    mCon.Open();
                }
                mCommand.Connection = mCon;

                dsReturn = new DataSet();
                da = new SqlDataAdapter(mCommand);
                da.Fill(dsReturn);

            }
            catch (Exception ex)
            {
                p_strErrCode = ex.ToString();
                if (mCon != null)
                {
                    if (mCon.State == ConnectionState.Open)
                        mCon.Close();
                    mCon.Dispose();
                    mCon = null;
                }
                UpdateEquipmentProgDateTime(IF_STATUS.Disconnected);
                listViewMsg.UpdateMsg($"DB Disconnected", true, true, true);
            }
            finally
            {
                if (mCommand != null) mCommand.Dispose();
                if (da != null) da.Dispose();
            }

            return dsReturn;
        }

        private string ResultProcess()
        {
            //string strEqpType = drEquipment.ItemArray[2].ToString();
            //string strEqpID = drEquipment.ItemArray[5].ToString();

            try
            {
                StringBuilder sbData = new StringBuilder();
                string strDate = "";
                DateTime NewProcessTime;
                string strSql;
                DataTable dt1 = null, dt2 = null, dt3 = null, dt4 = null, dt5 = null, dt6 = null, dt7 = null, dt8 = null;
                DataRow dr1 = null, dr2 = null, dr3 = null, dr4 = null, dr5 = null, dr6 = null, dr7 = null, dr8 = null;

                //                DateTime? NewProcessTime = (DateTime?)queriesTableAdapter.GetNewProcessTime(m_dtLastProcessTime);
                strSql = $" SELECT TOP 1 P.ProcessTime as ProcessTime  FROM Measurement M INNER JOIN ";
                strSql += $" ProcessDataSet P ON M.MeasurementID = P.MeasurementID ";
                strSql += $" WHERE(P.ProcessTime > '{m_dtLastProcessTime:yyyy-MM-dd HH:mm:ss.fff}') AND(NOT(M.DateFinished IS NULL))";
                strSql += $" ORDER BY P.ProcessTime ";
                //listViewMsg.UpdateMsg($"QueryString :{strSql} !", false, true, true, PC00D01.MSGTINF);
                dt1 = GetDataTable(strSql, ref p_strErrCode, ref p_strErrText);
                if (dt1 == null)
                {
                    listViewMsg.UpdateMsg($" dt1 == null :  {p_strErrCode} : {p_strErrText} ", false, true, true, PC00D01.MSGTERR);
                    return string.Empty;
                }
                if (dt1.Rows.Count == 0) return string.Empty;

                listViewMsg.UpdateMsg($"Query Started.", false, true, true, PC00D01.MSGTINF);

                dr1 = dt1.Rows[0];
                NewProcessTime = (DateTime)dr1["ProcessTime"];

                strSql = $" SELECT TOP(1) P.CedexSystemId, CedexSystem.Name AS CedexSystemNm, M.ReactorIdentifier, M.SampleIdentifier, ";
                // 2022-07-27 치환표에 따라 수정
                // strSql += $" CASE P.IROperator WHEN '12' THEN 'CXHiResImageOperator' WHEN '13' THEN 'HiRes Illumination Test(V1.10)' WHEN '11' THEN 'DBDM Operator' END AS Algorithm,";
                strSql += $" CASE P.IROperator WHEN '12' THEN 'CXHiResImageOperator' WHEN '13' THEN 'IlluminationTestImageOperator' WHEN '11' THEN 'DBDMOperator' END AS Algorithm,";
                strSql += $" CASE M.Precision WHEN '4' THEN 'MAXIMUM' WHEN '3' THEN 'SUPERIOR' WHEN '2' THEN 'NORMAL' WHEN '1' THEN 'MINIMUM' END AS MeasurementPrecision, ";
                strSql += $" [User].Username AS MeasurementUser, Workarea.Name AS Workarea, STR(M.PreparationFactor, 20, 3) AS Correction, P.MeasurementCellTypeName AS CellTypeName, ";
                strSql += $" CASE WHEN P.Dilution = 0 THEN '0' WHEN CAST(STR(1. / P.Dilution, 5, 0) AS float) ";
                strSql += $" = 1. / P.Dilution THEN '1 : ' + CAST(1. / P.Dilution AS VARCHAR(5)) ELSE '1 : ' + STR(1. / P.Dilution, 5, 3) END AS Dilution, ";
                strSql += $" P.DataSetName, P.ProcessTime, P.ReportCounter, M.ChamberHeight AS CH, M.FlowFactor AS Ff, P.Comment, ";
                strSql += $" M.Valid, M.DateFinished ";
                strSql += $" FROM  Measurement M INNER JOIN ";
                strSql += $" ProcessDataSet P ON M.MeasurementID = P.MeasurementID INNER JOIN ";
                strSql += $" CedexSystem ON M.CedexSystemID = CedexSystem.Id AND P.CedexSystemId = CedexSystem.Id INNER JOIN ";
                strSql += $" Workarea ON M.WorkareaID = Workarea.WorkareaId AND CedexSystem.Id = Workarea.CedexSystemId INNER JOIN ";
                strSql += $" [User] ON M.UserID = [User].UserId AND CedexSystem.Id = [User].CedexSystemId ";
                // 쿼리 재검토 결과 M.USERID 와 P.USERID가 다른 경우가 있어서 에러가 발생할 수 있음.
                // 리포트의 스크립트와 비교하여 P.UserID = [User].UserId 부분을 삭제함. 2022-07-27
                //              strSql += $" [User] ON M.UserID = [User].UserId AND P.UserID = [User].UserId AND CedexSystem.Id = [User].CedexSystemId ";
                strSql += $" WHERE(P.ProcessTime = '{NewProcessTime:yyyy-MM-dd HH:mm:ss.fff}') ";
                strSql += $" ORDER BY P.ProcessTime ";
                dt2 = GetDataTable(strSql, ref p_strErrCode, ref p_strErrText);
                if (dt2 == null)
                {
                    listViewMsg.UpdateMsg($" dt2 == null :  {p_strErrCode} : {p_strErrText} ", false, true, true, PC00D01.MSGTERR);
                    return string.Empty;
                }
                if (dt2.Rows.Count == 0)
                {
                    listViewMsg.UpdateMsg($"dt2.Rows.Count==0 ", false, true, true, PC00D01.MSGTERR);
                    return string.Empty;
                }
                dr2 = dt2.Rows[0];
                strDate = ((DateTime)dr2["ProcessTime"]).ToString("yyyyMMddHHmmss.fff");

                strSql = $" SELECT MeasurementResultData.OverallType, MeasurementResultData.MeasurementResultValue ";
                strSql += $" FROM  ProcessDataSet P INNER JOIN";
                strSql += $" Measurement M ON P.MeasurementID = M.MeasurementID INNER JOIN ";
                strSql += $" MeasurementResultData ON P.ProcessDataSetID = MeasurementResultData.ProcessDataSetID ";
                strSql += $" WHERE(P.ProcessTime = '{NewProcessTime:yyyy-MM-dd HH:mm:ss.fff}') ";
                strSql += $" ORDER BY MeasurementResultData.OverallType ";
                dt3 = GetDataTable(strSql, ref p_strErrCode, ref p_strErrText);
                if (dt3 == null)
                {
                    listViewMsg.UpdateMsg($" dt3 == null :  {p_strErrCode} : {p_strErrText} ", false, true, true, PC00D01.MSGTERR);
                    return string.Empty;
                }
                foreach (DataRow mr_dr in dt3.Rows)
                {
                    sbData.Append($"{mr_dr["OverallType"]},{strDate},{mr_dr["MeasurementResultValue"]}" + Environment.NewLine);
                }
                sbData.Append($"CedexSystemId,{strDate}, {dr2["CedexSystemID"]}" + Environment.NewLine);
                sbData.Append($"CedexSystemNm,{strDate}, {dr2["CedexSystemNm"]}" + Environment.NewLine);
                //sbData.Append($"SoftwareVersion,{strDate},{m_SoftwareVersion}" + Environment.NewLine);
                sbData.Append($"ReactorId,{strDate},{dr2["ReactorIdentifier"]}" + Environment.NewLine);
                sbData.Append($"SampleId,{strDate},{dr2["SampleIdentifier"]}" + Environment.NewLine);

                strSql = $" SELECT SystemOptionValue AS JpegQuality";
                strSql += $" FROM SystemConfiguration ";
                strSql += $" WHERE SystemOptionName = 'JpegQuality' ";
                dt4 = GetDataTable(strSql, ref p_strErrCode, ref p_strErrText);
                if (dt4 == null)
                {
                    listViewMsg.UpdateMsg($" dt4 == null :  {p_strErrCode} : {p_strErrText} ", false, true, true, PC00D01.MSGTERR);
                    return string.Empty;
                }
                if (dt4.Rows.Count == 0)
                {
                    listViewMsg.UpdateMsg($"dt4.Rows.Count==0 ", false, true, true, PC00D01.MSGTERR);
                    return string.Empty;
                }
                string JpeqQuality = string.Empty;
                if (dt4.Rows.Count > 0)
                {
                    dr4 = dt4.Rows[0];
                    JpeqQuality = (string)dr4["JpegQuality"];
                }
                sbData.Append($"JpegQuality,{strDate},{JpeqQuality}" + Environment.NewLine);
                sbData.Append($"Algorithm,{strDate},{dr2["Algorithm"]}" + Environment.NewLine);
                sbData.Append($"MeasurementPrecision,{strDate},{dr2["MeasurementPrecision"]}" + Environment.NewLine);
                sbData.Append($"MeasurementUser,{strDate},{dr2["MeasurementUser"]}" + Environment.NewLine);
                sbData.Append($"Workarea,{strDate},{dr2["Workarea"]}" + Environment.NewLine);
                sbData.Append($"Correction,{strDate},{dr2["Correction"]}" + Environment.NewLine);
                sbData.Append($"CellType,{strDate},{dr2["CellTypeName"]}" + Environment.NewLine);
                sbData.Append($"Dilution,{strDate},{dr2["Dilution"]}" + Environment.NewLine);
                sbData.Append($"DatasetName,{strDate},{dr2["DatasetName"]}" + Environment.NewLine);
                sbData.Append($"ProcessTime,{strDate},{strDate}" + Environment.NewLine);
                sbData.Append($"ReportCounter,{strDate},{dr2["ReportCounter"]}" + Environment.NewLine);

                string imgConf = string.Empty;
                strSql = $"SELECT TOP(1) Image.PixelsPerMilimetreX AS XM, Image.PixelsPerMilimetreY AS YM, Image.ResolutionX AS CX, Image.ResolutionY AS CY ";
                strSql += $"FROM  Measurement M INNER JOIN ";
                strSql += $"ProcessDataSet P ON M.MeasurementID = P.MeasurementID INNER JOIN ";
                strSql += $"Image ON M.MeasurementID = Image.MeasurementID ";
                strSql += $"WHERE(P.ProcessTime = '{NewProcessTime:yyyy-MM-dd HH:mm:ss.fff}') AND(Image.ImageNo = 1) ";
                strSql += $"ORDER BY P.ProcessTime ";
                //listViewMsg.UpdateMsg($"QueryString :{strSql} !", false, true, true, PC00D01.MSGTINF);
                dt5 = GetDataTable(strSql, ref p_strErrCode, ref p_strErrText);
                if (dt5 == null)
                {
                    listViewMsg.UpdateMsg($" dt5 == null :  {p_strErrCode} : {p_strErrText} ", false, true, true, PC00D01.MSGTERR);
                    return string.Empty;
                }
                if (dt5.Rows.Count == 0)
                {
                    //listViewMsg.UpdateMsg($"read false mageXm !", false, true, true, PC00D01.MSGTINF);

                    sbData.Append($"ImageXm,{strDate}, n/a" + Environment.NewLine);
                    sbData.Append($"ImageYm,{strDate}, n/a" + Environment.NewLine);
                    sbData.Append($"ImageCh,{strDate},{dr2["CH"]}" + Environment.NewLine);
                    sbData.Append($"ImageFf,{strDate},{dr2["Ff"]}" + Environment.NewLine);
                    sbData.Append($"ImageCx,{strDate}, n/a" + Environment.NewLine);
                    sbData.Append($"ImageCy,{strDate}, n/a" + Environment.NewLine);
                    imgConf = $"Xm = n/a, ";
                    imgConf += $"Ym = n/a, ";
                    imgConf += $"Ch = {dr2["CH"]}, ";
                    imgConf += $"Ff = {dr2["Ff"]}, ";
                    imgConf += $"Cx = n/a, ";
                    imgConf += $"Cy = n/a ";
                }
                else
                {
                    dr5 = dt5.Rows[0];
                    //                        DataRow id_dr = id_dt.Rows[0];
                    //listViewMsg.UpdateMsg($"read true ImageXm !", false, true, true, PC00D01.MSGTINF);
                    sbData.Append($"ImageXm,{strDate},{dr5["XM"]}" + Environment.NewLine);
                    sbData.Append($"ImageYm,{strDate},{dr5["YM"]}" + Environment.NewLine);
                    sbData.Append($"ImageCh,{strDate},{dr2["CH"]}" + Environment.NewLine);
                    sbData.Append($"ImageFf,{strDate},{dr2["Ff"]}" + Environment.NewLine);
                    sbData.Append($"ImageCx,{strDate},{dr5["CX"]}" + Environment.NewLine);
                    sbData.Append($"ImageCy,{strDate},{dr5["CY"]}" + Environment.NewLine);
                    imgConf = $"Xm = {dr5["XM"]}, ";
                    imgConf += $"Ym = {dr5["YM"]}, ";
                    imgConf += $"Ch = {dr2["CH"]}, ";
                    imgConf += $"Ff = {dr2["Ff"]}, ";
                    imgConf += $"Cx = {dr5["CX"]}, ";
                    imgConf += $"Cy = {dr5["CY"]} ";
                }
                sbData.Append($"ImageConf,{strDate},{imgConf}" + Environment.NewLine);
                sbData.Append($"Comment,{strDate},{dr2["Comment"]}" + Environment.NewLine);

                if (dr2["Valid"].ToString().Trim() == "True")
                    sbData.Append($"Valid,{strDate},Valid" + Environment.NewLine);
                else
                    sbData.Append($"Valid,{strDate},Invalid" + Environment.NewLine);

                strSql = $"SELECT COUNT(ProcessDataSetImageResult.ImageID) AS Images ";
                strSql += $"FROM ProcessDataSetImageResult INNER JOIN ";
                strSql += $"ProcessDataSet P ON ProcessDataSetImageResult.ProcessDataSetID = P.ProcessDataSetID ";
                strSql += $"WHERE(P.ProcessTime = '{NewProcessTime:yyyy-MM-dd HH:mm:ss.fff}' ) ";
                strSql += $"GROUP BY P.ProcessDataSetID, ProcessDataSetImageResult.ProcessDataSetID ";
                //listViewMsg.UpdateMsg($"QueryString :{strSql} !", false, true, true, PC00D01.MSGTINF);
                dt6 = GetDataTable(strSql, ref p_strErrCode, ref p_strErrText);
                if (dt6 == null)
                {
                    listViewMsg.UpdateMsg($" dt6 == null :  {p_strErrCode} : {p_strErrText} ", false, true, true, PC00D01.MSGTERR);
                    return string.Empty;
                }

                int images = 0;
                if (dt6.Rows.Count >  0)
                {
                    dr6 = dt6.Rows[0];
                    images = (int)dr6["Images"];
                }
                sbData.Append($"Images,{strDate},{images}" + Environment.NewLine);

                strSql = $"SELECT MeasurementIRParameter.IROperatorInputName, MeasurementIRParameter.IROperatorInputValue, P.ProcessTime ";
                strSql += $"FROM  Measurement M INNER JOIN ";
                strSql += $"ProcessDataSet P ON M.MeasurementID = P.MeasurementID INNER JOIN ";
                strSql += $"MeasurementIRParameter ON P.ProcessDataSetID = MeasurementIRParameter.ProcessDataSetId ";
                strSql += $"WHERE(P.ProcessTime = '{NewProcessTime:yyyy-MM-dd HH:mm:ss.fff}') ";
                strSql += $"ORDER BY MeasurementIRParameter.IROperatorInputName";
                // listViewMsg.UpdateMsg($"QueryString :{strSql} !", false, true, true, PC00D01.MSGTINF);

                dt7 = GetDataTable(strSql, ref p_strErrCode, ref p_strErrText);
                if (dt7 == null)
                {
                    listViewMsg.UpdateMsg($" dt7 == null :  {p_strErrCode} : {p_strErrText} ", false, true, true, PC00D01.MSGTERR);
                    return string.Empty;
                }
                string strPara = string.Empty;
                foreach (DataRow ir_dr in dt7.Rows)
                {
                    sbData.Append($"{ir_dr["IROperatorInputName"]},{strDate},{ir_dr["IROperatorInputValue"]}" + Environment.NewLine);
                    strPara += $"{ir_dr["IROperatorInputValue"]}, ";
                }

                strPara = strPara.Substring(0, strPara.Length - 2);
                sbData.Append($"Parameter,{strDate},{strPara}" + Environment.NewLine);

                strSql = $"SELECT dbo.Image.ImageNo, dbo.ProcessDataSetImageResult.ImageStatus ";
                strSql += $"FROM  dbo.ProcessDataSetImageResult INNER JOIN ";
                strSql += $"dbo.ProcessDataSet P ON dbo.ProcessDataSetImageResult.ProcessDataSetID = P.ProcessDataSetID INNER JOIN ";
                strSql += $"dbo.Image ON dbo.ProcessDataSetImageResult.ImageID = dbo.Image.ImageID ";
                strSql += $"WHERE(P.ProcessTime = '{NewProcessTime:yyyy-MM-dd HH:mm:ss.fff}') ";
                strSql += $"ORDER BY dbo.Image.ImageNo ";
//                listViewMsg.UpdateMsg($"QueryString :{strSql} !", false, true, true, PC00D01.MSGTINF);

                dt8 = GetDataTable(strSql, ref p_strErrCode, ref p_strErrText);
                if (dt8 == null)
                {
                    listViewMsg.UpdateMsg($" dt8 == null :  {p_strErrCode} : {p_strErrText} ", false, true, true, PC00D01.MSGTERR);
                    return string.Empty;
                }
                foreach (DataRow is_dr in dt8.Rows)
                {
                    if (is_dr["ImageStatus"].ToString().Trim() == "1")
                        sbData.Append($"Valid{is_dr["Imageno"]},{strDate},Valid" + Environment.NewLine);
                    else
                        sbData.Append($"Valid{is_dr["Imageno"]},{strDate},Invalid" + Environment.NewLine);
                }

                sbData.Append($"SvrTime,{strDate},{DateTime.Now:yyyy-MM-dd HH:mm:ss}" + Environment.NewLine);

                m_dtLastProcessTime = (DateTime)NewProcessTime;
                SetLastProcessTime(m_dtLastProcessTime);

                return sbData.ToString().Trim();
            }
            catch (Exception ex)
            {
                UpdateEquipmentProgDateTime(IF_STATUS.InternalError);
                listViewMsg.UpdateMsg($"Exception in ResultProcess - ({ex})", false, true, true, PC00D01.MSGTERR);
                return string.Empty;
            }
            finally
            {
            }
        }
    }
}
