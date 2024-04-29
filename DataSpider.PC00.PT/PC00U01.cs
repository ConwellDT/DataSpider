using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Net.NetworkInformation;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Globalization;
using CFW.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using NLog;
using NLog.Targets;
using System.Text.Json;
using System.Management.Automation.Language;
using System.Security.Cryptography;

namespace DataSpider.PC00.PT
{
    public class PC00U01
    {
        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileString(
                string section,
                string key,
                string Def,
                StringBuilder retVal,
                int size,
                string filePath);

        [DllImport("kernel32")]
        static extern int GetPrivateProfileString(string Section, int Key,
               string Value, [MarshalAs(UnmanagedType.LPArray)] byte[] Result,
               int Size, string FileName);
        
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(int Section, string Key,
               string Value, [MarshalAs(UnmanagedType.LPArray)] byte[] Result,
               int Size, string FileName);

        [DllImport("kernel32.dll")]
        private static extern long WritePrivateProfileString(
                                string section,
                                string key,
                                string val,
                                string filePath);

        //extrainfo 예 { TimeFormat : "yyyy-MM-dd HH:mm:ss, dd-MM-yyyy HH:mm:ss" }
        public static void SetEquipmentDateTimeFormat(string extraInfo)
        {
            string result = string.Empty;
            try
            {
                if (!string.IsNullOrWhiteSpace(extraInfo))
                {
                    JsonDocument document = JsonDocument.Parse(extraInfo);
                    result = document.RootElement.GetProperty("TimeFormat").ToString();
                }
            }
            catch (Exception ex)
            {

            }
            EquipmentDateTimeFomatSetting = result;
        }

        public static string[] GetEntryNames(string section, string FileName)   // 해당 section 안의 모든 키 값 가져오기
        {
            for (int maxsize = 500; true; maxsize *= 2)
            {
                byte[] bytes = new byte[maxsize];
                int size = GetPrivateProfileString(section, 0, "", bytes, maxsize, FileName);

                if (size < maxsize - 2)
                {
                    string entries = Encoding.ASCII.GetString(bytes, 0, size - (size > 0 ? 1 : 0));
                    return entries.Split(new char[] { '\0' });
                }
            }
        }

        public static void RemoveSectionFromIniFile(string file, string section)
        {
            using (var reader = File.OpenText(file))
            {
                using (var writer = File.CreateText(file + ".tmp"))
                {
                    var i = false;
                    while (reader.Peek() != -1)
                    {
                        var line = reader.ReadLine();
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            if (line.StartsWith("[") && line.EndsWith("]"))
                            {
                                if (i) i = false;
                                else if (line.Substring(1, line.Length - 2).Trim() == section) i = true;
                            }
                        }
                        if (!i) writer.WriteLine(line);
                    }
                }
            }
            File.Delete(file);
            File.Move(file + ".tmp", file);
        }

        public static string[] GetSectionNames(string ConfigFile)  // ini 파일 안의 모든 section 이름 가져오기
        {
            for (int maxsize = 500; true; maxsize *= 2)
            {
                byte[] bytes = new byte[maxsize];
                int size = GetPrivateProfileString(0, "", "", bytes, maxsize, ConfigFile);


                if (size < maxsize - 2)
                {
                    string Selected = Encoding.ASCII.GetString(bytes, 0, size - (size > 0 ? 1 : 0));
                    return Selected.Split(new char[] { '\0' });
                }
            }
        }

        public static string ReadConfigValue(string name, string Section, string ConfigFile)
        {
            StringBuilder Value = new StringBuilder(260);
            GetPrivateProfileString(Section, name, "", Value, 260, ConfigFile);
            Debug.WriteLine(name + ":" + Value.ToString());
            return Value.ToString();
        }
        public static bool WriteConfigValue(string name, string Section, string ConfigFile, string value)
        {
            WritePrivateProfileString(Section, name, value, ConfigFile);
            Debug.WriteLine(name + ":" + value.ToString());
            return true;
        }


        private static Queue m_MsgQueue = new Queue();

        public static QueueMsg ReadQueue()
        {
            QueueMsg msg = null;
            lock (m_MsgQueue)
            {
                if (m_MsgQueue.Count > 0)
                    msg = (QueueMsg)m_MsgQueue.Dequeue();
            }
            return msg;
        }

        public static void WriteQueue(QueueMsg msg)
        {
            lock (m_MsgQueue)
            {
                m_MsgQueue.Enqueue(msg);
            }
        }

        public static int QueueCount
        {
            get { return m_MsgQueue.Count; }
        }

