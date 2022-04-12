using System;
using System.Data;
using System.Web.UI;
using System.Reflection;
using System.Web.UI.WebControls;
using CFW.Web; 


namespace CFW.Web
{
	/// <summary>
	/// GridUtil�� ���� �����Դϴ�.
	/// </summary>
	///

	public class GridUtil : System.Web.UI.Page
	{
		#region ���� ���� ����
		private static string strDefaultCss = string.Empty;
		private static string strOverCss = string.Empty;
		
		#endregion

	    #region ������
		/// <summary>
		/// GridUtil ������ �Դϴ�.
		/// </summary>
		public GridUtil()
		{
		
		}

		#endregion
		
        //#region �������

        //private const string CUSTOM_SCRIPT_FILENAME = "../Scripts/Control/UltraWebGrid.js";

        //private const string CUSTOM_SCRIPT_COMMON_FILENAME = "../Scripts/Control/ControlCSOM.js";

        //private const string CUSTOM_GRID_IMAGE_DIRECTORY = "../Images/GridImages/";
		
        //#endregion


        #region CFW�� ������

        #region �׸��忡 �����͸� ���ε��ؼ� ä��ϴ�

        /// <summary>
        /// �׸��忡 �����͸� ���ε��ؼ� ä��ϴ�
        /// </summary>
        /// <param name="grid">�׸��尳ü�� ID</param>
        /// <param name="dataSource">������ �ҽ�(�����ͼ�, ������ ���̺� ��..)</param>
        public static void FillGrid(GridView grid, object dataSource, string langtype)
        {
            FillGrid(grid, dataSource, langtype, null);
        }

        #endregion

        #region �׸��忡 �����͸� ���ε��ؼ� ä��ϴ�-Ư���÷� ��ȣȭ(+1)

