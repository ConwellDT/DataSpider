using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using CFW.Data;
using System.Collections;

namespace DataSpider.PC00.PT
{
    public class PC00Z01
    {
        #region GetProgramInfo 프로그램 정보 취득
        public DataSet GetProgramInfo(string p_strPlantCd, string p_strPgmId, MsSqlDbAccess p_clsDbTrans, ref string p_strErrCode, ref string p_strErrTxt)
        {

            DataSet ds = new DataSet();
            DataCollection dc = new DataCollection();

            string strErrCode = string.Empty;
            string strErrText = string.Empty;

            StringBuilder strQuery = new StringBuilder();
            strQuery.Append(" SELECT PLANT_CD, PGM_ID, PGM_NAME, PGM_TYPE, PGM_PARA ");
            strQuery.Append(" FROM SS_PGM ");
            strQuery.Append(" WHERE PLANT_CD = '" + p_strPlantCd + "' ");
            strQuery.Append(" AND PGM_ID = '" + p_strPgmId + "' ");

            ds = CFW.Data.MsSqlDbAccess.GetDataSet(strQuery.ToString(), null, CommandType.Text, ref strErrCode, ref strErrText);

            return ds;
        }
        #endregion

        #region GetDeviceInfo 프로그램 장비 정보 취득
        public DataSet GetDeviceInfo(string p_strEquipType, string p_strPgmType, MsSqlDbAccess p_clsDbTrans, ref string p_strErrCode, ref string p_strErrTxt)
        {
            
            DataSet ds = new DataSet();
            DataCollection dc = new DataCollection();

            string strErrCode = string.Empty;
            string strErrText = string.Empty;

            string strQuery = @"
                SELECT 
                	A.PLANT_CD, 
                	A.PGM_ID, 
                	A.PGM_NAME, 
                	B.INIT_RUN_FLAG, 
                	C.PROC_ID, 
                	C.PROC_NAME, 
                	C.PROC_PARA, 
                	D.DEVICE_ID,
                	D.DEVICE_NM, 
                	D.DEVICE_IP, 
                	D.DEVICE_PORT, 
                	D.OPC_IP, 
                	D.OPC_PORT, 
                	C.OUT_DEVICE_ID, 
                	E.station_cd,
                	F.VisionUse,
                	F.Vision_FileFormat
                FROM 
                	SS_PGM A
                	INNER JOIN SS_PROC_MAPPING B 
                		ON A.PGM_ID = B.PGM_ID
                	INNER JOIN SS_PROC C 
                		ON B.PROC_ID = C.PROC_ID
                	LEFT OUTER JOIN MA_DEVICE_CD D 
                		ON C.IN_DEVICE_ID = D.DEVICE_ID
                	LEFT OUTER JOIN MA_STATION_DEVICE_MAPPING E 
                		ON D.DEVICE_ID = E.DEVICE_ID
                	LEFT OUTER JOIN MA_STATION_CD F
                		ON F.DEVICE_ID = E.DEVICE_ID
                WHERE 
                	A.PLANT_CD = :PLANT_CD 
                	AND A.PGM_ID = :PGM_ID
                	AND A.PGM_TYPE = :PGM_TYPE
            ";
            strQuery = strQuery
                .Replace(":PLANT_CD", string.Format("'{0}'", p_strEquipType));

            ds = CFW.Data.MsSqlDbAccess.GetDataSet(strQuery.ToString(), null, CommandType.Text, ref strErrCode, ref strErrText);

            return ds;
        }
        #endregion

        //public bool InsertResult(string _strTagName, string _strDateTime, string _strValue, ref string _strErrCode, ref string _strErrText)
        //{
        //    try
        //    {
        //        StringBuilder strQuery = new StringBuilder();
        //        strQuery.Append($"EXEC InsertResult '{_strTagName}', '{_strDateTime}', '{_strValue}'");

        //        bool result = CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        _strErrText = ex.ToString();
        //        return false;
        //    }
        //}

