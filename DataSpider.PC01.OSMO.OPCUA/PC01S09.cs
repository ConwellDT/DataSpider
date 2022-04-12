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
using SEIMM.PC00.PT;
using System.Diagnostics;
using System.Data;
// WildcardPattern
using System.Management.Automation;

namespace SEIMM.PC01.PT
{

    public class FIT_SC4P_DATA
    {
        private StringBuilder sbData = null;
        public string TIMESTAMP { get; set; }

        public FIT_SC4P_DATA(string _timeStamp = "")
        {
            sbData = new StringBuilder();
            TIMESTAMP = string.IsNullOrWhiteSpace(_timeStamp) ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : _timeStamp;
        }

        public bool Add(string name, string value)
        {
            sbData.AppendLine($"{name}, {TIMESTAMP}, {value}");
            return true;
        }

        public string GetStringData()
        {
            return sbData.ToString();
        }
    }

    /// <summary>
    /// File I/F, Filter Integrity Tester : Sartocheck 4 plus
    /// </summary>
    public class PC01S09 : PC00B03
    {
        public PC01S09()
        {
        }
        public PC01S09(IPC00F00 pOwner, string equipType, string equipName, string connectionInfo, string extraInfo, int nCurNo, bool bAutoRun = false) : base(pOwner, equipType, equipName, connectionInfo, extraInfo, nCurNo, bAutoRun)
        {
        }
        public PC01S09(IPC00F00 pOwner, DataRow dr, int nCurNo, bool bAutoRun = false) : this(pOwner, dr["EQUIP_TYPE_NM"].ToString(), dr["EQUIP_NM"].ToString(), dr["CONNECTION_INFO"].ToString(), dr["EXTRA_INFO"].ToString(), nCurNo, bAutoRun)
        {
            drEquipment = dr;
        }

        /// <summary>
        /// FIT_SC4P 장비의 경우 테스트 켄슬 등의 문제 발생 시, 차트데이터 (5초 인터벌의 데이터) 가 테스트 시간에 따라 가변적 등의 문제로 ttv 형태로 발췌하여 EnQueue 처리 필요
        /// </summary>
        /// <param name="sData"></param>
        /// <returns></returns>
        public override string GetFileData(string sData)
        {
            List<string> listData = sData.Split(new string[] { Environment.NewLine, "\n" }, StringSplitOptions.None).ToList();
            // DateTime
            FIT_SC4P_DATA data = new FIT_SC4P_DATA(listData[8]);

            try
            {
                // [Test results]
                data.Add("NetVolume", listData[3].Split(' ')[0]);
                data.Add("PressureDrop", listData[5].Split(' ')[0]);
                data.Add("WaterInstrusion", listData[7].Split(' ')[0]);
                data.Add("TestTime", listData[10].Split(' ')[0]);
                data.Add("Evaluation", listData[11].Split(' ')[0]);
                data.Add("TestPressure", listData[12].Split(' ')[0]);

                // [Data log]
                // 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 [Ext] 로 시작되는 줄에 "길이 데이터" 형태로 한줄로 데이터가 존재
                // 길이를 읽어 데이터를 추출하여 리스트에 저장
                int extLineNo = listData.FindIndex(x => x.Contains("[Ext]"));
                string extData = listData[extLineNo].Substring(50);

                List<string> listExt = PC00U01.ParseLengthDataType(extData);
                data.Add("Company", listExt[0]);
                data.Add("Building", listExt[1]);
                data.Add("Department", listExt[2]);
                data.Add("ManufactSite", listExt[3]);
                data.Add("Product", listExt[4]);
                data.Add("ProductLog", listExt[5]);
                data.Add("Filter", listExt[6]);
                data.Add("FilterLot", listExt[7]);
                data.Add("FilterLine", listExt[8]);
                data.Add("Housing", listExt[9]);
                data.Add("WettingMedium", listExt[10]);
                data.Add("TestGas", listExt[11]);
                data.Add("WaterQuality", listExt[12]);
                data.Add("Comment1", listExt[13]);
                data.Add("Comment2", listExt[14]);
                data.Add("Comment3", listExt[15]);

                // [Test Parameter]
                data.Add("MinDiffWitW", listData[extLineNo - 14].Split(' ')[0]);  //dif
                data.Add("ParamTestPressure", listData[extLineNo - 8].Split(' ')[0]);
                data.Add("WITMax", listData[extLineNo - 7].Split(' ')[0]);   //wit
                data.Add("MaxDiffusion", listData[extLineNo - 6].Split(' ')[0]);   //dif
                data.Add("MaxVolume", listData[extLineNo - 5].Split(' ')[2]);

                // [ETC]
                data.Add("User", listData[13]);
                data.Add("UnitNo", listData[17]);
                data.Add("ProgNo", listData[extLineNo + 27]);

                data.Add("SVRTIME", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                UpdateEquipmentProgDateTime(IF_STATUS.Normal);
            }
            catch (Exception ex)
            {
                UpdateEquipmentProgDateTime(IF_STATUS.InvalidData);
                listViewMsg.UpdateMsg(ex.ToString(), false, true, true, PC00D01.MSGTERR);
            }
            return data.GetStringData();
        }

        public string GetFileData_old(string sData)
        {
            List<string> listData = sData.Split(new string[] { "\n" }, StringSplitOptions.None).ToList<string>();
            // 테스트가 켄슬되는 경우 결과 데이터 중 차트 데이터 (5초 간격 데이터) 가 전부누락 또는 일부 누락하면서 줄수가 줄어드는 문제
            // 테스트 시간(초) 값 읽기. 차트 데이터 수량 읽기. 테스트 시간 / 5 의 값과 차트 데이터 수량 값이 같지 않으면 모자란 만큼 줄을 추가
            int testTime = 600;// int.Parse(listData[10]);
            int chartDataCount = int.Parse(listData[24]);
            for (int i = 0; i < (testTime - (chartDataCount * 5))/5; i++)
            {
                listData.Insert(25, string.Empty);
            }

            // [Ext] 데이터를 | 구분자로 한줄로 취합하여 마지막 줄에 추가
            StringBuilder sbExt = new StringBuilder();
            string extString = listData[165].Substring(50);
            while (!string.IsNullOrWhiteSpace(extString))
            {
                if (sbExt.Length > 0)
                {
                    sbExt.Append("|");
                }
                int offset = extString.IndexOf(' ');
                int length = int.Parse(extString.Substring(0, offset));
                sbExt.Append($"{extString.Substring(offset + 1, length)}");
                extString = extString.Substring(offset + 1 + length + 1);
            }
            listData.Add(sbExt.ToString());
            //sbData.Append(sbExt.ToString());
            return string.Join(Environment.NewLine, listData);
            //return sbData.ToString();
        }
    }
}
