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
    /// File I/F
    /// </summary>
    public class PC01S170 : PC00B01
    {
        protected DateTime dtLastWriteTime = DateTime.MinValue;
        private DateTime m_dtLastProcessTime = DateTime.MinValue;
        private string m_SoftwareVersion = string.Empty;
        //CedexHiResDataSet cedexhires = null;
        //public CedexHiResDataSetTableAdapters.QueriesTableAdapter queriesTableAdapter = null;
        //public CedexHiResDataSetTableAdapters.ReportValueTableAdapter reportValueTableAdapter = null;
        //public CedexHiResDataSetTableAdapters.ImageDataTableAdapter imageDataTableAdapter = null;
        //public CedexHiResDataSetTableAdapters.IROperatorInputValueTableAdapter irOperatorInputValueTableAdapter = null;
        //public CedexHiResDataSetTableAdapters.MeasurementResultDataTableAdapter measurementResultDataTableAdapter = null;
        //public CedexHiResDataSetTableAdapters.ImageStatusTableAdapter imageStatusTableAdapter = null;

        public PC01S170() : base()
        {
        }

        public PC01S170(PC01F01 pOwner, string equipType, string equipName, string connectionInfo, string extraInfo, int nCurNo, bool bAutoRun = false) : base(pOwner, equipType, equipName, connectionInfo, extraInfo, nCurNo, bAutoRun)
        {
        }

        public PC01S170(PC01F01 pOwner, DataRow dr, int nCurNo, bool bAutoRun = false) : this(pOwner, dr["EQUIP_TYPE_NM"].ToString(), dr["EQUIP_NM"].ToString(), dr["CONNECTION_INFO"].ToString(), dr["EXTRA_INFO"].ToString(), nCurNo, bAutoRun)
        {
            drEquipment = dr;


            //Properties.Settings.Default["CedexConnectionString"] = m_ConnectionInfo;
            //cedexhires = new CedexHiResDataSet();
            //queriesTableAdapter = new CedexHiResDataSetTableAdapters.QueriesTableAdapter();
            //reportValueTableAdapter = new CedexHiResDataSetTableAdapters.ReportValueTableAdapter();
            //imageDataTableAdapter = new CedexHiResDataSetTableAdapters.ImageDataTableAdapter();
            //irOperatorInputValueTableAdapter = new CedexHiResDataSetTableAdapters.IROperatorInputValueTableAdapter();
            //measurementResultDataTableAdapter = new CedexHiResDataSetTableAdapters.MeasurementResultDataTableAdapter();
            //imageStatusTableAdapter = new CedexHiResDataSetTableAdapters.ImageStatusTableAdapter();
            if (bAutoRun == true)
            {
                m_Thd = new Thread(ThreadJob);
                m_Thd.Start();
            }
        }
        private string GetSoftwareVersion()
        {
            string softwareVersion = PC00U01.ReadConfigValue("SoftwareVersion", m_Name, $@".\CFG\{m_Type}.ini");
            listViewMsg.UpdateMsg($"Read SoftwareVersion  : {softwareVersion}", false, true);
            listViewMsg.UpdateMsg($"SoftwareVersion :{softwareVersion}  !", false, true, true, PC00D01.MSGTINF);
            return softwareVersion;
        }
        private DateTime GetLastProcessTime()
        {
            DateTime LastProcessTime;
            string strLastProcessTime = PC00U01.ReadConfigValue("LastProcessTime", m_Name, $@".\CFG\{m_Type}.ini");
            DateTime.TryParseExact(strLastProcessTime, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal | DateTimeStyles.AllowInnerWhite, out LastProcessTime);
            if (LastProcessTime < new DateTime(DateTime.Now.Year, 1, 1, 0, 0, 0, 0))
                LastProcessTime = new DateTime(DateTime.Now.Year, 1, 1, 0, 0, 0, 0);
            listViewMsg.UpdateMsg($"Read last Process Time  : {LastProcessTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}", false, true);
            listViewMsg.UpdateMsg($"LastProcessTime :{LastProcessTime.ToString("yyyy-MM-dd HH:mm:ss.fff") } , {strLastProcessTime}  !", false, true, true, PC00D01.MSGTINF);
            return LastProcessTime;
        }
        private bool SetLastProcessTime(DateTime LastProcessTime)
        {
            if (!PC00U01.WriteConfigValue("LastProcessTime", m_Name, $@".\CFG\{m_Type}.ini", $"{LastProcessTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}"))
            {
                listViewMsg.UpdateMsg($"Error to write LastProcessTime to INI file", false, true);
                return false;
            }
            listViewMsg.UpdateMsg($"Write last LastEnqueuedDate : {LastProcessTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}", false, true);
            return true;
        }

        private void ThreadJob()
        {
            listViewMsg.UpdateStatus(true);
            listViewMsg.UpdateMsg("Thread started");
            m_SoftwareVersion = GetSoftwareVersion();
            listViewMsg.UpdateMsg($"Read From Ini File m_SoftwareVersion :{m_SoftwareVersion}", false, true, true, PC00D01.MSGTINF);
            m_dtLastProcessTime = GetLastProcessTime();
            listViewMsg.UpdateMsg($"Read From Ini File m_dtLastProcessTime :{m_dtLastProcessTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}", false, true, true, PC00D01.MSGTINF);

            while (!bTerminal)
            {
                try
                {
                    UpdateEquipmentProgDateTime(IF_STATUS.Normal);
                    string data = ResultProcess();
                    if (string.IsNullOrWhiteSpace(data))
                    {
                        listViewMsg.UpdateMsg("No data in DB", true, false);
                        Thread.Sleep(1000);
                        continue;
                    }
                    EnQueue(MSGTYPE.MEASURE, data);
                    listViewMsg.UpdateMsg($"{m_Name}({MSGTYPE.MEASURE}) Data has been enqueued", true, true);
                    fileLog.WriteData(data, "EnQ", $"{m_Name}({MSGTYPE.MEASURE})");
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
            listViewMsg.UpdateMsg("Thread finished");
        }

        private string ResultProcess()
        {
            string strCon = m_ConnectionInfo;
            SqlConnection SqlCon = new SqlConnection(m_ConnectionInfo);
            SqlCon.Open();

            string strEqpType = drEquipment.ItemArray[2].ToString();
            string strEqpID = drEquipment.ItemArray[5].ToString();
            
            try
            {
                StringBuilder sbData = new StringBuilder();
                string strDate = "";
                DateTime NewProcessTime;
                string strSql;

                //                DateTime? NewProcessTime = (DateTime?)queriesTableAdapter.GetNewProcessTime(m_dtLastProcessTime);
                strSql  = $" SELECT TOP 1 ProcessDataSet.ProcessTime as ProcessTime  FROM Measurement INNER JOIN ";
                strSql += $" ProcessDataSet ON Measurement.MeasurementID = ProcessDataSet.MeasurementID ";
                strSql += $" WHERE(ProcessDataSet.ProcessTime > '{m_dtLastProcessTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}') AND(NOT(Measurement.DateFinished IS NULL))";
                strSql += $" ORDER BY ProcessDataSet.ProcessTime ";
                //listViewMsg.UpdateMsg($"QueryString :{strSql} !", false, true, true, PC00D01.MSGTINF);

                SqlCommand Cmd = new SqlCommand(strSql, SqlCon);
                SqlDataReader reader = Cmd.ExecuteReader();
                if (reader.Read())
                {
                    NewProcessTime = (DateTime)(reader["ProcessTime"]);

                    //CedexHiResDataSet.ReportValueDataTable dt = reportValueTableAdapter.GetData(NewProcessTime);
                    strSql = $" SELECT TOP(1) ProcessDataSet.CedexSystemId, CedexSystem.Name AS CedexSystemNm, Measurement.ReactorIdentifier, Measurement.SampleIdentifier, ";
                    strSql += $" CASE dbo.ProcessDataSet.IROperator WHEN '12' THEN 'CXHiResImageOperator' WHEN '13' THEN 'HiRes Illumination Test(V1.10)' WHEN '11' THEN 'DBDM Operator' END AS Algorithm,";
                    strSql += $" CASE dbo.Measurement.Precision WHEN '4' THEN 'MAXIMUM' WHEN '3' THEN 'SUPERIOR' WHEN '2' THEN 'NORMAL' WHEN '1' THEN 'MINIMUM' END AS MeasurementPrecision, ";
                    strSql += $" [User].Username AS MeasurementUser, Workarea.Name AS Workarea, STR(Measurement.PreparationFactor, 20, 3) AS Correction, ProcessDataSet.MeasurementCellTypeName AS CellTypeName, ";
                    strSql += $" CASE WHEN dbo.ProcessDataSet.Dilution = 0 THEN '0' WHEN CAST(STR(1. / dbo.ProcessDataSet.Dilution, 5, 0) AS float) ";
                    strSql += $" = 1. / dbo.ProcessDataSet.Dilution THEN '1 : ' + CAST(1. / dbo.ProcessDataSet.Dilution AS VARCHAR(5)) ELSE '1 : ' + STR(1. / dbo.ProcessDataSet.Dilution, 5, 3) END AS Dilution, ";
                    strSql += $" ProcessDataSet.DataSetName, ProcessDataSet.ProcessTime, ProcessDataSet.ReportCounter, Measurement.ChamberHeight AS CH, Measurement.FlowFactor AS Ff, ProcessDataSet.Comment, ";
                    strSql += $" Measurement.Valid, Measurement.DateFinished ";
                    strSql += $" FROM  Measurement INNER JOIN ";
                    strSql += $" ProcessDataSet ON Measurement.MeasurementID = ProcessDataSet.MeasurementID INNER JOIN ";
                    strSql += $" CedexSystem ON Measurement.CedexSystemID = CedexSystem.Id AND ProcessDataSet.CedexSystemId = CedexSystem.Id INNER JOIN ";
                    strSql += $" Workarea ON Measurement.WorkareaID = Workarea.WorkareaId AND CedexSystem.Id = Workarea.CedexSystemId INNER JOIN ";
                    strSql += $" [User] ON Measurement.UserID = [User].UserId AND ProcessDataSet.UserID = [User].UserId AND CedexSystem.Id = [User].CedexSystemId ";
                    strSql += $" WHERE(ProcessDataSet.ProcessTime = '{NewProcessTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}') ";
                    strSql += $" ORDER BY ProcessDataSet.ProcessTime ";
                    listViewMsg.UpdateMsg($"QueryString :{strSql} !", false, true, true, PC00D01.MSGTINF);

                    SqlConnection SqlCon2 = new SqlConnection(m_ConnectionInfo);
                    SqlCon2.Open();
                    SqlCommand Cmd2 = new SqlCommand(strSql, SqlCon2);
                    SqlDataReader dr = Cmd2.ExecuteReader();
                    if( dr.Read() ) strDate = ((DateTime)dr["ProcessTime"]).ToString("yyyyMMddHHmmss.fff");

                    //DataRow dr = dt.Rows[0];
                    //strDate = DateTime.Parse(dr["ProcessTime"].ToString()).ToString("yyyyMMddHHmmss.fff");

                    //CedexHiResDataSet.MeasurementResultDataDataTable mr_dt = measurementResultDataTableAdapter.GetData(NewProcessTime);
                    strSql = $" SELECT MeasurementResultData.OverallType, MeasurementResultData.MeasurementResultValue ";
                    strSql += $" FROM  ProcessDataSet INNER JOIN";
                    strSql += $" Measurement ON ProcessDataSet.MeasurementID = Measurement.MeasurementID INNER JOIN ";
                    strSql += $" MeasurementResultData ON ProcessDataSet.ProcessDataSetID = MeasurementResultData.ProcessDataSetID ";
                    strSql += $" WHERE(ProcessDataSet.ProcessTime = '{NewProcessTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}') ";
                    strSql += $" ORDER BY MeasurementResultData.OverallType ";
                    listViewMsg.UpdateMsg($"QueryString :{strSql} !", false, true, true, PC00D01.MSGTINF);

                    SqlConnection SqlCon3 = new SqlConnection(m_ConnectionInfo);
                    SqlCon3.Open();
                    SqlCommand Cmd3 = new SqlCommand(strSql, SqlCon3);
                    SqlDataReader mr_dr = Cmd3.ExecuteReader();
                    while (mr_dr.Read())
                    {
                        sbData.Append($"{mr_dr["OverallType"]},{strDate},{mr_dr["MeasurementResultValue"]}" + Environment.NewLine);
                    }
                    SqlCon3.Close(); SqlCon3 = null;

                    //                sbData.Append($"{dr["OverallType"]},{strDate },{dr["MeasurementResultValue"]}" + Environment.NewLine);

                    //foreach (DataRow mr_dr in mr_dt.Rows)
                    //{
                    //    sbData.Append($"{mr_dr["OverallType"].ToString()},{strDate},{mr_dr["MeasurementResultValue"].ToString()}" + Environment.NewLine);
                    //}
                    //                            sbData.Append(dr["OverallType"].ToString() + "," + strDate + "," + dr["MeasurementResultValue"].ToString() + Environment.NewLine);
                    sbData.Append($"CedexSystemId,{strDate}, {dr["CedexSystemID"]}" + Environment.NewLine);
                    sbData.Append($"CedexSystemNm,{strDate}, {dr["CedexSystemNm"]}" + Environment.NewLine);
                    sbData.Append($"SoftwareVersion,{strDate},{m_SoftwareVersion}" + Environment.NewLine);
                    sbData.Append($"ReactorId,{strDate},{dr["ReactorIdentifier"]}" + Environment.NewLine);
                    sbData.Append($"SampleId,{strDate},{dr["SampleIdentifier"]}" + Environment.NewLine);

                    //string JpeqQuality = (string)queriesTableAdapter.GetOptionValue("JpegQuality");
                    strSql = $" SELECT SystemOptionValue AS JpegQuality";
                    strSql += $" FROM SystemConfiguration ";
                    strSql += $" WHERE SystemOptionName = 'JpegQuality' ";

                    SqlConnection SqlCon4 = new SqlConnection(m_ConnectionInfo);
                    SqlCon4.Open();
                    SqlCommand Cmd4 = new SqlCommand(strSql, SqlCon4);
                    SqlDataReader jpeq_dr = Cmd4.ExecuteReader();
                    string JpeqQuality = string.Empty;
                    if (jpeq_dr.Read()) JpeqQuality = (string)jpeq_dr["JpegQuality"];
                    SqlCon4.Close(); SqlCon4 = null;

                    sbData.Append($"JpegQuality,{strDate},{JpeqQuality}" + Environment.NewLine);
                    sbData.Append($"Algorithm,{strDate},{dr["Algorithm"]}" + Environment.NewLine);
                    sbData.Append($"MeasurementPrecision,{strDate},{dr["MeasurementPrecision"]}" + Environment.NewLine);
                    sbData.Append($"MeasurementUser,{strDate},{dr["MeasurementUser"]}" + Environment.NewLine);
                    sbData.Append($"Workarea,{strDate},{dr["Workarea"]}" + Environment.NewLine);
                    sbData.Append($"Correction,{strDate},{dr["Correction"]}" + Environment.NewLine);
                    sbData.Append($"CellType,{strDate},{dr["CellTypeName"]}" + Environment.NewLine);
                    sbData.Append($"Dilution,{strDate},{dr["Dilution"]}" + Environment.NewLine);
                    sbData.Append($"DatasetName,{strDate},{dr["DatasetName"]}" + Environment.NewLine);
                    sbData.Append($"ProcessTime,{strDate},{strDate}" + Environment.NewLine);
                    sbData.Append($"ReportCounter,{strDate},{dr["ReportCounter"]}" + Environment.NewLine);

                    string imgConf = string.Empty;
                    //CedexHiResDataSet.ImageDataDataTable id_dt = imageDataTableAdapter.GetData(NewProcessTime);
                    strSql = $"SELECT TOP(1) Image.PixelsPerMilimetreX AS XM, Image.PixelsPerMilimetreY AS YM, Image.ResolutionX AS CX, Image.ResolutionY AS CY ";
                    strSql += $"FROM  Measurement INNER JOIN ";
                    strSql += $"ProcessDataSet ON Measurement.MeasurementID = ProcessDataSet.MeasurementID INNER JOIN ";
                    strSql += $"Image ON Measurement.MeasurementID = Image.MeasurementID ";
                    strSql += $"WHERE(ProcessDataSet.ProcessTime = '{NewProcessTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}') AND(Image.ImageNo = 1) ";
                    strSql += $"ORDER BY ProcessDataSet.ProcessTime ";
                    listViewMsg.UpdateMsg($"QueryString :{strSql} !", false, true, true, PC00D01.MSGTINF);

                    SqlConnection SqlCon5 = new SqlConnection(m_ConnectionInfo);
                    SqlCon5.Open();
                    SqlCommand Cmd5 = new SqlCommand(strSql, SqlCon5);
                    SqlDataReader id_dr = Cmd5.ExecuteReader();

                    if (id_dr.Read() == false)
                    {
                        listViewMsg.UpdateMsg($"read false mageXm !", false, true, true, PC00D01.MSGTINF);

                        sbData.Append($"ImageXm,{strDate}, n/a" + Environment.NewLine);
                        sbData.Append($"ImageYm,{strDate}, n/a" + Environment.NewLine);
                        sbData.Append($"ImageCh,{strDate},{dr["CH"]}" + Environment.NewLine);
                        sbData.Append($"ImageFf,{strDate},{dr["Ff"]}" + Environment.NewLine);
                        sbData.Append($"ImageCx,{strDate}, n/a" + Environment.NewLine);
                        sbData.Append($"ImageCy,{strDate}, n/a" + Environment.NewLine);
                        imgConf = $"Xm = n/a, ";
                        imgConf += $"Ym = n/a, ";
                        imgConf += $"Ch = {dr["CH"]}, ";
                        imgConf += $"Ff = {dr["Ff"]}, ";
                        imgConf += $"Cx = n/a, ";
                        imgConf += $"Cy = n/a ";
                    }
                    else
                    {
                        //                        DataRow id_dr = id_dt.Rows[0];
                        listViewMsg.UpdateMsg($"read true ImageXm !", false, true, true, PC00D01.MSGTINF);
                        sbData.Append($"ImageXm,{strDate},{id_dr["XM"]}" + Environment.NewLine);
                        sbData.Append($"ImageYm,{strDate},{id_dr["YM"]}" + Environment.NewLine);
                        sbData.Append($"ImageCh,{strDate},{dr["CH"]}" + Environment.NewLine);
                        sbData.Append($"ImageFf,{strDate},{dr["Ff"]}" + Environment.NewLine);
                        sbData.Append($"ImageCx,{strDate},{id_dr["CX"]}" + Environment.NewLine);
                        sbData.Append($"ImageCy,{strDate},{id_dr["CY"]}" + Environment.NewLine);
                        imgConf = $"Xm = {id_dr["XM"]}, ";
                        imgConf += $"Ym = {id_dr["YM"]}, ";
                        imgConf += $"Ch = {dr["CH"]}, ";
                        imgConf += $"Ff = {dr["Ff"]}, ";
                        imgConf += $"Cx = {id_dr["CX"]}, ";
                        imgConf += $"Cy = {id_dr["CY"]} ";
                    }
                    sbData.Append($"ImageConf,{strDate},{imgConf}" + Environment.NewLine);
                    sbData.Append($"Comment,{strDate},{dr["Comment"]}" + Environment.NewLine);

                    if (dr["Valid"].ToString().Trim() == "True")
                        sbData.Append($"Valid,{strDate},Valid" + Environment.NewLine);
                    else
                        sbData.Append($"Valid,{strDate},Invalid" + Environment.NewLine);

                    SqlCon5.Close(); SqlCon5 = null;

                    //int? images=(int?)queriesTableAdapter.GetImages(NewProcessTime);
                    strSql = $"SELECT COUNT(ProcessDataSetImageResult.ImageID) AS Images ";
                    strSql += $"FROM ProcessDataSetImageResult INNER JOIN ";
                    strSql += $"ProcessDataSet ON ProcessDataSetImageResult.ProcessDataSetID = ProcessDataSet.ProcessDataSetID ";
                    strSql += $"WHERE(ProcessDataSet.ProcessTime = '{NewProcessTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}' ) ";
                    strSql += $"GROUP BY ProcessDataSet.ProcessDataSetID, ProcessDataSetImageResult.ProcessDataSetID ";
                    listViewMsg.UpdateMsg($"QueryString :{strSql} !", false, true, true, PC00D01.MSGTINF);

                    SqlConnection SqlCon6 = new SqlConnection(m_ConnectionInfo);
                    SqlCon6.Open();
                    SqlCommand Cmd6 = new SqlCommand(strSql, SqlCon6);
                    SqlDataReader image_dr = Cmd6.ExecuteReader();
                    int images = 0;
                    if (image_dr.Read()) images = (int)image_dr["Images"];
                    SqlCon6.Close(); SqlCon6 = null;

                    sbData.Append($"Images,{strDate},{images}" + Environment.NewLine);

                    //CedexHiResDataSet.IROperatorInputValueDataTable ir_dt = irOperatorInputValueTableAdapter.GetData(NewProcessTime);
                    strSql = $"SELECT MeasurementIRParameter.IROperatorInputName, MeasurementIRParameter.IROperatorInputValue, ProcessDataSet.ProcessTime ";
                    strSql += $"FROM  Measurement INNER JOIN ";
                    strSql += $"ProcessDataSet ON Measurement.MeasurementID = ProcessDataSet.MeasurementID INNER JOIN ";
                    strSql += $"MeasurementIRParameter ON ProcessDataSet.ProcessDataSetID = MeasurementIRParameter.ProcessDataSetId ";
                    strSql += $"WHERE(ProcessDataSet.ProcessTime = '{NewProcessTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}') ";
                    strSql += $"ORDER BY MeasurementIRParameter.IROperatorInputName";
                    listViewMsg.UpdateMsg($"QueryString :{strSql} !", false, true, true, PC00D01.MSGTINF);

                    SqlConnection SqlCon7 = new SqlConnection(m_ConnectionInfo);
                    SqlCon7.Open();
                    SqlCommand Cmd7 = new SqlCommand(strSql, SqlCon7);
                    SqlDataReader ir_dr = Cmd7.ExecuteReader();

                    string strPara = string.Empty;
                    //foreach (DataRow ir_dr in ir_dt.Rows)                       
                    while (ir_dr.Read())
                    {
                        sbData.Append($"{ir_dr["IROperatorInputName"]},{strDate},{ir_dr["IROperatorInputValue"]}" + Environment.NewLine);
                        strPara += $"{ir_dr["IROperatorInputValue"]}, ";
                    }
                    SqlCon7.Close(); SqlCon7 = null;

                    strPara = strPara.Substring(0, strPara.Length - 2);
                    sbData.Append($"Parameter,{strDate},{strPara}" + Environment.NewLine);
                    sbData.Append($"SvrTime,{strDate},{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}" + Environment.NewLine);

                    //                    CedexHiResDataSet.ImageStatusDataTable is_dt = imageStatusTableAdapter.GetData(NewProcessTime);
                    strSql = $"SELECT dbo.Image.ImageNo, dbo.ProcessDataSetImageResult.ImageStatus ";
                    strSql += $"FROM  dbo.ProcessDataSetImageResult INNER JOIN ";
                    strSql += $"dbo.ProcessDataSet ON dbo.ProcessDataSetImageResult.ProcessDataSetID = dbo.ProcessDataSet.ProcessDataSetID INNER JOIN ";
                    strSql += $"dbo.Image ON dbo.ProcessDataSetImageResult.ImageID = dbo.Image.ImageID ";
                    strSql += $"WHERE(dbo.ProcessDataSet.ProcessTime = '{NewProcessTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}') ";
                    strSql += $"ORDER BY dbo.Image.ImageNo ";
                    listViewMsg.UpdateMsg($"QueryString :{strSql} !", false, true, true, PC00D01.MSGTINF);

                    SqlConnection SqlCon8 = new SqlConnection(m_ConnectionInfo);
                    SqlCon8.Open();
                    SqlCommand Cmd8 = new SqlCommand(strSql, SqlCon8);
                    SqlDataReader is_dr = Cmd8.ExecuteReader();

                    //foreach (DataRow is_dr in is_dt.Rows)
                    while (is_dr.Read())
                    {
                        if (is_dr["ImageStatus"].ToString().Trim() == "1")
                            sbData.Append($"Valid{is_dr["Imageno"]},{strDate},Valid" + Environment.NewLine);
                        else
                            sbData.Append($"Valid{is_dr["Imageno"]},{strDate},Invalid" + Environment.NewLine);
                    }
                    SqlCon8.Close(); SqlCon8 = null;

                    m_dtLastProcessTime = (DateTime)NewProcessTime;
                    SetLastProcessTime(m_dtLastProcessTime);

                    SqlCon2.Close(); SqlCon2 = null;
                }

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
                SqlCon.Close(); SqlCon = null;
            }
        }
    }
}
