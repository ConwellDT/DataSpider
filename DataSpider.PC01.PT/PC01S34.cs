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
using DataSpider.PC00.PT;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Data;
using Opc.Ua;
using System.Text.Json;
using System.Management.Automation.Language;

namespace DataSpider.PC01.PT
{
    /// <summary>
    /// RAPIDPoint_500e
    /// SIEMENS RAPIDPoint 500e Blood Gas System LIS3
    /// </summary>
    public class PC01S34 : PC00B02
    {
        private string MOD = string.Empty;
        private string IID = string.Empty;

        private Dictionary<string, int> dicMsgType = null;

        public PC01S34() : base()
        {
        }

        public PC01S34(PC01F01 pOwner, string equipType, string equipName, string connectionInfo, string extraInfo, int nCurNo, bool bAutoRun = false) : base(pOwner, equipType, equipName, connectionInfo, extraInfo, nCurNo, bAutoRun)
        {
        }

        public PC01S34(PC01F01 pOwner, DataRow dr, int nCurNo, bool bAutoRun = false) : this(pOwner, dr["EQUIP_TYPE_NM"].ToString(), dr["EQUIP_NM"].ToString(), dr["CONNECTION_INFO"].ToString(), dr["EXTRA_INFO"].ToString(), nCurNo, bAutoRun)
        {
            drEquipment = dr;
            dicMsgType = new Dictionary<string, int>();
            ReadConnectionInfoForSocket();
            ReadConfigInfo();
            if (m_AutoRun == true)
            {
                m_Thd = new Thread(ThreadJob);
                m_Thd.Start();
            }
        }

        private new void ReadConfigInfo()
        {
            try
            {
                if (drEquipment == null)
                {
                    listViewMsg.UpdateMsg($"No Config Info.", false, true, true, PC00D01.MSGTERR);
                    return;
                }
                // SMP_NEW_DATA = 1 형태
                //string[] arrConfigInfo = drEquipment["CONFIG_INFO"]?.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                //foreach (string s in arrConfigInfo)
                //{
                //    string[] info = s.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                //    if (info.Length > 1)
                //    {
                //        string key = info[0].Trim();
                //        if (!dicMsgType.ContainsKey(key))
                //        {
                //            if (int.TryParse(info[1], out int value))
                //            {
                //                dicMsgType.Add(key, value);
                //                listViewMsg.UpdateMsg($"MSG_TYPE - {key} = {value}", false, true, true, PC00D01.MSGTINF);
                //            }
                //        }
                //    }
                //}
                string configInfo = drEquipment["CONFIG_INFO"]?.ToString();
                if (string.IsNullOrEmpty(configInfo)) 
                {
                    listViewMsg.UpdateMsg($"No Config Info.", false, true, true, PC00D01.MSGTERR);
                    return;
                }
                JsonDocument document = JsonDocument.Parse(configInfo);
                JsonElement root = document.RootElement;
                MOD = root.GetProperty("MOD").ToString();
                IID = root.GetProperty("IID").ToString();
                foreach (var e in root.GetProperty("IDENTIFIER").EnumerateArray())
                {
                    string key = e.GetProperty("NAME").ToString();
                    string val = e.GetProperty("MSG_TYPE").ToString();
                    if (!dicMsgType.ContainsKey(key))
                    {
                        if (int.TryParse(val, out int value))
                        {
                            dicMsgType.Add(key, value);
                            listViewMsg.UpdateMsg($"MSG_TYPE - {key} = {value}", false, true, true, PC00D01.MSGTINF);
                        }
                    }

                }
            }
            catch (Exception ex) 
            {
                listViewMsg.UpdateMsg($"Exceptioin - ReadConfigInfo ({ex})", false, true, true, PC00D01.MSGTERR);
            }
        }

        protected override void ParseMessage(string Msg)
        {
            int idxSTX;
            int idxEOT;
            LIS3_MESSAGE lis3 = new LIS3_MESSAGE();
            try
            {
                while (true)
                {
                    idxSTX = Msg.IndexOf((char)ASCII.STX);
                    if (idxSTX < 0)
                    {
                        Msg = string.Empty;
                        break;
                    }
                    Msg = Msg.Remove(0, idxSTX);

                    idxEOT = Msg.IndexOf((char)ASCII.EOT);
                    if (idxEOT < 0)
                    {
                        break;
                    }
                    string message = Msg.Substring(0, idxEOT + 1);
                    Msg = Msg.Remove(0, idxEOT + 1);
                    lis3.Parsing(message);

                    fileLog.WriteData(lis3.ToString(), "RecvData Parsed", "ParseMessage");
                    listViewMsg.UpdateMsg($"Recieved : {lis3.Identifier}", false, true, true, PC00D01.MSGTINF);

                    if (lis3.Checksum)
                    {
                        // 수신 메세지가 ACK 가 아니면 ACK 응답
                        if (!lis3.Identifier.Equals("<ACK>"))
                        {
                            listViewMsg.UpdateMsg("Send ACK", false, true, true, PC00D01.MSGTINF);
                            //fileLog.WriteData($"{BitConverter.ToString(lis3.ACKMessage)}", "SendMessage", "ACK");
                            Send(lis3.ACKMessage);
                        }
                        byte[] responseMessage = lis3.GetResponseMessage();
                        if (responseMessage != null)
                        {
                            listViewMsg.UpdateMsg($"Send Response for {lis3.Identifier}", false, true, true, PC00D01.MSGTINF);
                            //fileLog.WriteData($"{BitConverter.ToString(responseMessage)}", "SendMessage", $"Response for {lis3.Identifier}");
                            Send(responseMessage);
                        }

                        // 처리대상 인 경우만 EnQueue 처리
                        if (dicMsgType.TryGetValue(lis3.Identifier, out int msgType))
                        {
                            EnQueue(msgType, lis3.GetTTV());
                        }
                    }
                    else
                    {
                        listViewMsg.UpdateMsg("CheckSum False", false, true, true, PC00D01.MSGTERR);
                    }
                }
                state.sb = new StringBuilder();
                state.sb.Append(Msg);
            }
            catch (Exception ex)
            {
                listViewMsg.UpdateMsg(ex.ToString(), false, true, true, PC00D01.MSGTERR);
            }
        }

        private void Send(byte[] message)
        {
            fileLog.WriteData($"{dataEncoding.GetString(message)} ({BitConverter.ToString(message)})", "SendMessage", "");
            SendMessage(message);
        }
    }
}
