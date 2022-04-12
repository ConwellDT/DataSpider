using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SEIMM
{
    public class EquipmentClass
    {
        DateTime dtLastWriteTime = DateTime.MinValue;

        String m_Name;
        String m_Description;
        String m_ConnectionInfo;
        String m_Use;     // 0 /1 


        Thread m_EqThread = null;
        private bool m_bEqThreadRun = true;


        public EquipmentClass(String Name, String Description, String ConnectionInfo, String Use)
        {
            m_Name = Name;
            m_Description = Description;
            m_ConnectionInfo = ConnectionInfo;
            m_Use = Use;
            CreateThreads();
        }

        void SetUse(String Use)
        {
            m_Use = Use;
        }
        public void CreateThreads()
        {
            m_EqThread = new Thread(EqThread);
            m_EqThread.IsBackground = true;
            m_EqThread.Start((object)this);

        }
        public void EqThread(object Param)
        {
            while (m_bEqThreadRun)
            {
                try
                {
                    string data = FileProcess();
                    if (!string.IsNullOrWhiteSpace(data))
                    {
                        EnQueue(data);
                    }
                }
                catch (Exception ex)
                {
                    
                }
                Thread.Sleep(10);
            }
        }

        public void EnQueue(String Data)
        {
            GlobalClass.QueueMsg msg = new GlobalClass.QueueMsg();
            msg.m_EqName = m_Name;
            msg.m_Data = Data;
            GlobalClass.WriteQueue(msg);
        }


        private string FileProcess()
        {
            List<FileInfo> listFileInfo = new List<FileInfo>();

            DirectoryInfo di = new DirectoryInfo(m_ConnectionInfo);
            FileInfo[] fileInfo = di.GetFiles();

            foreach (FileInfo fi in fileInfo)
            {
                if (dtLastWriteTime.CompareTo(fi.LastWriteTime) < 0)
                {
                    listFileInfo.Add(fi);
                }
            }
            
            listFileInfo.Sort((x, y) => x.LastWriteTime.CompareTo(y.LastWriteTime));
            if (listFileInfo.Count > 0)
            {
                using (FileStream fs = listFileInfo[0].Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    if (fs != null)
                    {
                        StringBuilder sbData = new StringBuilder();
                        byte[] b = new byte[1024];
                        UTF8Encoding temp = new UTF8Encoding(true);
                        int count;
                        while ( (count = fs.Read(b, 0, b.Length)) > 0)
                        {
                            sbData.Append(temp.GetString(b, 0, count));
;                       }
                        dtLastWriteTime = listFileInfo[0].LastWriteTime;
                        Debug.WriteLine(dtLastWriteTime.ToString());
                        return sbData.ToString();
                    }
                }
            }
            return string.Empty;
            //foreach (FileInfo fi in listFileInfo)
            //{
            //    File.Copy(fi.FullName, $@"{GlobalClass.DatatPath}\{fi.Name}");
            //    dtLastWriteTime = fi.LastWriteTime;
            //    break;
            //}

        }
    }
}
