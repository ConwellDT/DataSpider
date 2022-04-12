using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Configuration;
using System.Net.NetworkInformation;
using DataSpider.PC00.PT;

namespace DataSpider.PC01.PT
{
    static class Program
    {
        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main(string[] args_old)
        {
            string[] args = new string[1];
            args[0] = "OSMO_LIS";
            if (args.Length < 1)
            {
                Application.Exit();
            }
            //mtxName = ConfigurationSettings.AppSettings["PGM_ID"];
            string mtxName = $"Global\\{{{$"5BAD71A7-D4C9-4B4C-A865-12AA88EC5DE7_{args[0]}"}}}";// $"5BAD71A7-D4C9-4B4C-A865-12AA88EC5DE7_{args[0]}";
            Mutex mtx = new Mutex(true, mtxName);
            bool success = mtx.WaitOne(1000);

            if (success)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                Application.Run(new PC01F01(args[0]));

                OrdThreadIsRun.threadRun = false;

                Application.Exit();
            }
            else
            {
                MessageBox.Show(string.Format(PC00D01.MSGP0033, args[0]), PC00D01.MSGP0002, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
            mtx.ReleaseMutex();
        }
    }
}
