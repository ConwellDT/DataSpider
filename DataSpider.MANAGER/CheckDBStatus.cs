using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DataSpider.PC00.PT;
using CFW.Data;
using System.Threading;

namespace DataSpider
{
    class CheckDBStatus
    {
        public delegate void OnDBStatusChangedDelegate(string name, int status);
        public event OnDBStatusChangedDelegate OnDBStatusChanged = null;
        private Thread checkThread = null;
        private bool threadStop = false;
        private int lastDBStatus = 0;

        public string Name { get; set; } = string.Empty;
        public string DBConnectionString { get; set; } = string.Empty;
        public int DBStatus
        {
            get
            {
                return lastDBStatus;
            }
            private set
            {
                //if (!lastDBStatus.Equals(value))
                {
                    OnDBStatusChanged?.Invoke(Name, value);
                    lastDBStatus = value;
                }
            }
        }
        public int Interval { get; set; } = 60 * 1000;
        public CheckDBStatus()
        {
        }
        public CheckDBStatus(string _name, int _interval = 60 * 1000)
        {
            Name = _name;
            Interval = _interval;
        }

        public void Start()
        {
            if (checkThread == null)
            {
                checkThread = new Thread(ThreadJob);
            }
            threadStop = false;
            if (!checkThread.IsAlive)
            {
                checkThread.Start();
            }
        }
        public void Stop()
        {
            threadStop = true;
            int count = 0;
            if (checkThread != null)
            {
                while (checkThread.IsAlive)
                {
                    if (count++ > 100)
                    {
                        checkThread.Abort();
                        break;
                    }
                    Thread.Sleep(10);
                }
            }
            checkThread = null;
        }
        private void ThreadJob()
        {
            while (!threadStop)
            {
                DBStatus = CheckDB() ? 1 : 2;
                Thread.Sleep(Interval);
            }
        }

        private bool CheckDB()
        {
            SqlConnection mCon = null;
            SqlCommand mCommand = null;
            int mRows = 0;
            try
            {
                mCommand = new SqlCommand();
                mCommand.CommandType = CommandType.Text;
                mCommand.CommandText = "SELECT GETDATE()";
                if (string.IsNullOrWhiteSpace(DBConnectionString))
                {
                    return false;
                }
                mCon = new SqlConnection(DBConnectionString);
                mCon.Open();

                mCommand.Connection = mCon;

                mRows = mCommand.ExecuteNonQuery();

                if (mRows == 0)
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                if (mCon != null)
                {
                    if (mCon.State == ConnectionState.Open)
                        mCon.Close();
                    mCon.Dispose();
                }
                if (mCommand != null) mCommand.Dispose();
            }

            return true;
        }
    }
}