        /// <summary>
        /// Ư���÷��� ��ȣȭ�Ͽ� �׸��忡 �����͸� ���ε��ؼ� ä��ϴ�.
        /// </summary>
        /// <param name="grid">�׸��尳ü�� ID</param>
        /// <param name="dataSource">������ �ҽ�(�����ͼ�, ������ ���̺� ��..)</param>
        /// <param name="cryptographyColumnName">��ȣȭ �� �÷� �̸�</param>
        public static void FillGrid(GridView grid, object dataSource, string langtype, params string[] cryptographyColumnName)
        {

            try
            {

                grid.Columns.Clear();
                if (dataSource.GetType() == typeof(DataTable) && cryptographyColumnName != null)
                    grid.DataSource = CryptographyColumn((DataTable)dataSource, cryptographyColumnName);
                else
                    grid.DataSource = dataSource;
                grid.DataBind();

                if (grid.Rows.Count <= 0)
                    SetGridNoData(grid, CFW.Common.Messaging.SearchMsgWEB("FC0306", langtype));//"��ȸ�� Data�� ���� ���� �ʽ��ϴ�"
                else
                {
                    for (int i = 0; i < grid.Rows.Count; i++)
                    {
                        for (int j = 0; j < grid.Columns.Count; j++)
                        {
                            grid.Rows[i].Cells[j].ToolTip = grid.Rows[i].Cells[j].Text;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        #endregion

        #region Ư��Į�� ��ȣȭ

        private static DataTable CryptographyColumn(DataTable dt, params string[] cryptographyColumnName)
        {
            //			for(int iCol=0; iCol < cryptographyColumnName.Length ; iCol++)
            //			{
            //				for(int iRow=0 ; iRow <dt.Rows.Count; iRow++)
            //				{					
            //					
            //					dt.Rows[iRow][cryptographyColumnName[iCol]] = System.Web.HttpUtility.UrlEncode(Interdev.Framework.Customizing.Encryptor.Encrypt((string)dt.Rows[iRow][cryptographyColumnName[iCol]]));
            //				}
            //			}
            return dt;
        }
        #endregion


        #region �׸��忡 �����Ͱ� ���� �� ����� ���ΰ�, �ϳ��� row �� ������ �޽����� ����ϴ� �޼ҵ��Դϴ�

        /// <summary>
        /// �׸��忡 �����Ͱ� ���� �� ����� ���ΰ�, �ϳ��� row �� ������ �޽����� ����ϴ� �޼ҵ��Դϴ�
        /// </summary>
        /// <param name="grid">�׸��尳ü�� ID</param>
        /// <param name="noDataMessage">����� �޽���</param>
        private static void SetGridNoData(GridView grid, string noDataMessage)
        {
            
                        //int columnCount = 0;

                        //try
                        //{
                        //    if(grid.Columns.Count>0)
                        //        columnCount = grid.Columns.Count;
                        //    else
                        //        columnCount = 100;

                        //    BoundField bfd = null;
                        //    int SpanCnt = 0;

                        //    //for (int i = 0; i < columnCount; i++)
                        //    //{                               
                        //    //    bfd = new BoundField();
                        //    //    bfd.HeaderText = noDataMessage;
                                
                        //    //    //if (!grid.Columns[i].Visible)
                        //    //    //{ 
                        //    //    //    SpanCnt++;
                        //    //    //}
                        //    //}

                            
                        //    //grid.Columns.Add(bfd);

                            

                            

                        //    foreach (GridViewRow gvr in grid.Rows)
                        //    {
                        //        gvr.Cells[0].ColumnSpan = columnCount;
                        //    }

                            


                        //    //for(int i=0;i<columnCount;i++)
                        //    //{		
                        //    //    oCell = new UltraGridCell();				
                        //    //    oCell.Text = noDataMessage;
                        //    //    oCell.Style.HorizontalAlign = System.Web.UI.WebControls.HorizontalAlign.Left;
                        //    //    //NoData �� 1024*768 �������� �߽ɿ� ���ĵ� �� �ֵ��� ��Ÿ�� ���� �߰�
                        //    //    oCell.Style.CssClass = "left_300";
                        //    //    oRow.Cells.Add(oCell);

                        //    //    //					if(grid.Columns[i].DataType == "System.Boolean")
                        //    //    //						grid.Columns[i].Hidden = true;
                        //    //    //
                        //    //    if(!grid.Columns[i].Hidden)
                        //    //    {		
                        //    //        //						if(grid.Columns[i].DataType == "System.Boolean")
                        //    //        //							grid.Columns[i].DataType = "System.String";
                        //    //        oRow.Cells[i].ColSpan = columnCount;
                        //    //        //break;
                        //    //    }

                        //    //}
                        //    //oRow.Tag = "NoData";
                        //    //grid.Rows.Add(oRow);
                        //}
                        //finally
                        //{
                        //    //oRow = null;
                        //    //oCell = null;
                        //}


        }

        public static void SetGridNoData(GridView grid, int columnCount, string langType)
        {
            grid.Rows[0].Cells.Clear();
            grid.Rows[0].Cells.Add(new TableCell());
            grid.Rows[0].Cells[0].ColumnSpan = columnCount;
            grid.Rows[0].Height = Unit.Pixel(30);
            grid.Rows[0].Cells[0].Text = CFW.Common.Messaging.SearchMsgWEB("COMMON.NoData", langType); //No Records Found.
        }
        #endregion

        #region �׸����� ����� �����մϴ�

        /// <summary>
        /// �׸����� ����� �����մϴ�
        /// </summary>
        /// <param name="grid">�׸��尳ü�� ID</param>
        /// <param name="headerTexts">��� �ؽ�Ʈ(Į�� �� ��ŭ�� string �迭)</param>
        /// <param name="width">�� Į���� ����(Į�� �� ��ŭ�� int �迭)</param>
        /// <param name="sizeUnit">������ ����(pixel or percent)</param>
        /// <param name="columnCssClasses">�� Į���� CSS Ŭ�����̸�(Į�� �� ��ŭ�� string �迭)</param>
        public static void SetHeader(GridView grid, string[] headerTexts, int[] width, GridEnum.SizeUnit sizeUnit,
            string[] columnCssClasses, string langType)
        {
            BoundField bfd = null;

            try
            {
                grid.Columns.Clear();


                if (headerTexts != null)
                {
                    for (int i = 0; i < headerTexts.Length; i++)
                    {
                        if (grid.Columns == null || i > grid.Columns.Count - 1)
                        {
                            bfd = new BoundField();
                            bfd.HeaderText = headerTexts[i];
                            bfd.DataField = headerTexts[i];
                            grid.Columns.Add(bfd);
                        }

                        string[] ht = headerTexts[i].Split(new char[] { '^' });

                        if (ht.Length > 1)
                        {
                            grid.HeaderRow.Cells[i].Text = CFW.Common.Dictionary.SearchStaticDicWeb(ht[0], langType) + "<br>" + CFW.Common.Dictionary.SearchStaticDicWeb(ht[1], langType);
                        }
                        else
                        {
                            grid.HeaderRow.Cells[i].Text = CFW.Common.Dictionary.SearchStaticDicWeb(headerTexts[i], langType);
                        }

                        if (width != null)
                        {
                            if ((int)sizeUnit == 0)
                            {
                                System.Web.UI.WebControls.Unit oUnit
                                    = new System.Web.UI.WebControls.Unit(width[i], System.Web.UI.WebControls.UnitType.Pixel);

                                grid.HeaderRow.Cells[i].Width = oUnit;
                            }
                            else if ((int)sizeUnit == 1)
                            {
                                System.Web.UI.WebControls.Unit oUnit
                                    = new System.Web.UI.WebControls.Unit(width[i], System.Web.UI.WebControls.UnitType.Percentage);

                                grid.HeaderRow.Cells[i].Width = oUnit;
                            }
                        }
                    }

                    if (columnCssClasses != null)
                    {
                        for (int j = 0; j < columnCssClasses.Length; j++)
                        {
                            if (columnCssClasses[j] != null && columnCssClasses[j].ToString() != string.Empty)
                            {
                                grid.HeaderRow.Cells[j].CssClass = columnCssClasses[j];
                            }
                        }
                    }
                }
                else
                {
                    return;
                }
            }
            finally
            {
                bfd = null;
            }
        }

        #endregion

        #region �׸��带 �⺻���� ��Ÿ�Ϸ� �������ݴϴ�(SetDefault)

        /// <summary>
        /// �׸��带 �⺻���� ��Ÿ�Ϸ� �������ݴϴ�
        /// </summary>
        /// <param name="grid">�׸��尳ü�� ID</param>
        /// <param name="width">int type width ��</param>
        /// <param name="height">int type height ��</param>
        /// <param name="widthSizeUnit">Width Size ���� ����(Pixel/Percentage)</param>
        /// <param name="heightSizeUnit">Height Size ���� ����(Pixel/Percentage)</param>
        public static void SetDefault(GridView grid, int width, int height, GridEnum.SizeUnit widthSizeUnit, GridEnum.SizeUnit heightSizeUnit)
        {
            try
            {
                //*****************************************************************************************************
                // ���� �׸��� Layout ����
                // 1. ����
                // 2. �÷��̵�
                // 3. �� ���� ����
                // 4. �⺻ CSS
                // 5. �׸��� ��Ƽ ���� ���
                // 6. �÷� ����
                // 7. �÷� ���� ���� ǳ�����򸻷�.
                // 8. CSS ���� (Table CSS, ���콺 over CSS, ���콺 out CSS)
                //*****************************************************************************************************


                // 1. �÷� ���� ��� (�̱� ����) --> Data�� ���� �� �ӵ��� ������. -> ������
                //grid.DisplayLayout.AllowSortingDefault = Infragistics.WebUI.UltraWebGrid.AllowSorting.No;
                //grid.DisplayLayout.HeaderClickActionDefault = Infragistics.WebUI.UltraWebGrid.HeaderClickAction.NotSet;



                // 2. �÷� �̵� ��� ����
                //grid.DisplayLayout.AllowColumnMovingDefault = AllowColumnMoving.None;
                //grid.DisplayLayout.Bands[0].AllowColumnMoving = Infragistics.WebUI.UltraWebGrid.AllowColumnMoving.None;

                // 3. �� ���� ���� ��� �߰�
                //				grid.DisplayLayout.AllowColSizingDefault = Infragistics.WebUI.UltraWebGrid.AllowSizing.Free;

                //grid.CellPadding = 0;
                //grid.CellSpacing = 0;
                //grid.BorderWidth = 0;

                //4.�⺻ css

                //grid.CssClass = "thTitle2 center";


                //				grid.DisplayLayout.TableLayout = Infragistics.WebUI.UltraWebGrid.TableLayout.Fixed;
                //				grid.DisplayLayout.StationaryMargins = Infragistics.WebUI.UltraWebGrid.StationaryMargins.Header;
                //�ο� ���� X
                //grid.DisplayLayout.ActivationObject.AllowActivation = false;

                ////grid.DisplayLayout.ActiveCell.AllowEditing = Infragistics.WebUI.UltraWebGrid.AllowEditing.No;
                //grid.DisplayLayout.CellClickActionDefault = Infragistics.WebUI.UltraWebGrid.CellClickAction.NotSet;

                //// ���� �Ұ�
                //grid.DisplayLayout.AllowDeleteDefault = AllowDelete.No;

                //Header ��Ÿ��
                //				System.Web.UI.WebControls.Unit oUnit
                //					= new System.Web.UI.WebControls.Unit(300, System.Web.UI.WebControls.UnitType.Pixel);
                //				grid.Width = oUnit;

                //				grid.DisplayLayout.HeaderStyleDefault.VerticalAlign = System.Web.UI.WebControls.VerticalAlign.Middle;
                //				grid.DisplayLayout.HeaderStyleDefault.CssClass = "bgtitle";
                //				grid.DisplayLayout.ViewType  = Infragistics.WebUI.UltraWebGrid.ViewType.Flat;

                //�ο� �����͸� ������
                //grid.DisplayLayout.RowSelectorsDefault = Infragistics.WebUI.UltraWebGrid.RowSelectors.No;



                // Activation False
                //				grid.DisplayLayout.SelectTypeRowDefault = SelectType.Extended;
                //				grid.DisplayLayout.ActivationObject.AllowActivation = true;
                //				grid.DisplayLayout.ActivationObject.BorderStyle = System.Web.UI.WebControls.BorderStyle.Solid;
                //				grid.DisplayLayout.ActivationObject.BorderWidth = System.Web.UI.WebControls.Unit.Pixel(0);				

                // SizeUnit ����
                if (width > 0)
                {
                    if (widthSizeUnit == GridEnum.SizeUnit.Pixel)
                    {
                        System.Web.UI.WebControls.Unit oUnit
                            = new System.Web.UI.WebControls.Unit(width, System.Web.UI.WebControls.UnitType.Pixel);
                        grid.Width = oUnit;
                    }
                    else
                    {
                        System.Web.UI.WebControls.Unit oUnit
                            = new System.Web.UI.WebControls.Unit(width, System.Web.UI.WebControls.UnitType.Percentage);
                        grid.Width = oUnit;

                    }
                }
                if (height > 0)
                {
                    if (heightSizeUnit == GridEnum.SizeUnit.Pixel)
                    {
                        System.Web.UI.WebControls.Unit oUnit
                            = new System.Web.UI.WebControls.Unit(height, System.Web.UI.WebControls.UnitType.Pixel);
                        grid.Height = oUnit;
                    }
                    else
                    {
                        System.Web.UI.WebControls.Unit oUnit
                            = new System.Web.UI.WebControls.Unit(height, System.Web.UI.WebControls.UnitType.Percentage);
                        grid.Height = oUnit;

                    }

                }

                // RowHeightDefault
                //grid.DisplayLayout.RowHeightDefault = System.Web.UI.WebControls.Unit.Pixel(20);

                grid.RowStyle.Height = System.Web.UI.WebControls.Unit.Pixel(20);


                //grid.DisplayLayout.RowStyleDefault.Wrap = true;

                // ��� Fix
                //grid.DisplayLayout.StationaryMargins = Infragistics.WebUI.UltraWebGrid.StationaryMargins.Header;
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion


        #region �׸��忡 ��Ÿ�Ͻ�Ʈ�� �����մϴ�


        /// <summary>
        /// �׸��忡 ���콺���� ��Ÿ�Ͻ�Ʈ�� �����մϴ� 
        /// </summary>
        /// <param name="grid">�׸��尳ü�� ID</param>
        /// <param name="tableCSS">���̺� ��ü Ŭ����</param>
        /// <param name="mouseOverCSS">�׸��忡�� ���콺�� �÷��� �� TR �±��� Ŭ����</param>
        /// <param name="defaultRowCSS">TR�� ����Ʈ�� ���� Ŭ����</param>
        /// <param name="alternateRowCSS">�� ������ ���� ���� Ŭ����</param>
        /// <param name="isFixed">�� ���� ��� ��� ����</param>
        public static void SetGridCSS(GridView grid, string tableCSS, string mouseOverCSS, string defaultRowCSS, string alternateRowCSS, bool isFixed)
        {
            System.Text.StringBuilder sbGridInitScript = null;
            System.Text.StringBuilder sbGridMouseOverScript = null;
            System.Text.StringBuilder sbGridMouseOutScript = null;
            try
            {

                grid.RowStyle.CssClass = defaultRowCSS;
                grid.AlternatingRowStyle.CssClass = alternateRowCSS;

                grid.HeaderStyle.CssClass = "thTitle2 center";


                //if (isFixed)
                //{
                //    grid.CellPadding = 0;
                //    grid.CellSpacing = 0;
                //    grid.BorderWidth = 0;

                //    grid.RowStyle.CssClass = defaultRowCSS;
                //    grid.AlternatingRowStyle.CssClass = alternateRowCSS;

                //}
                //else
                //{
                    string strGridFontSize = CFW.Configuration.ConfigManager.Default.ReadConfig("Grid","GridFontSize");
                    //int nColCnt = grid.Columns.Count;
                    if (strGridFontSize.Length > 0)
                    {

                        strGridFontSize = strGridFontSize + "px";
                        grid.Font.Size = new System.Web.UI.WebControls.FontUnit(strGridFontSize);
                        //for (int i = 0; i < nColCnt; i++)
                        //{
                        //    grid.Columns[i].ItemStyle.Font.Size = new System.Web.UI.WebControls.FontUnit(strGridFontSize);
                        //}

                    }
                    else
                    {
                        strGridFontSize = 12 + "px";
                        grid.Font.Size = new System.Web.UI.WebControls.FontUnit(strGridFontSize);
                        //for (int i = 0; i < nColCnt; i++)
                        //{
                        //    grid.Columns[i].ItemStyle.Font.Size = new System.Web.UI.WebControls.FontUnit(strGridFontSize);
                        //}
                    }
                    grid.CellPadding = 0;
                    grid.CellSpacing = 1;
                    grid.BorderWidth = 0;

                    
                //}

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                sbGridInitScript = null;
                sbGridMouseOverScript = null;
                sbGridMouseOutScript = null;
            }
        }
        #endregion

        #endregion

		#region �׸����� ����� �����մϴ�(+1)

		/// <summary>
		/// �׸����� ����� �����մϴ�
		/// </summary>
		/// <param name="grid">�׸��尳ü�� ID</param>
		/// <param name="headerTexts">��� �ؽ�Ʈ(Į�� �� ��ŭ�� string �迭)</param>
		/// <param name="width">�� Į���� ����(Į�� �� ��ŭ�� int �迭)</param>
		/// <param name="sizeUnit">������ ����(pixel or percent)</param>
		public static void SetHeader(GridView grid, string[] headerTexts, int[] width, GridEnum.SizeUnit sizeUnit, string  langType)
		{
			SetHeader(grid, headerTexts, width, sizeUnit, null,langType);
		}

		#endregion

		#region �׸����� ����� �����մϴ�(+2)

		/// <summary>
		/// 
		/// </summary>
		/// <param name="grid">�׸��尳ü�� ID</param>
		/// <param name="headerTexts">��� �ؽ�Ʈ(Į�� �� ��ŭ�� string �迭)</param>
		/// <param name="columnCssClasses">�� Į���� CSS Ŭ�����̸�(Į�� �� ��ŭ�� string �迭)</param>
		public static void SetHeader(GridView grid, string[] headerTexts, string[] columnCssClasses,string  langType)
		{
			SetHeader(grid, headerTexts, null, GridEnum.SizeUnit.Percent, columnCssClasses, langType);
		}

		#endregion

		#region �׸����� ����� �����մϴ�(+3)

        /// <summary>
        /// �׸����� ����� �����մϴ�
        /// </summary>
        /// <param name="grid">�׸��尳ü�� ID</param>
        /// <param name="headerTexts">��� �ؽ�Ʈ(Į�� �� ��ŭ�� string �迭)</param>
        /// <param name="columnCssClasses">�� Į���� CSS Ŭ�����̸�(Į�� �� ��ŭ�� string �迭)</param>
        //public static void SetHeader(GridView grid, string[] headerTexts, string[] columnCssClasses)
        //{
        //    SetHeader(grid, headerTexts, null, GridEnum.SizeUnit.Percent, columnCssClasses);
        //}

        //public static void SetHeader(GridView grid, string[] headerTexts, string[] columnCssClasses, int defaultSortColumn)
        //{
        //    SetHeader(grid, headerTexts, null, GridEnum.SizeUnit.Percent, defaultSortColumn, columnCssClasses);
        //}

		#endregion

		#region ������ Į���� �������� ����ϴ�(����ϴ�)

		/// <summary>
		/// ������ Į���� �������� ����ϴ�(����ϴ�)
		/// </summary>
		/// <param name="Grid">�׸��尳ü�� ID</param>
		/// <param name="Columns">�������� �� Į���� ��ġ</param>
		/// <example>
		/// �� ������ �׸����� 1,3 ��° Į���� �������� ����ϴ�
		/// <code>
		/// MES.FW.Web.GridUtil oGridUtil = new MES.FW.Web.GridUtil();
		/// 
		/// int[] iColHiddens = new int[]{1, 3};
		/// 
		/// oGridUtil.SetColumnHidden(UserListGrid, iColHiddens);
		/// </code>
		/// </example>
		public static void SetColumnHidden(GridView Grid, int[] Columns)
		{
			try
			{
                for (int i = 0; i < Columns.Length; i++)
                {
                    //Į���� �������� �ϴ°̴ϴ�
                    if (Grid.Columns.Count > Columns[i])
                    {
                        Grid.HeaderRow.Cells[Columns[i]].Visible = false;
                        foreach (GridViewRow gvr in Grid.Rows)
                        {
                            gvr.Cells[Columns[i]].Visible = false;
                        }
                    }
                }
			}
			catch(Exception err)
			{
				throw err;
			}
		}
		#endregion

		#region ������ Į���� �������� ����ϴ�(+1)
		/// <summary>
		/// ������ Į���� �������� ����ϴ�(����ϴ�)
		/// </summary>
		/// <param name="Grid">�׸��尳ü�� ID</param>
		/// <param name="Columns">�������� �� Į���� Key Value</param>
		/// <example>
		/// �� ������ �׸����� NO,CD Į���� �������� ����ϴ�
		/// <code>
		/// MES.FW.Web.GridUtil oGridUtil = new MES.FW.Web.GridUtil();
		/// 
		/// string[] strColHiddens = new string[]{"NO", "CD"};
		/// 
		/// oGridUtil.SetColumnHidden(UserListGrid, strColHiddens);
		/// </code>
		/// </example>
        public static void SetColumnHidden(GridView Grid, string[] Column, string langType)
		{
			try
			{
                for (int i = 0; i < Column.Length; i++)
                {
                    for (int j = 0; j < Grid.Columns.Count; j++)
                    {
                        if (Grid.HeaderRow.Cells[j].Text == CFW.Common.Dictionary.SearchStaticDicWeb(Column[i], langType))
                        {
                            Grid.HeaderRow.Cells[j].Visible = false;
                            foreach (GridViewRow gvr in Grid.Rows)
                            {
                                gvr.Cells[j].Visible = false;
                            }
                        }
                    }
                }
			}
			catch(Exception err)
			{
				throw err;
			}
		}
		#endregion

        #region ������

        #region �׸��忡 üũ�ڽ� Į���� �߰��մϴ�

        /// <summary>
		/// �׸��忡 üũ�ڽ� Į���� �߰��մϴ�
		/// </summary>
		/// <param name="grid">�׸��尳ü�� ID</param>
		/// <param name="column">üũ�ڽ� Į���� �߰��� ��ġ(����)</param>
		/// <param name="useCheckBox">����� üũ�ڽ� ǥ�� ����</param>
		/// <param name="headerText">��� �ؽ�Ʈ</param>
		/// <param name="onClickClientScriptSignature">Ŭ�� �� ����� ��ũ��Ʈ �Լ� �̸�</param>
		public static void AddCheckBoxColumn(GridView grid, int column, bool useCheckBox
											 ,string headerText, string onClickClientScriptSignature,string langType)
		{
            //Infragistics.WebUI.UltraWebGrid.UltraGridColumn oColumn = null;
			
            //try
            //{

            //    int nRowCount = grid.Rows.Count;
				
            //    grid.DisplayLayout.AllowSortingDefault = Infragistics.WebUI.UltraWebGrid.AllowSorting.No;
            //    oColumn = new Infragistics.WebUI.UltraWebGrid.UltraGridColumn();
            //    System.Text.StringBuilder sb = new System.Text.StringBuilder(512);
            //    //oColumn.Width = 25;				
            //    string strNoData = "";

            //    if(nRowCount == 1 && grid.Rows[0].Tag != null )
            //    {
            //        if(grid.Rows[0].Tag.ToString() == "NoData")
            //        {
            //            if(useCheckBox)
            //            {
            //                sb.Append("<input class = 'checkbox' type='checkbox'>");
            //            }
            //            strNoData = grid.Rows[0].Cells[0].Text;
					

            //        }
            //    }
            //    else
            //    {
            //        if(useCheckBox)
            //        {
            //            sb.Append("<input class = 'checkbox' type='checkbox' onclick=\"javascript:fn_ToggleAllCheckBox('chk_");
            //            sb.Append(grid.ClientID);
            //            sb.Append("',this.checked);\">");
            //        }
            //    }

            //    if(headerText != null && headerText.Length > 0)
            //    {
            //        if(sb.Length > 0) sb.Append("&nbsp;");
            //        sb.Append( MES.FW.Common.Dictionary.SearchStaticDicWeb(headerText,langType));
					
            //    }

            //    oColumn.HeaderText = sb.ToString();
            //    oColumn.Header.Style.CssClass = "thtitle2";			
            //    grid.Columns.Insert(column, oColumn);
            //    sb.Remove(0, sb.Length);
            //    int nColCount = grid.Columns.Count;

            //    if(nRowCount == 1 && grid.Rows[0].Tag != null )
            //    {
            //        if(grid.Rows[0].Tag.ToString() == "NoData")
            //        {
            //            for(int i=0;i<nColCount;i++)
            //            {										
            //                grid.Rows[0].Cells[i].Text = strNoData;
            //                grid.Rows[0].Cells[i].Style.HorizontalAlign = System.Web.UI.WebControls.HorizontalAlign.Center;
            //                grid.Rows[0].Cells[i].ColSpan = nColCount;					
            //            }
            //        }
            //    }
            //    else
            //    {
            //        for(int i=0;i < nRowCount; i++)
            //        {
            //            sb.Append("<input class = 'checkbox' type=\"checkbox\"");
            //            sb.Append(" name=\"chk_");
            //            sb.Append(grid.ClientID);
            //            sb.Append("\"");
            //            if ( onClickClientScriptSignature != null &&  onClickClientScriptSignature.Length > 0)
            //            {
            //                sb.Append(" onclick=\"javascript:");
            //                sb.Append(onClickClientScriptSignature);
            //                sb.Append("(this);\"");
            //            }
            //            sb.Append(" value=\"");
            //            sb.Append(i.ToString());
            //            sb.Append("\">");

            //            grid.Rows[i].Cells[column].Text = sb.ToString();
            //            sb.Remove(0, sb.Length);
            //        }
            //    }				
            //}

            //finally
            //{
            //    oColumn = null;
            //}
		}



		/// <summary>
		/// �׸��忡 üũ�ڽ� Į���� �߰��մϴ�
		/// </summary>
		/// <param name="grid">�׸��尳ü�� ID</param>
		/// <param name="column">üũ�ڽ� Į���� �߰��� ��ġ(����)</param>
		/// <param name="useCheckBox">����� üũ�ڽ� ǥ�� ����</param>
		/// <param name="headerText">��� �ؽ�Ʈ</param>
		/// <param name="onClickClientScriptSignature">Ŭ�� �� ���� �� �ڹٽ�ũ��Ʈ �Լ� ��</param>
		/// <param name="columnKey">���� �÷� Ű ��</param>
		/// <param name="selectedValue">���� ��</param>
		/// <param name="disabledValue">���� �Ұ� ��</param>
        public static void AddCheckBoxColumn(GridView grid, int column, bool useCheckBox
			,string headerText, string onClickClientScriptSignature , string columnKey, string selectedValue, string disabledValue,string langType )
		{
            //Infragistics.WebUI.UltraWebGrid.UltraGridColumn oColumn = null;
	 
            //try
            //{

            //    int nRowCount = grid.Rows.Count;
				
            //    grid.DisplayLayout.AllowSortingDefault = Infragistics.WebUI.UltraWebGrid.AllowSorting.No;
            //    oColumn = new Infragistics.WebUI.UltraWebGrid.UltraGridColumn();
            //    System.Text.StringBuilder sb = new System.Text.StringBuilder(512);
            //    //oColumn.Width = 25;
            //    string strNoData = "";

            //    if(grid.Rows.Count == 1 && grid.Rows[0].Tag != null )
            //    {
            //        if(grid.Rows[0].Tag.ToString() == "NoData")
            //        {
            //            if(useCheckBox)
            //            {
            //                sb.Append("<input class = 'checkbox' type='checkbox'>");
            //            }
            //            strNoData = grid.Rows[0].Cells[0].Text;
            //        }
            //    }
            //    else
            //    {
            //        if(useCheckBox)
            //        {
            //            sb.Append("<input class = 'checkbox' type='checkbox' onclick=\"javascript:fn_ToggleAllCheckBox('chk_");
            //            sb.Append(grid.ClientID);
            //            sb.Append("',this.checked);\">");
            //        }
            //    }

            //    if(headerText != null && headerText.Length > 0)
            //    {
            //        if(sb.Length > 0) sb.Append("&nbsp;");
            //        sb.Append(  MES.FW.Common.Dictionary.SearchStaticDicWeb(headerText,langType));
            //    }
            //    oColumn.HeaderText = sb.ToString();	
            //    oColumn.Header.Style.CssClass = "thtitle2";	
            //    grid.Columns.Insert(column, oColumn);
            //    sb.Remove(0, sb.Length);

            //    int nColCount = grid.Columns.Count;

            //    if(nRowCount == 1 && grid.Rows[0].Tag != null )
            //    {
            //        if(grid.Rows[0].Tag.ToString() == "NoData")
            //        {
            //            for(int i=0;i<nColCount;i++)
            //            {										
            //                grid.Rows[0].Cells[i].Text = strNoData;
            //                grid.Rows[0].Cells[i].Style.HorizontalAlign = System.Web.UI.WebControls.HorizontalAlign.Center;
            //                grid.Rows[0].Cells[i].ColSpan = nColCount;					
            //            }
            //        }
            //    }
            //    else
            //    {
            //        for(int i=0;i < nRowCount; i++)
            //        {
            //            sb.Append("<input class = 'checkbox' type=\"checkbox\"");
            //            sb.Append(" name=\"chk_");
            //            sb.Append(grid.ClientID);
            //            sb.Append("\"");
            //            if ( onClickClientScriptSignature != null &&  onClickClientScriptSignature.Length > 0)
            //            {
            //                sb.Append(" onclick=\"javascript:");
            //                sb.Append(onClickClientScriptSignature);
            //                sb.Append("(this);\"");
            //            }
            //            sb.Append(" value=\"");
            //            sb.Append(i.ToString());
            //            sb.Append("\"");
					
            //            if (grid.Rows[i].Cells.FromKey(columnKey).Text  == selectedValue) sb.Append(" checked>");
            //            else if(grid.Rows[i].Cells.FromKey(columnKey).Text  == disabledValue) sb.Append("  disabled>");
            //            else sb.Append(">");	

            //            grid.Rows[i].Cells[column].Text = sb.ToString();
            //            sb.Remove(0, sb.Length);
            //        }
            //    }
            //}
            //finally
            //{
            //    oColumn = null;
            //}
		}

		
		
		/// <summary>
		/// �׸��忡 üũ�ڽ� Į���� �߰��մϴ�
		/// </summary>
		/// <param name="grid">�׸��尳ü�� ID</param>
		/// <param name="column">üũ�ڽ� Į���� �߰��� ��ġ(����)</param>
		/// <param name="useCheckBox">����� CheckBox ǥ�� ����</param>
		/// <param name="headerText">��� �ؽ�Ʈ</param>
		/// <example>
		/// �� ������ UserListGrid ��� ID �� �׸����� 1��° Į���� üũ�ڽ� Į���� �߰��ϴ� �����Դϴ�
		/// <code>
		/// MES.FW.WEB.GridUtil.AddCheckBoxColumn(UserListGrid, 1, true,"Model");
		/// </code>
		/// </example>
        public static void AddCheckBoxColumn(GridView grid, int column, bool useCheckBox, string headerText, string langType)
		{
			AddCheckBoxColumn(grid, column, useCheckBox, headerText,"",langType);
		}

		#endregion

		#region �׸��忡 üũ�ڽ� Į���� �߰��մϴ�(+1)

		/// <summary>
		/// �׸��忡 üũ�ڽ� Į���� �߰��մϴ�
		/// �����ε�� �����Դϴ�
		/// �� �޼ҵ�� ����Ʈ�� 0��° Į���� üũ�ڽ� Į���� �߰��մϴ�
		/// </summary>
		/// <param name="grid">�׸��尳ü�� ID</param>
        public static void AddCheckBoxColumn(GridView grid, string langType)
		{
			//ihj proj
			AddCheckBoxColumn(grid, 0 ,true,"",langType);
		}

		#endregion
        

		#region ������ �÷��� Header�� Sorting ��Ÿ������ �����մϴ�.
		/// <summary>
		/// ������ �÷��� Header�� Sorting ��Ÿ������ �����մϴ�
		/// </summary>
		/// <param name="grid">�׸��尳ü�� ID</param>
		/// <param name="sortingHeaderKey">������ ����� �÷� Ű</param>
        public static void SetSortingColumnHeader(GridView grid, string[] sortingHeaderKey)
		{
            //if(sortingHeaderKey.Length>0)
            //{		
            //    for(int i = 0; i< sortingHeaderKey.Length; i++)
            //    {
            //        if(grid.Columns.FromKey(sortingHeaderKey[i]) != null)
            //        {
            //            grid.Columns.FromKey(sortingHeaderKey[i]).Header.Caption = "<a href = '#'>"+ grid.Columns.FromKey(sortingHeaderKey[i]).Header.Caption + "</a>";
            //        }
            //    }
            //}
		}
		#endregion
        		

		#region �׸��带 �⺻���� ��Ÿ�Ϸ� �������ݴϴ�(+1)(SetDefault)

		/// <summary>
		/// �׸��带 �⺻���� ��Ÿ�Ϸ� �������ݴϴ�(+1)
		/// </summary>
		/// <param name="grid">�׸��尳ü�� ID</param>
		/// <param name="width">int type width ��</param>
		/// <param name="height">int type height ��</param>
		/// <param name="sizeUnit">Size ���� ����(Pixel/Percentage)</param>
        public static void SetDefault(GridView grid, int width, int height, GridEnum.SizeUnit sizeUnit)
        {
            SetDefault(grid, width, height, sizeUnit, sizeUnit);
        }
		#endregion

		#region �׸��带 �⺻���� ��Ÿ�Ϸ� �������ݴϴ�(+2)(SetDefault)
		/// <summary>
		/// �׸��带 �⺻���� ��Ÿ�Ϸ� �������ݴϴ�(+2)
		/// </summary>
		/// <param name="grid">�׸��尳ü�� ID</param>
        public static void SetDefault(GridView grid)
		{
			SetDefault(grid, 0, 0, GridEnum.SizeUnit.Pixel);			
		}
		#endregion

		#region �׸��带 �⺻���� ��Ÿ�Ϸ� �������ݴϴ�(+3)(SetDefault)
		/// <summary>
		/// �׸��带 �⺻���� ��Ÿ�Ϸ� �������ݴϴ�(+3)
		/// </summary>
		/// <param name="grid"></param>
		/// <param name="width">int type px ��</param>
		/// <param name="height">int type px ��</param>		
        public static void SetDefault(GridView grid, int width, int height)
		{
			SetDefault(grid, width, height, GridEnum.SizeUnit.Pixel);
		}

		#endregion

        

		#region ������ ���� ��ũ�� �̴ϴ�
			/// <summary>
		/// ������ ���� ��ũ�� �̴ϴ�
		/// </summary>
		/// <param name="grid">�׸��尳ü�� ID</param>
		/// <param name="column">Į���� ��ġ(����)</param>
		/// <param name="row">�ο��� ��ġ</param>
		/// <param name="linkURL">��ũ�� �������� URL</param>
		/// <param name="paramNames">��ũ�� �߰��� �Ķ������ �̸�</param>
		/// <param name="paramValues">�Ķ���Ϳ� �ش��ϴ� ��</param>
		/// <param name="linkMethod">��ũ�� �Ŵ� ���</param>
		/// <param name="targetFrame">Ÿ�� ������</param>
		/// <param name="scriptParam">��ũ��Ʈ �Ķ����</param>
		/// <param name="modalFeature">��� �Ӽ�</param>
		/// <param name="scriptLanguage">��ũ��Ʈ�� ���</param>
		public static void SetLinkCell(GridView grid, int column, int row, string linkURL, string[] paramNames, 
			string[] paramValues, GridEnum.LinkMethod linkMethod, string targetFrame, string[] scriptParam, 
			string modalFeature, GridEnum.ScriptLanguage scriptLanguage)
		{
			string strPreValue = string.Empty;
			string strChangedValue = string.Empty;
			string strFullLink = string.Empty;
			string strFullParam = string.Empty;
			string strLinkMethodParam = string.Empty;

			try
			{
				if((int)linkMethod == 0 || (int)linkMethod == 2 || (int)linkMethod == 3)
				{
					strFullParam = string.Empty;
					strFullLink = string.Empty;

					if(paramNames != null && paramValues != null)
					{
						for(int j=0;j<paramNames.Length;j++)
						{		
							string strParamValue = string.Empty;

							//"Column5"�� ���·� Į���� ������ ���
							if(paramValues[j].Length > 6 && paramValues[j].Substring(0,6).ToUpper() == "COLUMN")
							{
								string strColParam = paramValues[j];
								int startIndex = 6;
								int endIndex = paramValues[j].Length - 6;
								strColParam = strColParam.Substring(startIndex, endIndex);

								//Į���� �߸������ؼ� Į���� ���� ���� �ֱ� ������
								if(grid.Columns.Count > Convert.ToInt32(strColParam))
								{
									//FillGrid ���� �� ���� Value �� �Ȱ��� Tag �� �־�����
									//���߿� �߰��� ���� �ֱ⶧����....
                                    //if(grid.Rows[row].Cells[Convert.ToInt32(strColParam)].Tag != null)
                                    //{
                                    //    strParamValue = grid.Rows[row].Cells[Convert.ToInt32(strColParam)].Tag.ToString();
                                    //}
                                    //else
                                    //{
                                    //    string strTemp = grid.Rows[row].Cells[Convert.ToInt32(strColParam)].Text;
                                    //    grid.Rows[row].Cells[Convert.ToInt32(strColParam)].Tag = strTemp;
                                    //    strParamValue = strTemp;
                                    //}
								}
								else
								{
									throw new Exception("");
									//throw new Exception(KumhoAsiana.Telepia.FrameWork.TelepiaDictionary.GetMessage(SubSystemType.Common,"0046")[3]);//�Ķ���ͷ� ������ Į���� �����ϴ�
								}
							}
							else
							{
								strParamValue = paramValues[j];
							}

							strFullParam += paramNames[j];
							strFullParam += "=";
							if(strParamValue != null && !strParamValue.EndsWith(string.Empty))
							{
								strFullParam += strParamValue.Trim();
							}
							else
							{
								strFullParam += strParamValue;
							}

							if(j != paramNames.Length-1)
							{
								strFullParam += "&";
							}
						}
					}

					if((int)linkMethod == 0)
					{
						if(targetFrame != null && targetFrame != string.Empty)
						{
							strLinkMethodParam = "Frame";
						}
						else
						{
							strLinkMethodParam = "Self";
						}
					}
					else if((int)linkMethod == 0)
					{
						strLinkMethodParam = "Modal";
					}
					else
					{
						strLinkMethodParam = "Popup";
					}

					strFullLink = " onclick=\"javascript:fn_GridCellLink_onClick('";
					strFullLink = strFullLink + strLinkMethodParam + "','";

					if(strFullParam != null && strFullParam != string.Empty)
					{
						strFullLink += linkURL + "','" + strFullParam.Trim() + "','";
					}
					else
					{
						strFullLink += linkURL + "','" + strFullParam + "','";
					}

					strFullLink += modalFeature + "','" + targetFrame + "');";
				}
				else if((int)linkMethod == 1)
				{
					strFullLink = string.Empty;
 
					if(scriptParam != null)
					{
						if((int)scriptLanguage == 0 || (int)scriptLanguage == 1)
						{
							strFullLink = "javascript:" + linkURL + "('";
						}
						else
						{
							strFullLink = "vbscript:" + linkURL + "('";
						}

						for(int k=0;k<scriptParam.Length;k++)
						{
							string strParamValue = string.Empty;

							if(scriptParam[k].Length > 6 && scriptParam[k].Substring(0,6).ToUpper() == "COLUMN")
							{
								string strColParam = scriptParam[k];
								int startIndex = 6;
								int endIndex = scriptParam[k].Length - 6;
								strColParam = strColParam.Substring(startIndex, endIndex);

								//Į���� �߸������ؼ� Į���� ���� ���� �ֱ� ������
								if(grid.Columns.Count > Convert.ToInt32(strColParam))
								{
									//FillGrid ���� �� ���� Value �� �Ȱ��� Tag �� �־�����
									//���߿� �߰��� ���� �ֱ⶧����....
                                    //if(grid.Rows[row].Cells[Convert.ToInt32(strColParam)].Tag != null)
                                    //{
                                    //    strParamValue = grid.Rows[row].Cells[Convert.ToInt32(strColParam)].Tag.ToString();
                                    //}
                                    //else
                                    //{
                                    //    string strTemp = grid.Rows[row].Cells[Convert.ToInt32(strColParam)].Text;
                                    //    grid.Rows[row].Cells[Convert.ToInt32(strColParam)].Tag = strTemp;
                                    //    strParamValue = strTemp;
                                    //}
								}
								else
								{
									throw new Exception("�Ķ���ͷ� ������ Į���� �����ϴ�");
								}
							}
							else
							{
								strParamValue = scriptParam[k];
							}

							strFullLink += strParamValue;

							if(k != scriptParam.Length -1)
							{
								strFullLink += "','";
							}
							else
							{
								if((int)scriptLanguage == 0 || (int)scriptLanguage == 1)
								{
									strFullLink += "');";
								}
								else
								{
									strFullLink += "')";
								}
							}
						}
					}

					strFullLink = " onclick=\"" + strFullLink;
				}
				
                //strPreValue = grid.Rows[row].Cells[column].Text;
                //strChangedValue = "<a href='#'" + strFullLink + "\" style=\"cursor:hand;underline\">";
                //strChangedValue += strPreValue;
                //strChangedValue += "</a>";

                //grid.Rows[row].Cells[column].Text = strChangedValue;
                //grid.Rows[row].Cells[column].Tag = strPreValue;
			}
			catch(Exception ex)
			{
				throw ex;
			}
		}

		#endregion

		#region ������ ���� ��ũ�� �̴ϴ�(+1)
		
		/// <summary>
		/// ������ ���� ��ũ�� �̴ϴ�
		/// </summary>
		/// <param name="grid">�׸��尳ü�� ID</param>
		/// <param name="column">Į���� ��ġ(����)</param>
		/// <param name="row">�ο��� ��ġ</param>
		/// <param name="linkURL">��ũ�� �������� URL</param>
		/// <param name="paramNames">��ũ�� �߰��� �Ķ������ �̸�</param>
		/// <param name="paramValues">�Ķ���Ϳ� �ش��ϴ� ��</param>
		/// <param name="targetFrame">Ÿ�� ������</param>
		public static void SetLinkCell(GridView grid, int column, int row, string linkURL, string[] paramNames, 
			string[] paramValues, string targetFrame)
		{
			SetLinkCell(grid, column, row, linkURL, paramNames, paramValues, GridEnum.LinkMethod.URL, targetFrame, 
				null, null, GridEnum.ScriptLanguage.None);
		}

		#endregion

		#region ������ ���� ��ũ�� �̴ϴ�(+2)
		/// <summary>
		/// ������ ���� ��ũ�� �̴ϴ�
		/// </summary>
		/// <param name="grid">�׸��尳ü�� ID</param>
		/// <param name="column">Į���� ��ġ(����)</param>
		/// <param name="row">�ο��� ��ġ</param>
		/// <param name="linkURL">��ũ�� �������� URL</param>
		/// <param name="paramNames">��ũ�� �߰��� �Ķ������ �̸�</param>
		/// <param name="paramValues">�Ķ���Ϳ� �ش��ϴ� ��</param>
		/// <param name="linkMethod">��ũ�� �Ŵ� ���</param>
		public static void SetLinkCell(GridView grid, int column, int row, string linkURL, string[] paramNames, 
			string[] paramValues, GridEnum.LinkMethod linkMethod)
		{
			SetLinkCell(grid, column, row, linkURL, paramNames, paramValues, linkMethod, 
				null, null, null, GridEnum.ScriptLanguage.None);
		}

		#endregion

		#region ������ ���� ��ũ�� �̴ϴ�(+3)
		/// <summary>
		/// ������ ���� ��ũ�� �̴ϴ�
		/// </summary>
		/// <param name="grid">�׸��尳ü�� ID</param>
		/// <param name="column">Į���� ��ġ(����)</param>
		/// <param name="row">�ο��� ��ġ</param>
		/// <param name="linkURL">��ũ�� �������� URL</param>
		/// <param name="paramNames">��ũ�� �߰��� �Ķ������ �̸�</param>
		/// <param name="paramValues">�Ķ���Ϳ� �ش��ϴ� ��</param>
		public static void SetLinkCell(GridView grid, int column, int row, string linkURL, string[] paramNames, 
			string[] paramValues)
		{
			SetLinkCell(grid, column, row, linkURL, paramNames, paramValues, GridEnum.LinkMethod.URL, null, null, null, 
				GridEnum.ScriptLanguage.None);
		}

		#endregion

		#region ������ ���� ��ũ�� �̴ϴ�(+4)
		/// <summary>
		/// ������ ���� ��ũ�� �̴ϴ�
		/// </summary>
		/// <param name="grid">�׸��尳ü�� ID</param>
		/// <param name="column">Į���� ��ġ(����)</param>
		/// <param name="row">�ο��� ��ġ</param>
		/// <param name="linkURL">��ũ�� �������� URL</param>
		public static void SetLinkCell(GridView grid, int column, int row, string linkURL)
		{
			SetLinkCell(grid, column, row, linkURL, null, null, GridEnum.LinkMethod.URL, null, null, null, GridEnum.ScriptLanguage.None);
		}

		#endregion

		#region ������ ���� ��ũ�� �̴ϴ�(���������)
		/// <summary>
		/// ������ ���� ��ũ�� �̴ϴ�(���������)
		/// </summary>
		/// <param name="grid">�׸��尳ü�� ID</param>
		/// <param name="column">Į���� ��ġ(����)</param>
		/// <param name="row">�ο��� ��ġ</param>
		/// <param name="linkURL">��ũ�� �������� URL</param>
		/// <param name="paramNames">��ũ�� �߰��� �Ķ������ �̸�</param>
		/// <param name="paramValues">�Ķ���Ϳ� �ش��ϴ� ��</param>
		/// <param name="modalFeature">��� �Ӽ�</param>
		public static void SetModalLinkCell(GridView grid, int column, int row, string linkURL, string[] paramNames, 
			string[] paramValues, string modalFeature)
		{
			SetLinkCell(grid, column, row, linkURL, paramNames, paramValues, GridEnum.LinkMethod.Modal, null, null, 
				modalFeature, GridEnum.ScriptLanguage.None);
		}

		#endregion

		#region ������ ���� ��ũ�� �̴ϴ�(��ũ��Ʈ)

		/// <summary>
		/// ������ ���� ��ũ�� �̴ϴ�(��ũ��Ʈ)
		/// </summary>		
		/// <param name="grid">�׸��尳ü�� ID</param>
		/// <param name="column">Į���� ��ġ(����)</param>
		/// <param name="row">�ο��� ��ġ</param>
		/// <param name="linkURL">��ũ�� �������� URL</param>
		/// <param name="scriptParam">��ũ��Ʈ �Ķ����</param>
		/// <param name="scriptLanguage">��ũ��Ʈ�� ���</param>

		public static void SetScriptLinkCell(GridView grid, int column, int row, string linkURL, string[] scriptParam, 
			GridEnum.ScriptLanguage scriptLanguage)
		{
			SetLinkCell(grid, column, row, linkURL, null, null, GridEnum.LinkMethod.Script, null, scriptParam, null, scriptLanguage);
		}

		#endregion

		#region ������ ���� ��ũ�� �̴ϴ�(��ũ��Ʈ)(+1)
		/// <summary>
		/// ������ ���� ��ũ�� �̴ϴ�(��ũ��Ʈ)
		/// </summary>
		/// <param name="grid">�׸��尳ü�� ID</param>
		/// <param name="column">Į���� ��ġ(����)</param>
		/// <param name="row">�ο��� ��ġ</param>
		/// <param name="linkURL">��ũ�� �������� URL</param>
		/// <param name="scriptParam">��ũ��Ʈ �Ķ����</param>
		public static void SetScriptLinkCell(GridView grid, int column, int row, string linkURL, string[] scriptParam)
		{
			SetLinkCell(grid, column, row, linkURL, null, null, GridEnum.LinkMethod.Script, null, scriptParam, null, 
				GridEnum.ScriptLanguage.Javascript);
		}

		#endregion

		#region ������ Į���� ��ũ�� �̴ϴ�
		/// <summary>
		/// ������ Į���� ��ũ�� �̴ϴ�
		/// </summary>
		/// <param name="grid">�׸��尳ü�� ID</param>
		/// <param name="column">Į���� ��ġ(����)</param>
		/// <param name="linkURL">��ũ�� �������� URL</param>
		/// <param name="paramNames">��ũ�� �߰��� �Ķ������ �̸�</param>
		/// <param name="paramValues">�Ķ���Ϳ� �ش��ϴ� ��</param>
		/// <param name="linkMethod">��ũ�� �Ŵ� ���</param>
		/// <param name="targetFrame">Ÿ�� ������</param>
		/// <param name="scriptParam">��ũ��Ʈ �Ķ����</param>
		/// <param name="modalFeature">��� �Ӽ�</param>
		/// <param name="scriptLanguage">��ũ��Ʈ�� ���</param>
		public static void SetLinkColumn(GridView grid, int column, string linkURL, string[] paramNames, string[] paramValues, 
			GridEnum.LinkMethod linkMethod, string targetFrame, string[] scriptParam, string modalFeature, GridEnum.ScriptLanguage scriptLanguage)
		{
			try
			{
				//ekw
				for(int i=0;i<grid.Columns.Count;i++)
				{
					SetLinkCell(grid, column, i, linkURL, paramNames, paramValues, linkMethod, targetFrame, 
						scriptParam, modalFeature, scriptLanguage);
				}
			}
			catch(Exception ex)
			{
				throw ex;
			}
		}

		#endregion
        
		#region ������ Į���� ��ũ�� �̴ϴ�(+1)
		/// <summary>
		/// ������ Į���� ��ũ�� �̴ϴ�
		/// </summary>		
		/// <param name="grid">�׸��尳ü�� ID</param>
		/// <param name="column">Į���� ��ġ(����)</param>
		/// <param name="linkURL">��ũ�� �������� URL</param>
		/// <param name="paramNames">��ũ�� �߰��� �Ķ������ �̸�</param>
		/// <param name="paramValues">�Ķ���Ϳ� �ش��ϴ� ��</param>
		/// <param name="targetFrame">Ÿ�� ������</param>
		public static void SetLinkColumn(GridView grid, int column, string linkURL, string[] paramNames, string[] paramValues, string targetFrame)
		{
			SetLinkColumn(grid, column, linkURL, paramNames, paramValues, GridEnum.LinkMethod.URL, targetFrame, 
				null, null, GridEnum.ScriptLanguage.None);
		}

		#endregion

		#region ������ Į���� ��ũ�� �̴ϴ�(+2)

		/// <summary>
		/// ������ Į���� ��ũ�� �̴ϴ�
		/// </summary>
		/// <param name="grid">�׸��尳ü�� ID</param>
		/// <param name="column">Į���� ��ġ(����)</param>
		/// <param name="linkURL">��ũ�� �������� URL</param>
		/// <param name="paramNames">��ũ�� �߰��� �Ķ������ �̸�</param>
		/// <param name="paramValues">�Ķ���Ϳ� �ش��ϴ� ��</param>
		/// <param name="linkMethod">��ũ�� �Ŵ� ���</param>
		public static void SetLinkColumn(GridView grid, int column, string linkURL, string[] paramNames, string[] paramValues, 
			GridEnum.LinkMethod linkMethod)
		{
			
			SetLinkColumn(grid, column, linkURL, paramNames, paramValues, linkMethod, null, null, null, GridEnum.ScriptLanguage.None);
		}

		#endregion

		#region ������ Į���� ��ũ�� �̴ϴ�(+3)

		/// <summary>
		/// ������ Į���� ��ũ�� �̴ϴ�
		/// </summary>
		/// <param name="grid">�׸��尳ü�� ID</param>
		/// <param name="column">Į���� ��ġ(����)</param>
		/// <param name="linkURL">��ũ�� �������� URL</param>
		/// <param name="paramNames">��ũ�� �߰��� �Ķ������ �̸�</param>
		/// <param name="paramValues">�Ķ���Ϳ� �ش��ϴ� ��</param>
		public static void SetLinkColumn(GridView grid, int column, string linkURL, string[] paramNames, string[] paramValues)
		{
			SetLinkColumn(grid, column, linkURL, paramNames, paramValues, GridEnum.LinkMethod.URL, null, null, null, GridEnum.ScriptLanguage.None);
		}

		#endregion

		#region ������ Į���� ��ũ�� �̴ϴ�(+4)

		/// <summary>
		/// ������ Į���� ��ũ�� �̴ϴ�
		/// </summary>
		/// <param name="grid">�׸��尳ü�� ID</param>
		/// <param name="column">Į���� ��ġ(����)</param>
		/// <param name="linkURL">��ũ�� �������� URL</param>
		public static void SetLinkColumn(GridView grid, int column, string linkURL)
		{
			SetLinkColumn(grid, column, linkURL, null, null, GridEnum.LinkMethod.URL, null, null, null, GridEnum.ScriptLanguage.None);
		}

		#endregion

		#region ������ Į���� ��ũ�� �̴ϴ�(���������)

		/// <summary>
		/// ������ Į���� ��ũ�� �̴ϴ�(���������)
		/// </summary>
		/// <param name="grid">�׸��尳ü�� ID</param>
		/// <param name="column">Į���� ��ġ(����)</param>
		/// <param name="linkURL">��ũ�� �������� URL</param>
		/// <param name="paramNames">��ũ�� �߰��� �Ķ������ �̸�</param>
		/// <param name="paramValues">�Ķ���Ϳ� �ش��ϴ� ��</param>
		/// <param name="modalFeature">��� �Ӽ�</param>
		public static void SetModalLinkColumn(GridView grid, int column, string linkURL, string[] paramNames, string[] paramValues, 
			string modalFeature)
		{
			SetLinkColumn(grid, column, linkURL, paramNames, paramValues, GridEnum.LinkMethod.Modal, null, null, modalFeature, GridEnum.ScriptLanguage.None);
		}

		#endregion

		#region ������ Į���� ��ũ�� �̴ϴ�(�˾�������)
		/// <summary>
		/// ������ Į���� ��ũ�� �̴ϴ�(�˾�������)
		/// </summary>
		/// <param name="grid">�׸��尳ü�� ID</param>
		/// <param name="column">Į���� ��ġ(����)</param>
		/// <param name="linkURL">��ũ�� �������� URL</param>
		/// <param name="paramNames">��ũ�� �߰��� �Ķ������ �̸�</param>
		/// <param name="paramValues">�Ķ���Ϳ� �ش��ϴ� ��</param>
		/// <param name="popupFeature">�˾� �Ӽ�</param>
		public static void SetPopupLinkColumn(GridView grid, int column, string linkURL, string[] paramNames, string[] paramValues, 
			string popupFeature)
		{
			SetLinkColumn(grid, column, linkURL, paramNames, paramValues, GridEnum.LinkMethod.Popup, null, null, popupFeature, GridEnum.ScriptLanguage.None);
		}
		#endregion

		#region ������ Į���� ��ũ�� �̴ϴ�(��ũ��Ʈ ����)
		/// <summary>
		/// ��ũ�������� Script ����
		/// </summary>
		/// <param name="grid">�׸��� object</param>
		/// <param name="columnKey">�÷�key</param>
		/// <param name="functionName">�Լ���</param>
		public static void SetScriptLinkColumn(GridView grid, string columnKey, string functionName)
		{
            //grid.DisplayLayout.ClientSideEvents.CellClickHandler  =  functionName;

            
            //int iRowCount = grid.Rows.Count;
			
            //if(columnKey != null)
            //{
            //    for(int i=0; i < iRowCount; i++)
            //    {
            //        grid.Rows[i].Cells[""].V.FromKey(columnKey).Value = "<a href='#' style=\"cursor:hand\">" + grid.Rows[i].Cells.FromKey(columnKey).Value + "</a>";
            //    }
            //}
            //else
            //{
            //    throw new Exception(CFW.Common.Messaging.SearchMsg("FC0407", new string[]{columnKey}));
            //}
		}
		#endregion

		#region ������ Į���� �����մϴ�.
		/// <summary>
		/// ������ Į���� �����մϴ�.
		/// </summary>
		/// <param name="grid">�׸��� object</param>
		/// <param name="columnKey">�÷�key</param>
		public static void SetFixedColumn(GridView grid,string columnKey) 
		{
            //int iColCount = grid.Columns.Count;
            //grid.DisplayLayout.UseFixedHeaders = true;

            //int iFixCol = grid.Columns.FromKey(columnKey).Index;
            //for(int i = 0; i< iFixCol+1; i++)
            //{
            //    grid.Bands[0].Columns[i].Header.Fixed = true; 
            //}

            //for(int j = 0; j<iColCount; j++)
            //{
            //    grid.Bands[0].Columns[j].Header.Style.CssClass = "thtitle";
            //}
		}
		#endregion

		#region ������ Į���� ��ũ�� �̴ϴ�(��ũ��Ʈ)

		/// <summary>
		/// ������ Į���� ��ũ�� �̴ϴ�(��ũ��Ʈ)
		/// </summary>
		/// <param name="grid">�׸��尳ü�� ID</param>
		/// <param name="column">Į���� ��ġ(����)</param>
		/// <param name="linkURL">��ũ�� �������� URL</param>
		/// <param name="scriptParam">��ũ��Ʈ �Ķ����</param>
		/// <param name="scriptLanguage">��ũ��Ʈ�� ���</param>
		public static void SetScriptLinkColumn(GridView grid, int column, string linkURL, string[] scriptParam, GridEnum.ScriptLanguage scriptLanguage)
		{
			SetLinkColumn(grid, column, linkURL, null, null, GridEnum.LinkMethod.Script, null, scriptParam, null, scriptLanguage);
		}

		#endregion

		#region ������ Į���� ��ũ�� �̴ϴ�(��ũ��Ʈ)(+1)
		
		/// <summary>
		/// ������ Į���� ��ũ�� �̴ϴ�(��ũ��Ʈ)
		/// </summary>
		/// <param name="grid">�׸��尳ü�� ID</param>
		/// <param name="column">Į���� ��ġ(����)</param>
		/// <param name="javaScriptFunctionName">�ڹٽ�ũ��Ʈ �Լ� ��</param>
		/// <param name="scriptParam">��ũ��Ʈ �Ķ����</param>
		public static void SetScriptLinkColumn(GridView grid, int column, string javaScriptFunctionName, string[] scriptParam)
		{
			
			SetLinkColumn(grid, column, javaScriptFunctionName, null, null, GridEnum.LinkMethod.Script, null, scriptParam, null, GridEnum.ScriptLanguage.Javascript);
		}

		#endregion

		#region ������ Į���� üũ�ڽ� Į������ ����ϴ�

	
		/// <summary>
		/// ������ Į���� üũ�ڽ� Į������ ����ϴ�
		/// </summary>
		/// <param name="grid">�׸��尳ü�� ID</param>
		/// <param name="columnKeys">üũ�ڽ� Į������ ���� Į���� Ű��</param>
		/// <param name="selectedValue">���� ��</param>
		/// <param name="disabledValue">���� �Ұ� ��</param>
		/// <param name="onClickClientScriptMethodName">Ŭ�� �� ���� �� �ڹٽ�ũ��Ʈ �Լ� ��</param>
		/// <param name="headerMode">��� ǥ�� ���</param>
		public static void SetCheckBoxColumn(GridView grid, string [] columnKeys, string selectedValue, string disabledValue, string onClickClientScriptMethodName, GridEnum.CheckBoxHeaderMode headerMode)
		{

            System.Text.StringBuilder sb = new System.Text.StringBuilder(512);
            ////Infragistics.WebUI.UltraWebGrid.UltraGridColumn oColumn = null;
            //GridViewColumn oColumn = null;
			
			
            int columnCount = columnKeys.Length;
            for(int j = 0; j< columnCount; j++)
            {
            //    oColumn = grid.Rows[0].Columns.FromKey(columnKeys[j]);
                //for(int i=0;i<grid.Rows.Count;i++)
                //{
                //    if (i > 0) sb.Remove(0, sb.Length);

                //    sb.Append("<input class = 'checkbox' type=\"checkbox\"");
                //    sb.Append(" name=\"chk_");
                //    sb.Append(grid.ClientID);
                //    sb.Append("_");
                //    //sb.Append(columnKeys[j]);
                //    sb.Append(j);
                //    sb.Append("\"");
                //    sb.Append(" onclick=\"javascript:");
                //    sb.Append(onClickClientScriptMethodName);
                //    sb.Append("(this);\"");
                //    sb.Append(" value=\"");
                //    sb.Append(i.ToString());
                //    sb.Append("\"");


                //    if (grid.HeaderRow.Cells[j].Text == selectedValue)
                //        sb.Append(" checked>");
                //    else if (grid.HeaderRow.Cells[j].Text == disabledValue) sb.Append("  disabled>");
                //    else
                //        sb.Append(">");
                //    grid.HeaderRow.Cells[j].Text = sb.ToString();
                //}
                int i = 0;
                foreach (GridViewRow gvr in grid.Rows)
                {                    
                    sb.Remove(0, sb.Length);

                    sb.Append("<input class = 'checkbox' runat=\"server\" type=\"checkbox\"");
                    sb.Append(" name=\"chk_");
                    sb.Append(grid.ClientID);
                    sb.Append("_");
                    sb.Append(columnKeys[j]);
                    //sb.Append(j);
                    sb.Append("\"");
                    sb.Append(" id=\"chk_");
                    sb.Append(i);
                    //sb.Append(j);
                    sb.Append("\"");
                    sb.Append(" onclick=\"javascript:");
                    sb.Append(onClickClientScriptMethodName);
                    sb.Append("(this);\"");
                    sb.Append(" value=\"");
                    //sb.Append(i.ToString());
                    sb.Append("\"");


                    //if (grid.HeaderRow.Cells[j].Text == selectedValue)
                    //    sb.Append(" checked>");
                    //else if (grid.HeaderRow.Cells[j].Text == disabledValue) sb.Append("  disabled>");
                    //else
                        sb.Append(">");
                    gvr.Cells[0].Text = sb.ToString();
                    i++;
                }

                sb = null;
                if (headerMode == GridEnum.CheckBoxHeaderMode.None)
                {

                    //grid.Columns.FromKey(columnKeys[j]).HeaderText = "";
                    grid.HeaderRow.Cells[j].Text = "";

                }
                else if (headerMode == GridEnum.CheckBoxHeaderMode.CheckBoxAll)
                {

                    sb = new System.Text.StringBuilder();
                    sb.Append("<input class = 'checkbox' type='checkbox' onclick=\"javascript:fn_ToggleAllCheckBox('chk_");
                    sb.Append(grid.ClientID);
                    sb.Append("_");
                    sb.Append(columnKeys[j]);
                    sb.Append("', this.checked);\">");
                    grid.HeaderRow.Cells[j].Text = sb.ToString();

                }
                else if (headerMode == GridEnum.CheckBoxHeaderMode.Both)
                {
                    sb = new System.Text.StringBuilder();
                    sb.Append("<input class = 'checkbox' type='checkbox' onclick=\"javascript:fn_ToggleAllCheckBox('chk_");
                    sb.Append(grid.ClientID);
                    sb.Append("_");
                    sb.Append(columnKeys[j]);
                    sb.Append("', this.checked);\">&nbsp;");
                    grid.HeaderRow.Cells[j].Text = sb.ToString() + grid.HeaderRow.Cells[j].Text;

                }
            }

		}

		#endregion

		

		#region ������ư Į���� �׸��忡 �߰��մϴ�

//		/// <summary>
//		/// ������ư Į���� �׸��忡 �߰��մϴ�
//		/// </summary>
//		/// <param name="grid">�׸� �尳ü�� ID</param>
//		/// <param name="column">������ư Į������ ���� Į���� ��ȣ</param>
//		/// <param name="selectedRow">ó�� �ε��ɶ� ������ư�� üũ�ǰ� �� Row�� ��ȣ</param>
//		public static void AddRadioButtonColumn(GridView grid, int column, int selectedRow)
//		{
//			Infragistics.WebUI.UltraWebGrid.UltraGridColumn oColumn = null;
//
//			try
//			{
//				//ekw
//				oColumn = new Infragistics.WebUI.UltraWebGrid.UltraGridColumn();
//				oColumn.HeaderText = "";
//				grid.Columns.Insert(column, oColumn);
//
//				for(int i=0;i<grid.Rows.Count;i++)
//				{
//					string strColText = null;
//					strColText = "<input class = \"INPUT\" type=\"radio\" id=\"rdo_" + grid.ClientID + "_" + i.ToString() + "\"";
//					strColText = strColText + " name=\"rdo_" + grid.ClientID + "\"";
//					int j = i+1;
//					strColText = strColText + " value=\"" + j.ToString() + "\"";
//
//					if(i == selectedRow)
//					{
//						strColText += " checked>";
//					}
//					else
//					{
//						strColText = strColText + ">";
//					}
//
//					grid.Rows[i].Cells[column].Text = strColText;
//				}
//			}
//			catch(Exception ex)
//			{
//				throw ex;
//			}
//			finally
//			{
//				oColumn = null;
//			}
//		}


		/// <summary>
		/// ������ư Į���� �׸��忡 �߰��մϴ�
		/// </summary>
		/// <param name="grid">�׸� �尳ü�� ID</param>
		/// <param name="column">������ư Į������ ���� Į���� �ε���</param>
		/// <param name="selectedRow">���� �� �ο� �ε���</param>
		/// <param name="headerText">��� �ؽ�Ʈ</param>
		/// <param name="onClickClientScriptMethodName">Ŭ�� �� ����� ��ũ��Ʈ �Լ� �̸�</param>
		// 2007.09.13 by ������. �ý��� ������� ���� ��� �߰� ( langType ) 
		public static void AddRadioButtonColumn(GridView grid, int column, int selectedRow, string headerText, string onClickClientScriptMethodName,string langType)
		{
            //Infragistics.WebUI.UltraWebGrid.UltraGridColumn oColumn = null;

            //System.Text.StringBuilder sb = new System.Text.StringBuilder(255);
            //int nRowCount = grid.Rows.Count;
			
            //try
            //{	
            //    oColumn = new Infragistics.WebUI.UltraWebGrid.UltraGridColumn();
            //    oColumn.HeaderText =  MES.FW.Common.Dictionary.SearchStaticDicWeb(headerText,langType);
            //    grid.Columns.Insert(column, oColumn);
            //    oColumn.Header.Style.CssClass = "thtitle2";
            //    int nColCount = grid.Columns.Count;
            //    if(nRowCount == 1 && grid.Rows[0].Tag != null )
            //    {
            //        if(grid.Rows[0].Tag.ToString() == "NoData")
            //        {
            //            string strNoData = grid.Rows[0].Cells[1].Text;
            //            for(int i=0;i<nColCount;i++)
            //            {										
            //                grid.Rows[0].Cells[i].Text = strNoData;
            //                grid.Rows[0].Cells[i].Style.HorizontalAlign = System.Web.UI.WebControls.HorizontalAlign.Center;
            //                grid.Rows[0].Cells[i].ColSpan = nColCount;					
            //            }
            //        }
            //    }
            //    else
            //    {

            //        for(int i=0;i<nRowCount;i++)
            //        {
            //            if (i > 0)
            //            {
            //                sb.Remove(0, sb.Length);
            //            }
            //            sb.Append("<input class = \"INPUT\" type=\"radio\" id=\"rdo_");
            //            sb.Append(grid.ClientID + "_" + i.ToString() + "\"");
            //            sb.Append(" name=\"rdo_" + grid.ClientID + "\"");
            //            if ((onClickClientScriptMethodName != null) && (onClickClientScriptMethodName.Length > 0))
            //            {
            //                sb.Append(" onclick=\"javascript:");
            //                sb.Append(onClickClientScriptMethodName);
            //                sb.Append("(this);\"");
            //            }
            //            //int j = i+1;
            //            sb.Append(" value=\"" + ((i + 1)).ToString() + "\"");
            //            if (i == selectedRow)
            //            {
            //                sb.Append(" checked>");
            //            }
            //            else
            //            {
            //                sb.Append(">");
            //            }
            //            grid.Rows[i].Cells[column].Text = sb.ToString();
            //        }
            //    }
            //}
            //catch(Exception ex)
            //{
            //    throw ex;
            //}
            //finally
            //{
            //    oColumn = null;
            //}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="grid">�׸� �尳ü�� ID</param>
		/// <param name="column">������ư Į������ ���� Į���� �ε���</param>
		/// <param name="headerText">��� �ؽ�Ʈ</param>
		/// <param name="onClickClientScriptMethodName">Ŭ�� �� ����� ��ũ��Ʈ �Լ� �̸�</param>
		/// <param name="columnKey">���� �÷� Ű��</param>
		/// <param name="selectedValue">���� ��</param>
		/// <param name="disabledValue">���� �Ұ� ��</param>
		/// 
	    // 2007.09.13 by ������. �ý��� ������� ���� ��� �߰� ( langType ) 
		public static void AddRadioButtonColumn(GridView grid, int column
			  , string headerText, string onClickClientScriptMethodName, string columnKey, string selectedValue, string disabledValue,string langType)
		{
            //int nRowCount = grid.Rows.Count;
			
            //Infragistics.WebUI.UltraWebGrid.UltraGridColumn oColumn = null;

            //System.Text.StringBuilder sb = new System.Text.StringBuilder(255);
            //try
            //{	
            //    oColumn = new Infragistics.WebUI.UltraWebGrid.UltraGridColumn();
            //    oColumn.HeaderText =   MES.FW.Common.Dictionary.SearchStaticDicWeb(headerText,langType);
            //    grid.Columns.Insert(column, oColumn);
            //    oColumn.Header.Style.CssClass = "thtitle2";
            //    int nColCount = grid.Columns.Count;

            //    if(nRowCount == 1 && grid.Rows[0].Tag != null )
            //    {
            //        if(grid.Rows[0].Tag.ToString() == "NoData")
            //        {
            //            string strNoData = grid.Rows[0].Cells[1].Text;
            //            for(int i=0;i<nColCount;i++)
            //            {										
            //                grid.Rows[0].Cells[i].Text = strNoData;
            //                grid.Rows[0].Cells[i].Style.HorizontalAlign = System.Web.UI.WebControls.HorizontalAlign.Center;
            //                grid.Rows[0].Cells[i].ColSpan = nColCount;					
            //            }
            //        }
            //    }
            //    else
            //    {
            //        for(int i=0;i<nRowCount;i++)
            //        {
            //            if (i > 0)
            //            {
            //                sb.Remove(0, sb.Length);
            //            }
            //            sb.Append("<input class = \"INPUT\" type=\"radio\" id=\"rdo_");
            //            sb.Append(grid.ClientID + "_" + i.ToString() + "\"");
            //            sb.Append(" name=\"rdo_" + grid.ClientID + "\"");
            //            if ((onClickClientScriptMethodName != null) && (onClickClientScriptMethodName.Length > 0))
            //            {
            //                sb.Append(" onclick=\"javascript:");
            //                sb.Append(onClickClientScriptMethodName);
            //                sb.Append("(this);\"");
            //            }
            //            //int j = i+1;
            //            sb.Append(" value=\"" + ((i + 1)).ToString() + "\"");
					
				
            //            if (grid.Rows[i].Cells.FromKey(columnKey).Text  == selectedValue) sb.Append(" checked>");
            //            else if(grid.Rows[i].Cells.FromKey(columnKey).Text  == disabledValue) sb.Append("  disabled>");
            //            else sb.Append(">");	
            //            grid.Rows[i].Cells[column].Text = sb.ToString();
            //        }
            //    }
            //}
            //catch(Exception ex)
            //{
            //    throw ex;
            //}
            //finally
            //{
            //    oColumn = null;
            //}
		}



		#endregion
		
		#region ������ư Į������ �����մϴ�
  

		/// <summary>
		/// ������ư Į������ �����մϴ�.
		/// </summary>
		/// <param name="grid">�׸��尳ü�� ID</param>
		/// <param name="columnKey">Į�� KEY</param>
		/// <param name="selectedValue">���� �� ��</param>
		/// <param name="disabledValue">���� �Ұ� ��</param>
		/// <param name="onClickClientScriptMethodName">Ŭ�� �� ����� ��ũ��Ʈ �Լ� �̸�</param>
		public static void SetRadioButtonColumn(GridView grid, string columnKey, string selectedValue, string disabledValue, string onClickClientScriptMethodName)
		{
            //System.Text.StringBuilder sb = new System.Text.StringBuilder(512);
            //Infragistics.WebUI.UltraWebGrid.UltraGridColumn oColumn = grid.Bands[0].Columns.FromKey(columnKey);
            //oColumn.Width = 25;
	
            ////oColumn.HeaderText = "";  

            //for(int i=0;i<grid.Rows.Count;i++)
            //{
            //    if (i > 0) sb.Remove(0, sb.Length);

            //    sb.Append("<input class = 'INPUT' type=\"radio\"");
            //    sb.Append(" name=\"rdo_");
            //    sb.Append(grid.ClientID);
            //    sb.Append("\"");

            //    if(onClickClientScriptMethodName != null && onClickClientScriptMethodName.Length>0)
            //    {
            //        sb.Append(" onclick=\"javascript:");
            //        sb.Append(onClickClientScriptMethodName);
            //        sb.Append("(this);\"");
            //    }
            //    sb.Append(" value=\"");
            //    sb.Append(i.ToString());
            //    sb.Append("\"");

            //    if (grid.Rows[i].Cells.FromKey(columnKey).Text  == selectedValue) sb.Append(" checked>");
            //    if (grid.Rows[i].Cells.FromKey(columnKey).Text  == disabledValue) sb.Append(" disabled>");
            //    else sb.Append(">");				
            //    grid.Rows[i].Cells.FromKey(columnKey).Text =sb.ToString();
			
            //    //grid.Rows[i].Cells[column].Style.HorizontalAlign = System.Web.UI.WebControls.HorizontalAlign.Center;
            //}
		}

		#endregion

		

		#region ���콺 ���� ��Ÿ���� ���� �Ǿ� ������ �ȴ�

		/// <summary>
		/// ���콺 ���� ��Ÿ���� ���� �Ǿ� ������ �ȴ�.
		/// �ۼ��� - �蹮��(��Ÿ�ý���)
		/// </summary>
		/// <param name="defaultRowCSS">�Ϲ� ��Ÿ��</param>
		/// <param name="mouseOverCSS">���콺 ���� ��Ÿ��</param>
		private static void GridLinkCellTRStyle(string defaultRowCSS, string mouseOverCSS)
		{
			strDefaultCss = defaultRowCSS;
			strOverCss = mouseOverCSS;
		}
		#endregion

		#region �׸����� ����� ������ �ʰ� �մϴ�

		/// <summary>
		/// �׸����� ����� ������ �ʰ� �մϴ�
		/// </summary>
		/// <param name="grid">�׸��� ��ü�� ID</param>
		public static void SetHeaderInvisible(GridView grid)
		{
			try
			{
				//ekw
				//grid.Style.DisplayLayout.HeaderStyleDefault.CustomRules = "Display:None";
			}
			catch(Exception ex)
			{
				throw ex;
			}
		}

		#endregion

        #endregion

    }
}

