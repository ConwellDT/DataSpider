using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataSpider
{
    public partial class Form_Splash : Form
    {
        delegate void ProgressDelegate(int i);
        delegate void CloseDelegate();
        private Thread loading = null;
        private bool stop = false;
        
        public Form_Splash()
        {
            InitializeComponent();
        }

        private void Form_Splash_Load(object sender, EventArgs e)
        {
            loading = new Thread(ThreadWork);
            loading.Start();
        }
        private void FormClose()
        {
            this.Close();
        }
        public void Stop()
        {
            stop = true;
            int count = 0;
            while (loading.IsAlive)
            { 
                if (count++ > 10)
                {
                    loading.Abort();
                }
                Thread.Sleep(100);
            }
            this.Invoke(new CloseDelegate(FormClose));
        }
        private void ThreadWork()
        {
            int count = 0;
            while (!stop)
            {
                if (count++ > 300)
                {
                    break;
                }
                Thread.Sleep(100);
            }
        }
    }
}
