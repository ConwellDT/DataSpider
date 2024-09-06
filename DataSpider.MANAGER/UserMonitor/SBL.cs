using LibraryWH.ControlElement;
using DataSpider.PC00.PT;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataSpider.UserMonitor
{
    enum TYPE { OSMO, IND570 };

    public class SBLEventArgs : EventArgs
    {
        public IF_STATUS State { get; set; }
        public EqType EquipType { get; set; }
        public DateTime Time { get; set; }
    }

    public class SBL
    {
        public enum EqState { ON, OFF, ERR, ETC };
        //        protected IF_STATUS m_nState = IF_STATUS.Stop;
        protected IF_STATUS m_nState = IF_STATUS.Normal;
        protected DataRow m_pRow;


        public delegate void OnChangeStateHandler(SBL sender, EventArgs e);
        public event OnChangeStateHandler OnChangeStateEvent = null;


        public virtual IF_STATUS State { get { return m_nState; } set { m_nState = value; } }
        protected void OnChangeStateEventFn()
        {
            if (OnChangeStateEvent != null)
            {
                var ev = new SBLEventArgs();
                ev.State = State;
                OnChangeStateEvent(this, ev);
            }
        }



        string sName;
        string sDescription;
        string m_sImageName;
        string sZoneType;

        public virtual string Image 
        { 
            get { return m_sImageName; }
            set
            {
                m_sImageName = value;
            }
        }
             
        public virtual string Name
        {
            get
            {
                return sName;
            }
            set
            {
                sName = value;
            }
        }

        public virtual string Description
        {
            get
            {
                return sDescription;
            }
            set
            {
                sDescription = value;
            }
        }

        public virtual string ZoneType
        {
            get
            {
                return sZoneType;
            }
            set
            {
                sZoneType = value;
            }
        }
        //public virtual string Status { get; set; } = string.Empty;

        // 20210503, SHS, DataRow 필드명 값 리턴 함수
        public string GetData(string fieldName)
        {
            return m_pRow?[fieldName]?.ToString();
        }
    }

    public class EqType : SBL
    {

        //DataRow m_pRow;
        public List<Eq> EqLists
        { get; set; }
        public EqType()
        {
            EqLists = new List<Eq>();

        }
        public EqType(DataRow pRow)
        {
            m_pRow = pRow;
        }
        public override string Name
        {
            get
            {
                if (m_pRow == null)
                    return "NULL";
                return $"{m_pRow["EQUIPTYPE_NM"]}";// ({m_pRow["CODE_VALUE"]})";
            }
        }
        public override string Description 
        {
            get
            {
                if (m_pRow == null)
                    return "NULL";
                return $"{m_pRow["EQUIPTYPE_DESC"]}";
            }

        }
        public string TypeCode
        {
            get
            {
                return m_pRow["EQUIPTYPE_CD"].ToString().Trim();
            }

        }

        public override string ZoneType
        {
            get
            {
                return m_pRow["ZONE_TYPE"].ToString();
            }

        }
        public override IF_STATUS State
        {
            get { return m_nState; }
            set
            {
                if (m_nState != value)
                {
                    m_nState = value;
                    OnChangeStateEventFn();
                }
            }
        }

        //public override string Status
        //{
        //    get 
        //    {
        //        return EqLists.Max(e => e.Status);
        //    }
        //}
    }


    public class Eq : SBL
    {
        private EqType m_pType;
        //private DataSpiderDataSet.MA_EQUIPMENT_CDRow m_pRow;
        //private DataRow m_pRow;
 
        public Eq()
        { }
        public Eq(DataRow pRow, EqType pType=null)
        {
            m_pRow = pRow;
            m_pType = pType;
            if (m_pRow != null)
            {
                int.TryParse(m_pRow["PROG_STATUS"].ToString(), out int status);
                State = (IF_STATUS)status;
            }
        }

        public EqType EquipType
        {
            get
            {  
                return m_pType;
            }
            set
            {
                m_pType = value;
            }

        }
        public override string Name
        {
            get
            {
                if (m_pRow == null)
                    return "NULL";
                return m_pRow["EQUIP_NM"].ToString();
            }

        }

        public override string Image
        {
            get
            {
                if (EquipType != null)
                    return $"{EquipType.Name}";
                return "Undifinded";
            }

        }
        public override string Description
        {
            get
            {
                if (m_pRow == null)
                    return string.Empty;
                return m_pRow["EQUIP_DESC"].ToString();
            }
        }
        //public string Desc
        //{
        //    get
        //    {
        //        if (m_pRow == null)
        //            return "DescTest";
        //        return m_pRow["EQUIP_DESC"].ToString();
        //    }

        //}

       
        public string Type
        {
            get
            {
                
                if ( m_pType != null)
                    return m_pType.TypeCode;               
                if (m_pRow != null)
                    return m_pRow["EQUIP_TYPE"].ToString().Trim(); 
                return "NULL EQUIP_TYPE";
            }

        }
        public override IF_STATUS State
        {
            get { return m_nState; }
            set
            {
                if (m_nState != value)
                {
                    m_nState = value;
                    OnChangeStateEventFn();
                }
            }
        }

        public override string ZoneType
        {
            get
            {

                if (m_pType != null)
                    return m_pType.ZoneType;
                if (m_pRow != null)
                    return m_pRow["ZONE_TYPE"].ToString().Trim();
                return "NULL ZONE_TYPE";
            }

        }

        //public override string Status
        //{
        //    get
        //    {
        //        return m_pRow?["PROG_STATUS"].ToString().Trim() ?? "0";
        //    }
        //}
    }

    public class Zone : SBL
    {

        //DataRow m_pRow;
        public List<EqType> EqTypeLists
        { get; set; }
        
        public Zone()
        {
            EqTypeLists = new List<EqType>();
        }
        public Zone(DataRow pRow)
        {
            m_pRow = pRow;
        }
        public override string Name
        {
            get
            {
                if (m_pRow == null)
                    return "NULL";
                return $"{m_pRow["CODE_NM"]}";
            }
        }
        public override string Description
        {
            get
            {
                if (m_pRow == null)
                    return "NULL";
                return $"{m_pRow["CODE_VALUE"]}";
            }

        }
        public string TypeCode
        {
            get
            {
                return m_pRow["CODE"].ToString();
            }

        }
        public override IF_STATUS State
        {
            get { return m_nState; }
            set
            {
                if (m_nState != value)
                {
                    m_nState = value;
                    OnChangeStateEventFn();
                }
            }
        }
    }

}

