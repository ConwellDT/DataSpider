using DataSpider.PC00.PT;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Transactions;

namespace DataSpider.PC01.PT
{
    /// <summary>
    /// File I/F
    /// </summary>
    public class PC01S04 : PC00B01
    {
        protected DateTime dtLastWriteTime = DateTime.MinValue;

        public PC01S04() : base()
        {
        }

        public PC01S04(PC01F01 pOwner, DataRow dr, string equipType, string equipName, string connectionInfo, string extraInfo, int nCurNo, bool bAutoRun = false) : base(pOwner, dr, equipType, equipName, connectionInfo, extraInfo, nCurNo, bAutoRun)
        {
        }

        public PC01S04(PC01F01 pOwner, DataRow dr, int nCurNo, bool bAutoRun = false) : this(pOwner, dr, dr["EQUIP_TYPE_NM"].ToString(), dr["EQUIP_NM"].ToString(), dr["CONNECTION_INFO"].ToString(), dr["EXTRA_INFO"].ToString(), nCurNo, bAutoRun)
        {
            if (bAutoRun == true)
            {
                m_Thd = new Thread(ThreadJob);
                m_Thd.Start();
            }
        }

        private void ThreadJob()
        {
            // 접속을 못했을 때 normal로 표시되었다가 끊어지는 것으로 표시됨. 
            // 오해의 소지가 있어서 삭제.
            //            listViewMsg.UpdateStatus(true);
            listViewMsg.UpdateMsg("Thread started");

            while (!bTerminal)
            {
                try
                {
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
                    //UpdateEquipmentProgDateTime(IF_STATUS.Normal);
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
            string strSql = "";
            SqlConnection SqlCon = null;

            string strCon = m_ConnectionInfo;
            string strEqpType = drEquipment["EQUIP_TYPE"]?.ToString();//.ItemArray[2].ToString();
            // 20240611, SHS, CEDEX_BIO PC01S04 MA_EQUIPMENT_CD 테이블 EXTRA_INFO 에 P3S-MEAN-1110 처럼 장비ID 만 설정 -> JSON 포맷으로 EquipmentID 프로퍼티로 설정해야 함. { "EquipmentID": "P3S-MEAN-1110" }
            //string strEqpID = drEquipment.Table.Rows[0]["EXTRA_INFO"].ToString(); //drEquipment.ItemArray[6].ToString();
            string strEqpID = PC00U01.GetJsonProperty(drEquipment["EXTRA_INFO"]?.ToString(), "EquipmentID");

            try
            {
                SqlCon = new SqlConnection(strCon);
                SqlCon.Open();


                StringBuilder sbData = new StringBuilder();
                string strDate = "";

                if (strEqpType.Trim() == "00")  //NO USE
                {
                    //strSql = "SELECT TOP 1 Seq, SampleDrawTime, MeasurementResultValue, OverallType, OverallName, ";
                    //strSql += " CedexSystemID, CedexSystemNm, ReactorIdentifier, SampleIdentifier, SaveID, Workarea, Dilution, DatasetName, ProcessTime, Valid ";  //information 관련
                    strSql = " SELECT TOP 1 * FROM S2HIRES ";
                    strSql += " WHERE CedexSystemID = '" + strEqpID + "' AND (TRIM(IF_FLAG) IS NULL OR TRIM(IF_FLAG) = 'N' ) ";
                    strSql += " ORDER BY SaveTime,ReactorIdentifier,SampleIdentifier,CedexSystemID, cast(OverallType as int)";

                    SqlCommand Cmd = new SqlCommand(strSql, SqlCon);
                    SqlDataReader reader = Cmd.ExecuteReader();

                    string arrSeqNo = "";
                    string strMID = "";
                    string procData = "";


                    while (reader.Read())
                    {
                        strDate = DateTime.Parse(reader["ProcessTime"].ToString()).ToString("yyyyMMddHHmmss.fff");

                        procData = reader["ProcessTime"].ToString(); 
                        if (reader["MeasurementResultValue"].ToString().Trim() != "")
                        {
                            sbData.Append(reader["OverallType"].ToString() + "," + strDate + "," + reader["MeasurementResultValue"].ToString() + Environment.NewLine);
                            sbData.Append("CedexSystemId," + strDate + "," + reader["CedexSystemID"].ToString() + Environment.NewLine);
                            sbData.Append("CedexSystemNm," + strDate + "," + reader["CedexSystemNm"].ToString() + Environment.NewLine);
                            sbData.Append("SoftwareVersion," + strDate + "," + reader["SoftwareVersion"].ToString() + Environment.NewLine);
                            sbData.Append("ReactorId," + strDate + "," + reader["ReactorIdentifier"].ToString() + Environment.NewLine);
                            sbData.Append("SampleId," + strDate + "," + reader["SampleIdentifier"].ToString() + Environment.NewLine);
                            sbData.Append("JpegQuality," + strDate + "," + reader["JpegQuality"].ToString() + Environment.NewLine);
                            sbData.Append("Algorithm," + strDate + "," + reader["Algorithm"].ToString() + Environment.NewLine);
                            sbData.Append("MeasurementPrecision," + strDate + "," + reader["MeasurementPrecision"].ToString() + Environment.NewLine);
                            sbData.Append("MeasurementUser," + strDate + "," + reader["MeasurementUser"].ToString() + Environment.NewLine);
                            sbData.Append("Workarea," + strDate + "," + reader["Workarea"].ToString() + Environment.NewLine);
                            sbData.Append("Correction," + strDate + "," + reader["Correction"].ToString() + Environment.NewLine);
                            sbData.Append("CellType," + strDate + "," + reader["CellTypeName"].ToString() + Environment.NewLine);
                            sbData.Append("Dilution," + strDate + "," + reader["Dilution"].ToString() + Environment.NewLine);
                            sbData.Append("DatasetName," + strDate + "," + reader["DatasetName"].ToString() + Environment.NewLine);
                            sbData.Append("ProcessTime," + strDate + "," + strDate + Environment.NewLine);
                            sbData.Append("ReportCounter," + strDate + "," + reader["ReportCounter"].ToString() + Environment.NewLine);

                            sbData.Append("ImageXm," + strDate + "," + reader["XM"].ToString() + Environment.NewLine);
                            sbData.Append("ImageYm," + strDate + "," + reader["YM"].ToString() + Environment.NewLine);
                            sbData.Append("ImageCh," + strDate + "," + reader["CH"].ToString() + Environment.NewLine);
                            sbData.Append("ImageFf," + strDate + "," + reader["Ff"].ToString() + Environment.NewLine);
                            sbData.Append("ImageCx," + strDate + "," + reader["CX"].ToString() + Environment.NewLine);
                            sbData.Append("ImageCy," + strDate + "," + reader["CY"].ToString() + Environment.NewLine);

                            string imgConf = "Xm = " + reader["XM"].ToString() + ", ";
                            imgConf += "Ym = " + reader["YM"].ToString() + ", ";
                            imgConf += "Ch = " + reader["CH"].ToString() + ", ";
                            imgConf += "Ff = " + reader["Ff"].ToString() + ", ";
                            imgConf += "Cx = " + reader["CX"].ToString() + ", ";
                            imgConf += "Cy = " + reader["CY"].ToString();

                            sbData.Append("ImageConf," + strDate + "," + imgConf + Environment.NewLine);
                            sbData.Append("Comment," + strDate + "," + reader["Comment"].ToString() + Environment.NewLine);

                            if (reader["Valid"].ToString().Trim() == "True")
                                sbData.Append("Valid," + strDate + "," + "Valid" + Environment.NewLine);
                            else
                                sbData.Append("Valid," + strDate + "," + "Invalid" + Environment.NewLine);

                            sbData.Append("Images," + strDate + "," + reader["MeasurementImages"].ToString() + Environment.NewLine);

                            sbData.Append("BorderWidth," + strDate + "," + reader["BorderWidth"].ToString() + Environment.NewLine);
                            sbData.Append("DCAppearance," + strDate + "," + reader["DCAppearance"].ToString() + Environment.NewLine);
                            sbData.Append("LCAppearance," + strDate + "," + reader["LCAppearance"].ToString() + Environment.NewLine);
                            sbData.Append("AggrMinSize," + strDate + "," + reader["AggrMinSize"].ToString() + Environment.NewLine);
                            sbData.Append("LCAggrApp," + strDate + "," + reader["LCAggrApp"].ToString() + Environment.NewLine);
                            sbData.Append("CMinSize," + strDate + "," + reader["CMinSize"].ToString() + Environment.NewLine);
                            sbData.Append("CMaxSize," + strDate + "," + reader["CMaxSize"].ToString() + Environment.NewLine);
                            sbData.Append("AggrMaxSize," + strDate + "," + reader["AggrMaxSize"].ToString() + Environment.NewLine);
                            sbData.Append("ClassificationBias," + strDate + "," + reader["ClassificationBias"].ToString() + Environment.NewLine);
                            sbData.Append("MinPollutionArea," + strDate + "," + reader["MinPollutionArea"].ToString() + Environment.NewLine);
                            sbData.Append("DCAggrApp," + strDate + "," + reader["DCAggrApp"].ToString() + Environment.NewLine);

                            string strPara = reader["AggrMaxSize"].ToString() + ", " + reader["AggrMinSize"].ToString() + ", ";
                            strPara += reader["BorderWidth"].ToString() + ", " + reader["ClassificationBias"].ToString() + ", ";
                            strPara += reader["CMaxSize"].ToString() + ", " + reader["CMinSize"].ToString() + ", ";
                            strPara += reader["DCAggrApp"].ToString() + ", " + reader["DCAppearance"].ToString() + ", ";
                            strPara += reader["LCAggrApp"].ToString() + ", " + reader["LCAppearance"].ToString() + ", ";
                            strPara += reader["MinPollutionArea"].ToString();

                            sbData.Append("Parameter," + strDate + "," + strPara + Environment.NewLine);


                            sbData.Append("SvrTime," + strDate + "," + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + Environment.NewLine);
                        }

                        arrSeqNo = reader["SEQ"].ToString();
                        strMID = reader["MeasurementID"].ToString();
                    }

                    reader.Close();

                    if (arrSeqNo.Trim() != "")
                    {

                        SqlTransaction trans = SqlCon.BeginTransaction();

                        string upSql = "";
                        //Update 처리                        
                        upSql = " UPDATE S2HIRES SET IF_FLAG = 'Y', IF_DATE = GETDATE() WHERE SEQ = '" + arrSeqNo.Trim() + "' ";

                        SqlCommand Cmd1 = new SqlCommand(upSql, SqlCon);

                        Cmd1.Transaction = trans;

                        try
                        {
                            Cmd1.ExecuteNonQuery();

                            trans.Commit();
                        }
                        catch(Exception ex)
                        {
                            trans.Rollback();
                        }
                    }


                    //IMAGE DATA
                    string strSql2 = " SELECT TOP 1 * FROM S2HIRES_IMAGE ";
                    //strSql2 += " WHERE MeasurementID = '" + strMID + "' AND PROCESSTIME = '"+ procData + "' AND (TRIM(IF_FLAG) IS NULL OR TRIM(IF_FLAG) = 'N') ";
                    strSql2 += " WHERE CedexSystemID = '" + strEqpID + "' AND (TRIM(IF_FLAG) IS NULL OR TRIM(IF_FLAG) = 'N' ) ";
                    strSql2 += " ORDER BY CONVERT(INT, Imageno) ASC ";

                    SqlCommand Cmd2 = new SqlCommand(strSql2, SqlCon);
                    SqlDataReader reader1 = Cmd2.ExecuteReader();

                    //int nMaxImgNo = 0;
                    string imgid = "";
                    string strMID2 = "";

                    while (reader1.Read())
                    {
                        //strDate = DateTime.Parse(reader1["ProcessTime"].ToString()).ToString("yyyyMMddHHmmss.fff");
                        strDate = Convert.ToDateTime(reader1["ProcessTime"]).ToString("yyyyMMddHHmmss.fff");
                        imgid = reader1["ImageID"].ToString();
                        strMID2 = reader1["MeasurementID"].ToString();

                        if (reader1["ImageStatus"].ToString().Trim() == "1")
                            sbData.Append("Valid" + reader1["Imageno"].ToString().Trim() + "," + strDate + "," + "Valid" + Environment.NewLine);
                        else
                            sbData.Append("Valid" + reader1["Imageno"].ToString().Trim() + "," + strDate + "," + "Invalid" + Environment.NewLine);

                        //nMaxImgNo = Convert.ToInt16(reader1["Imageno"].ToString());
                    }

                    //if (nMaxImgNo > 0)
                    //{
                    //    if (sbData.ToString().Trim() != "" && nMaxImgNo < 30)
                    //    {
                    //        for (int i = nMaxImgNo + 1; i < 31; i++)
                    //        {
                    //            sbData.Append("Valid" + i.ToString() + "," + strDate + "," + " " + Environment.NewLine);
                    //        }
                    //    }
                    //}

                    reader1.Close();


                    SqlTransaction trans1 = SqlCon.BeginTransaction();

                    string upSql1 = "";
                    //Update 처리                        
                    //upSql1 = " UPDATE S2HIRES_IMAGE SET IF_FLAG = 'Y', IF_DATE = GETDATE() WHERE MeasurementID = '" + strMID + "' ";
                    upSql1 = " UPDATE S2HIRES_IMAGE SET IF_FLAG = 'Y', IF_DATE = GETDATE() WHERE MeasurementID = '" + strMID2 + "' and ImageID = '" + imgid + "' ";

                    SqlCommand Cmd3 = new SqlCommand(upSql1, SqlCon);
                    
                    Cmd3.Transaction = trans1;

                    try
                    {
                        Cmd3.ExecuteNonQuery();

                        trans1.Commit();
                    }
                    catch (Exception ex)
                    {
                        trans1.Rollback();
                    }
                }       // cedex hires m2i 통해 처리하던 것. cedex hires 는 직접 처리하는 로직 s17 으로 대체되어 사용되지 않음
                else if (strEqpType.Trim() == "11")     // cedex bip 처리 (cedex bio 장비의 장비타입번호가 11 이어야 함)
                {
                    strSql = " SELECT TOP 1 * FROM ROCHE ";
                    strSql += " WHERE MTRL_CD = '" + strEqpID + "' AND (TRIM(IF_FLAG) IS NULL OR TRIM(IF_FLAG) = 'N')  ";
                    strSql += " ORDER BY RST_INP_DT, RST_INP_TM, RCV_OUT_SPEC ";

                    SqlCommand Cmd = new SqlCommand(strSql, SqlCon);
                    SqlDataReader reader = Cmd.ExecuteReader();
                    string strRcpCd = "";
                    string strEqpCd = "";
                    string strMtrlCd = "";
                    string strLvlCd = "";
                    string strItemCd = "";
                    string strItemNm = "";
                    string strReqSeq = "";
                    string strQcbarNo = "";

                    while (reader.Read())
                    {
                        strRcpCd = reader["RCP_CD"].ToString();
                        strEqpCd = reader["EQP_CD"].ToString();
                        strMtrlCd = reader["MTRL_CD"].ToString();
                        strLvlCd = reader["LVL_CD"].ToString();
                        strItemCd = reader["ITEM_CD"].ToString();
                        strItemNm = reader["ITEMNM"].ToString();
                        strReqSeq = reader["REQ_SEQ"].ToString();
                        strQcbarNo = reader["QC_BAR_NO"].ToString();
                        strDate = reader["RST_INP_DT"].ToString() + reader["RST_INP_TM"].ToString();

                        if (reader["QC_RST"].ToString().Trim() != "")
                        {
                            sbData.Append(strItemNm + "_VALUE," + strDate + "," + reader["QC_RST"].ToString() + Environment.NewLine);
                            sbData.Append(strItemNm + "_VALID," + strDate + "," + reader["FLAG"].ToString() + Environment.NewLine);
                            sbData.Append(strItemNm + "_UNIT," + strDate + "," + reader["RST_UNIT"].ToString() + Environment.NewLine);
                            sbData.Append("EQUIP" + "," + strDate + "," + strMtrlCd + Environment.NewLine);
                            sbData.Append("USER" + "," + strDate + "," + reader["REQ_ID"].ToString() + "/" + reader["RST_INP_ID"].ToString() + Environment.NewLine);
                            sbData.Append("VESSEL" + "," + strDate + "," + reader["VESSELNO"].ToString() + Environment.NewLine);
                            sbData.Append("BATCH" + "," + strDate + "," + reader["BATCHNO"].ToString() + Environment.NewLine);
                            sbData.Append("ORDER" + "," + strDate + "," + reader["REQ_DT"].ToString() + reader["REQ_TM"].ToString() + Environment.NewLine);
                            sbData.Append("SAMPLE_NO" + "," + strDate + "," + reader["QC_BAR_NO"].ToString() + Environment.NewLine);
                            sbData.Append("DESCRIPTION" + "," + strDate + "," + reader["DESCRIPTION"].ToString() + Environment.NewLine);  

                            sbData.Append("SvrTime," + strDate + "," + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + Environment.NewLine);
                        }
                    }

                    reader.Close();

                    if (strItemNm.Trim() != "")
                    {
                        DateTime CurDt = System.DateTime.Now;

                        SqlTransaction trans1 = SqlCon.BeginTransaction();


                        string upSql = "";
                        //Update 처리
                        upSql = " UPDATE ROCHE SET IF_FLAG = 'Y', IF_DATE = GETDATE() ";
                        upSql += " WHERE RCP_CD = '" + strRcpCd + "' ";
                        upSql += " AND EQP_CD = '" + strEqpCd + "' ";
                        upSql += " AND MTRL_CD = '" + strMtrlCd + "' ";
                        upSql += " AND LVL_CD = '" + strLvlCd + "' ";
                        upSql += " AND ITEM_CD = '" + strItemCd + "' ";
                        upSql += " AND REQ_SEQ = '" + strReqSeq + "' ";
                        upSql += " AND QC_BAR_NO = '" + strQcbarNo + "' ";

                        SqlCommand Cmd1 = new SqlCommand(upSql, SqlCon);
                        
                        Cmd1.Transaction = trans1;

                        try
                        {
                            Cmd1.ExecuteNonQuery();

                            trans1.Commit();
                        }
                        catch (Exception ex)
                        {
                            trans1.Rollback();
                        }
                    }
                }

                SqlCon.Close();

                if (sbData.ToString().Trim() == "")
                {
                    //UpdateEquipmentProgDateTime(IF_STATUS.Normal);
                    return string.Empty;
                }
                else
                    return sbData.ToString();
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
