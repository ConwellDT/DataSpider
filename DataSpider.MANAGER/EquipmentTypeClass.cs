using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SEIMM
{
    public class EquipmentTypeClass
    {
        Thread m_EqThread = null;
        private bool m_bEqThreadRun = true;

        private string equipName;
        private EquipmentDataProcess dataProcess = new EquipmentDataProcess();

        public EquipmentTypeClass()
        {
            equipName = "P3S-SC-2840";
            InitEquip();
            CreateThreads();
        }

        private void InitEquip()
        {
            // equip type 이 같은 equip 의 tag 정보를 전달해야 함
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
                    GlobalClass.QueueMsg msg = GlobalClass.ReadQueue();
                    if (msg != null)
                    {

                        Debug.WriteLine(msg.m_EqName);
                        Debug.Print(msg.m_Data);
                        string[] LineData = msg.m_Data.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None);
                        // 라인별 배열에 저장 후 파싱, DB 저장 처리
                        dataProcess.DataProcess(msg.m_EqName, LineData);
                    }

                }
                catch (Exception ex)
                {

                }
                Thread.Sleep(10);
            }
        }


    }
}
