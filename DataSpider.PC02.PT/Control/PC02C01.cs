using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DataSpider.PC02.PT.Controls
{
    public class AutoClosingMessageBox
    {
        System.Threading.Timer _timeoutTimer;
        IntPtr mbWnd;

        string _caption = string.Empty;
        const int WM_CLOSE = 0x0010;

        private AutoClosingMessageBox(string text, string caption, MessageBoxButtons btn, MessageBoxIcon icon, int timeout)
        {
            this._caption = caption;
            this._timeoutTimer = new System.Threading.Timer(OnTimerElapsed, null, timeout, 
                                                            System.Threading.Timeout.Infinite);
            MessageBox.Show(text, caption, btn, icon);
        }

        /// <summary>
        /// Open AutoCloseingMessageBox
        /// </summary>
        /// <param name="text">MessageText</param>
        /// <param name="caption">MessageTitle</param>
        /// <param name="btn">MessageBoxButtons</param>
        /// <param name="icon">MessageBoxIcon</param>
        /// <param name="timeout">SuspendTime</param>
        public static void Show(string text, string caption, MessageBoxButtons btn, MessageBoxIcon icon, int timeout)
        {
            new AutoClosingMessageBox(text, caption, btn, icon, timeout);
        }

        void OnTimerElapsed(object state)
        {
            try
            {
                this.mbWnd = FindWindow(null, this._caption);

                if (this.mbWnd != IntPtr.Zero)
                {
                    SendMessage(mbWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                }
            }
            finally
            {
                this._timeoutTimer.Dispose();
            }
        }


        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        //[DllImport("user32.dll")] 
        //[return: MarshalAs(UnmanagedType.Bool)] 
        //internal static extern bool SetWindowPos(int hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

        //[DllImport("user32.dll")]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //internal static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    }
}
