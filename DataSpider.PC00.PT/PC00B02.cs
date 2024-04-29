using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataSpider.PC00.PT
{
    /// <summary>
    /// Base Class For Socket 
    /// </summary>
    public class PC00B02 : PC00B01
    {
        protected StateObject state = new StateObject();

        private string IpAddress
        {
            get { return m_IpAddress; }
            set { m_IpAddress = value; }
        }
        private string m_IpAddress;
        private int nPortNo
        {
            get { return m_nPortNo[m_nActive]; }
            set { m_nPortNo[m_nActive] = value; }
        }
        private int m_nActive = 0;
        private int[] m_nPortNo = new int[2];
        protected List<string> m_StartStringList = new List<string>();
        protected List<string> m_StatusStringList = new List<string>();
        // 20211123 kwc Value자리에 들어오는 문자열 처리를 위해 수정
        protected List<string> m_EventStringList = new List<string>();

        protected List<string> m_KeyStringList = new List<string>();
        protected List<int> m_LineLengthList = new List<int>();
        protected List<string> m_IgnoreStringList = new List<string>();

        protected int m_MessageLineCount = 0;
        protected int minimumLineCount = 0;
        protected int maximumLineCount = 0;
        protected string itemLineConf = string.Empty;


        protected int CommStatus { get; set; } = 0;

        private ManualResetEvent connectDone = new ManualResetEvent(false);

        protected class StateObject
        {
            public Socket workSocket = null;
            public const int BufferSize = 1024;
            public byte[] buffer = new byte[BufferSize];    // Recv Buffer
            public StringBuilder sb = new StringBuilder();// Received data string.  
            public DateTime dtLastReceived = DateTime.Now;
        }
        // private -> protected 변경 (2022.01.21) -CSH
        protected DateTime dtLastStatusUpdate = DateTime.MinValue;

        public PC00B02()
        {
        }
        public PC00B02(IPC00F00 pOwner, string equipType, string equipName, string connectionInfo, string extraInfo, int nCurNo, bool bAutoRun) : base(pOwner, equipType, equipName, connectionInfo, extraInfo, nCurNo, bAutoRun)
        {
        }
        protected void SendControlMessage(byte controlChar)
        {
            state.workSocket.Send(new byte[] { controlChar });
        }
        protected void SendMessage(byte[] message)
        {
            state.workSocket.Send(message);
        }
        protected void ThreadJob()
        {
            Thread.Sleep(1000);
            bool connectedflag = false;
            Connect();
            Thread.Sleep(1000);
            listViewMsg.UpdateStatus(true);
            listViewMsg.UpdateMsg("Thread started", true, true, true, PC00D01.MSGTINF);

            while (!bTerminal)
            {
                try
                {
                    //connectDone.WaitOne();
                    if (IsConnected == false || state.workSocket.Connected==false)
                    {
                        connectedflag = false;
                        UpdateEquipmentProgDateTime(IF_STATUS.Disconnected);
                        listViewMsg.UpdateMsg("Disconnected. Try to connect", true, true, true, PC00D01.MSGTERR);
                        Connect();
                        //Thread.Sleep(1000);
                    }
                    else
                    {
                        if (!connectedflag)
                        {
                            listViewMsg.UpdateMsg("Connected", true, true, true, PC00D01.MSGTINF);
                            connectedflag = true;
                        }
                        else
                        {
                            if (DateTime.Now.Subtract(dtLastStatusUpdate).TotalSeconds > 10)
                            {
                                listViewMsg.UpdateMsg("Connected", true, false, false, PC00D01.MSGTINF);
                            }
                        }
                        dtLastStatusUpdate = DateTime.Now;


                        if ((CommStatus != 0) && (DateTime.Now.Subtract(state.dtLastReceived).TotalSeconds > 30))
                        {
                            CommStatus = 0;
                        }
                        UpdateEquipmentProgDateTime(IF_STATUS.Normal);
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
                Thread.Sleep(1000);
            }
            Disconnect();
            UpdateEquipmentProgDateTime(IF_STATUS.Stop);
            listViewMsg.UpdateStatus(false);
            listViewMsg.UpdateMsg("Thread finished");
        }
        protected void ReadConnectionInfoForSocket()
        {
            string[] split = m_ConnectionInfo.Split(',');
            if (split.Length > 1)
            {
                m_IpAddress = split[0];
                int.TryParse(split[1], out m_nPortNo[0]);
                // 2023-02-16 KWC 
                m_nPortNo[1] = m_nPortNo[0];
                if (split.Length > 2)
                {
                    int.TryParse(split[2], out m_nPortNo[1]);
                }
            }
            else
            {
                listViewMsg.UpdateStatus(false);
                listViewMsg.UpdateMsg("Not started", true, false);
                listViewMsg.UpdateMsg($"ConnectionInfo error ({m_ConnectionInfo})", false, true, true, PC00D01.MSGTERR);
            }
        }

        // ini 파일 읽기
        protected void ReadConfig()
        {
            string section = m_Name;
            string configFile = @".\CFG\" + m_Type + ".INI";

            try
            {
                string property, value;
                if (!int.TryParse(PC00U01.ReadConfigValue("MESSAGE_LINE_COUNT", section, configFile), out m_MessageLineCount))
                {
                    //listViewMsg.UpdateMsg($"MESSAGE_LINE_COUNT setting error.", false, true, true, PC00D01.MSGTERR);
                }
                if (!int.TryParse(PC00U01.ReadConfigValue("START_STRING_COUNT", section, configFile), out int startStringCount))
                {
                    //listViewMsg.UpdateMsg($"START_STRING_COUNT setting error.", false, true, true, PC00D01.MSGTERR);
                }
                if (!int.TryParse(PC00U01.ReadConfigValue("STATUS_STRING_COUNT", section, configFile), out int statusStringCount))
                {
                    //listViewMsg.UpdateMsg($"STATUS_STRING_COUNT setting error.", false, true, true, PC00D01.MSGTERR);
                }
                // 20211123 kwc Value자리에 들어오는 문자열 처리를 위해 수정
                if (!int.TryParse(PC00U01.ReadConfigValue("EVENT_STRING_COUNT", section, configFile), out int eventStringCount))
                {
                    //listViewMsg.UpdateMsg($"EVENT_STRING_COUNT setting error.", false, true, true, PC00D01.MSGTERR);
                }
                if (!int.TryParse(PC00U01.ReadConfigValue("KEY_STRING_COUNT ", section, configFile), out int keyStringCount))
                {
                    //listViewMsg.UpdateMsg($"KEY_STRING_COUNT setting error.", false, true, true, PC00D01.MSGTERR);
                }
                if (!int.TryParse(PC00U01.ReadConfigValue("MAXIMUM_LINE_COUNT ", section, configFile), out maximumLineCount))
                {
                    //listViewMsg.UpdateMsg($"MAXIMUM_LINE_COUNT setting error.", false, true, true, PC00D01.MSGTERR);
                }
                if (!int.TryParse(PC00U01.ReadConfigValue("MINIMUM_LINE_COUNT ", section, configFile), out minimumLineCount))
                {
                    //listViewMsg.UpdateMsg($"MINIMUM_LINE_COUNT setting error.", false, true, true, PC00D01.MSGTERR);
                }
                itemLineConf = PC00U01.ReadConfigValue("ITEM_LINE_CONF ", section, configFile);

                for (int nLine = 0; nLine < startStringCount; nLine++)
                {
                    property = "START_STRING" + nLine.ToString("D02");
                    value = PC00U01.ReadConfigValue(property, section, configFile);
                    listViewMsg.UpdateMsg($"{property} : {value}");
                    if (!string.IsNullOrWhiteSpace(value))
                        m_StartStringList.Add(value);
                }
                for (int nLine = 0; nLine < statusStringCount; nLine++)
                {
                    property = "STATUS_STRING" + nLine.ToString("D02");
                    value = PC00U01.ReadConfigValue(property, section, configFile);
                    listViewMsg.UpdateMsg($"{property} : {value}");
                    if (!string.IsNullOrWhiteSpace(value))
                        m_StatusStringList.Add(value);
                }
                // 20211123 kwc Value자리에 들어오는 문자열 처리를 위해 수정
                for (int nLine = 0; nLine < eventStringCount; nLine++)
                {
                    property = "EVENT_STRING" + nLine.ToString("D02");
                    value = PC00U01.ReadConfigValue(property, section, configFile);
                    listViewMsg.UpdateMsg($"{property} : {value}");
                    if (!string.IsNullOrWhiteSpace(value))
                        m_EventStringList.Add(value);
                }
                for (int nLine = 0; nLine < keyStringCount; nLine++)
                {
                    property = "KEY_STRING" + nLine.ToString("D02");
                    value = PC00U01.ReadConfigValue(property, section, configFile);
                    listViewMsg.UpdateMsg($"{property} : {value}");
                    if (!string.IsNullOrWhiteSpace(value))
                        m_KeyStringList.Add(value);
                    property = "LINE_LENGTH" + nLine.ToString("D02");
                    value = PC00U01.ReadConfigValue(property, section, configFile);
                    listViewMsg.UpdateMsg($"{property} : {value}");
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        int.TryParse(value, out int count);
                        m_LineLengthList.Add(count);
                    }
                }
                property = "IGNORE_LINE_STRING";
                value = PC00U01.ReadConfigValue(property, section, configFile);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    listViewMsg.UpdateMsg($"{property} : {value}");
                    m_IgnoreStringList.AddRange(value.Split(','));
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                listViewMsg.UpdateMsg($"Exceptioin - ReadConfig ({ex})", false, true, true, PC00D01.MSGTERR);
            }
        }
        /// <summary>
        /// MA_EQUIPMENT_CD 테이블 CONFIG_INFO 컬럼값 읽기
        /// </summary>
        protected void ReadConfigInfo()
        {
            try
            {
                if (drEquipment == null)
                {
                    return;
                }
                Dictionary<string, string> dicConfigInfo = new Dictionary<string, string>();
                string result = string.Empty;
                string[] arrConfigInfo = drEquipment["CONFIG_INFO"]?.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                int index = -1;
                string infoName = string.Empty;
                foreach (string info in arrConfigInfo)
                {
                    index = info.IndexOf("=");
                    if (index < 0)
                    {
                        continue;
                    }
                    dicConfigInfo.TryAdd(info.Substring(0, index).Trim(), info.Substring(index + 1).Trim());
                }

                if (!int.TryParse(dicConfigInfo.TryGetValue("MESSAGE_LINE_COUNT"), out m_MessageLineCount))
                {
                    //listViewMsg.UpdateMsg($"MESSAGE_LINE_COUNT setting error.", false, true, true, PC00D01.MSGTERR);
                }
                if (!int.TryParse(dicConfigInfo.TryGetValue("START_STRING_COUNT"), out int startStringCount))
                {
                    //listViewMsg.UpdateMsg($"START_STRING_COUNT setting error.", false, true, true, PC00D01.MSGTERR);
                }
                if (!int.TryParse(dicConfigInfo.TryGetValue("STATUS_STRING_COUNT"), out int statusStringCount))
                {
                    //listViewMsg.UpdateMsg($"STATUS_STRING_COUNT setting error.", false, true, true, PC00D01.MSGTERR);
                }
                // 20211123 kwc Value자리에 들어오는 문자열 처리를 위해 수정
                if (!int.TryParse(dicConfigInfo.TryGetValue("EVENT_STRING_COUNT"), out int eventStringCount))
                {
                    //listViewMsg.UpdateMsg($"EVENT_STRING_COUNT setting error.", false, true, true, PC00D01.MSGTERR);
                }
                if (!int.TryParse(dicConfigInfo.TryGetValue("KEY_STRING_COUNT"), out int keyStringCount))
                {
                    //listViewMsg.UpdateMsg($"KEY_STRING_COUNT setting error.", false, true, true, PC00D01.MSGTERR);
                }
                if (!int.TryParse(dicConfigInfo.TryGetValue("MAXIMUM_LINE_COUNT"), out maximumLineCount))
                {
                    //listViewMsg.UpdateMsg($"MAXIMUM_LINE_COUNT setting error.", false, true, true, PC00D01.MSGTERR);
                }
                if (!int.TryParse(dicConfigInfo.TryGetValue("MINIMUM_LINE_COUNT"), out minimumLineCount))
                {
                    //listViewMsg.UpdateMsg($"MINIMUM_LINE_COUNT setting error.", false, true, true, PC00D01.MSGTERR);
                }
                itemLineConf = dicConfigInfo.TryGetValue("ITEM_LINE_CONF");

                string property, value;
                for (int nLine = 0; nLine < startStringCount; nLine++)
                {
                    property = "START_STRING" + nLine.ToString("D02");
                    value = dicConfigInfo.TryGetValue(property);
                    listViewMsg.UpdateMsg($"{property} : {value}");
                    if (!string.IsNullOrWhiteSpace(value))
                        m_StartStringList.Add(value);
                }
                for (int nLine = 0; nLine < statusStringCount; nLine++)
                {
                    property = "STATUS_STRING" + nLine.ToString("D02");
                    value = dicConfigInfo.TryGetValue(property);
                    listViewMsg.UpdateMsg($"{property} : {value}");
                    if (!string.IsNullOrWhiteSpace(value))
                        m_StatusStringList.Add(value);
                }
                // 20211123 kwc Value자리에 들어오는 문자열 처리를 위해 수정
                for (int nLine = 0; nLine < eventStringCount; nLine++)
                {
                    property = "EVENT_STRING" + nLine.ToString("D02");
                    value = dicConfigInfo.TryGetValue(property);
                    listViewMsg.UpdateMsg($"{property} : {value}");
                    if (!string.IsNullOrWhiteSpace(value))
                        m_EventStringList.Add(value);
                }
                for (int nLine = 0; nLine < keyStringCount; nLine++)
                {
                    property = "KEY_STRING" + nLine.ToString("D02");
                    value = dicConfigInfo.TryGetValue(property);
                    listViewMsg.UpdateMsg($"{property} : {value}");
                    if (!string.IsNullOrWhiteSpace(value))
                        m_KeyStringList.Add(value);
                    property = "LINE_LENGTH" + nLine.ToString("D02");
                    value = dicConfigInfo.TryGetValue(property);
                    listViewMsg.UpdateMsg($"{property} : {value}");
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        int.TryParse(value, out int count);
                        m_LineLengthList.Add(count);
                    }
                }
                property = "IGNORE_LINE_STRING";
                value = dicConfigInfo.TryGetValue(property);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    listViewMsg.UpdateMsg($"{property} : {value}");
                    m_IgnoreStringList.AddRange(value.Split(','));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                listViewMsg.UpdateMsg($"Exceptioin - ReadConfigInfo ({ex})", false, true, true, PC00D01.MSGTERR);
            }
        }


        protected bool IsConnected { get; set; } = false;
        //protected bool IsConnected
        //{
        //    get
        //    {
        //        if (state == null) return false;
        //        if (state.workSocket == null) return false;
        //        return state.workSocket.Connected;
        //    }
        //}
        protected void Connect()
        {
            try
            {
                if (IsConnected) Disconnect();

                ///
                //state = new StateObject();
                state.sb.Clear();
                state.workSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //state.workSocket.Connect(new IPEndPoint(IPAddress.Parse(m_Server), m_conPort));

                connectDone.Reset();
                state.workSocket.BeginConnect(new IPEndPoint(IPAddress.Parse(IpAddress), nPortNo), new AsyncCallback(ConnectCallback), state);
                connectDone.WaitOne(10000);
                state.workSocket.Blocking = false;
                //if (!state.workSocket.Connected)
                if (!IsConnected)
                {
                    Debug.WriteLine("Not Connected!");
                    //state.workSocket.Shutdown(SocketShutdown.Both); //접속되지 않았으므로 호출시 Exception 발생함.
                    state.workSocket.Close();  // Exception 발생하지 않음.
                    m_nActive = (m_nActive + 1) % 2;
                }
                else
                {
                    Debug.WriteLine("Connected!");

                    ///
                    // Get the size of the uint to use to back the byte array
                    int size = Marshal.SizeOf((uint)0);

                    // Create the byte array
                    byte[] keepAlive = new byte[size * 3];

                    // Pack the byte array:
                    // Turn keepalive on
                    Buffer.BlockCopy(BitConverter.GetBytes((uint)1), 0, keepAlive, 0, size);
                    // Set amount of time without activity before sending a keepalive to 5 seconds
                    Buffer.BlockCopy(BitConverter.GetBytes((uint)1500), 0, keepAlive, size, size);
                    // Set keepalive interval to 5 seconds
                    Buffer.BlockCopy(BitConverter.GetBytes((uint)500), 0, keepAlive, size * 2, size);

                    // Set the keep-alive settings on the underlying Socket
                    state.workSocket.IOControl(IOControlCode.KeepAliveValues, keepAlive, null);

                    state.workSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, SocketFlags.None,
                                                    new AsyncCallback(ReadCallback), state);
                    listViewMsg.UpdateMsg($"state.workSocket.BeginReceive - Connect", false, true, true, PC00D01.MSGTINF);
                }
            }
            catch (SocketException se)
            {
                Debug.WriteLine(se.ToString());
                listViewMsg.UpdateMsg($"SocketException in Connect - ({se})", false, true, true, PC00D01.MSGTERR);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                listViewMsg.UpdateMsg($"Exception in Connect - ({ex})", false, true, true, PC00D01.MSGTERR);
            }
            finally { }
        }

        protected void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                if (state.workSocket == null)
                {
                    IsConnected = false;
                    listViewMsg.UpdateMsg($"state.workSocket == null in ConnectCallback", false, true, true, PC00D01.MSGTINF);
                    connectDone.Set();
                    return;
                }
                if (!state.workSocket.Connected)
                {
                    IsConnected = false;
                    listViewMsg.UpdateMsg($"state.workSocket.Connected = FALSE in ConnectCallback", false, true, true, PC00D01.MSGTINF);
                    connectDone.Set();
                    return;
                }
                state.workSocket?.EndConnect(ar);
                IsConnected = true;
                listViewMsg.UpdateMsg($"state.workSocket.Connected = TRUE in ConnectCallback ", false, true, true, PC00D01.MSGTINF);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString(), "ConnectCallback");
                listViewMsg.UpdateMsg($"Exception in ConnectCallback - ({ex})", false, true, true, PC00D01.MSGTERR);
                IsConnected = false;
            }
            finally
            {
                connectDone.Set();
            }
        }
        protected void Disconnect()
        {
            try
            {
                if (state != null && state.workSocket != null)
                {
                    if (state.workSocket.Connected)
                    {
                        state.workSocket.Shutdown(SocketShutdown.Both);
                        state.workSocket.Close();
                        UpdateEquipmentProgDateTime(IF_STATUS.Disconnected);
                        listViewMsg.UpdateMsg($"Socket Close in Disconnect", false, true, true, PC00D01.MSGTINF);

                    }
                }
            }
            catch (SocketException se)
            {
                Debug.WriteLine(se.ToString());
                listViewMsg.UpdateMsg($"SocketException in Disconnect - ({se})", false, true, true, PC00D01.MSGTERR);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                listViewMsg.UpdateMsg($"Exception in Disconnect - ({ex})", false, true, true, PC00D01.MSGTERR);
            }
            finally
            {
                IsConnected = false;
            }
        }

        protected void ReadCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                SocketError errorCode;

                if (state.workSocket == null)
                {
                    IsConnected = false;
                    listViewMsg.UpdateMsg($"state.workSocket == null in ReadCallback", false, true, true, PC00D01.MSGTINF);
                    return;
                }
                if (state.workSocket.Connected == false)
                {
                    IsConnected = false;
                    listViewMsg.UpdateMsg($"state.workSocket.Connected={state.workSocket.Connected} in ReadCallback", false, true, true, PC00D01.MSGTINF);
                    //state.workSocket.Shutdown(SocketShutdown.Both);
                    state.workSocket.Close();
                    return;
                }

                int bytesRead = state.workSocket.EndReceive(ar, out errorCode);
                //int bytesRead = state.workSocket.EndReceive(ar);
                if (errorCode != SocketError.Success)
                {
                    IsConnected = false;
                    listViewMsg.UpdateMsg($" EndReceive Socket Error {errorCode} in ReadCallback", false, true, true, PC00D01.MSGTINF);
                    state.workSocket.Shutdown(SocketShutdown.Both);
                    state.workSocket.Close();
                    return;
                }
                state.dtLastReceived = DateTime.Now;
                if (bytesRead > 0)
                {
                    // 20210419, SHS, Encoding 을 ASCII -> UTF8 로 변경
                    //fileLog.WriteData(Encoding.ASCII.GetString(state.buffer, 0, bytesRead), "RecvData", "ReadCallback");
                    //state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                    //string receivedString = Encoding.UTF8.GetString(state.buffer, 0, bytesRead);
                    // 20210421, SHS, CR LF LF 형태를 CR LF 로 Replace 처리. CR LF LF 형태의 데이터는 CR LF 로 Split 후 LF 가 남아 있어 로그에 빈줄이 보여짐. 실제 처리시 에는 CR LF 로 Split 하기 때문에 문제 안됨
                    //string receivedString = dataEncoding.GetString(state.buffer, 0, bytesRead).Replace("\r\n\r", Environment.NewLine);
                    // 20210423, SHS, OM 에서 문제 발생(CRLFCRLF 를 CRLFLF 로 변경되는문제) , lf 는 안지워도 상관없어서 복원
                    string receivedString = dataEncoding.GetString(state.buffer, 0, bytesRead);

                    fileLog.WriteData(BitConverter.ToString(state.buffer, 0, bytesRead), "RecvData", $"ReadCallback BYTE({bytesRead})");
                    fileLog.WriteData(receivedString, "RecvData", "ReadCallback");
                    state.sb.Append(receivedString);
                    ParseMessage(state.sb.ToString());

                    // Not all data received. Get more.  
                    state.workSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize,
                            SocketFlags.None, new AsyncCallback(ReadCallback), state);
                    listViewMsg.UpdateMsg($"state.workSocket.BeginReceive - ReadCallback ", false, true, true, PC00D01.MSGTINF);
                }
                else
                {
                    //listViewMsg.UpdateMsg($"Server Disconnected.", false, true, true, PC00D01.MSGTERR);
                    state.workSocket.Shutdown(SocketShutdown.Both);
                    state.workSocket.Close();
                    IsConnected = false;
                    UpdateEquipmentProgDateTime(IF_STATUS.Disconnected);
                    listViewMsg.UpdateMsg($"Socket Close in ReadCallback", false, true, true, PC00D01.MSGTINF);
                }
            }
            catch (Exception ex)
            {
                UpdateEquipmentProgDateTime(IF_STATUS.InternalError);
                Debug.WriteLine(ex.ToString());
                state.workSocket.Shutdown(SocketShutdown.Both);
                state.workSocket.Close();
                IsConnected = false;
                listViewMsg.UpdateMsg($"Exception in ReadCallback - ({ex})", false, true, true, PC00D01.MSGTERR);
            }
            finally
            {
            }
        }

        protected virtual void ParseMessage(string Msg)
        { }

        protected bool CheckStartString(string data)
        {
            // 20230111, SHS, data 가 null 체크 안되고 호출될 경우 #TIME 에 대해 00:00:00 로 변환되면서 true 리턴되는 문제
            if (string.IsNullOrWhiteSpace(data))
            {
                return false;
            }
            DateTime dt;
            foreach (string start in m_StartStringList)
            {
                switch (start)
                {
                    case "#TIME":
                        //if (PC00U01.TryParseExact($"{DateTime.Now.ToString("yyyy-MM-dd")} {data}", out dt))
                        if (DateTime.TryParse($"{DateTime.Now.ToString("yyyy-MM-dd")} {data}", out dt))
                            return true;
                        break;
                    case "#DATETIME":
                        if (PC00U01.TryParseExact(data, out dt))
                            return true;
                        break;
                    case "<ESC>":
                        if (data.StartsWith("\u001b"))
                            return true;
                        break;
                    default:
                        break;
                }
            }
            return false;
        }
    }
}
