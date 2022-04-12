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

namespace DataSpider.PC03.PT
{

    public abstract class PC03B01
    {
        protected string m_Name;
        protected string m_Description;
        protected string m_ConnectionInfo;
        protected string m_Use;     // 0 /1 

        protected Logging m_clsLog = new Logging();

        public bool bTerminal = false;
        protected PC03F01 mOwner = null;
        protected PC00Z01 m_sqlBiz = new PC00Z01();

        protected string m_strDeviceID = string.Empty;
        protected string m_strDeviceNm = string.Empty;
        protected string m_strStationCd = string.Empty;
        protected string m_strGetPath = string.Empty;
        protected string m_strSetPath = string.Empty;
        protected string m_strEType = string.Empty;
        protected string m_strEName = string.Empty;
        protected int m_nCurNo = 0;
        protected int m_nImageKeepDays = 0;

        // 20200929, DQK
        protected string m_strPlantCode = "";
        protected string m_strLineCode = "";
        protected int m_nImageBackupDays = 0;
        protected string m_strBackupPath;
        protected DateTime m_dtLastBackup = default(DateTime);
        protected string m_strVisionFileFormat = "";
        protected string m_strPgmPara = "";

        public Thread m_Thd = null;
        public Thread m_ThdChkPI = null;
        protected bool DeleteFile = false;

        protected string m_strDataFilePath = "";

        public PC03B01()
        {

        }

        public PC03B01(PC03F01 pOwner, string strPlantCode, string strDeviceId, string strDeviceNm, string strStationCd, string strGetPath, string strSetPath, string strBackupPath, string strProcId, int nCurNo, bool bAutoRun, PIInfo m_clsPIInfo)
        {
            mOwner = pOwner;
            m_strDeviceID = strDeviceId;
            m_strDeviceNm = strDeviceNm;
            m_strStationCd = strStationCd;
            m_strGetPath = strGetPath;
            m_strSetPath = strSetPath;
            //m_strProcID = strProcId;
            m_nCurNo = nCurNo;
            m_nImageKeepDays = 0;

            m_strPlantCode = strPlantCode;
            m_nImageBackupDays = 0;
            m_strBackupPath = strBackupPath;
            m_strVisionFileFormat = "";
            m_strPgmPara = "";

            if (bAutoRun == true)
            {
                m_Thd = new Thread(ThreadJob);
                m_Thd.Start();
            }
        }

        public PC03B01(PC03F01 pOwner, string strEquipType, string strEquipName, int nCurNo, bool bAutoRun, PIInfo m_clsPIInfo)
        {
            mOwner = pOwner;
            m_strEType = strEquipType;
            m_strEName = strEquipName;
            m_nCurNo = nCurNo;

            if (bAutoRun == true)
            {
                m_Thd = new Thread(ThreadJob);
                m_Thd.Start();
            }

        }


        protected abstract void ThreadJob();
    }

}
