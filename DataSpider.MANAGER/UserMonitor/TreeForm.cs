using DataSpider.PC00.PT;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace DataSpider.UserMonitor
{
    public partial class TreeForm : LibraryWH.FormCtrl.UserForm
    {
        private const int TVM_SETEXTENDEDSTYLE = 0x1100 + 44;
        private const int TVM_GETEXTENDEDSTYLE = 0x1100 + 45;
        private const int TVS_EX_DOUBLEBUFFER = 0x0004;

        [DllImport("user32")]
        private static extern bool SetForegroundWindow(IntPtr handle);
        [DllImport("user32")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

        delegate void ChangeAllTreeInformation_Callback(EquipCtrl pObj);
        private PC00Z01 sqlBiz = new PC00Z01();
        public delegate bool OnRefreshTreeDataDelegate();
        public event OnRefreshTreeDataDelegate OnRefreshTreeData = null;
        private TreeNode treeNodeLastSelected = null;
        //private System.Windows.Forms.Timer timerTreeViewState = null;
        private int autoRefreshInterval = 10;
        private DataTable dtEqState = null;
        private PC00Z01 sql = null;
        private MonitorForm parent = null;
        private DateTime dtLastRefreshed = DateTime.MinValue;
        private Thread threadDataRefresh = null;
        public bool threadStop = false;
        private bool threadPause = false;

        StringBuilder strQuery = new StringBuilder();
        string errCode = string.Empty;
        string errText = string.Empty;

        private DateTime dtLastUpdated = DateTime.MinValue;
        private int lastEquipmentCount = 0;


        public TreeForm(MonitorForm _parent)
        {
            InitializeComponent();
            parent = _parent;

            if (ConfigHelper.GetAppSetting("HideTransferActiveNode").Trim().ToUpper().Equals("Y"))//.Contains("y"))
            {
                contextMenuStripEQControl.Items["activeToolStripMenuItem"].Visible = false;
            }
            else
            {
                contextMenuStripEQControl.Items["activeToolStripMenuItem"].Visible = true;
            }

        }

        private void TreeForm_Load(object sender, EventArgs e)
        {
            sql = new PC00Z01();
            UserLogInChanged();
            this.treeViewEQStatus.Nodes.Clear();
            
            // 장비설정 변경 체크를 위한 정보를 최초 1회 업데이트 하여 쓰레드에서 최초 장비설정변경으로 처리되는것 막기위해
            IsEquipmentUpdated();

            if (!int.TryParse(ConfigHelper.GetAppSetting("TreeAutoRefreshInterval").Trim(), out autoRefreshInterval))
            {
                autoRefreshInterval = 10;
            }

            textBox_RefreshInterval.Text = autoRefreshInterval.ToString();
            threadDataRefresh = new Thread(new ThreadStart(ThreadJob));
            threadDataRefresh.Start();
        }

        private void ThreadJob()
        {
            Thread.Sleep(1000);

            while (!threadStop)
            {
                if (threadPause)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                if (dtLastRefreshed.AddSeconds(autoRefreshInterval).CompareTo(DateTime.Now) <= 0)
                {
                    if (IsEquipmentUpdated())
                    {
                        RefreshTreeView();
                    }
                    else
                    {
                        UpdateTreeViewState();
                    }
                }
                Thread.Sleep(1000);
            }
        }

        private bool IsEquipmentUpdated()
        {
            bool result = false;
            string errCode = string.Empty;
            string errText = string.Empty;
            DataTable dtEqMod = sql.GetEquipmentModifiedInfo(ref errCode, ref errText);
            if (dtEqMod != null && dtEqMod.Rows.Count > 0)
            {
                if (!DateTime.TryParse(dtEqMod.Rows[0][0].ToString(), out DateTime dt))
                {
                    dt = DateTime.MinValue;
                }
                int.TryParse(dtEqMod.Rows[0][1].ToString(), out int eqCount);

                if (!dtLastUpdated.Equals(dt) || !lastEquipmentCount.Equals(eqCount))
                {
                        dtLastUpdated = dt;
                        lastEquipmentCount = eqCount;
                        result = true;
                }
            }
            return result;
        }

        /// <summary>
        /// 트리뷰 각 노도 상태값 DB 조회하여 업데이트
        /// 최초, 자동리프레시, 수동리스레시, 트리구조 변경 시 
        /// </summary>
        private void UpdateTreeViewState()
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate { UpdateTreeViewState(); });
            }
            else
            {
                string errCode = string.Empty;
                string errText = string.Empty;
                dtEqState = sql.GetProgramStatus("", ref errCode, ref errText);
                if (dtEqState != null && dtEqState.Rows.Count > 0 && treeViewEQStatus.Nodes.Count > 0)
                {
                    treeViewEQStatus.BeginUpdate();
                    UpdateTreeNodeState(treeViewEQStatus.Nodes[0] as TreeNode_SBL);
                    treeViewEQStatus.EndUpdate();
                }
                dtLastRefreshed = DateTime.Now;
            }
        }
        /// <summary>
        /// 트리뷰 각 노도 상태값 업데이트
        /// 장비타입 노드 : 정상장비/전체장비 수, 프로그램 상태
        /// 장비 노드 : 태그수, 장비 상태
        /// </summary>
        /// <param name="node"></param>
        private void UpdateTreeNodeState(TreeNode_SBL node)
        {
            foreach (TreeNode_SBL child in node.Nodes)
            {
                UpdateTreeNodeState(child);
            }

            if (node.Obj is Eq)
            {
                DataRow[] dr = dtEqState.Select($"[Equipment Name] = '{node.Obj.Name}'");
                if (dr != null && dr.Length > 0)
                {
                    int.TryParse(dr[0]["Status Code"].ToString(), out int code);
                    node.Obj.State = (IF_STATUS)code;
                    node.Text = $"{node.Obj.Name} [{dr[0]["Active Server"]}] ({dr[0]["Tag Count"]}) [{node.Obj.State}]";
                }
            }
            else if (node.Obj is EqType)
            {
                List<Eq> listEq = (node.Obj as EqType).EqLists;
                int normalCount = listEq.FindAll(e => e.State.Equals(IF_STATUS.Normal)).Count;

                if (listEq.Count < 1)
                {
                    node.Obj.State = IF_STATUS.Unknown;
                }
                else if (normalCount != listEq.Count)
                {
                    node.Obj.State = IF_STATUS.Stop;
                }
                else
                {
                    node.Obj.State = IF_STATUS.Normal;
                }

                node.Text = $"{node.Obj.Name} ({node.Obj.Description}) [{normalCount}/{listEq.Count}]";
            }
            else
                node.Obj.State = IF_STATUS.Normal;
        }


        public void OnChangeTreeData(object sender, SBLEventArgs e)
        {
            ChangeAllTreeInformation((EquipCtrl)sender);
        }

        /// <summary>
        /// 트리뷰 구성
        /// 최초 시작시, 장비 추가/수정/삭제 시
        /// </summary>
        /// <param name="pObj"></param>
        private void ChangeAllTreeInformation(EquipCtrl pObj)
        {
            try
            {
                if (treeViewEQStatus.InvokeRequired)
                {
                    ChangeAllTreeInformation_Callback d = new ChangeAllTreeInformation_Callback(ChangeAllTreeInformation);
                    this.Invoke(d, new object[] { pObj });
                }
                else
                {
                    treeNodeLastSelected = treeViewEQStatus.SelectedNode;
                    
                    treeViewEQStatus.Nodes.Clear();
                    TreeNode_SBL rootNode = new TreeNode_SBL(new SBL());
                    rootNode.Text = "DataSpider";
                    rootNode.Obj.State = IF_STATUS.Normal;
                    treeViewEQStatus.Nodes.Add(rootNode);

                    List<Zone> zTypeList = pObj.zLists;
                    List<EqType> pTypeList = pObj.TypeLists;

                    if (pTypeList != null)
                    {
                        TreeNode_SBL zNode;
                        TreeNode_SBL pNode;
                        TreeNode_SBL pChildNode;

                        
                        //    foreach (EqType pType in pTypeList)
                        //    {
                        //        pNode = new TreeNode_SBL(pType);

                        //        rootNode.Nodes.Add(pNode);
                        //        foreach (Eq q in pType.EqLists)
                        //        {
                        //            pChildNode = new TreeNode_SBL(q);
                        //            pChildNode.ToolTipText = q.GetData("CONNECTION_INFO");
                        //            pNode.Nodes.Add(pChildNode);
                        //        }
                        //    }
                        

                        foreach (Zone zType in zTypeList)
                        {
                            zNode = new TreeNode_SBL(zType);

                            rootNode.Nodes.Add(zNode);

                            zType.EqTypeLists = pTypeList.FindAll(element => (element.ZoneType.ToString().Trim() == zType.TypeCode.ToString().Trim()));


                            foreach (EqType pType in zType.EqTypeLists)
                            {
                                pNode = new TreeNode_SBL(pType);
                                zNode.Nodes.Add(pNode);

                                //pType.EqLists = pType.EqLists.FindAll(element => (element.Type.ToString().Trim() == pType.TypeCode.ToString().Trim()
                                //&& element.ZoneTypeCd.ToString().Trim() == pType.ZoneType.ToString().Trim()));

                                foreach (Eq q in pType.EqLists)
                                {
                                    pChildNode = new TreeNode_SBL(q);
                                    pChildNode.ToolTipText = q.GetData("CONNECTION_INFO");
                                    pNode.Nodes.Add(pChildNode);
                                }
                            }                            
                        }
                    }
                    UpdateTreeViewState();

                    //현재 해당하는 서버만 Expand 처리
                    treeViewEQStatus.Nodes[0].Expand();                    

                    foreach (Zone zType in zTypeList)
                    {
                        TreeNode_SBL zNode = new TreeNode_SBL(zType);

                        treeViewEQStatus.Nodes[0].Nodes[int.Parse(zType.TypeCode.ToString().Trim()) -1].Expand();

                        if (zType.EqTypeLists != null)
                        {
                            TreeNode_SBL pNode1;

                            int lidx = 0;
                            foreach (EqType pType in zType.EqTypeLists)
                            {
                                pNode1 = new TreeNode_SBL(pType);

                                if (pNode1.Obj.State != IF_STATUS.Unknown)
                                    treeViewEQStatus.Nodes[0].Nodes[int.Parse(zType.TypeCode.ToString().Trim())-1].Nodes[lidx].Expand();

                                lidx++;
                            }
                        }
                        if (zType.TypeCode.ToString().Trim() == "1")
                            treeViewEQStatus.SelectedNode = GetLastSelectedNode(treeNodeLastSelected);

                    }

                    ////현재 해당하는 서버만 Expand 처리
                    //treeViewEQStatus.Nodes[0].Expand();

                    //if (pTypeList != null)
                    //{
                    //    TreeNode_SBL pNode;

                    //    int lidx = 0;
                    //    foreach (EqType pType in pTypeList)
                    //    {
                    //        pNode = new TreeNode_SBL(pType);

                    //        if (pNode.Obj.State != IF_STATUS.Unknown)
                    //            treeViewEQStatus.Nodes[0].Nodes[lidx].Expand();

                    //        lidx++;
                    //    }
                    //}
                    //treeViewEQStatus.SelectedNode = GetLastSelectedNode(treeNodeLastSelected);// treeNodeLastSelected == null ? treeViewEQStatus.Nodes[0] : treeNodeLastSelected;
                }
            }
            catch (Exception e)
            {

                MessageBox.Show(e.Message);
                //Log.ExceptionLog(e);
                // //throw;
            }
            //GetEquipmentType();
        }

        private TreeNode GetLastSelectedNode(TreeNode nodeLastSelected)
        {
            if (nodeLastSelected != null)
            {
                TreeNode[] nodeFound = treeViewEQStatus.Nodes.Find(nodeLastSelected.Name, true);
                if (nodeFound != null && nodeFound.Length > 0)
                {
                    return nodeFound[0];
                }
                return GetLastSelectedNode(nodeLastSelected.Parent);
            }
            return treeViewEQStatus.Nodes[0];
        }

        /// <summary>
        /// 선택된 노드종류에 따라 (장비타입, 장비) 컨텍스트 메뉴 조절
        /// 장비타입의 경우 프로세스 상태에 따라 컨텍스트 메뉴 프로그램 실행/종료 표시
        /// 프로세스가 없거나 종료되었으면 프로세스의 MainWindowsTitle 이름으로 찾아오도록
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            treeNodeLastSelected = treeViewEQStatus.SelectedNode = e.Node;
        }

        private bool IsLoggedIn()
        {
            return UserAuthentication.UserLevel.Equals(UserLevel.Admin) || UserAuthentication.UserLevel.Equals(UserLevel.Manager);
        }

        public void UserLogInChanged()
        {
            switch (UserAuthentication.UserLevel)
            {
                case UserLevel.Admin:
                case UserLevel.Manager:
                    foreach (ToolStripItem item in contextMenuStripEQControl.Items)
                    {
                        item.Enabled = true;
                    }
                    contextMenuStripEQControl.Items["editToolStripMenuItem"].Text = "Edit";
                    break;
                default:
                    break;
            }
        }

        private void RefreshTreeView()
        {
            if (OnRefreshTreeData != null)
            {
                OnRefreshTreeData();
                // 주기적으로 장비설정 변경을 체크하여 리프레시 하는 것과 사용자가 장비설정 변경을 하여 리프레시 하는 것 이 있음
                // 사용자가 변경하여 리프레시 되었을 때 장비변경 정보를 업데이트 하여 주기적으로 장비설정 변경 체크하여 실행하는 것을 제한
                IsEquipmentUpdated();
            }
        }

        private void AddEquipmentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SBL nodeTag = treeViewEQStatus.SelectedNode.Tag as SBL;
            EquipmentAddEdit form = new EquipmentAddEdit(nodeTag);
            if (DialogResult.OK.Equals(form.ShowDialog(this)))
            {
                RefreshTreeView();
            }
        }

        private void EditEquipmentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SBL nodeTag = treeViewEQStatus.SelectedNode.Tag as SBL;
            EquipmentAddEdit form = new EquipmentAddEdit(nodeTag);
            if (DialogResult.OK.Equals(form.ShowDialog(this)))
            {
                RefreshTreeView();
            }
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SBL nodeTag = treeViewEQStatus.SelectedNode.Tag as SBL;
            EquipmentAddEdit form = new EquipmentAddEdit(nodeTag, true);
            if (DialogResult.OK.Equals(form.ShowDialog(this)))
            {
                RefreshTreeView();
            }
        }

        private void DeleteEquipmentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                SBL nodeTag = treeViewEQStatus.SelectedNode.Tag as SBL;

                if (nodeTag.GetType().Equals(typeof(EqType))) 
                {
                    List<Eq> listEq = (nodeTag as EqType).EqLists;

                    if (listEq.Count > 0)
                    {
                        MessageBox.Show($"{nodeTag.Name} 에 등록된 장비가 존재합니다. 장비 삭제후 처리가 가능합니다.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    if (DialogResult.Yes.Equals(MessageBox.Show($"{nodeTag.Name}의 장비타입을 삭제하시겠습니까 ?", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)))
                    {
                        string strErrCode = string.Empty;
                        string strErrText = string.Empty;
                                               
                        if (sqlBiz.DeleteEquipmentTypeInfo(nodeTag.Name, nodeTag.GetData("ZONE_TYPE").ToString(), ref strErrCode, ref strErrText))
                        {
                            MessageBox.Show($"{nodeTag.Name} 장비타입의 매핑 정보가 삭제되었습니다.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            RefreshTreeView();
                        }
                        else
                        {
                            MessageBox.Show($"장비타입의 매핑 정보 삭제 중 오류가 발생하였습니다. {strErrCode} - {strErrText}", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        
                    }
                }
                else
                {
                    if (DialogResult.Yes.Equals(MessageBox.Show($"{nodeTag.Name} 장비를 삭제하시겠습니까 ?", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)))
                    {
                        string strErrCode = string.Empty;
                        string strErrText = string.Empty;

                        DataTable dtEquipment = sqlBiz.GetEquipmentInfo("", "", true, ref strErrCode, ref strErrText);

                        DataRow[] drSelectEquip = dtEquipment.Select($"EQUIP_NM = '{nodeTag.Name}'");
                        if (drSelectEquip != null && drSelectEquip.Length > 0)
                        {
                            if (sqlBiz.DeleteEquipmentInfo(nodeTag.Name, ref strErrCode, ref strErrText))
                            {
                                MessageBox.Show($"{nodeTag.Name} 장비가 삭제되었습니다.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                RefreshTreeView();
                            }
                            else
                            {
                                MessageBox.Show($"장비 삭제 중 오류가 발생하였습니다. {strErrCode} - {strErrText}", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        else
                        {
                            MessageBox.Show($"Equipment ({nodeTag.Name}) is not exist", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"장비 삭제 중 오류가 발생하였습니다. {ex.Message}", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Process ExecuteIFPrograms(string equipTypeName)
        {
            Process prc = Process.Start(new ProcessStartInfo(@"DataSpiderPC01.exe")
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = equipTypeName
                //Arguments = $"{equipTypeName} IgnoreServerName"
                //WorkingDirectory = @".\"
            });
            if (prc != null)
            {
                ShowWindowAsync(prc.MainWindowHandle, 1);
                SetForegroundWindow(prc.MainWindowHandle);
            }
            return prc;
        }

        private void ForegroundEquipmentType(string equipTypeName, bool bVal)
        {
            strQuery.Clear();
            if (bVal == true)
            {
                strQuery.Append($" UPDATE MA_FAILOVER_CD SET HIDE_SHOW = 1 FROM MA_EQUIPMENT_CD E,MA_COMMON_CD C ");
                strQuery.Append($" WHERE E.EQUIP_NM = MA_FAILOVER_CD.EQUIP_NM AND C.CODE = E.EQUIP_TYPE AND C.CODE_NM = '{equipTypeName}' ");
            }
            else
            {
                strQuery.Append($" UPDATE MA_FAILOVER_CD SET HIDE_SHOW = 0 FROM MA_EQUIPMENT_CD E,MA_COMMON_CD C ");
                strQuery.Append($" WHERE E.EQUIP_NM = MA_FAILOVER_CD.EQUIP_NM AND C.CODE = E.EQUIP_TYPE AND C.CODE_NM = '{equipTypeName}' ");
            }
            sqlBiz.ExecuteNonQuery(strQuery.ToString(), ref errCode, ref errText);
        }
        private void ForegroundEquipment(string equipName, bool bVal)
        {
            strQuery.Clear();          
            if (bVal == true)
                strQuery.Append($" UPDATE MA_FAILOVER_CD SET HIDE_SHOW=1  WHERE EQUIP_NM='{equipName}'  ");
            else
                strQuery.Append($" UPDATE MA_FAILOVER_CD SET HIDE_SHOW=0  WHERE EQUIP_NM='{equipName}'  ");
            sqlBiz.ExecuteNonQuery(strQuery.ToString(), ref errCode, ref errText);
        }

        private void button_Refresh_Click(object sender, EventArgs e)
        {
            UpdateTreeViewState();
        }

        private void button_SetInterval_Click(object sender, EventArgs e)
        {
            if (int.TryParse(textBox_RefreshInterval.Text, out int tempInterval))
            {
                //timerRefresh.Interval = tempInterval * 1000;
                autoRefreshInterval = tempInterval;
            }
        }

        private void checkBox_AutoRefresh_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as CheckBox).Checked)
            {
                threadPause = false;
            }
            else
            {
                threadPause = true;
            }
        }

        private void contextMenuStripEQControl_Opening(object sender, CancelEventArgs e)
        {
            SBL nodeTag = treeViewEQStatus.SelectedNode.Tag as SBL;

            if (nodeTag is Eq)
            {
                strQuery.Clear();
                strQuery.Append($"EXEC GetFailoverInfo '{nodeTag.Name}'");
                DataTable fot = sqlBiz.GetTableInfo(strQuery.ToString(), ref errCode, ref errText);
                if (fot == null || fot.Rows.Count == 0)
                {
                    return;
                }
                DataRow fo_dr = fot.Rows[0];


                //EquipName = nodeTag.Name;
                if ( IsLoggedIn() )
                {
                    contextMenuStripEQControl.Items["editToolStripMenuItem"].Enabled = true; contextMenuStripEQControl.Items["editToolStripMenuItem"].Text = "Edit";
                    contextMenuStripEQControl.Items["copyToolStripMenuItem"].Enabled = true;
                    contextMenuStripEQControl.Items["addToolStripMenuItem"].Enabled = false;
                    contextMenuStripEQControl.Items["deleteToolStripMenuItem"].Enabled = true;


                    contextMenuStripEQControl.Items["programShowToolStripMenuItem"].Enabled = false;
                    contextMenuStripEQControl.Items["programHideToolStripMenuItem"].Enabled = false;
                    if (0 == (int)fo_dr["FAILOVER_MODE"])  // MANL
                    {
                        // DSC프로그램에서 종료시 99를 써야 한다.
                        if (99 == (int)fo_dr["PROG_STATUS"] || 0 == (int)fo_dr["PROG_STATUS"]) 
                        {
                            contextMenuStripEQControl.Items["programRunToolStripMenuItem"].Enabled = true;
                            contextMenuStripEQControl.Items["programStopToolStripMenuItem"].Enabled = false;
                        }
                        else
                        {
                            contextMenuStripEQControl.Items["programRunToolStripMenuItem"].Enabled = false;
                            contextMenuStripEQControl.Items["programStopToolStripMenuItem"].Enabled = true;
                            if (0 == (int)fo_dr["HIDE_SHOW"])
                            {
                                contextMenuStripEQControl.Items["programShowToolStripMenuItem"].Enabled = true;
                                contextMenuStripEQControl.Items["programHideToolStripMenuItem"].Enabled = false;

                            }
                            else
                            {
                                contextMenuStripEQControl.Items["programShowToolStripMenuItem"].Enabled = false;
                                contextMenuStripEQControl.Items["programHideToolStripMenuItem"].Enabled = true;
                            }

                        }
                        contextMenuStripEQControl.Items["modeChangeManualToolStripMenuItem"].Enabled = false;
                        contextMenuStripEQControl.Items["modeChangeAutoToolStripMenuItem"].Enabled = true;
                    }
                    else
                    {
                        contextMenuStripEQControl.Items["programRunToolStripMenuItem"].Enabled = false;
                        contextMenuStripEQControl.Items["programStopToolStripMenuItem"].Enabled = false;
                        if (99 == (int)fo_dr["PROG_STATUS"])
                        {
                            contextMenuStripEQControl.Items["programShowToolStripMenuItem"].Enabled = false;
                            contextMenuStripEQControl.Items["programHideToolStripMenuItem"].Enabled = false;
                        }
                        else
                        {
                            if (0 == (int)fo_dr["HIDE_SHOW"])
                            {
                                contextMenuStripEQControl.Items["programShowToolStripMenuItem"].Enabled = true;
                                contextMenuStripEQControl.Items["programHideToolStripMenuItem"].Enabled = false;

                            }
                            else
                            {
                                contextMenuStripEQControl.Items["programShowToolStripMenuItem"].Enabled = false;
                                contextMenuStripEQControl.Items["programHideToolStripMenuItem"].Enabled = true;
                            }
                        }
                        contextMenuStripEQControl.Items["modeChangeManualToolStripMenuItem"].Enabled = true;
                        contextMenuStripEQControl.Items["modeChangeAutoToolStripMenuItem"].Enabled = false;
                    }
                    contextMenuStripEQControl.Items["activeToolStripMenuItem"].Enabled = true;
                    string ToActive = (((int)fo_dr["ACTIVE_SERVER"] + 1) % 2 == 0) ? "P" : "S";
                    contextMenuStripEQControl.Items["activeToolStripMenuItem"].Text = $"Transfer ActiveNode To {ToActive}";
                }
                else
                {
                    contextMenuStripEQControl.Items["editToolStripMenuItem"].Enabled = true; 
                    contextMenuStripEQControl.Items["editToolStripMenuItem"].Text = "Equipment Info";
                    contextMenuStripEQControl.Items["copyToolStripMenuItem"].Enabled = false;
                    contextMenuStripEQControl.Items["addToolStripMenuItem"].Enabled = false;
                    contextMenuStripEQControl.Items["deleteToolStripMenuItem"].Enabled = false;                   

                    contextMenuStripEQControl.Items["programRunToolStripMenuItem"].Enabled = false;
                    contextMenuStripEQControl.Items["programStopToolStripMenuItem"].Enabled = false;
                    contextMenuStripEQControl.Items["programShowToolStripMenuItem"].Enabled = false;
                    contextMenuStripEQControl.Items["programHideToolStripMenuItem"].Enabled = false;
                    contextMenuStripEQControl.Items["modeChangeManualToolStripMenuItem"].Enabled = false;
                    contextMenuStripEQControl.Items["modeChangeAutoToolStripMenuItem"].Enabled = false;
                    contextMenuStripEQControl.Items["activeToolStripMenuItem"].Enabled = false;

                }
            }
            else
            {  // EquipType
                //EquipTypeName = nodeTag.Name;
                if (IsLoggedIn())
                {
                    contextMenuStripEQControl.Items["editToolStripMenuItem"].Enabled = false; 
                    contextMenuStripEQControl.Items["editToolStripMenuItem"].Text = "Edit";
                    contextMenuStripEQControl.Items["copyToolStripMenuItem"].Enabled = false;

                    if (nodeTag.Name == null || nodeTag.Name.ToString() == "DataSpider" || treeViewEQStatus.SelectedNode.Parent.Text.ToString() == "DataSpider")
                    {
                        contextMenuStripEQControl.Items["addToolStripMenuItem"].Enabled = false;
                        contextMenuStripEQControl.Items["deleteToolStripMenuItem"].Enabled = false;
                    }
                    else
                    {
                        contextMenuStripEQControl.Items["addToolStripMenuItem"].Enabled = true;
                        contextMenuStripEQControl.Items["deleteToolStripMenuItem"].Enabled = true;
                    }

                    contextMenuStripEQControl.Items["programRunToolStripMenuItem"].Enabled = false;
                    contextMenuStripEQControl.Items["programStopToolStripMenuItem"].Enabled = false;
                    contextMenuStripEQControl.Items["programShowToolStripMenuItem"].Enabled = true;
                    contextMenuStripEQControl.Items["programHideToolStripMenuItem"].Enabled = true;
                    contextMenuStripEQControl.Items["modeChangeManualToolStripMenuItem"].Enabled = false;
                    contextMenuStripEQControl.Items["modeChangeAutoToolStripMenuItem"].Enabled = false;
                    contextMenuStripEQControl.Items["activeToolStripMenuItem"].Enabled = false;                   
                }
                else
                {
                    contextMenuStripEQControl.Items["editToolStripMenuItem"].Enabled = false; 
                    contextMenuStripEQControl.Items["editToolStripMenuItem"].Text = "Equipment Info";
                    contextMenuStripEQControl.Items["copyToolStripMenuItem"].Enabled = false;
                    contextMenuStripEQControl.Items["addToolStripMenuItem"].Enabled = false;
                    contextMenuStripEQControl.Items["deleteToolStripMenuItem"].Enabled = false;

                    contextMenuStripEQControl.Items["programRunToolStripMenuItem"].Enabled = false;
                    contextMenuStripEQControl.Items["programStopToolStripMenuItem"].Enabled = false;
                    contextMenuStripEQControl.Items["programShowToolStripMenuItem"].Enabled = false;
                    contextMenuStripEQControl.Items["programHideToolStripMenuItem"].Enabled = false;
                    contextMenuStripEQControl.Items["modeChangeManualToolStripMenuItem"].Enabled = false;
                    contextMenuStripEQControl.Items["modeChangeAutoToolStripMenuItem"].Enabled = false;
                    contextMenuStripEQControl.Items["activeToolStripMenuItem"].Enabled = false;
                }
            }
        }

        // 프로그램 run
        private void programRunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                SBL nodeTag = treeViewEQStatus.SelectedNode.Tag as SBL;

                if (nodeTag is EqType)
                {
                    return;
                }
                if (DialogResult.No.Equals(MessageBox.Show($"{nodeTag.Name} 장비 수집 프로그램을 실행하시겠습니까 ?", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)))
                {
                    return;
                }

                int MY_ID = sqlBiz.GetServerId(Environment.MachineName);
                if (MY_ID == -1)
                {
                    MessageBox.Show($"Server Code does not exist in database", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                strQuery.Clear();
                strQuery.Append($" UPDATE MA_FAILOVER_CD SET RUN_REQ{MY_ID}=1  WHERE EQUIP_NM='{nodeTag.Name}'  ");
                sqlBiz.ExecuteNonQuery(strQuery.ToString(), ref errCode, ref errText);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"장비 I/F 프로그램 실행 중 오류가 발생하였습니다. {ex.Message}", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        // 프로그램 stop
        private void programStopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SBL nodeTag = treeViewEQStatus.SelectedNode.Tag as SBL;

            if (nodeTag is EqType)
            {
                return;
            }
            if (DialogResult.No.Equals(MessageBox.Show($"{nodeTag.Name} 장비 수집 프로그램을 종료하시겠습니까 ?", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)))
            {
                return;
            }

            int MY_ID = sqlBiz.GetServerId(Environment.MachineName);
            if (MY_ID == -1)
            {
                MessageBox.Show($"Server Code does not exist in database", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            strQuery.Clear();
            strQuery.Append($" UPDATE MA_FAILOVER_CD SET STOP_REQ{MY_ID}=1  WHERE EQUIP_NM='{nodeTag.Name}'  ");
            sqlBiz.ExecuteNonQuery(strQuery.ToString(), ref errCode, ref errText);
        }

        // 실행서버 전환
        private void activeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SBL nodeTag = treeViewEQStatus.SelectedNode.Tag as SBL;

            if (nodeTag is EqType)
            {
                return;
            }
            if (DialogResult.No.Equals(MessageBox.Show($"{nodeTag.Name} 장비 수집 프로그램 실행서버를 전환하시겠습니까 ?", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)))
            {
                return;
            }

            int MY_ID = sqlBiz.GetServerId(Environment.MachineName);
            if (MY_ID == -1)
            {
                MessageBox.Show($"Server Code does not exist in database", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            strQuery.Clear();
            strQuery.Append($" UPDATE MA_FAILOVER_CD SET ACTIVE_SERVER=(ACTIVE_SERVER+1)%2, PROG_STATUS=99  WHERE EQUIP_NM='{nodeTag.Name}'  ");
            sqlBiz.ExecuteNonQuery(strQuery.ToString(), ref errCode, ref errText);
        }

        // Failover Auto Mode 로 전환 
        private void modeChangeAutoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SBL nodeTag = treeViewEQStatus.SelectedNode.Tag as SBL;

            if (nodeTag is EqType)
            {
                return;
            }
            if (DialogResult.No.Equals(MessageBox.Show($"{nodeTag.Name} 장비 수집 프로그램 Failover Mode Auto 로 전환하시겠습니까 ?", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)))
            {
                return;
            }

            int MY_ID = sqlBiz.GetServerId(Environment.MachineName);
            if (MY_ID == -1)
            {
                MessageBox.Show($"Server Code does not exist in database", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            strQuery.Clear();
            strQuery.Append($" UPDATE MA_FAILOVER_CD SET FAILOVER_MODE=1  WHERE EQUIP_NM='{nodeTag.Name}'  ");
            sqlBiz.ExecuteNonQuery(strQuery.ToString(), ref errCode, ref errText);
        }

        // Failover Manual Mode 로 전환
        private void modeChangeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SBL nodeTag = treeViewEQStatus.SelectedNode.Tag as SBL;

            if (nodeTag is EqType)
            {
                return;
            }

            if (DialogResult.No.Equals(MessageBox.Show($"{nodeTag.Name} 장비 수집 프로그램 Failover Manual Mode 로 전환하시겠습니까 ?", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)))
            {
                return;
            }

            int MY_ID = sqlBiz.GetServerId(Environment.MachineName);
            if (MY_ID == -1)
            {
                MessageBox.Show($"Server Code does not exist in database", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            strQuery.Clear();
            strQuery.Append($" UPDATE MA_FAILOVER_CD SET FAILOVER_MODE=0 WHERE EQUIP_NM='{nodeTag.Name}'  ");
            sqlBiz.ExecuteNonQuery(strQuery.ToString(), ref errCode, ref errText);
        }

        // 프로그램 표시
        private void programShowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SBL nodeTag = treeViewEQStatus.SelectedNode.Tag as SBL;

            if (nodeTag is EqType)
            {
                ForegroundEquipmentType(nodeTag.Name, true);
            }
            else
            {
                ForegroundEquipment(nodeTag.Name, true);
            }

        }

        // 프로그램 숨김
        private void programHideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SBL nodeTag = treeViewEQStatus.SelectedNode.Tag as SBL;

            if (nodeTag is EqType)
            {
                ForegroundEquipmentType(nodeTag.Name, false);
            }
            else
            {
                ForegroundEquipment(nodeTag.Name, false);
            }

        }
    }
}