        private static string[] dateTimeFormats = {
            "M/d/yyyy h:mm:ss tt", "M/d/yyyy h:mm tt",
            "MM/dd/yyyy hh:mm:ss", "M/d/yyyy h:mm:ss",
            "M/d/yyyy hh:mm tt", "M/d/yyyy hh tt",
            "M/d/yyyy h:mm", "M/d/yyyy h:mm",
            "MM/dd/yyyy hh:mm", "M/dd/yyyy hh:mm",

            "yyyy-MM-dd HH:mm:ss", 
            "yyyy M d H m s", "dd/MM/yyyy hh:mm:ss tt", "yyyyMMddHHmmss", "yyyyMMdd HHmmss", 
            //20 Feb 2019 4:28:01 PM
            "d M yyyy h:m:s tt", "dd.MM.yyyy HH:mm", "yyMMdd HHmm",
            "yyyy-MM-dd tt h:mm:ss", "dd/MM/yyyy HH.mm.ss", "dd.MM.yyyy HH:mm:ss", "yyyyMMddHHmmss.fff"
        };
        public static string DateTimeFomatSetting { get; set; } = "dd.MM.yyyy HH:mm:ss, yyyyMMddHHmmss.fff, yyyy M d H m s, ddMMMyy HH:mm, dd-MMM-yy HH:mm";
        public static string EquipmentDateTimeFomatSetting { get; set; } = string.Empty;

        public static bool TryParseExact(string strDateTime, out DateTime dtDateTime)
        {
            if (string.IsNullOrWhiteSpace(EquipmentDateTimeFomatSetting))
            {
                if (DateTime.TryParseExact(strDateTime.Trim(), DateTimeFomatSetting.Split(',').ToArray(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal | DateTimeStyles.AllowInnerWhite, out dtDateTime))
                {
                    return true;
                }
                if (DateTime.TryParse(strDateTime.Trim(), out dtDateTime))
                {
                    return true;
                }
                if (DateTime.TryParse(strDateTime.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dtDateTime))
                {
                    return true;
                }
            }
            else
            {
                if (DateTime.TryParseExact(strDateTime.Trim(), EquipmentDateTimeFomatSetting.Split(',').ToArray(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal | DateTimeStyles.AllowInnerWhite, out dtDateTime))
                {
                    return true;
                }
            }
            return false;
        }

        public static List<string> ParseLengthDataType(string data)
        {
            List<string> listData = new List<string>();
            while (!string.IsNullOrWhiteSpace(data))
            {
                int offset = data.IndexOf(' ');
                int length = int.Parse(data.Substring(0, offset));
                listData.Add($"{data.Substring(offset + 1, length)}");
                data = data.Substring(offset + 1 + length + 1);
            }
            return listData;
        }

        public static bool CheckPosInfo(string posInfo, out string errMessage)
        {
            errMessage = string.Empty;

            if (posInfo.Trim().StartsWith("#"))
            {
                if (posInfo.Trim().Length < 2)
                {
                    errMessage = "Replace TAG name is empty";
                    return false;
                }
                return true;
            }

            string[] info = posInfo.Split(',');
            if (info.Length != 3 && info.Length != 5)
            {
                errMessage = $"Items count must be 3 or 5 - {string.Join(", ", info)}";
                return false;
            }

            for (int i = 0; i < info.Length; i++)
            {
                if (!int.TryParse(info[i], out int result) && !(info.Length == 5 && i == 1))
                {
                    errMessage = $"Format must be integer only - {string.Join(", ", info)}";
                    return false;
                }
                if (result < 1 && !(info.Length == 5 && i == 1))
                {
                    int sizeIndex = info.Length - 1;
                    // 20210426, SHS, 사이즈 -3, -4 인경우 추가  kwc -5추가
                    if (i != sizeIndex || (result != -1 && result != -2 && result != -3 && result != -4 && result != -5))
                    {
                        errMessage = $"Format value must be greater than 1 excluding Delimeter and Size - {string.Join(", ", info)}";
                        return false;
                    }
                }
            }
            return true;
        }

        public static void ExecuteNetUse(string path, string id, string pw)
        {
            // 20240320, SHS, NET USE 실행 전 NET USE DELETE 로 이전 연결 종료, 동일한 사용자가 둘 이상의 사용자 이름으로 서버 또는 ... 오류 발생으로 보완
            Process.Start(new ProcessStartInfo("NET.EXE")
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = $@"USE {path} /DELETE"
            });

            Process.Start(new ProcessStartInfo("NET.EXE")
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = $@"USE {path} {pw} /USER:{id}"
            }); 
        }