        public string ReadSTCommon(string cdGrp, string code)
        {
            string result = string.Empty;
            string _strErrCode = string.Empty, _strErrText = string.Empty;
            try
            {
                StringBuilder strQuery = new StringBuilder();
                strQuery.Append($"EXEC GetSTCommon '{cdGrp}', '{code}'");

                DataSet ds = CFW.Data.MsSqlDbAccess.GetDataSet(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
                if (ds != null && ds.Tables[0] != null)
                {
                    result = ds.Tables[0].Rows?[0][0].ToString();
                }
            }
            catch (Exception ex)
            {
                _strErrText = ex.ToString();
            }
            return result;
        }

        public bool WriteSTCommon(string cdGrp, string code, string value)
        {
            string _strErrCode = string.Empty, _strErrText = string.Empty;
            try
            {
                StringBuilder strQuery = new StringBuilder();
                strQuery.Append($"EXEC MergeSTCommon '{cdGrp}', '{code}', '{value}'");

                bool result = CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
                return result;
            }
            catch (Exception ex)
            {
                _strErrText = ex.ToString();
            }
            return false;
        }

        public bool InsertResult(string _strTagName, string _strDateTime, string _strValue, string _piIFFlag, string _piIFDateTime, string remark, ref string _strErrCode, ref string _strErrText)
        {
            try
            {
                StringBuilder strQuery = new StringBuilder();
                strQuery.Append($"EXEC InsertResult '{_strTagName}', '{_strDateTime}', '{_strValue}', '{_piIFFlag}', '{_piIFDateTime}', '{remark}'");

                bool result = CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
                return result;
            }
            catch (Exception ex)
            {
                _strErrText = ex.ToString();
                return false;
            }
        }

        public DataTable GetEquipmentInfo(string equipType, string serverName, bool onlyUseflag, ref string _strErrCode, ref string _strErrText)
        {
            DataTable result = null;
            try
            {
                string useFlag = onlyUseflag ? "Y" : string.Empty;
                StringBuilder strQuery = new StringBuilder();
                strQuery.Append($"EXEC GetEquipmentInfo '{equipType}', '{serverName}', '{useFlag}'");

                DataSet ds = CFW.Data.MsSqlDbAccess.GetDataSet(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
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
        public DataTable GetEquipmentInfoForDSC(string equipName, bool onlyUseflag, ref string _strErrCode, ref string _strErrText)
        {
            DataTable result = null;
            try
            {
                string useFlag = onlyUseflag ? "Y" : string.Empty;
                StringBuilder strQuery = new StringBuilder();
                strQuery.Append($"EXEC GetEquipmentInfoForDSC '{equipName}', '{useFlag}'");

                DataSet ds = CFW.Data.MsSqlDbAccess.GetDataSet(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
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
        public DataTable GetRequest(string equipName, string serverCode, ref string _strErrCode, ref string _strErrText)
        {
            DataTable result = null;
            try
            {
                StringBuilder strQuery = new StringBuilder();
                strQuery.Append($"EXEC GetRequest '{equipName}'");

                DataSet ds = CFW.Data.MsSqlDbAccess.GetDataSet(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);

                if (ds != null && ds.Tables[0] != null)
                {
                    result = ds.Tables[0];
                }
                if (result.Rows.Count > 0)
                {
                    if (result.Rows[0][$"STOP_REQ{serverCode}"].Equals(1) )
                    {
                        strQuery.Clear();
                        // SET STOP_REQ{ServerCode} = 0
                        strQuery.Append($"EXEC ResetRequest '{equipName}', '{serverCode}'");
                        if (!MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText))
                        {
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _strErrText = ex.ToString();
            }
            return result;
        }
        public DataTable GetEquipmentInfoForCombo(ref string _strErrCode, ref string _strErrText)
        {
            DataTable result = null;
            try
            {
                StringBuilder strQuery = new StringBuilder();
                strQuery.Append($"EXEC GetEquipmentInfoForLogCombo");

                DataSet ds = CFW.Data.MsSqlDbAccess.GetDataSet(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
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

        public DataTable GetTagInfo(string equipType, string serverName, string tagName, bool onlyUseflag, ref string _strErrCode, ref string _strErrText)
        {
            DataTable result = null;
            try
            {
                string useFlag = onlyUseflag ? "Y" : string.Empty;
                StringBuilder strQuery = new StringBuilder();
                strQuery.Append($"EXEC GetTagInfo '{equipType}', '{serverName}', '{tagName}', '{useFlag}'");

                DataSet ds = CFW.Data.MsSqlDbAccess.GetDataSet(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
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
        public DataTable GetTagInfoForDSC(string equipName, string tagName, bool onlyUseflag, ref string _strErrCode, ref string _strErrText)
        {
            DataTable result = null;
            try
            {
                string useFlag = onlyUseflag ? "Y" : string.Empty;
                StringBuilder strQuery = new StringBuilder();
                strQuery.Append($"EXEC GetTagInfoForDSC '{equipName}', '{tagName}', '{useFlag}'");

                DataSet ds = CFW.Data.MsSqlDbAccess.GetDataSet(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
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
        public DataTable GetTagInfoByTag(string tagName, ref string _strErrCode, ref string _strErrText)
        {
            DataTable result = null;
            try
            {
                StringBuilder strQuery = new StringBuilder();
                strQuery.Append($"EXEC GetTagInfoByTag '{tagName}'");

                DataSet ds = CFW.Data.MsSqlDbAccess.GetDataSet(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
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

        //
        // 2022. 2. 16 : Han, Ilho
        //      : Add procedure interface for GetTagInfoByEquip
        //
        public DataTable GetTagInfoByEquip(string equipName, ref string _strErrCode, ref string _strErrText)
        {
            DataTable result = null;
            try
            {
                StringBuilder strQuery = new StringBuilder();
                strQuery.Append($"EXEC GetTagInfoByEquip '{equipName}'");

                DataSet ds = CFW.Data.MsSqlDbAccess.GetDataSet(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
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
        // --------------------------------------

        public DataTable GetCommonCode(string codeGroup, ref string _strErrCode, ref string _strErrText)
        {
            DataTable result = null;
            try
            {
                StringBuilder strQuery = new StringBuilder();
                strQuery.Append($"EXEC GetCommonCode '{codeGroup}', ''");

                DataSet ds = CFW.Data.MsSqlDbAccess.GetDataSet(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
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
        public DataTable GetCommonCode(string codeGroup, string code, ref string _strErrCode, ref string _strErrText)
        {
            DataTable result = null;
            try
            {
                StringBuilder strQuery = new StringBuilder();
                strQuery.Append($"EXEC GetCommonCode '{codeGroup}', '{code}'");

                DataSet ds = CFW.Data.MsSqlDbAccess.GetDataSet(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
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
        public DataTable GetMeasureResult(string strEquipType, ref string _strErrCode, ref string _strErrText)
        {
            DataTable result = null;
            try
            {
                StringBuilder strQuery = new StringBuilder();

                strQuery.Append($"EXEC GetMeasureResult '{strEquipType}'");
                
                DataSet ds = CFW.Data.MsSqlDbAccess.GetDataSet(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
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
        public DataTable GetMeasureResultForPIConnection(ref string _strErrCode, ref string _strErrText)
        {
            DataTable result = null;
            try
            {
                StringBuilder strQuery = new StringBuilder();

                strQuery.Append($"EXEC GetMeasureResultForPIConnection");

                DataSet ds = CFW.Data.MsSqlDbAccess.GetDataSet(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
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

        public DataTable GetMeasureResult(ref string _strErrCode, ref string _strErrText)
        {
            DataTable result = null;
            try
            {
                StringBuilder strQuery = new StringBuilder();
                strQuery.Append($"EXEC GetMeasureResult");

                DataSet ds = CFW.Data.MsSqlDbAccess.GetDataSet(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
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

        public bool UpdateMeasureResult(int hiSeq, string strFlag, int if_count, string errMsg, ref string _strErrCode, ref string _strErrText)
        {
            try
            {
                StringBuilder strQuery = new StringBuilder();
                strQuery.Append($"EXEC UpdateMeasureResult {hiSeq},'{strFlag}', {if_count}, '{errMsg}' ");

                bool result = CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
                return result;
            }
            catch (Exception ex)
            {
                _strErrText = ex.ToString();
                return false;
            }
        }

        /// <summary>
        /// 장비별 상태를 확인하기 위해 MA_EQUIPMENT_CD.ProgDateTime 에 현재시간을 업데이트
        /// </summary>
        /// <param name="equipName"></param>
        /// <param name="_strErrCode"></param>
        /// <param name="_strErrText"></param>
        /// <returns></returns>
        public bool UpdateEquipmentProgDateTime(string equipName, int status, ref string _strErrCode, ref string _strErrText)
        {
            try
            {
                StringBuilder strQuery = new StringBuilder();
                strQuery.Append($"EXEC UpdateEquipmentProgDateTime '{equipName}', {status} ");

                bool result = CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
                return result;
            }
            catch (Exception ex)
            {
                _strErrText = ex.ToString();
                return false;
            }
        }
        public bool UpdateEquipmentProgDateTimeForProgram(string equipName, int status, ref string _strErrCode, ref string _strErrText)
        {
            try
            {
                StringBuilder strQuery = new StringBuilder();
                strQuery.Append($"EXEC UpdateEquipmentProgDateTimeForProgram '{equipName}', {status} ");

                bool result = CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
                return result;
            }
            catch (Exception ex)
            {
                _strErrText = ex.ToString();
                return false;
            }
        }

        /// <summary>
        /// 장비명을 받아 해당 장비의 상태 및 기본 정보를 조회
        /// 장비 상태는 ProgDateTime 이 현재시간과 비교하여 60초 이상 차이가 나면 STOP, 아니면 START 로 처리
        /// </summary>
        /// <param name="equipType"></param>
        /// <param name="_strErrCode"></param>
        /// <param name="_strErrText"></param>
        /// <returns></returns>
        public DataTable GetProgramStatus(string equipType, ref string _strErrCode, ref string _strErrText)
        {
            DataTable result = null;
            try
            {
                StringBuilder strQuery = new StringBuilder();
                strQuery.Append($"EXEC GetProgramStatus '{equipType}'");

                DataSet ds = CFW.Data.MsSqlDbAccess.GetDataSet(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
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

        public DataTable GetProgramStatus2(ref string _strErrCode, ref string _strErrText)
        {
            DataTable result = null;
            try
            {
                StringBuilder strQuery = new StringBuilder();
                strQuery.Append($"EXEC GetProgramStatus2 ");

                DataSet ds = CFW.Data.MsSqlDbAccess.GetDataSet(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
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

        public DataTable GetEquipmentModifiedInfo(ref string _strErrCode, ref string _strErrText)
        {
            DataTable result = null;
            try
            {
                StringBuilder strQuery = new StringBuilder();
                strQuery.Append($"EXEC GetEquipmentModifiedInfo");

                DataSet ds = CFW.Data.MsSqlDbAccess.GetDataSet(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
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

        public DataTable GetCurrentTagValue(string equipType, string equipName, ref string _strErrCode, ref string _strErrText)
        {
            DataTable result = null;
            try
            {
                StringBuilder strQuery = new StringBuilder();
                strQuery.Append($"EXEC GetCurrentTagValue '{equipType}', '{equipName}'");

                DataSet ds = CFW.Data.MsSqlDbAccess.GetDataSet(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
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
        
        public DataTable GetTagValueHistory(string tagName, int periodDays, ref string _strErrCode, ref string _strErrText)
        {
            DataTable result = null;
            try
            {
                StringBuilder strQuery = new StringBuilder();
                strQuery.Append($"EXEC GetTagValueHistory '{tagName}', {periodDays}");

                DataSet ds = CFW.Data.MsSqlDbAccess.GetDataSet(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
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

        public DataTable GetPIAlarmStatus(string equipType, string equipName, ref string _strErrCode, ref string _strErrText)
        {
            DataTable result = null;
            try
            {
                StringBuilder strQuery = new StringBuilder();
                strQuery.Append($"EXEC GetPIAlarmStatus '{equipType}', '{equipName}'");

                DataSet ds = CFW.Data.MsSqlDbAccess.GetDataSet(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
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

        public DataTable GetAssignedEquipmentTypeList(string serverName, ref string _strErrCode, ref string _strErrText)
        {
            DataTable result = null;
            try
            {
                StringBuilder strQuery = new StringBuilder();
                strQuery.Append($"EXEC GetAssignedEquipmentTypeList '{serverName}'");

                DataSet ds = CFW.Data.MsSqlDbAccess.GetDataSet(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
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
        
        public bool DeleteEquipmentInfo(string equipName, ref string _strErrCode, ref string _strErrText)
        {
            try
            {
                StringBuilder strQuery = new StringBuilder();
                strQuery.Append($"EXEC DeleteEquipmentInfo '{equipName}', '{UserAuthentication.UserID}'");
                bool result = CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);

                //
                // 2022. 3. 30: Han, Ilho
                //      delete row from MA_FAILOVER_CD
                //
                if (result == true)
                {
                    strQuery.Clear();

                    strQuery.Append($"DELETE FROM [dbo].[MA_FAILOVER_CD] WHERE EQUIP_NM = '{equipName}'");

                    result = CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
                }
                ///////////////////////////
                ///
                return result;
            }
            catch (Exception ex)
            {
                _strErrText = ex.ToString();
                return false;
            }
        }
        
        public bool InsertUpdateEquipmentInfo(bool add, string equipName, string description, string equipType, string interfaceType, string connectionInfo, string extraInfo, string serverName, string useFalg, string configInfo, ref string _strErrCode, ref string _strErrText)
        {
            try
            {
                StringBuilder strQuery = new StringBuilder();
                if (add)
                {
                    strQuery.Append($"EXEC InsertEquipmentInfo ");// '{equipName}', '{description}', '{equipType}', '{connectionInfo}', '{extraInfo}', '{serverName}', '{useFalg}', '{UserAuthentication.UserID}'");
                }
                else
                {
                    strQuery.Append($"EXEC UpdateEquipmentInfo ");// '{equipName}', '{description}', '{equipType}', '{connectionInfo}', '{extraInfo}', '{serverName}', '{useFalg}', '{UserAuthentication.UserID}'");
                }
                strQuery.Append($" '{equipName}', '{description}', '{equipType}', '{interfaceType}', '{connectionInfo}', '{extraInfo}', '{serverName}', '{useFalg}', '{UserAuthentication.UserID}', '{configInfo}'");
                bool result = CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);

                //
                // 2022. 3. 30: Han, Ilho
                //      Add row to MA_FAILOVER_CD
                //
                if(result)
                {
                    int serverId= GetServerId(serverName);
                    if (serverId == -1) serverId = 0;
                    strQuery.Clear();

                    strQuery.Append($"EXEC InsertUpdateFailoverInfo '{equipName}', {serverId}");

                    //if (add)
                    //{
                    //    strQuery.Append($"INSERT INTO [dbo].[MA_FAILOVER_CD]([EQUIP_NM], [FILE_PATH], DEFAULT_SERVER) ");
                    //    strQuery.Append($" VALUES ('{equipName}', '.\\DataSpiderPC01.EXE {equipName}', {serverId})");
                    //}
                    //else
                    //{
                    //    strQuery.Append($"UPDATE [dbo].[MA_FAILOVER_CD] SET [FILE_PATH] = '.\\DataSpiderPC01.EXE {equipName}', DEFAULT_SERVER = {serverId} ");
                    //    strQuery.Append($" WHERE EQUIP_NM = '{equipName}'");
                    //}
                    result = CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
                }
                ///////////////////////////
                return result;
            }
            catch (Exception ex)
            {
                _strErrText = ex.ToString();
                return false;
            }
        }
        public bool InsertUpdateTagInfo(bool add, string tagName, string msgType, string equipName, string description, string piTagName, string valuePosition, string datePosition, string timePosition, string itemName, ref string _strErrCode, ref string _strErrText)
        {
            try
            {
                StringBuilder strQuery = new StringBuilder();
                if (add)
                {
                    strQuery.Append($"EXEC InsertTagInfo ");// '{tagName}', '{msgType}', '{equipName}', '{description}', '{piTagName}', '{valuePosition}', '{datePosition}', '{timePosition}', '{itemName}'");
                }
                else
                {
                    strQuery.Append($"EXEC UpdateTagInfo ");// '{tagName}', '{msgType}', '{equipName}', '{description}', '{piTagName}', '{valuePosition}', '{datePosition}', '{timePosition}', '{itemName}'");
                }
                strQuery.Append($" '{tagName}', '{msgType}', '{equipName}', '{description}', '{piTagName}', '{valuePosition}', '{datePosition}', '{timePosition}', '{itemName}', '{UserAuthentication.UserID}'");
                bool result = CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
                return result;
            }
            catch (Exception ex)
            {
                _strErrText = ex.ToString();
                return false;
            }
        }

        public bool DeleteTagInfo(string tagName, ref string _strErrCode, ref string _strErrText)
        {
            try
            {
                StringBuilder strQuery = new StringBuilder();
                strQuery.Append($"EXEC DeleteTagInfo '{tagName}', '{UserAuthentication.UserID}'");
                bool result = CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
                return result;
            }
            catch (Exception ex)
            {
                _strErrText = ex.ToString();
                return false;
            }
        }
        public DataTable GetUserInfo(string id, ref string _strErrCode, ref string _strErrText)
        {
            DataTable result = null;
            try
            {
                StringBuilder strQuery = new StringBuilder();
                strQuery.Append($"EXEC GetUserInfo '{id}'");

                DataSet ds = CFW.Data.MsSqlDbAccess.GetDataSet(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
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
        public bool DeleteUserInfo(string id, ref string _strErrCode, ref string _strErrText)
        {
            try
            {
                StringBuilder strQuery = new StringBuilder();
                strQuery.Append($"EXEC DeleteUserInfo '{id}', '{UserAuthentication.UserID}'");
                bool result = CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
                return result;
            }
            catch (Exception ex)
            {
                _strErrText = ex.ToString();
                return false;
            }
        }
        public bool InsertUpdateUserInfo(bool add, string userID, string userPW, string userName, UserLevel userLevel, string department, string description, ref string _strErrCode, ref string _strErrText)
        {
            try
            {
                StringBuilder strQuery = new StringBuilder();
                if (add)
                {
                    strQuery.Append($"EXEC InsertUserInfo ");
                }
                else
                {
                    strQuery.Append($"EXEC UpdateUserInfo ");
                }
                strQuery.Append($" '{userID}', '{userPW}', '{userName}', {(int)userLevel}, '{department}', '{description}', '{UserAuthentication.UserID}'");
                bool result = CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
                return result;
            }
            catch (Exception ex)
            {
                _strErrText = ex.ToString();
                return false;
            }
        }
        public bool InitializeUserPassword(string userID, string userPW, ref string _strErrCode, ref string _strErrText)
        {
            try
            {
                StringBuilder strQuery = new StringBuilder();

                strQuery.Append($"EXEC InitializeUserPassword '{userID}', '{userPW}', '{UserAuthentication.UserID}'");

                bool result = CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
                return result;
            }
            catch (Exception ex)
            {
                _strErrText = ex.ToString();
                return false;
            }
        }

        //
        // 2022. 3. 8 : Han, Ilho
        //      : Add procedure for Tag Group
        //
        public DataTable GetTagGroupByEQType(string eqType, ref string _strErrCode, ref string _strErrText)
        {
            DataTable result = null;
            try
            {
                StringBuilder strQuery = new StringBuilder();
                strQuery.Append($"EXEC GetTagGroupByEQType '{eqType}'");

                DataSet ds = CFW.Data.MsSqlDbAccess.GetDataSet(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
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

        public DataTable GetTagGroupInfo(string groupName, ref string _strErrCode, ref string _strErrText)
        {
            DataTable result = null;
            try
            {
                StringBuilder strQuery = new StringBuilder();
                strQuery.Append($"EXEC GetTagGroupInfo '{groupName}'");

                DataSet ds = CFW.Data.MsSqlDbAccess.GetDataSet(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
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

        public DataTable GetAllTagHistoryValue(string tagName, string minDate, string maxDate, ref string _strErrCode, ref string _strErrText)
        {
            DataTable result = null;
            try
            {
                StringBuilder strQuery = new StringBuilder();
                strQuery.Append($"EXEC GetAllTagValueHistory '{tagName}', '{minDate}', '{maxDate}'");

                DataSet ds = CFW.Data.MsSqlDbAccess.GetDataSet(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
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
        
        public DataTable GetTagValueHistoryByEquip(string equipName, string minDate, string maxDate, ref string _strErrCode, ref string _strErrText)
        {
            DataTable result = null;
            try
            {
                StringBuilder strQuery = new StringBuilder();
                strQuery.Append($"EXEC GetTagValueHistoryByEquip '{equipName}', '{minDate}', '{maxDate}'");

                DataSet ds = CFW.Data.MsSqlDbAccess.GetDataSet(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
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
        public DataTable GetTagValueHistoryByTag(string tagName, string minDate, string maxDate, ref string _strErrCode, ref string _strErrText)
        {
            DataTable result = null;
            try
            {
                StringBuilder strQuery = new StringBuilder();
                strQuery.Append($"EXEC GetTagValueHistoryByTag '{tagName}', '{minDate}', '{maxDate}'");

                DataSet ds = CFW.Data.MsSqlDbAccess.GetDataSet(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
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
        public DataTable GetSystemLog(string minDate, string maxDate, string equipName,  string level, ref string _strErrCode, ref string _strErrText)
        {
            DataTable result = null;
            try
            {
                StringBuilder strQuery = new StringBuilder();
                strQuery.Append($"EXEC GetSystemLog '{minDate}', '{maxDate}', '{equipName}', '{level}'");

                DataSet ds = CFW.Data.MsSqlDbAccess.GetDataSet(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
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


        public bool InsertTagGroupInfo(string groupName, List <String> tagNames, ref string _strErrCode, ref string _strErrText)
        {
            try
            {
                var tvTagInfo = new DataTable();
                tvTagInfo.Columns.Add(new DataColumn("TAG_NM", typeof(String)));
                tvTagInfo.Columns.Add(new DataColumn("GROUP_NM", typeof(String)));

                // populate DataTable from your List here
                foreach (string tName in tagNames)
                {
                    tvTagInfo.Rows.Add(tName, groupName);
                }

                try
                {
                    StringBuilder strQuery = new StringBuilder();
                    strQuery.Append($"InsertTagGroupInfo");

                    var parameters = new[]
                    {
                        new SqlParameter 
                        {
                            ParameterName = "listTGIP",
                            Value = tvTagInfo,
                            SqlDbType = SqlDbType.Structured,
                            TypeName = "dbo.TagGroupInfo"
                        }
                    };

                    bool result = CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), parameters, CommandType.StoredProcedure, ref _strErrCode, ref _strErrText);
                    return result;
                }
                catch (Exception ex)
                {
                    _strErrText = ex.ToString();
                    return false;
                }
            }
            catch (Exception ex)
            {
                _strErrText = ex.ToString();
                return false;
            }
        }

        public bool DeleteTagGroupInfo(string groupName, List<String> tagNames, ref string _strErrCode, ref string _strErrText)
        {
            try
            {
                try
                {
                    StringBuilder strQuery = new StringBuilder();

                    if (tagNames == null)
                    {
                        strQuery.Append($"EXEC DeleteTagGroupInfoAll '{groupName}'");
                        bool result = CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);

                        return result;
                    }
                    else
                    {
                        foreach (string tName in tagNames)
                        {
                            strQuery.Append($"EXEC DeleteTagGroupInfo '{tName}', '{groupName}'");
                            bool result = CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);

                            if (result == false) return result;
                        }
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _strErrText = ex.ToString();
                    return false;
                }
            }
            catch (Exception ex)
            {
                _strErrText = ex.ToString();
                return false;
            }
        }

        public bool InsertTagGroup(string strTagGroup, string strstrEQType, string strDesc, ref string _strErrCode, ref string _strErrText)
        {
            try
            {
                StringBuilder strQuery = new StringBuilder();
                strQuery.Append($"EXEC InsertTagGroup '{strTagGroup}', '{strstrEQType}', '{strDesc}'");

                bool result = CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
                return result;
            }
            catch (Exception ex)
            {
                _strErrText = ex.ToString();
                return false;
            }
        }

        public bool UpdateTagGroup(string strTagGroup, string strEQType, string strDesc, ref string _strErrCode, ref string _strErrText)
        {
            try
            {
                StringBuilder strQuery = new StringBuilder();
                strQuery.Append($"EXEC UpdateTagGroup '{strTagGroup}', '{strEQType}', '{strDesc}'");

                bool result = CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
                return result;
            }
            catch (Exception ex)
            {
                _strErrText = ex.ToString();
                return false;
            }
        }

        public bool DeleteTagGroup(string strTagGroup, string strEQType, ref string _strErrCode, ref string _strErrText)
        {
            try
            {
                StringBuilder strQuery = new StringBuilder();
                strQuery.Append($"EXEC DeleteTagGroup '{strEQType}', '{strTagGroup}'");

                bool result = CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
                return result;
            }
            catch (Exception ex)
            {
                _strErrText = ex.ToString();
                return false;
            }
        }


        // KWC
        public DataTable GetTableInfo(string strQuery, ref string _strErrCode, ref string _strErrText)
        {
            DataTable result = null;
            try
            {
                DataSet ds = CFW.Data.MsSqlDbAccess.GetDataSet(strQuery, null, CommandType.Text, ref _strErrCode, ref _strErrText);
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
        
        public bool ExecuteNonQuery(string strQuery, ref string _strErrCode, ref string _strErrText)
        {
            try
            {
                bool result = CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery, null, CommandType.Text, ref _strErrCode, ref _strErrText);
                return result;
            }
            catch (Exception ex)
            {
                _strErrText = ex.ToString();
                return false;
            }
        }

        public int GetServerId(string serverName)
        {
            string errCode = string.Empty;
            string errText = string.Empty;
            //StringBuilder strQuery = new StringBuilder();
            //int MyId = -1;
            //strQuery = new StringBuilder();
            //strQuery.Append($" SELECT CODE_VALUE FROM MA_COMMON_CD WHERE CD_GRP='SERVER_CODE' AND CODE='{serverName}' ");
            //DataTable MyIdTbl = GetTableInfo(strQuery.ToString(), ref errCode, ref errText);
            //if (MyIdTbl == null) return MyId;
            //if (MyIdTbl.Rows.Count > 0) MyId = int.Parse(MyIdTbl.Rows[0]["CODE_VALUE"].ToString());
            //return MyId;

            int result = -1;
            try
            {
                DataTable dtResult = GetCommonCode("SERVER_CODE", serverName, ref errCode, ref errText);
                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (!int.TryParse(dtResult.Rows[0]["CODE_VALUE"].ToString(), out result))
                    {
                        result = -1;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }
        public bool RestPIIFFlag(int hiSeq)
        {
            string _strErrCode = string.Empty;
            string _strErrText = string.Empty;

            try
            {
                StringBuilder strQuery = new StringBuilder();
                //strQuery.Append($"UPDATE HI_MEASURE_RESULT SET IF_FLAG = 'N', IF_COUNT = 0 WHERE HI_SEQ = {hiSeq}");
                strQuery.Append($"EXEC ResetPIIFFlag {hiSeq}");

                bool result = CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
                return result;
            }
            catch (Exception ex)
            {
                _strErrText = ex.ToString();
                return false;
            }
        }
        public bool RemovePIAlarm(int hiSeq)
        {
            string _strErrCode = string.Empty;
            string _strErrText = string.Empty;

            try
            {
                StringBuilder strQuery = new StringBuilder();
                //strQuery.Append($"INSERT INTO HI_MEASURE_RESULT_BK SELECT TAG_NM, MEASURE_VALUE, MEASURE_DATE, 'Z', getdate(), getdate(), '{UserAuthentication.UserID}', REMARK, IF_COUNT FROM HI_MEASURE_RESULT WHERE HI_SEQ = {hiSeq};");
                //strQuery.Append($"DELETE HI_MEASURE_RESULT WHERE HI_SEQ = {hiSeq}");
                strQuery.Append($"EXEC RemovePiAlarm '{UserAuthentication.UserID}', {hiSeq};");

                bool result = CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref _strErrCode, ref _strErrText);
                return result;
            }
            catch (Exception ex)
            {
                _strErrText = ex.ToString();
                return false;
            }
        }
    }
}
    