using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using LibraryWH;

namespace SEIMM.FileTrans
{

    [XmlRoot(ElementName = "KCC CFG Data", Namespace = "KCC :MixingLines")]
    [Serializable]

    public class CfgData
    {
        public string ConnectionStr { get; set; } = "192.168.20.229";
        public string UserID { get; set; } = "Administrator";
        public string UserPass { get; set; } = "conwell1!";
        public string srcPath { get; set; } = ".\\";
        public string destPath { get; set; } = ".\\";

    }
    public class CFG 
    {
        [XmlIgnore]

        public string FileName = "config.cfg";
        public string FullFilePath = ".\\config.cfg";

        public static CfgData DATA = new CfgData();
         
        private string Serialize<T>(string fileName, T result)
        {
            string sReturn = string.Empty;
            try
            {
                var xmlPath = fileName;
                XmlSerializer xmlSer = new XmlSerializer(typeof(T));
                FileStream fs = new FileStream(xmlPath, FileMode.Create, FileAccess.Write);
                xmlSer.Serialize(fs, result);
                fs.Close();
            }
            catch (Exception ex)
            {
                sReturn = ex.Message;  //
            }
            return sReturn;
        }

        private T Deserial<T>(string fileName, T paramObj) where T : new()
        {
            var result = new T();

            if (File.Exists(fileName) == false)
                this.Serialize<T>(fileName, result);

            XmlSerializer xmlSer = new XmlSerializer(typeof(T));
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None);

            result = (T)xmlSer.Deserialize(fs);

            fs.Close();

            return result;
        }

        public string xmlSaveData( string sFilePath = null)
        {
            string sFile ="";
            string sResult = string.Empty;
            try
            {
                if (sFilePath != null)
                {
                    DirectoryInfo di = new DirectoryInfo(sFilePath);
                    if (di.Exists == false)
                        di.Create();

                    FullFilePath = System.IO.Path.Combine(sFilePath, FileName);
                }
                else
                    FullFilePath = System.IO.Path.Combine(".", FileName);

                 sResult = Serialize(FullFilePath, DATA);

            }
            catch (Exception ex)
            {
                sResult = ex.Message;

                MessageBox.Show(sResult);
            }

            return sResult;
        }

        public void xmlLoadData(string sFilePath = null)
        {
            try
            {

                if (sFilePath != null)
                    FullFilePath = System.IO.Path.Combine(sFilePath, FileName);
         
                else
                    FullFilePath = System.IO.Path.Combine(".", FileName);

 
                DATA = Deserial<CfgData>(FullFilePath, DATA);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
