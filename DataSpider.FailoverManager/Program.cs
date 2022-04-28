using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataSpider.FailoverManager
{
    static class Program
    {
        /// <summary>
        /// 해당 애플리케이션의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {

            Process[] localByName = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
            if (localByName.Length == 1)
            {
                if (args.Length == 1)
                {
                    //                    MessageBox.Show(args[0]);
                    int nDelay;
                    int.TryParse(args[0], out nDelay);
                    Thread.Sleep(nDelay * 1000);
                }
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }

        }
    }
}
