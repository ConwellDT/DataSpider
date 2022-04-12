using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SEIMM
{

    static class GlobalClass
    {
        public static SEIMMDataSet gSEIMMDataSet;

        public static EquipmentTypeClass gEquipmentTypeClass = null;// new EquipmentTypeClass();
        public static List<EquipmentClass> equipmentClassList = new List<EquipmentClass>();

        public static String DatatPath = Application.StartupPath + @"\Data";
        public static String BackUpPath = Application.StartupPath + @"\BackUp";

        public static Queue m_MsgQueue = new Queue();

        public class QueueMsg
        {
            public String m_EqName;
            public String m_Data;
        }

        public static void InitDataBase()
        {
            try
            {
                gSEIMMDataSet = new SEIMMDataSet();
                gSEIMMDataSet.DataSetName = "SEIMM";
                gSEIMMDataSet.EnforceConstraints = false;
                gSEIMMDataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public static void CreateEquipment()
        {
            gEquipmentTypeClass = new EquipmentTypeClass();

            EquipmentClass newEquipment = new EquipmentClass("P3S-SC-2840", "Description", @"\\192.168.20.216\si\01. Project\10. 삼성바이오로직스 (2021)\99. test\Files", "Use");
            
            equipmentClassList.Add(newEquipment);
        }

        public static QueueMsg ReadQueue()
        {
            QueueMsg msg=null;
            lock (m_MsgQueue)
            {
                if(m_MsgQueue.Count > 0 )
                    msg = (QueueMsg)m_MsgQueue.Dequeue();
            }
            return msg;
        }
        public static void WriteQueue(QueueMsg msg)
        {
            lock (m_MsgQueue)
            {
                m_MsgQueue.Enqueue(msg);
            }
        }
    }
}
