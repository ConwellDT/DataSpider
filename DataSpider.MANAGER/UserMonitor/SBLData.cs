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
    #region  Control

    class EquipCtrl : SBL
    {

        public delegate void OnChangeDataHandler(SBL sender, SBLEventArgs e);
        public event OnChangeDataHandler OnChangeDataEvent = null;

        private List<Zone> m_zList = new List<Zone>();
        private List<Eq> m_pList = new List<Eq>();
        private List<EqType> m_pTypeList = new List<EqType>();

        //private DataSpiderDataSetTableAdapters.MA_EQUIPMENT_CDTableAdapter MA_EQUIPMENT_CDTableAdapter;
        //DataSpiderDataSet.MA_EQUIPMENT_CDDataTable m_pTable;

        SqlDependency sd;

        Timer pTimer = new Timer();

        PC00Z01 m_SqlBiz = new PC00Z01();

        public List<Zone> zLists
        {
            get
            {

                return m_zList;
            }
            set
            {
                m_zList = value;
            }
        }

        public List<Eq> Lists
        {
            get
            {
      
                return m_pList;
            }
            set
            {
                m_pList = value;
            }
        }
        public List<EqType> TypeLists
        {
            get
            {
       
                return m_pTypeList;
            }
            set
            {
                m_pTypeList = value;
                OnChangeDataEventFn();
            }
        }


        /// <summary>
        /// InitData, TypeLists 프로터피 갱신시 호출되어 OnChangeDataEvent (트리구성)호출 
        /// </summary>
        protected void OnChangeDataEventFn()
        {
            if (OnChangeDataEvent != null)
            {
                var ev = new SBLEventArgs();
                ev.State = IF_STATUS.Stop;

                OnChangeDataEvent(this, ev);
            }
        }
 

        public EquipCtrl()
        {
            //MA_EQUIPMENT_CDTableAdapter = new DataSpider.DataSpiderDataSetTableAdapters.MA_EQUIPMENT_CDTableAdapter();
            //m_pTable = new DataSpiderDataSet.MA_EQUIPMENT_CDDataTable();

            //SqlDependency.Start(MA_EQUIPMENT_CDTableAdapter.Connection.ConnectionString);

            pTimer.Interval = 10000;
            pTimer.Tick +=  new System.EventHandler(this.timer1_Tick);
            //pTimer.Start();

        }
        private void timer1_Tick(object sender, EventArgs e)
        {
          // OnChangeDataEventFn();
        }

        /// <summary>
        /// 장비정보 업데이트 하여 트리뷰 데이터 조회, 장비 추가/수정/삭제 시 호출
        /// </summary>
        /// <returns></returns>
        public bool InitData(bool status)
        {
            bool bReturn = false;
            try
            {
                GetZoneList();
                GetEquipmentList(status);

                GetEquipmentTypeList();

                foreach (Zone zType in m_zList)
                {
                    zType.EqTypeLists = m_pTypeList.FindAll(element => (element.ZoneType.ToString().Trim() == zType.TypeCode.ToString().Trim()));

                    foreach (EqType pType in zType.EqTypeLists)
                    {
                        pType.EqLists = m_pList.FindAll(p => ((p.Type == pType.TypeCode) && (p.ZoneType.ToString().Trim() == pType.ZoneType.ToString().Trim())));

                        foreach (Eq equipment in pType.EqLists)
                        {
                            equipment.EquipType = pType;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
                bReturn = false;
            }
            finally
            {
                OnChangeDataEventFn();
            }
            return bReturn;
        }

        public bool GetZoneList()
        {
            try
            {
                string strErrCode = string.Empty;
                string strErrText = string.Empty;
                DataTable dtZone = this.m_SqlBiz.GetCommonCode("ZONE_TYPE", "", ref strErrCode, ref strErrText);

                m_zList.Clear();
                if (dtZone == null || dtZone.Rows.Count <= 0)
                {
                    MessageBox.Show("No Zone information exist!! Check DB connection!!");
                    return false;
                }
                else
                {
                    foreach (DataRow dr in dtZone.Rows)
                    {
                        m_zList.Add(new Zone(dr));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }
            return true;
        }

     
        public bool GetEquipmentList(bool status)
        {
            try
            {
                string strErrCode = string.Empty;
                string strErrText = string.Empty;
                
                DataTable dtEquipment = this.m_SqlBiz.GetEquipmentInfo("", "", status, ref strErrCode, ref strErrText);

                m_pList.Clear();
                if (dtEquipment == null || dtEquipment.Rows.Count <= 0)
                {
                    MessageBox.Show("No equipment information exist!! Check DB connection!!");
                    return false;
                }
                else
                {
                    foreach (DataRow dr in dtEquipment.Rows)
                    {
                        m_pList.Add(new Eq(dr));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }
            return true;
        }

        private void GetEquipmentTypeList()
        {
            try
            {
                string strErrCode = string.Empty;
                string strErrText = string.Empty;
                //DataTable dtResult = this.m_SqlBiz.GetCommonCode("EQUIP_TYPE", ref strErrCode, ref strErrText);
                DataTable dtResult = this.m_SqlBiz.GetEquipmentTypeList(ref strErrCode, ref strErrText);

                EqType pType;
                m_pTypeList.Clear();
                foreach (DataRow dr in dtResult.Rows)
                {
                    pType = new EqType(dr);
                    m_pTypeList.Add(pType);
                }
            }
            catch (Exception ex)
            {

            }
        }
    }

    #endregion // Equipment Control

}
