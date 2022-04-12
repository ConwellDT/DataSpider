using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Configuration;
using System.Net.NetworkInformation;
using DataSpider.PC00.PT;

namespace DataSpider.PC03.PT
{
    static class Program
    {
        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Mutex mtx;
            string mtxName = string.Empty;
            bool success = false;

            mtxName = $"Global\\{{{"9A5A287F-B8F3-4EBE-ABEE-6840547BCD5B"}}}";//"9A5A287F-B8F3-4EBE-ABEE-6840547BCD5B";// ConfigurationSettings.AppSettings["PGM_ID"];
            mtx = new Mutex(true, mtxName);
            success = mtx.WaitOne(1000);

            if (success)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new PC03F01());

                OrdThreadIsRun.threadRun = false;

                Application.Exit();
            }
            else
            {
                MessageBox.Show(PC00D01.MSGP0033, PC00D01.MSGP0002, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

        }
    }
}
