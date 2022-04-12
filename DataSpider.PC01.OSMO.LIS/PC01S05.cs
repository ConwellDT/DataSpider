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
using System.Net.Sockets;
using System.Net;
using System.Data;

namespace SEIMM.PC01.PT
{
    /// <summary>
    /// SIEMENS RAPIDLab 348EX LIS2
    /// </summary>
    public class PC01S05 : PC00B02
    {
        public abstract class CommState
        {
            public const int IDLE = 0;
            public const int WAITING = 1;
            public const int CS1 = 2;
            public const int CS2 = 3;
            public const int EOT = 4;

        }

        public PC01S05() : base()
        {
        }

        public PC01S05(PC01F01 pOwner, string equipType, string equipName, string connectionInfo, string extraInfo, int nCurNo, bool bAutoRun = false) : base(pOwner, equipType, equipName, connectionInfo, extraInfo, nCurNo, bAutoRun)
        {
        }

        public PC01S05(PC01F01 pOwner, DataRow dr, int nCurNo, bool bAutoRun = false) : this(pOwner, dr["EQUIP_TYPE_NM"].ToString(), dr["EQUIP_NM"].ToString(), dr["CONNECTION_INFO"].ToString(), dr["EXTRA_INFO"].ToString(), nCurNo, bAutoRun)
        {
            drEquipment = dr;
            ReadConfig();
            ReadConnectionInfoForSocket();
            if (m_AutoRun == true)
            {
                m_Thd = new Thread(ThreadJob);
                m_Thd.Start();
            }
        }
        protected override void ParseMessage(string Msg)
        {
            string msgString = string.Empty;
            string recvCS = string.Empty;
            CommStatus = CommState.IDLE;

            try
            {
                for (int nCh = 0; nCh < Msg.Length; nCh++)
                {
                    switch (CommStatus)
                    {
                        case CommState.IDLE:
                            if (Msg[nCh] == ASCII.STX)
                            {
                                msgString = string.Empty;
                                msgString += Msg[nCh];
                                CommStatus = CommState.WAITING;
                            }
                            break;
                        case CommState.WAITING:
                            msgString += Msg[nCh];
                            if (Msg[nCh] == ASCII.ETX) //|| Msg[nCh] == ASCII.ETB)
                            {
                                CommStatus = CommState.CS1;
                            }
                            break;
                        case CommState.CS1:
                            msgString += Msg[nCh];
                            recvCS += Msg[nCh];
                            CommStatus = CommState.CS2;
                            break;
                        case CommState.CS2:
                            msgString += Msg[nCh];
                            recvCS += Msg[nCh];
                            CommStatus = CommState.EOT;
                            break;
                        case CommState.EOT:
                            msgString += Msg[nCh];
                            if (Msg[nCh] == ASCII.EOT)
                            {
                                if (CalcedCheckSum(msgString) != recvCS)
                                {
                                    listViewMsg.UpdateMsg("CalcedCheckSum False - " + msgString, false, true, true, PC00D01.MSGTERR);
                                }

                                char[] separators = new char[] { (char)ASCII.STX, (char)ASCII.FS, (char)ASCII.RS };
                                StringBuilder ssb = new StringBuilder();
                                string stringDate = "";
                                string stringTime = "";
                                string[] split = msgString.Split(separators);
                                foreach (string ln in split)
                                {
                                    string[] sp = ln.Split((char)ASCII.GS);
                                    if (sp.Length >= 5 && sp[0] == "rDATE")
                                        stringDate = sp[1];
                                    if (sp.Length >= 5 && sp[0] == "rTIME")
                                        stringTime = sp[1];
                                }
                                int nType = MSGTYPE.UNKNOWN;
                                string typeName = string.Empty;
                                if (msgString.Contains("SMP_NEW_DATA"))
                                {
                                    nType = MSGTYPE.SMP_NEW_DATA;
                                    typeName = "SND";
                                }
                                if (msgString.Contains("SMP_EDIT_DATA"))
                                {
                                    nType = MSGTYPE.SMP_EDIT_DATA;
                                    typeName = "SED";
                                }
                                if (msgString.Contains("QC_NEW_DATA"))
                                {
                                    nType = MSGTYPE.QC_NEW_DATA;
                                    typeName = "QND";
                                }
                                if (msgString.Contains("QC_DEL"))
                                {
                                    nType = MSGTYPE.QC_DEL;
                                    typeName = "QDL";
                                }
                                if (msgString.Contains("CAL_NEW_DATA"))
                                {
                                    nType = MSGTYPE.CAL_NEW_DATA;
                                    typeName = "CND";
                                }
                                ssb.AppendLine($"{typeName}_SVRTIME, {stringDate} {stringTime}, {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

                                foreach (string ln in split)
                                {
                                    string[] sp = ln.Split((char)ASCII.GS);
                                    if (sp.Length >= 5)
                                    {
                                        //sp[0]
                                        ssb.AppendLine($"{typeName}_{sp[0]}_VAL, {stringDate} {stringTime}, {sp[1]}");
                                        ssb.AppendLine($"{typeName}_{sp[0]}_UNIT, {stringDate} {stringTime}, {sp[2]}");
                                        ssb.AppendLine($"{typeName}_{sp[0]}_NOTE, {stringDate} {stringTime}, {sp[3].Replace((char)ASCII.ETB, ' ')}");
                                    }
                                }
                                if (nType != MSGTYPE.UNKNOWN)
                                    EnQueue(nType, ssb.ToString());
                                else
                                {
                                    listViewMsg.UpdateMsg("MSGTYPE.UNKNOWN - " + msgString, false, true, true, PC00D01.MSGTERR);
                                }
                            }
                            else
                            {
                                listViewMsg.UpdateMsg("Not EOT - " + msgString, false, true, true, PC00D01.MSGTERR);
                            }
                            // EOT가 아니면 에러! 특별히 할 것이 없음.
                            state.sb = new StringBuilder();
                            state.sb.Append(Msg.Substring(nCh + 1));
                            CommStatus = CommState.IDLE;
                            break;
                        default:
                            CommStatus = CommState.IDLE;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                listViewMsg.UpdateMsg(ex.ToString(), false, true, true, PC00D01.MSGTERR);
            }
        }
        string CalcedCheckSum(string Msg)
        {
            byte byteCheckSum = 0;
            bool bCheckSum = false;
            foreach (char ch in Msg.ToCharArray())
            {
                switch ((int)ch)
                {
                    case ASCII.STX:
                        byteCheckSum = ASCII.STX;
                        bCheckSum = true;
                        break;
                    case ASCII.ETX:
                    //case ASCII.ETB:
                        byteCheckSum += (byte)ch;
                        bCheckSum = false;
                        break;
                    default:
                        if (bCheckSum == true)
                            byteCheckSum += (byte)ch;
                        break;
                }
            }
            return byteCheckSum.ToString("X02");
        }
    }
}
