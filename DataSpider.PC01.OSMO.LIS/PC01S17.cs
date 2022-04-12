using SEIMM.PC00.PT;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Threading;

namespace SEIMM.PC01.PT
{
    /// <summary>
    /// File I/F
    /// </summary>
    public class PC01S17 : PC00B01
    {
        protected DateTime dtLastWriteTime = DateTime.MinValue;
        private DateTime m_dtLastProcessTime = DateTime.MinValue;
        private string m_SoftwareVersion = string.Empty;
        CedexHiResDataSet cedexhires = null;
        public CedexHiResDataSetTableAdapters.QueriesTableAdapter queriesTableAdapter = null;
        public CedexHiResDataSetTableAdapters.ReportValueTableAdapter reportValueTableAdapter = null;
        public CedexHiResDataSetTableAdapters.ImageDataTableAdapter imageDataTableAdapter = null;
        public CedexHiResDataSetTableAdapters.IROperatorInputValueTableAdapter irOperatorInputValueTableAdapter = null;
        public CedexHiResDataSetTableAdapters.MeasurementResultDataTableAdapter measurementResultDataTableAdapter = null;
        public CedexHiResDataSetTableAdapters.ImageStatusTableAdapter imageStatusTableAdapter = null;

        public PC01S17() : base()
        {
        }

        public PC01S17(PC01F01 pOwner, string equipType, string equipName, string connectionInfo, string extraInfo, int nCurNo, bool bAutoRun = false) : base(pOwner, equipType, equipName, connectionInfo, extraInfo, nCurNo, bAutoRun)
        {
        }

