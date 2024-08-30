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
    /// <summary>
    /// MTBA_BPFLEX2 I/F
    /// </summary>
    public class PC01S12 : PC00B03
    {
        public const int STARTWITH_IDX = 0;
        public const int NOTCONTAINS1_IDX = 1;
        public const int NOTCONTAINS2_IDX = 2;
        public const int NTH_VALUE_IDX = 3;
        public const int SIZE_IDX = 4;

        IDictionary<string, System.Collections.Generic.List<string>> m_Flex2Dic = new Dictionary<string, System.Collections.Generic.List<string>>();
        StringBuilder sbTextFile = new StringBuilder();

        public PC01S12()
        {
        }
        public PC01S12(IPC00F00 pOwner, DataRow dr, string equipType, string equipName, string connectionInfo, string extraInfo, int nCurNo, bool bAutoRun = false) : base(pOwner, dr, equipType, equipName, connectionInfo, extraInfo, nCurNo, bAutoRun)
        {
        }
        public PC01S12(IPC00F00 pOwner, DataRow dr, int nCurNo, bool bAutoRun = false) : this(pOwner, dr, dr["EQUIP_TYPE_NM"].ToString(), dr["EQUIP_NM"].ToString(), dr["CONNECTION_INFO"].ToString(), dr["EXTRA_INFO"].ToString(), nCurNo, bAutoRun)
        {
            ReadConfig();
        }

        public void ReadConfig()
        {
            string line;
            string cfgFileName = $@".\CFG\{m_Type}.csv";   //MTBA_BPFLEX2.csv
            System.Collections.Generic.List<string> list = null;
            m_Flex2Dic.Clear();
            using (StreamReader file = new StreamReader(cfgFileName, Encoding.Default))
            {
                while ((line = file.ReadLine()) != null)
                {
                    if (line.StartsWith(";") == true) continue;
                    string[] split = line.Split(',');
                    list = new System.Collections.Generic.List<string>();
                    for (int nItem = 1; nItem < split.Length; nItem++)
                        list.Add(split[nItem]);
                    m_Flex2Dic.Add(split[0], list);
                    fileLog.WriteData(line, $"{cfgFileName}", $"ReadConfig");

                }
            }
        }


        public override string GetFileRawData(FileInfo fileInfo)
        {
            try
            {
                string retString = string.Empty;
                listViewMsg.UpdateMsg($"Open File {fileInfo.Name} ({fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss.fffffff})", false, true);
                PDDocument doc = PDDocument.load(fileInfo.FullName);
                PDFTextStripperByArea stripper = new PDFTextStripperByArea();
                int nWidth = 2159;
                int nHeight = 2793;
                string sNameRec = "region00";
                Rectangle2D rect = new Rectangle2D.Double(0, 0, nWidth, nHeight); // 영역 지정.
                stripper.setSortByPosition(true);
                stripper.addRegion(sNameRec, rect);
                PDPage page1 = (PDPage)doc.getDocumentCatalog().getAllPages().get(0); // 첫번째 페이지. 
                stripper.extractRegions(page1);
                retString = stripper.getTextForRegion(sNameRec);
                fileLog.WriteData(retString, "RawData", "GetFileRawData");
                doc.close();
                dtCurrFileWriteTime = fileInfo.LastWriteTime;
                Debug.WriteLine(dtCurrFileWriteTime.ToString());
                return retString;
            }
            catch (Exception ex)
            {
                listViewMsg.UpdateMsg(ex.ToString(), false, true, true, PC00D01.MSGTERR);
            }
            return null;
        }


        // name,time,value \r\n 목록을 리턴해야 함.
        public override string GetFileData(string sbData)
        {
            string retString = string.Empty;
            string keyString = string.Empty;
            string notContains1 = string.Empty;
            string notContains2 = string.Empty;
            int nThData = 0;
            int nSize = 0;
            System.Collections.Generic.List<string> list = null;
            string strGetData = string.Empty;
            DateTime dtTagTime = DateTime.MinValue;
            try
            {
                UpdateEquipmentProgDateTime(IF_STATUS.Normal);
                sbTextFile=new StringBuilder(InsertNewLine(sbData));

                foreach (KeyValuePair<string, System.Collections.Generic.List<string>> kvp in m_Flex2Dic)
                {
                    list = kvp.Value;
                    keyString = list[STARTWITH_IDX];
                    notContains1 = list[NOTCONTAINS1_IDX];
                    notContains2 = list[NOTCONTAINS2_IDX];
                    int.TryParse(list[NTH_VALUE_IDX], out nThData);
                    int.TryParse(list[SIZE_IDX], out nSize);

                    strGetData = GetData(sbTextFile, keyString, notContains1, notContains2, nThData, nSize);
                    if (kvp.Key.StartsWith("DateTime"))
                        PC00U01.TryParseExact(strGetData, out dtTagTime);

                    retString += $"{kvp.Key.ToUpper()},{dtTagTime:yyyy-MM-dd HH:mm:ss},{strGetData}"+Environment.NewLine;
                }
                //fileLog.WriteData(retString, "EnqueueData", "GetFileData");

            }
            catch (Exception ex)
            {
                listViewMsg.UpdateMsg(ex.ToString(), false, true, true, PC00D01.MSGTERR);
            }
            finally
            {
            }
            return retString;
        }
        string InsertNewLine(string sbData)
        {
            string retString = string.Empty;
            System.Collections.Generic.List<string> list = null;
            string[] spline = sbData.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            int at;
            string targetLine = string.Empty;
            try
            {
                foreach (string line in spline)
                {
                    targetLine = line;
                    foreach (KeyValuePair<string, System.Collections.Generic.List<string>> kvp in m_Flex2Dic)
                    {
                        list = kvp.Value;
                        if (targetLine.Contains(list[STARTWITH_IDX]))
                        {
                            if ((list[NOTCONTAINS1_IDX] == string.Empty && list[NOTCONTAINS2_IDX] == string.Empty) ||
                               (list[NOTCONTAINS1_IDX] != string.Empty && list[NOTCONTAINS2_IDX] == string.Empty && targetLine.Contains(list[NOTCONTAINS1_IDX]) == false) ||
                               (list[NOTCONTAINS1_IDX] == string.Empty && list[NOTCONTAINS2_IDX] != string.Empty && targetLine.Contains(list[NOTCONTAINS2_IDX]) == false) ||
                               (list[NOTCONTAINS1_IDX] != string.Empty && list[NOTCONTAINS2_IDX] != string.Empty && targetLine.Contains(list[NOTCONTAINS1_IDX]) == false && targetLine.Contains(list[NOTCONTAINS2_IDX]) == false)
                               )
                            {
                                at = targetLine.IndexOf(list[STARTWITH_IDX]);
                                if( at> -1 )
                                    targetLine = targetLine.Substring(0, at) + Environment.NewLine + targetLine.Substring(at);
                            }
                        }
                    }
                    if (targetLine.Contains("RESULTS:"))
                    {
                        at = targetLine.IndexOf("RESULTS:");
                        if (at > -1)
                            targetLine = targetLine.Substring(0, at) + Environment.NewLine + targetLine.Substring(at);
                    }
                    retString += targetLine + Environment.NewLine;
                }
            }
            catch (Exception ex)
            {
                listViewMsg.UpdateMsg(ex.ToString(), false, true, true, PC00D01.MSGTERR);
            }
            finally
            {
            }
            return retString;
        }

        string GetData(StringBuilder sbTextFile, string Key, string notContains1, string notContains2, int nThData, int nSize)
        {
            string retString = string.Empty;
            string tempString = string.Empty;
            string[] spline = sbTextFile.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            foreach (string line in spline)
            {
                if (line.StartsWith(Key) == true)
                {
                    if ((notContains1 == String.Empty && notContains1 == String.Empty) ||
                         (notContains1 != String.Empty && notContains2 == String.Empty && line.Contains(notContains1) == false) ||
                         (notContains1 == String.Empty && notContains2 != String.Empty && line.Contains(notContains2) == false) ||
                         (notContains1 != String.Empty && notContains2 != String.Empty && line.Contains(notContains1) == false && line.Contains(notContains2) == false)
                        )
                    {
                        tempString = line.Substring(Key.Length).Trim(); ;

                        int nCount = 0;
                        int at = 0;
                        int nTotalOffset = 0;


                        if (Key == "Osm" ||
                            Key == "pH" || Key == "PO2" || Key == "PCO2" ||

                            Key == "Gln" || Key == "Glu" || Key == "Gluc" ||
                            Key == "Lac" || Key == "NH4+" || Key == "Na+" ||
                            Key == "K+" || Key == "Ca++" ||

                            Key == "pH @ Temp" || Key == "PO2 @ Temp" || Key == "PCO2 @ Temp" ||
                            Key == "O2 Saturation" || Key == "CO2 Saturation" || Key == "HCO3")
                        {
                            string[] spl = tempString.Split(' ');
                            decimal dc;
                            if (decimal.TryParse(spl[0], out dc) == false)
                                tempString = " " + tempString;
                        }

                        while (nCount < (nThData - 1))
                        {
                            at = tempString.Substring(nTotalOffset).IndexOf(' ');
                            if (at >= 0)
                            {
                                nTotalOffset += at + 1;
                            }
                            else
                                nTotalOffset = tempString.Length;
                            nCount++;
                        }
                        if (nSize == -1) // 나머지 데이터를 모두 취한다.
                        {
                            retString = tempString.Substring(nTotalOffset);
                        }
                        else if (nSize == -2) // 다음 space까지 문자열을 취한다.
                        {
                            at = tempString.Substring(nTotalOffset).IndexOf(' ');
                            if (at >= 0)
                                retString = tempString.Substring(nTotalOffset, at + 1);
                            else
                                retString = tempString.Substring(nTotalOffset);
                        }
                        else // 문자열의 크기만큼을 취한다.
                        {
                            if (tempString.Length > nTotalOffset + nSize)
                                retString = tempString.Substring(nTotalOffset, nSize);
                            else
                                retString = tempString.Substring(nTotalOffset);
                        }
                        break;
                    }
                }
            }
            return retString.Trim();
        }

    }
}