        /// <summary>
        /// 문자열 에서 숫자부분만 추출하여 리턴 (값과 단위가 공백없이 붙어있는 경우)
        /// 정수, 실수 모두 처리 가능
        /// 앞에 공백이 있으면 안되고 +, - 기호는 맨앞에만 와야 함
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        public static string GetNumber(string sentence)
        {
            string result = string.Empty;

            for (int i = 0; i < sentence.Length; i++)
            {
                if (i == 0 && (sentence[0].Equals('+') || sentence[0].Equals('-')))
                    continue;
                if (float.TryParse(sentence.Substring(0, i + 1), out float fResult))
                {
                    result = fResult.ToString();
                }
                else
                {
                    break;
                }
            }
            return result;
        }
        /// <summary>
        /// 문자열 에서 숫자부분만 추출하여 리턴 (값과 단위가 공백없이 붙어있는 경우)
        /// 정수, 실수 모두 처리 가능
        /// 앞에 공백이 있으면 안되고 +, - 기호는 맨앞에만 와야 함
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        public static string GetNumberString(string sentence)
        {
            string result = string.Empty;

            for (int i = 0; i < sentence.Length; i++)
            {
                if (i == 0 && (sentence[0].Equals('+') || sentence[0].Equals('-')))
                    continue;
                if (float.TryParse(sentence.Substring(0, i + 1), out float fResult))
                {
                    result = sentence.Substring(0, i + 1);
                }
                else
                {
                    break;
                }
            }
            return result;
        }