        public PC01S17(PC01F01 pOwner, DataRow dr, int nCurNo, bool bAutoRun = false) : this(pOwner, dr["EQUIP_TYPE_NM"].ToString(), dr["EQUIP_NM"].ToString(), dr["CONNECTION_INFO"].ToString(), dr["EXTRA_INFO"].ToString(), nCurNo, bAutoRun)
        {
            drEquipment = dr;


            Properties.Settings.Default["CedexConnectionString"] = m_ConnectionInfo;
            cedexhires = new CedexHiResDataSet();
            queriesTableAdapter = new CedexHiResDataSetTableAdapters.QueriesTableAdapter();
            reportValueTableAdapter = new CedexHiResDataSetTableAdapters.ReportValueTableAdapter();
            imageDataTableAdapter = new CedexHiResDataSetTableAdapters.ImageDataTableAdapter();
            irOperatorInputValueTableAdapter = new CedexHiResDataSetTableAdapters.IROperatorInputValueTableAdapter();
            measurementResultDataTableAdapter = new CedexHiResDataSetTableAdapters.MeasurementResultDataTableAdapter();
            imageStatusTableAdapter = new CedexHiResDataSetTableAdapters.ImageStatusTableAdapter();
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
            if (LastProcessTime < new DateTime(2021, 1, 1, 0, 0, 0, 0))
                LastProcessTime = new DateTime(2021, 1, 1, 0, 0, 0, 0);
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
            string strEqpType = drEquipment.ItemArray[2].ToString();
            string strEqpID = drEquipment.ItemArray[5].ToString();
            
            try
            {
                StringBuilder sbData = new StringBuilder();
                string strDate = "";

                if (strEqpType.Trim() == "17")
                {
                    DateTime? NewProcessTime =(DateTime ?)queriesTableAdapter.GetNewProcessTime(m_dtLastProcessTime);

                    if (NewProcessTime.HasValue)
                    {
                        CedexHiResDataSet.ReportValueDataTable dt = reportValueTableAdapter.GetData(NewProcessTime);

                        DataRow dr = dt.Rows[0];
                        strDate = DateTime.Parse(dr["ProcessTime"].ToString()).ToString("yyyyMMddHHmmss.fff");

                        CedexHiResDataSet.MeasurementResultDataDataTable mr_dt = measurementResultDataTableAdapter.GetData(NewProcessTime);
                        foreach (DataRow mr_dr in mr_dt.Rows)
                        {
                            sbData.Append($"{mr_dr["OverallType"].ToString()},{strDate},{mr_dr["MeasurementResultValue"].ToString()}" + Environment.NewLine);
                        }
                        //                            sbData.Append(dr["OverallType"].ToString() + "," + strDate + "," + dr["MeasurementResultValue"].ToString() + Environment.NewLine);
                        sbData.Append("CedexSystemId," + strDate + "," + dr["CedexSystemID"].ToString() + Environment.NewLine);
                        sbData.Append("CedexSystemNm," + strDate + "," + dr["CedexSystemNm"].ToString() + Environment.NewLine);
                        sbData.Append("SoftwareVersion," + strDate + "," + m_SoftwareVersion + Environment.NewLine);
                        sbData.Append("ReactorId," + strDate + "," + dr["ReactorIdentifier"].ToString() + Environment.NewLine);
                        sbData.Append("SampleId," + strDate + "," + dr["SampleIdentifier"].ToString() + Environment.NewLine);
                        string JpeqQuality = (string)queriesTableAdapter.GetOptionValue("JpegQuality");
                        sbData.Append("JpegQuality," + strDate + "," + JpeqQuality + Environment.NewLine);
                        sbData.Append("Algorithm," + strDate + "," + dr["Algorithm"].ToString() + Environment.NewLine);
                        sbData.Append("MeasurementPrecision," + strDate + "," + dr["MeasurementPrecision"].ToString() + Environment.NewLine);
                        sbData.Append("MeasurementUser," + strDate + "," + dr["MeasurementUser"].ToString() + Environment.NewLine);
                        sbData.Append("Workarea," + strDate + "," + dr["Workarea"].ToString() + Environment.NewLine);
                        sbData.Append("Correction," + strDate + "," + dr["Correction"].ToString() + Environment.NewLine);
                        sbData.Append("CellType," + strDate + "," + dr["CellTypeName"].ToString() + Environment.NewLine);
                        sbData.Append("Dilution," + strDate + "," + dr["Dilution"].ToString() + Environment.NewLine);
                        sbData.Append("DatasetName," + strDate + "," + dr["DatasetName"].ToString() + Environment.NewLine);
                        sbData.Append("ProcessTime," + strDate + "," + strDate + Environment.NewLine);
                        sbData.Append("ReportCounter," + strDate + "," + dr["ReportCounter"].ToString() + Environment.NewLine);

                        string imgConf = string.Empty;
                        CedexHiResDataSet.ImageDataDataTable id_dt = imageDataTableAdapter.GetData(NewProcessTime);
                        if (id_dt.Count == 0)
                        {
                            sbData.Append("ImageXm," + strDate + ", n/a" + Environment.NewLine);
                            sbData.Append("ImageYm," + strDate + ", n/a" + Environment.NewLine);
                            sbData.Append("ImageCh," + strDate + "," + dr["CH"].ToString() + Environment.NewLine);
                            sbData.Append("ImageFf," + strDate + "," + dr["Ff"].ToString() + Environment.NewLine);
                            sbData.Append("ImageCx," + strDate + ", n/a" + Environment.NewLine);
                            sbData.Append("ImageCy," + strDate + ", n/a" + Environment.NewLine);
                            imgConf  = "Xm = n/a, ";
                            imgConf += "Ym = n/a, ";
                            imgConf += "Ch = " + dr["CH"].ToString() + ", ";
                            imgConf += "Ff = " + dr["Ff"].ToString() + ", ";
                            imgConf += "Cx = n/a, ";
                            imgConf += "Cy = n/a ";
                        }
                        else
                        {
                            DataRow id_dr = id_dt.Rows[0];
                            sbData.Append("ImageXm," + strDate + "," + id_dr["XM"].ToString() + Environment.NewLine);
                            sbData.Append("ImageYm," + strDate + "," + id_dr["YM"].ToString() + Environment.NewLine);
                            sbData.Append("ImageCh," + strDate + "," + dr["CH"].ToString() + Environment.NewLine);
                            sbData.Append("ImageFf," + strDate + "," + dr["Ff"].ToString() + Environment.NewLine);
                            sbData.Append("ImageCx," + strDate + "," + id_dr["CX"].ToString() + Environment.NewLine);
                            sbData.Append("ImageCy," + strDate + "," + id_dr["CY"].ToString() + Environment.NewLine);
                            imgConf = "Xm = " + id_dr["XM"].ToString() + ", ";
                            imgConf += "Ym = " + id_dr["YM"].ToString() + ", ";
                            imgConf += "Ch = " + dr["CH"].ToString() + ", ";
                            imgConf += "Ff = " + dr["Ff"].ToString() + ", ";
                            imgConf += "Cx = " + id_dr["CX"].ToString() + ", ";
                            imgConf += "Cy = " + id_dr["CY"].ToString();

                        }
                        sbData.Append("ImageConf," + strDate + "," + imgConf + Environment.NewLine);
                        sbData.Append("Comment," + strDate + "," + dr["Comment"].ToString() + Environment.NewLine);

                        if (dr["Valid"].ToString().Trim() == "True")
                            sbData.Append("Valid," + strDate + "," + "Valid" + Environment.NewLine);
                        else
                            sbData.Append("Valid," + strDate + "," + "Invalid" + Environment.NewLine);
                        int? images=queriesTableAdapter.GetImages(NewProcessTime);

                        sbData.Append($"Images,{strDate},{images??0}" + Environment.NewLine);

                        CedexHiResDataSet.IROperatorInputValueDataTable ir_dt = irOperatorInputValueTableAdapter.GetData(NewProcessTime);
                        string strPara = string.Empty;
                        foreach (DataRow ir_dr in ir_dt.Rows)
                        {
                            sbData.Append($"{ir_dr["IROperatorInputName"].ToString()},{strDate},{ir_dr["IROperatorInputValue"].ToString()}" + Environment.NewLine);
                            strPara += ir_dr["IROperatorInputValue"].ToString() + ", ";
                        }
                        strPara = strPara.Substring(0, strPara.Length - 1);
                        sbData.Append("Parameter," + strDate + "," + strPara + Environment.NewLine);
                        sbData.Append("SvrTime," + strDate + "," + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + Environment.NewLine);

                        CedexHiResDataSet.ImageStatusDataTable is_dt = imageStatusTableAdapter.GetData(NewProcessTime);
                        foreach (DataRow is_dr in is_dt.Rows)
                        {
                            if (is_dr["ImageStatus"].ToString().Trim() == "1")
                                sbData.Append("Valid" + is_dr["Imageno"].ToString().Trim() + "," + strDate + "," + "Valid" + Environment.NewLine);
                            else
                                sbData.Append("Valid" + is_dr["Imageno"].ToString().Trim() + "," + strDate + "," + "Invalid" + Environment.NewLine);
                        }
                        m_dtLastProcessTime = NewProcessTime ?? m_dtLastProcessTime;
                        SetLastProcessTime(m_dtLastProcessTime);
                    }
                }
                return sbData.ToString().Trim();

            }
            catch (Exception ex)
            {
                UpdateEquipmentProgDateTime(IF_STATUS.InternalError);
                listViewMsg.UpdateMsg($"Exception in ResultProcess - ({ex})", false, true, true, PC00D01.MSGTERR);
                return string.Empty;
            }
        }
    }
}
