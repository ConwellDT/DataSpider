using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIMM
{
    public class Data
    {
        public int Line { get; private set; } = -1;
        public int Offset { get; private set; } = -1;
        public int Size { get; private set; } = -1;
        public string Value { get; private set; } = string.Empty;
        public bool Available
        {
            get { return Line > 0 && Offset > 0 && Size > 0; }
        }
        public string DataPosition
        {
            get { return $"{Line},{Offset},{Size}"; }
        }
        public bool UpdateDataPosition(string posInfo)
        {
            if (string.IsNullOrWhiteSpace(posInfo))
                return false;

            string[] info = posInfo.Split(',');

            if (!CheckPosInfo(info))
                return false;

            Line = int.Parse(info[0]);
            Offset = int.Parse(info[1]);
            Size = int.Parse(info[2]);

            return true;
        }
        private bool CheckPosInfo(string[] posInfo)
        {
            if (posInfo.Length < 3)
                return false;

            for (int i = 0; i < posInfo.Length; i++)
            {
                if (!int.TryParse(posInfo[i], out int result))
                    return false;
                if (result < 1)
                    return false;
            }
            return true;
        }
        public bool DataProcess(string[] data)
        {
            Value = string.Empty;

            if (!Available)
                return false;

            if (data.Length < Line)
                return false;

            string line = data[Line - 1];
            if (string.IsNullOrWhiteSpace(line))
                return false;
            if (line.Length < Offset)
                return false;

            Value = line.Substring(Offset - 1, Size).Trim();
            return true;
        }        
    }

    public class TAG
    {
        private Data tagValue = new Data();
        private Data dateValue = new Data();
        private Data timeValue = new Data();
        public string DateTime
        {
            get { return dateValue.Available && timeValue.Available ? System.DateTime.Parse($"{dateValue.Value} {timeValue.Value}").ToString("yyyy-MM-dd HH:mm:ss") : System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); }
        }
        public string Value
        {
            get { return tagValue.Value; }
        }
        public string TTV
        {
            get { return $"{TagName},{DateTime},{Value}"; }
        }
        public bool IsValueUpdated { get; set; }

        public string TagName { get; set; }
        public string EquipName { get; set; }
        public string DataType { get; set; }
        public string PiTagName { get; set; }
        public string OpcTagName { get; set; }
        public string DataPosition 
        {
            get { return tagValue.DataPosition; }
        }
        public bool UpdateDataPosition(string posInfo)
        {
            return tagValue.UpdateDataPosition(posInfo);
        }
        public string DatePosition
        {
            get { return dateValue.DataPosition; }
        }
        public bool UpdateDatePosition(string posInfo)
        {
            return dateValue.UpdateDataPosition(posInfo);
        }
        public string TimePosition
        {
            get { return timeValue.DataPosition; }
        }
        public bool UpdateTimePosition(string posInfo)
        {
            return timeValue.UpdateDataPosition(posInfo);
        }

        public TAG(string tagName, string equipName, string dataPosition, string datePosition, string timePosition)
        {
            TagName = tagName;
            EquipName = equipName;
            UpdateDataPosition(dataPosition);
            UpdateDatePosition(datePosition);
            UpdateTimePosition(timePosition);
        }        

        public bool DataProcess(string[] data)
        {
            IsValueUpdated = false;

            if (!tagValue.Available)
                return false;

            if (!tagValue.DataProcess(data))
                return false;

            if (dateValue.Available && timeValue.Available)
            {
                if (!dateValue.DataProcess(data))
                    return false;
                if (!timeValue.DataProcess(data))
                    return false;
            }
            Debug.WriteLine(TTV);

            IsValueUpdated = true;
            return true;
        }
    }

    public class EquipmentDataProcess
    {
        public string FilePath { get; private set; } = $@"{Environment.CurrentDirectory}\Data";
        public string EquipName { get; private set; }
        public Dictionary<string, List<TAG>> DicTAGList { get; private set; } = new Dictionary<string, List<TAG>>();
        //public List<TAG> ListTAG { get; private set; } = new List<TAG>();

        public EquipmentDataProcess()
        {
        }

        public bool Add(string tagName, string equipName, string dataPosition, string datePosition, string timePosition)
        {
            if (!DicTAGList.ContainsKey(equipName))
            {
                DicTAGList.Add(equipName, new List<TAG>());
            }
            DicTAGList[equipName].Add(new TAG(tagName, equipName, dataPosition, datePosition, timePosition));

            return true;
        }
        public bool Add(DataRow[] drEquipCD)
        {
            foreach (DataRow dr in drEquipCD)
            {
                Add(dr["tag_nm"].ToString(), dr["equip_nm"].ToString(), dr["data_position"].ToString(), dr["date_position"].ToString(), dr["time_position"].ToString());
            }
            return true;
        }

        public bool DataProcess(string equipName, string[] data)
        {
            if (!DicTAGList.TryGetValue(equipName, out List<TAG> listTAG))
                return false;

            foreach (TAG tag in listTAG)
            {
                tag.DataProcess(data);
            }

            string fullFileName;
            while (true)
            {
                fullFileName = $@"{FilePath}_{DateTime.Now:yyyyMMddHHmmssfff}.tdv";
                if (File.Exists(fullFileName))
                    continue;
                File.AppendAllLines(fullFileName, listTAG.FindAll(x => x.IsValueUpdated).Select(x => x.TTV));
                break;
            }   

            return true;
        }
    }
}