        // 미완
        public static string ExtractStringEx(string line, int offset, int size)
        {
            string value = string.Empty;
            if (offset == -1)
            {
                int stringLength = line.Length;
                int absOffset = Math.Abs(offset);
                int startIndex = 0;
                int length = 0;
                // 역순으로 처리
                if (offset < 0)
                {
                    if (size == -1)
                    {
                        startIndex = 0;
                        length = stringLength - absOffset + 1;
                    }

                }
                else
                {
                    startIndex = offset;
                    if (size < 0)
                    {
                        length = 0;
                    }
                    else
                    {
                        length = size;
                    }
                }
                switch (size)
                {
                    // abcd
                    // 끝까지
                    case -1:
                        value = line.Substring(0,  length - absOffset - 1).Trim();
                        break;
                    // 공백이 나올때까지, 공백을 찾아야 하는데 일단 앞뒤 공백제거 후 해야 함
                    case -2:
                        string currString = line.Substring(offset - 1).Trim();
                        int endIndex = currString.IndexOf(' ');
                        value = endIndex == -1 ? currString : currString.Substring(0, endIndex + 1).Trim();
                        break;
                    // 정해진 줄, 정해진 위치부터 숫자만 추출
                    case -3:
                        value = PC00U01.GetNumber(line.Substring(offset - 1).Trim());
                        break;
                    // 정해진 사이즈 길이만큼
                    default:
                        value = line.Substring(offset - 1, line.Length < size ? line.Length : size).Trim();
                        break;
                }
            }
            else
            {
                switch (size)
                {
                    // 끝까지
                    case -1:
                        value = line.Substring(offset - 1).Trim();
                        break;
                    // 공백이 나올때까지, 공백을 찾아야 하는데 일단 앞뒤 공백제거 후 해야 함
                    case -2:
                        string currString = line.Substring(offset - 1).Trim();
                        int endIndex = currString.IndexOf(' ');
                        value = endIndex == -1 ? currString : currString.Substring(0, endIndex + 1).Trim();
                        break;
                    // 정해진 줄, 정해진 위치부터 숫자만 추출
                    case -3:
                        value = PC00U01.GetNumber(line.Substring(offset - 1).Trim());
                        break;
                    // 정해진 사이즈 길이만큼
                    default:
                        value = line.Substring(offset - 1, line.Length < size ? line.Length : size).Trim();
                        break;
                }
            }
            return value;
        }
        public static string ExtractString(string line, int offset, int size)
        {
            string value;
            switch (size)
            {
                // 끝까지
                case -1:
                    value = line.Substring(offset - 1).Trim();
                    break;
                // 공백이 나올때까지, 공백을 찾아야 하는데 일단 앞뒤 공백제거 후 해야 함
                case -2:
                    string currString = line.Substring(offset - 1).Trim();
                    int endIndex = currString.IndexOf(' ');
                    value = endIndex == -1 ? currString : currString.Substring(0, endIndex + 1).Trim();
                    break;
                // 뒤에서 부터 offset 부터 끝까지
                case -3:
                    value = line.Substring(offset > line.Length ? 0 : line.Length - offset).Trim();
                    break;
                // 정해진 줄, 정해진 위치부터 숫자만 추출
                case -4:
                    value = PC00U01.GetNumber(line.Substring(offset - 1).Trim());
                    break;
                // 정해진 줄, 정해진 위치부터 숫자에 해당하는 문자열 추출
                case -5:
                    value = PC00U01.GetNumberString(line.Substring(offset - 1).Trim());
                    break;
                // 정해진 사이즈 길이만큼
                default:
                    value = line.Substring(offset - 1, line.Length < size ? line.Length : size).Trim();
                    break;
            }
            return value;
        }


    }

    public class QueueMsg
    {
        public string m_EqName;
        public int m_MsgType;
        public string m_Data;
    }
    public class FormListViewMsg
    {
        private IPC00F00 ownerForm = null;
        private string processName = string.Empty;
        private int listViewRowNo = 0;
        private FileLog fileLog = null;
        protected Logger _logger = null;

        public FormListViewMsg(IPC00F00 owner, string name, int rowNo, string equipType = "")
        {
            ownerForm = owner;
            processName = name;
            listViewRowNo = rowNo;
            fileLog = new FileLog(!string.IsNullOrWhiteSpace(equipType) ? $"{equipType}_{name}" : name );
            fileLog.SetDbLogger(name);
        }
        public void UpdateMsg(string msg, bool statusView = true, bool logView = true, bool fileWrite = false, string msgType = PC00D01.MSGTINF, [System.Runtime.CompilerServices.CallerMemberName] string callerName = "")
        {
            if (statusView || logView)
            {
                ownerForm.listViewMsg(processName, msg, statusView, listViewRowNo, 5, logView, msgType);
            }
            if (fileWrite)
            {
                fileLog.WriteLog(msg, msgType, callerName);
            }
        }
        public void UpdateStatus(bool status)
        {
            ownerForm.listViewMsg(processName, status ? PC00D01.ON : PC00D01.OFF, true, listViewRowNo, 3, true, PC00D01.MSGTINF);
        }
    }

    public class FileLog
    {
        protected Logging CFWLog = null;
        protected string logFileName = string.Empty;
        protected Logger _logger = null;

        public FileLog(string fileName)
        {
            logFileName = fileName;
            CFWLog = new Logging();
        }
        public void WriteLog(string msg, string msgType = PC00D01.MSGTINF, [System.Runtime.CompilerServices.CallerMemberName] string callerName = "")
        {
            CFWLog.LogToFile("LOG", logFileName, msgType, callerName, msg);
            switch(msgType)
            {
                case PC00D01.MSGTERR: _logger.Error(msg); break;
                case PC00D01.MSGTDBG: _logger.Debug(msg); break;
                case PC00D01.MSGTINF: _logger.Info(msg); break;
                default: _logger.Warn(msg); break;
            }
        }
        public void WriteData(string msg, string msgType, string title)
        {
            CFWLog.LogToFile("DATA", logFileName, msgType, title, msg);
            _logger.Trace(msg);
        }

        public void SetDbLogger(string connectionString, string EquipName)
        {
            DatabaseTarget target = new DatabaseTarget();
            DatabaseParameterInfo param;
            //                target.ConnectionString = "Data Source=192.168.20.229;Initial Catalog=DataSpider;Persist Security Info=True;User ID=SBLADMIN;Password=SBLADMIN#01";
            target.ConnectionString = connectionString;
            target.CommandText = "INSERT INTO [dbo].[LO_SYSTEM] ([TIMESTAMP],[EQUIP_NM],[MACHINE_NM],[LEVEL],[MESSAGE]) VALUES (@timestamp,@logger,@machinename, @level, @message);";
            param = new DatabaseParameterInfo();
            param.Name = "@timestamp";
            param.Layout = "${date}";
            target.Parameters.Add(param);
            param = new DatabaseParameterInfo();
            param.Name = "@machinename";
            param.Layout = "${machinename}";
            target.Parameters.Add(param);
            param = new DatabaseParameterInfo();
            param.Name = "@level";
            param.Layout = "${level}";
            target.Parameters.Add(param);
            param = new DatabaseParameterInfo();
            param.Name = "@logger";
            param.Layout = "${logger}";
            target.Parameters.Add(param);
            param = new DatabaseParameterInfo();
            param.Name = "@message";
            param.Layout = "${message}";
            target.Parameters.Add(param);
            NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);
            _logger = LogManager.GetLogger(EquipName);
        }
        public void SetDbLogger(string EquipName)
        {
            DatabaseTarget target = new DatabaseTarget();
            DatabaseParameterInfo param;
            //                target.ConnectionString = "Data Source=192.168.20.229;Initial Catalog=DataSpider;Persist Security Info=True;User ID=SBLADMIN;Password=SBLADMIN#01";
            target.ConnectionString = CFW.Data.MsSqlDbDef.ConnectionString;
            target.CommandText = "INSERT INTO [dbo].[LO_SYSTEM] ([TIMESTAMP],[EQUIP_NM],[MACHINE_NM],[LEVEL],[MESSAGE]) VALUES (@timestamp,@logger,@machinename, @level, @message);";
            param = new DatabaseParameterInfo();
            param.Name = "@timestamp";
            param.Layout = "${date}";
            target.Parameters.Add(param);
            param = new DatabaseParameterInfo();
            param.Name = "@machinename";
            param.Layout = "${machinename}";
            target.Parameters.Add(param);
            param = new DatabaseParameterInfo();
            param.Name = "@level";
            param.Layout = "${level}";
            target.Parameters.Add(param);
            param = new DatabaseParameterInfo();
            param.Name = "@logger";
            param.Layout = "${logger}";
            target.Parameters.Add(param);
            param = new DatabaseParameterInfo();
            param.Name = "@message";
            param.Layout = "${message}";
            target.Parameters.Add(param);
            NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);
            _logger = LogManager.GetLogger(EquipName);
        }

    }

    public static class Extensions
    {
        /// <summary>
        /// GetFiles Multi-Patterns. DirectoryInfo 의 Extension Method.
        /// *.a | *.b 형태의 다중 searchPattern 처리 구현
        /// </summary>
        /// <param name="di"></param>
        /// <param name="searchPattern"></param>
        /// <param name="searchOption"></param>
        /// <returns></returns>
        public static FileInfo[] GetFilesMP(this DirectoryInfo di, string searchPattern, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            try
            {
                return searchPattern.Split('|').SelectMany(pattern => di.GetFiles(pattern, searchOption)).ToArray();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public static void DoubleBuffered(this DataGridView dgv, bool setting)
        {
            Type dgvType = dgv.GetType();
            PropertyInfo pi = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(dgv, setting, null);
        }
        public static void DoubleBuffered(this ListView dgv, bool setting)
        {
            Type dgvType = dgv.GetType();
            PropertyInfo pi = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(dgv, setting, null);
        }

        /// <summary>
        /// Dictionary ContainsKey 를 수행하여 해당 key 가 없으면 Add 하고 리턴 true, 있으면 리턴 false 하는 확장 메서드
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryAdd<T>(this Dictionary<T, T> dic, T key, T value)
        {
            if (!dic.ContainsKey(key))
            {
                dic.Add(key, value);
                return true;
            }
            return false;
        }

        public static bool TryAdd<T>(this Dictionary<T, List<string>> dic, T key, List<string> value)
        {
            if (!dic.ContainsKey(key))
            {
                dic.Add(key, value);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Dictionary TrayGetValue 를 수행하여 성공하면 그 값을 리턴하고 실패하면 default 값을 리턴하는 확장 메서드
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T TryGetValue<T>(this Dictionary<T, T> dic, T key)
        {
            if (dic.TryGetValue(key, out T value))
            {
                return value;
            }
            return default;
        }
    }

    #region app.config 설정을 가져오는 메서드
    public static class ConfigHelper
    {
        public static string GetAppSetting(string keyString)
        {
            string result = string.Empty;

            var appSettings = ConfigurationManager.AppSettings;
            result = appSettings[keyString] ?? string.Empty;
            return result;
        }

        public static void SetAppSetting(string keyString, string keyValue)
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);

                config.AppSettings.Settings[keyString].Value = keyValue;
                config.Save(ConfigurationSaveMode.Modified);
            }
            catch (Exception ex)
            {

            }
        }

        public static DbInfo GetDbInfo()
        {
            DbInfo m_DbInfo = new DbInfo();
            m_DbInfo.strConStr = CFW.Common.SecurityUtil.DecryptString(GetAppSetting("SQL_ConnectionString"));

            m_DbInfo.strConStr = CFW.Common.SecurityUtil.EncryptString("Data Source = 192.168.20.229,1433; Initial Catalog = DataSpider; User ID = P4TEST; Password = conwell#1;");

            m_DbInfo.strDB_IP = GetAppSetting("DB_IP");

            return m_DbInfo;
        }
        
        public static PgmRegInfo GetPgmRegInfo()
        {
            PgmRegInfo pInfo = new PgmRegInfo();
            pInfo.strEQUIP_TYPE = GetAppSetting("EQUIP_TYPE");
            return pInfo;
        }

        public static PIInfo GetPIInfo()
        {
            PIInfo m_PIInfo = new PIInfo();
            m_PIInfo.strPI_Server = GetAppSetting("PI_SERVER");
            m_PIInfo.strPI_DB =     GetAppSetting("PI_DB");
            m_PIInfo.strPI_USER =   GetAppSetting("PI_USER");
            m_PIInfo.strPI_PWD = GetAppSetting("PI_PWD");

            return m_PIInfo;
        }

    }
    #endregion

    public static class UserAuthentication
    {
        private static PC00Z01 sql = new PC00Z01();
        public static string UserID { get; private set; } = string.Empty;
        public static string UserName { get; private set; } = string.Empty;
        public static UserLevel UserLevel { get; private set; } = UserLevel.UnAuthorized;
        public static string Departmemnt { get; private set; } = string.Empty;
        public static string Description { get; private set; } = string.Empty;

        public static bool IsAuthorized { get; private set; } = false;

        public static bool FreePass()
        {
            UserID = "administrator";
            UserName = "administrator";
            UserLevel = UserLevel.Admin;
            IsAuthorized = true;
            return true;
        }

        public static bool LogIn(string id, string pw)
        {
            IsAuthorized = false;
            if (UserLogIn(id, pw))
            {
                UserID = id;
                UserLevel = UserLevel.Manager;
                IsAuthorized = true;
            }
            return IsAuthorized;
        }
        public static void LogOut()
        {
            UserID = string.Empty;
            UserName = string.Empty;
            UserLevel = UserLevel.UnAuthorized;
            Departmemnt = string.Empty;
            Description = string.Empty;
            IsAuthorized = false;
        }
        public static string Decrypt(string str)
        {
            return SecurityUtil.DecryptString(str);
        }
        public static string Encrypt(string str)
        {
            return SecurityUtil.EncryptString(str);
        }
        private static bool UserLogIn(string id, string pw)
        {
            string strErrCode = string.Empty;
            string strErrText = string.Empty;

            System.Data.DataTable dt = sql.GetUserInfo(id, ref strErrCode, ref strErrText);
            if (dt != null && dt.Rows.Count > 0)
            {                
                if (SecurityUtil.DecryptString(dt.Rows[0]["PASSWORD"].ToString()).Equals(pw))
                {
                    UserID = id;
                    UserName = dt.Rows[0]["USER_NAME"].ToString();
                    int.TryParse(dt.Rows[0]["USER_LEVEL"].ToString(), out int uLevel);
                    UserLevel = (UserLevel)uLevel;
                    Departmemnt = dt.Rows[0]["DEPARTMENT"].ToString();
                    Description = dt.Rows[0]["DESCRIPTION"].ToString();
                    IsAuthorized = true;
                }
            }
            return false;
        }
        public static bool InitializePassword(string userID, ref string strErrText)
        {
            string strErrCode = string.Empty;
            string userPW = CFW.Common.SecurityUtil.EncryptString(userID);
            return sql.InitializeUserPassword(userID, userPW,  ref strErrCode, ref strErrText);
        }
        public static bool InsertUpdateUserInfo(bool insert, string userID, string userPW, string userName, UserLevel userLevel, string department, string description, ref string strErrText)
        {
            string strErrCode = string.Empty;
            string encryptedUserPW = CFW.Common.SecurityUtil.EncryptString(userPW);
            return sql.InsertUpdateUserInfo(insert, userID, encryptedUserPW, userName, userLevel, department, description, ref strErrCode, ref strErrText);
        }


    }

    #region OrderDataLogHelper
    public static class OrderDataLogHelper
    {
        private static object objLock = new object();

        public static bool isCheckedInfo;
        public static bool isCheckedError;
        public static bool isCheckedDebug;

        private static Queue<OrderLogItem> OrderLogItemQueue = new Queue<OrderLogItem>();

        public static void SetValue(OrderLogItem item)
        {
            try
            {
                lock (objLock)
                {
                    if (OrderLogItemQueue.Count >= 1000)
                    {
                        OrderLogItemQueue.Clear();
                        OrderLogItemQueue = new Queue<OrderLogItem>();
                    }

                    if (OrderLogItemQueue.Count <= 1000)
                    {
                        switch (item.strMsgType)
                        {
                            case "INFO":
                                if (!isCheckedInfo)
                                {
                                    return;
                                }
                                break;

                            case "ERROR":
                                if (!isCheckedError)
                                {
                                    return;
                                }
                                break;

                            case "DEBUG":
                                if (!isCheckedDebug)
                                {
                                    return;
                                }
                                break;
                        }

                        OrderLogItemQueue.Enqueue(item);
                    }
                }
            }
            catch (Exception ex) { }

        }

        public static OrderLogItem[] GetValue()
        {
            try
            {
                if (OrderLogItemQueue.Count > 0)
                {
                    int nValCnt = OrderLogItemQueue.Count;

                    OrderLogItem[] OrderLogItemArray = new OrderLogItem[nValCnt];

                    for (int i = 0; i < nValCnt; i++)
                    {
                        OrderLogItemArray[i] = OrderLogItemQueue.Dequeue();
                    }

                    return OrderLogItemArray;
                }
            }
            catch (Exception ex) { }

            return null;
        }

    }
    #endregion

    #region MainThread가 계속적으로 돌기 위한 While문 조건 Get Set
    public static class OrdThreadIsRun
    {
        private static bool _threadRun = false;

        public static bool threadRun
        {
            get
            {
                return _threadRun;
            }

            set
            {
                _threadRun = value;
            }
        }

        private static bool _ThreadStop = true;

        public static bool ThreadStop
        {
            get
            {
                return _ThreadStop;
            }
            set
            {
                _ThreadStop = value;
            }
        }

        private static bool _isRunListViewLogUpdateThread = false;

        public static bool isRunListViewLogUpdateThread
        {
            get
            {
                return _isRunListViewLogUpdateThread;
            }
            set
            {
                _isRunListViewLogUpdateThread = value;
            }
        }
    }
    #endregion

    #region SocketConnStaHelper Socket이 연결 되었는지 확인
    public static class SocketConnStaHelper
    {
        private static object objLock = new object();

        private static Dictionary<DeviceInfo, bool> SocketConnStaDic = new Dictionary<DeviceInfo, bool>();

        public static void SetValue(DeviceInfo devInfo, bool isConnected)
        {
            try
            {
                lock (objLock)
                {
                    if (SocketConnStaDic.ContainsKey(devInfo))
                    {
                        SocketConnStaDic[devInfo] = isConnected;
                    }
                    else
                    {
                        SocketConnStaDic.Add(devInfo, isConnected);
                    }
                }
            }
            catch (Exception ex) { }
        }

        public static bool isConnected(DeviceInfo devinfo)
        {
            if (SocketConnStaDic.ContainsKey(devinfo))
            {
                return SocketConnStaDic[devinfo];
            }
            else
            {
                return false;
            }
        }

    }
    #endregion

    #region Ping Test Helper
    public static class PingHelper
    {
        public static bool PingTest(string IPAddress)
        {
            Ping PingSender = new Ping();
            PingOptions options = new PingOptions();
            options.DontFragment = true;

            string data = "abcdefghijklmnopqr";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 120;
            PingReply reply = PingSender.Send(IPAddress, timeout, buffer, options);

            if (reply.Status == IPStatus.Success)
            {
                return true;
            }

            return false;
        }
    }
    #endregion


    #region LIS3_MESSAGE
    public class LIS3_MESSAGE
    {
        private string message;

        private string identifier;
        private string rTimestamp;
        private string aTimestamp;
        private string checksum;
        //private List<string[]> listGroups = new List<string[]>();
        private Dictionary<string, List<string>> dicGroups = new Dictionary<string, List<string>>();

        public LIS3_MESSAGE()
        {
        }

        public string Identifier
        {
            get { return identifier; }
        }

        public bool Checksum
        {
            get { return (message.Substring(0, message.Length - 3).Sum(x => (byte)x) % 256).ToString("X02").Equals(checksum); }
        }

        //public List<string[]> Groups
        //{
        //    get { return listGroups; }
        //}
        public Dictionary<string, List<string>> Groups
        {
            get { return dicGroups; }
        }

        public byte[] ACKMessage 
        {
            get 
            {
                List<byte> listBytes = new List<byte>();
                listBytes.Add(ASCII.STX);
                listBytes.Add(ASCII.ACK);
                listBytes.Add(ASCII.ETX);
                listBytes.AddRange(Encoding.ASCII.GetBytes("0B"));
                listBytes.Add(ASCII.EOT);

                return listBytes.ToArray();
            }
        }

        public void Parsing(string _message)
        {
            identifier = checksum = rTimestamp = aTimestamp = string.Empty;
            //listGroups.Clear();
            dicGroups.Clear();
            message = _message;

            if (message[1].Equals((char)ASCII.ACK))
            {
                identifier = "<ACK>";
                checksum = message.Substring(3, 2);
            }
            else
            {
                // RS 
                string[] rs = message.Split(new char[] { (char)ASCII.RS }, StringSplitOptions.RemoveEmptyEntries);
                identifier = rs[0].Substring(1, rs[0].Length - 2);
                if (identifier.Equals("ID_REQ"))
                {
                    checksum = rs[1].Substring(1, 2);
                }
                else
                {
                    checksum = rs[2].Substring(1, 2);
                    List<string> listFields = rs[1].Split(new char[] { (char)ASCII.FS }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    foreach (string s in listFields)
                    {
                        List<string> groups = s.Split(new char[] { (char)ASCII.GS }).ToList();      // GS 는 공백이 있어도 Split 처리
                        string[] exceptions = groups[3].Split(new char[] { (char)ASCII.ETB }, StringSplitOptions.RemoveEmptyEntries);
                        groups[3] = string.Join(" ", exceptions);   // Exception 2개까지 나올 수 있음. ETB 로 Split 하고 그 사이는 348EX 와 동일하게 공백으로 구분
                        groups.RemoveAt(4);     // GS 로 Split 하면 GS 로 끝나기 때문에 1개가 더 생김. 5번째 삭제

                        //listGroups.Add(groups.ToArray());

                        dicGroups.TryAdd(groups[0], groups);
                    }
                    //rTimestamp = $"{listGroups.Where(x => x[0].Equals("rDATE")).First()[1]} {listGroups.Where(x => x[0].Equals("rTIME")).First()[1]}";
                    //aTimestamp = $"{listGroups.Where(x => x[0].Equals("aDATE")).First()[1]} {listGroups.Where(x => x[0].Equals("aTIME")).First()[1]}";

                    rTimestamp = dicGroups.ContainsKey("rDATE") && dicGroups.ContainsKey("rTIME") ? $"{dicGroups["rDATE"][1]} {dicGroups["rTIME"][1]}" : string.Empty;
                    aTimestamp = dicGroups.ContainsKey("aDATE") && dicGroups.ContainsKey("aTIME") ? $"{dicGroups["aDATE"][1]} {dicGroups["aTIME"][1]}" : string.Empty;
                }
            }
        }

        public byte[] GetResponseMessage()
        {
            Dictionary<string, string> dicParams = new Dictionary<string, string>();
            string reqIdentifier = string.Empty;

            switch (Identifier)
            {
                // ACK, REQ
                case "ID_REQ":
                    reqIdentifier = "ID_DATA";
                    dicParams.TryAdd("aMOD", "LIS");
                    dicParams.TryAdd("iIID", "DSB");
                    break;

                case "SMP_NEW_AV":
                case "QC_NEW_AV":
                case "CAL_NEW_AV":
                    reqIdentifier = $"{Identifier.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries)[0]}_REQ";
                    dicParams.TryAdd("aMOD", dicGroups["aMOD"][1]);
                    dicParams.TryAdd("iIID", dicGroups["iIID"][1]);
                    dicParams.TryAdd("rSEQ", dicGroups["rSEQ"][1]);
                    break;

                case "SMP_NEW_DATA":
                case "SMP_EDIT_DATA":
                case "QC_NEW_DATA":
                case "CAL_NEW_DATA":
                case "SMP_NOT_AV":
                case "QC_NOT_AV":
                case "CAL_NOT_AV":
                    break;

                default:
                    // SMP_START, SMP_ABORT, QC_START, QC_ABORT, CAL_START, CAL_ABORT
                    // SYSTEM STATUS : SYS_READY, SYS_NOT_READY, SYS_WOPR, SYS_MEASURING, SYS_CAL_PEND, SYS_CAL_REP
                    // TIME_DATA
                    break;
            }
            if (!string.IsNullOrWhiteSpace(reqIdentifier))
            {
                return BuildMessage(reqIdentifier, dicParams);
            }

            return null;
        }

        private byte[] BuildMessage(string reqdentifier, Dictionary<string, string> dicParams)
        {
            List<byte> listBytes = new List<byte>();
            listBytes.Add(ASCII.STX);
            listBytes.AddRange(Encoding.ASCII.GetBytes(reqdentifier));
            listBytes.Add(ASCII.FS);

            listBytes.Add(ASCII.RS);

            foreach (KeyValuePair<string, string> kvp in dicParams)
            {
                listBytes.AddRange(Encoding.ASCII.GetBytes(kvp.Key));
                listBytes.Add(ASCII.GS);
                listBytes.AddRange(Encoding.ASCII.GetBytes(kvp.Value));
                listBytes.Add(ASCII.GS);
                listBytes.Add(ASCII.GS);
                listBytes.Add(ASCII.GS);
                listBytes.Add(ASCII.FS);
            }

            listBytes.Add(ASCII.RS);

            listBytes.Add(ASCII.ETX);
            listBytes.AddRange(Encoding.ASCII.GetBytes(CalcChecksum(listBytes)));
            listBytes.Add(ASCII.EOT);

            return listBytes.ToArray();
        }

        public string GetTTV()
        {
            int customCount = 1;
            string name;
            StringBuilder sb = new StringBuilder();
            string timestamp = string.IsNullOrWhiteSpace(rTimestamp) ? aTimestamp : rTimestamp;

            //foreach (string[] s in listGroups)
            foreach(List<string> s in dicGroups.Values)
            {
                if (s[0].StartsWith("iCUST_"))
                {
                    name = $"Custom_{customCount++:D02}";
                    sb.AppendLine($"{name}_NAME, {timestamp}, {s[0]}");
                }
                else
                {
                    name = $"{s[0]}";
                }
                sb.AppendLine($"{name}_VAL, {timestamp}, {s[1]}");
                sb.AppendLine($"{name}_UNIT, {timestamp}, {s[2]}");
                sb.AppendLine($"{name}_EXCEPTION, {timestamp}, {s[3]}");
            }
            sb.AppendLine($"SVRTIME, {timestamp}, {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            return sb.ToString();
        }

        public override string ToString()
        {
            List<string> result = new List<string>();
            result.Add($"identifier : {identifier}");
            result.Add($"checksum : {Checksum}");
            //foreach (string[] s in listGroups)
            foreach (List<string> s in dicGroups.Values)
            {
                result.Add($"{string.Join(",", s)}");
            }

            return string.Join(" | ", result);
        }
        private string CalcChecksum(List<byte> listBytes)
        {
            int sumByte = listBytes.Sum(x => x) % 256;

            return sumByte.ToString("X02");
        }
    }
    #endregion

}
