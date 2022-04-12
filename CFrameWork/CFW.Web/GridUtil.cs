using System;
using System.Data;
using System.Web.UI;
using System.Reflection;
using System.Web.UI.WebControls;
using CFW.Web; 


namespace CFW.Web
{
	/// <summary>
	/// GridUtil에 대한 설명입니다.
	/// </summary>
	///

	public class GridUtil : System.Web.UI.Page
	{
		#region 전역 변수 선언
		private static string strDefaultCss = string.Empty;
		private static string strOverCss = string.Empty;
		
		#endregion

	    #region 생성자
		/// <summary>
		/// GridUtil 생성자 입니다.
		/// </summary>
		public GridUtil()
		{
		
		}

		#endregion
		
        //#region 상수선언

        //private const string CUSTOM_SCRIPT_FILENAME = "../Scripts/Control/UltraWebGrid.js";

        //private const string CUSTOM_SCRIPT_COMMON_FILENAME = "../Scripts/Control/ControlCSOM.js";

        //private const string CUSTOM_GRID_IMAGE_DIRECTORY = "../Images/GridImages/";
		
        //#endregion


        #region CFW용 재정의

        #region 그리드에 데이터를 바인딩해서 채웁니다

        /// <summary>
        /// 그리드에 데이터를 바인딩해서 채웁니다
        /// </summary>
        /// <param name="grid">그리드개체의 ID</param>
        /// <param name="dataSource">데이터 소스(데이터셋, 데이터 테이블 등..)</param>
        public static void FillGrid(GridView grid, object dataSource, string langtype)
        {
            FillGrid(grid, dataSource, langtype, null);
        }

        #endregion

        #region 그리드에 데이터를 바인딩해서 채웁니다-특정컬럼 암호화(+1)

        /// <summary>
        /// 특정컬럼을 암호화하여 그리드에 데이터를 바인딩해서 채웁니다.
        /// </summary>
        /// <param name="grid">그리드개체의 ID</param>
        /// <param name="dataSource">데이터 소스(데이터셋, 데이터 테이블 등..)</param>
        /// <param name="cryptographyColumnName">암호화 할 컬럼 이름</param>
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
                    SetGridNoData(grid, CFW.Common.Messaging.SearchMsgWEB("FC0306", langtype));//"조회된 Data가 존재 하지 않습니다"
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

        #region 특정칼럼 암호화

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


        #region 그리드에 데이터가 없을 때 헤더는 놔두고, 하나의 row 에 지정한 메시지를 출력하는 메소드입니다

        /// <summary>
        /// 그리드에 데이터가 없을 때 헤더는 놔두고, 하나의 row 에 지정한 메시지를 출력하는 메소드입니다
        /// </summary>
        /// <param name="grid">그리드개체의 ID</param>
        /// <param name="noDataMessage">출력할 메시지</param>
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
                        //    //    //NoData 시 1024*768 기준으로 중심에 정렬될 수 있도록 스타일 설정 추가
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

        #region 그리드의 헤더를 설정합니다

        /// <summary>
        /// 그리드의 헤더를 설정합니다
        /// </summary>
        /// <param name="grid">그리드개체의 ID</param>
        /// <param name="headerTexts">헤더 텍스트(칼럼 수 만큼의 string 배열)</param>
        /// <param name="width">각 칼럼의 넓이(칼럼 수 만큼의 int 배열)</param>
        /// <param name="sizeUnit">넓이의 단위(pixel or percent)</param>
        /// <param name="columnCssClasses">각 칼럼별 CSS 클래스이름(칼럼 수 만큼의 string 배열)</param>
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

        #region 그리드를 기본적인 스타일로 설정해줍니다(SetDefault)

        /// <summary>
        /// 그리드를 기본적인 스타일로 설정해줍니다
        /// </summary>
        /// <param name="grid">그리드개체의 ID</param>
        /// <param name="width">int type width 값</param>
        /// <param name="height">int type height 값</param>
        /// <param name="widthSizeUnit">Width Size 단위 설정(Pixel/Percentage)</param>
        /// <param name="heightSizeUnit">Height Size 단위 설정(Pixel/Percentage)</param>
        public static void SetDefault(GridView grid, int width, int height, GridEnum.SizeUnit widthSizeUnit, GridEnum.SizeUnit heightSizeUnit)
        {
            try
            {
                //*****************************************************************************************************
                // 공통 그리드 Layout 설정
                // 1. 소팅
                // 2. 컬럼이동
                // 3. 셀 넓이 조정
                // 4. 기본 CSS
                // 5. 그리드 멀티 선택 기능
                // 6. 컬럼 고정
                // 7. 컬럼 제목 명을 풍선도움말로.
                // 8. CSS 설정 (Table CSS, 마우스 over CSS, 마우스 out CSS)
                //*****************************************************************************************************


                // 1. 컬럼 소팅 기능 (싱글 소팅) --> Data가 많을 시 속도가 느리다. -> 사용안함
                //grid.DisplayLayout.AllowSortingDefault = Infragistics.WebUI.UltraWebGrid.AllowSorting.No;
                //grid.DisplayLayout.HeaderClickActionDefault = Infragistics.WebUI.UltraWebGrid.HeaderClickAction.NotSet;



                // 2. 컬럼 이동 기능 삭제
                //grid.DisplayLayout.AllowColumnMovingDefault = AllowColumnMoving.None;
                //grid.DisplayLayout.Bands[0].AllowColumnMoving = Infragistics.WebUI.UltraWebGrid.AllowColumnMoving.None;

                // 3. 셀 넓이 조정 기능 추가
                //				grid.DisplayLayout.AllowColSizingDefault = Infragistics.WebUI.UltraWebGrid.AllowSizing.Free;

                //grid.CellPadding = 0;
                //grid.CellSpacing = 0;
                //grid.BorderWidth = 0;

                //4.기본 css

                //grid.CssClass = "thTitle2 center";


                //				grid.DisplayLayout.TableLayout = Infragistics.WebUI.UltraWebGrid.TableLayout.Fixed;
                //				grid.DisplayLayout.StationaryMargins = Infragistics.WebUI.UltraWebGrid.StationaryMargins.Header;
                //로우 선택 X
                //grid.DisplayLayout.ActivationObject.AllowActivation = false;

                ////grid.DisplayLayout.ActiveCell.AllowEditing = Infragistics.WebUI.UltraWebGrid.AllowEditing.No;
                //grid.DisplayLayout.CellClickActionDefault = Infragistics.WebUI.UltraWebGrid.CellClickAction.NotSet;

                //// 삭제 불가
                //grid.DisplayLayout.AllowDeleteDefault = AllowDelete.No;

                //Header 스타일
                //				System.Web.UI.WebControls.Unit oUnit
                //					= new System.Web.UI.WebControls.Unit(300, System.Web.UI.WebControls.UnitType.Pixel);
                //				grid.Width = oUnit;

                //				grid.DisplayLayout.HeaderStyleDefault.VerticalAlign = System.Web.UI.WebControls.VerticalAlign.Middle;
                //				grid.DisplayLayout.HeaderStyleDefault.CssClass = "bgtitle";
                //				grid.DisplayLayout.ViewType  = Infragistics.WebUI.UltraWebGrid.ViewType.Flat;

                //로우 셀렉터를 디저블
                //grid.DisplayLayout.RowSelectorsDefault = Infragistics.WebUI.UltraWebGrid.RowSelectors.No;



                // Activation False
                //				grid.DisplayLayout.SelectTypeRowDefault = SelectType.Extended;
                //				grid.DisplayLayout.ActivationObject.AllowActivation = true;
                //				grid.DisplayLayout.ActivationObject.BorderStyle = System.Web.UI.WebControls.BorderStyle.Solid;
                //				grid.DisplayLayout.ActivationObject.BorderWidth = System.Web.UI.WebControls.Unit.Pixel(0);				

                // SizeUnit 설정
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

                // 헤더 Fix
                //grid.DisplayLayout.StationaryMargins = Infragistics.WebUI.UltraWebGrid.StationaryMargins.Header;
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion


        #region 그리드에 스타일시트를 지정합니다


        /// <summary>
        /// 그리드에 마우스오버 스타일시트를 지정합니다 
        /// </summary>
        /// <param name="grid">그리드개체의 ID</param>
        /// <param name="tableCSS">테이블 전체 클래스</param>
        /// <param name="mouseOverCSS">그리드에서 마우스를 올렸을 때 TR 태그의 클래스</param>
        /// <param name="defaultRowCSS">TR에 디폴트로 들어가는 클래스</param>
        /// <param name="alternateRowCSS">열 구분을 위해 들어가는 클래스</param>
        /// <param name="isFixed">열 고정 기능 사용 여부</param>
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

		#region 그리드의 헤더를 설정합니다(+1)

		/// <summary>
		/// 그리드의 헤더를 설정합니다
		/// </summary>
		/// <param name="grid">그리드개체의 ID</param>
		/// <param name="headerTexts">헤더 텍스트(칼럼 수 만큼의 string 배열)</param>
		/// <param name="width">각 칼럼의 넓이(칼럼 수 만큼의 int 배열)</param>
		/// <param name="sizeUnit">넓이의 단위(pixel or percent)</param>
		public static void SetHeader(GridView grid, string[] headerTexts, int[] width, GridEnum.SizeUnit sizeUnit, string  langType)
		{
			SetHeader(grid, headerTexts, width, sizeUnit, null,langType);
		}

		#endregion

		#region 그리드의 헤더를 설정합니다(+2)

		/// <summary>
		/// 
		/// </summary>
		/// <param name="grid">그리드개체의 ID</param>
		/// <param name="headerTexts">헤더 텍스트(칼럼 수 만큼의 string 배열)</param>
		/// <param name="columnCssClasses">각 칼럼별 CSS 클래스이름(칼럼 수 만큼의 string 배열)</param>
		public static void SetHeader(GridView grid, string[] headerTexts, string[] columnCssClasses,string  langType)
		{
			SetHeader(grid, headerTexts, null, GridEnum.SizeUnit.Percent, columnCssClasses, langType);
		}

		#endregion

		#region 그리드의 헤더를 설정합니다(+3)

        /// <summary>
        /// 그리드의 헤더를 설정합니다
        /// </summary>
        /// <param name="grid">그리드개체의 ID</param>
        /// <param name="headerTexts">헤더 텍스트(칼럼 수 만큼의 string 배열)</param>
        /// <param name="columnCssClasses">각 칼럼별 CSS 클래스이름(칼럼 수 만큼의 string 배열)</param>
        //public static void SetHeader(GridView grid, string[] headerTexts, string[] columnCssClasses)
        //{
        //    SetHeader(grid, headerTexts, null, GridEnum.SizeUnit.Percent, columnCssClasses);
        //}

        //public static void SetHeader(GridView grid, string[] headerTexts, string[] columnCssClasses, int defaultSortColumn)
        //{
        //    SetHeader(grid, headerTexts, null, GridEnum.SizeUnit.Percent, defaultSortColumn, columnCssClasses);
        //}

		#endregion

		#region 지정한 칼럼을 히든으로 만듭니다(숨깁니다)

		/// <summary>
		/// 지정한 칼럼을 히든으로 만듭니다(숨깁니다)
		/// </summary>
		/// <param name="Grid">그리드개체의 ID</param>
		/// <param name="Columns">히든으로 할 칼럼의 위치</param>
		/// <example>
		/// 이 예제는 그리드의 1,3 번째 칼럼을 히든으로 만듭니다
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
                    //칼럼이 있을때만 하는겁니다
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

		#region 지정한 칼럼을 히든으로 만듭니다(+1)
		/// <summary>
		/// 지정한 칼럼을 히든으로 만듭니다(숨깁니다)
		/// </summary>
		/// <param name="Grid">그리드개체의 ID</param>
		/// <param name="Columns">히든으로 할 칼럼의 Key Value</param>
		/// <example>
		/// 이 예제는 그리드의 NO,CD 칼럼을 히든으로 만듭니다
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

        #region 사용안함

        #region 그리드에 체크박스 칼럼을 추가합니다

        /// <summary>
		/// 그리드에 체크박스 칼럼을 추가합니다
		/// </summary>
		/// <param name="grid">그리드개체의 ID</param>
		/// <param name="column">체크박스 칼럼을 추가할 위치(정수)</param>
		/// <param name="useCheckBox">헤더에 체크박스 표시 여부</param>
		/// <param name="headerText">헤더 텍스트</param>
		/// <param name="onClickClientScriptSignature">클릭 시 실행된 스크립트 함수 이름</param>
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
		/// 그리드에 체크박스 칼럼을 추가합니다
		/// </summary>
		/// <param name="grid">그리드개체의 ID</param>
		/// <param name="column">체크박스 칼럼을 추가할 위치(정수)</param>
		/// <param name="useCheckBox">헤더에 체크박스 표시 여부</param>
		/// <param name="headerText">헤더 텍스트</param>
		/// <param name="onClickClientScriptSignature">클릭 시 실행 될 자바스크립트 함수 명</param>
		/// <param name="columnKey">기준 컬럼 키 값</param>
		/// <param name="selectedValue">선택 값</param>
		/// <param name="disabledValue">선택 불가 값</param>
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
		/// 그리드에 체크박스 칼럼을 추가합니다
		/// </summary>
		/// <param name="grid">그리드개체의 ID</param>
		/// <param name="column">체크박스 칼럼을 추가할 위치(정수)</param>
		/// <param name="useCheckBox">헤더에 CheckBox 표시 여부</param>
		/// <param name="headerText">헤더 텍스트</param>
		/// <example>
		/// 이 예제는 UserListGrid 라는 ID 의 그리드의 1번째 칼럼에 체크박스 칼럼을 추가하는 샘플입니다
		/// <code>
		/// MES.FW.WEB.GridUtil.AddCheckBoxColumn(UserListGrid, 1, true,"Model");
		/// </code>
		/// </example>
        public static void AddCheckBoxColumn(GridView grid, int column, bool useCheckBox, string headerText, string langType)
		{
			AddCheckBoxColumn(grid, column, useCheckBox, headerText,"",langType);
		}

		#endregion

		#region 그리드에 체크박스 칼럼을 추가합니다(+1)

		/// <summary>
		/// 그리드에 체크박스 칼럼을 추가합니다
		/// 오버로드된 버전입니다
		/// 이 메소드는 디폴트로 0번째 칼럼에 체크박스 칼럼을 추가합니다
		/// </summary>
		/// <param name="grid">그리드개체의 ID</param>
        public static void AddCheckBoxColumn(GridView grid, string langType)
		{
			//ihj proj
			AddCheckBoxColumn(grid, 0 ,true,"",langType);
		}

		#endregion
        

		#region 지정한 컬럼의 Header를 Sorting 스타일으로 설정합니다.
		/// <summary>
		/// 지정한 컬럼의 Header를 Sorting 스타일으로 설정합니다
		/// </summary>
		/// <param name="grid">그리드개체의 ID</param>
		/// <param name="sortingHeaderKey">정렬을 사용할 컬럼 키</param>
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
        		

		#region 그리드를 기본적인 스타일로 설정해줍니다(+1)(SetDefault)

		/// <summary>
		/// 그리드를 기본적인 스타일로 설정해줍니다(+1)
		/// </summary>
		/// <param name="grid">그리드개체의 ID</param>
		/// <param name="width">int type width 값</param>
		/// <param name="height">int type height 값</param>
		/// <param name="sizeUnit">Size 단위 설정(Pixel/Percentage)</param>
        public static void SetDefault(GridView grid, int width, int height, GridEnum.SizeUnit sizeUnit)
        {
            SetDefault(grid, width, height, sizeUnit, sizeUnit);
        }
		#endregion

		#region 그리드를 기본적인 스타일로 설정해줍니다(+2)(SetDefault)
		/// <summary>
		/// 그리드를 기본적인 스타일로 설정해줍니다(+2)
		/// </summary>
		/// <param name="grid">그리드개체의 ID</param>
        public static void SetDefault(GridView grid)
		{
			SetDefault(grid, 0, 0, GridEnum.SizeUnit.Pixel);			
		}
		#endregion

		#region 그리드를 기본적인 스타일로 설정해줍니다(+3)(SetDefault)
		/// <summary>
		/// 그리드를 기본적인 스타일로 설정해줍니다(+3)
		/// </summary>
		/// <param name="grid"></param>
		/// <param name="width">int type px 값</param>
		/// <param name="height">int type px 값</param>		
        public static void SetDefault(GridView grid, int width, int height)
		{
			SetDefault(grid, width, height, GridEnum.SizeUnit.Pixel);
		}

		#endregion

        

		#region 지정한 셀에 링크를 겁니다
			/// <summary>
		/// 지정한 셀에 링크를 겁니다
		/// </summary>
		/// <param name="grid">그리드개체의 ID</param>
		/// <param name="column">칼럼의 위치(정수)</param>
		/// <param name="row">로우의 위치</param>
		/// <param name="linkURL">링크할 페이지의 URL</param>
		/// <param name="paramNames">링크에 추가될 파라미터의 이름</param>
		/// <param name="paramValues">파라미터에 해당하는 값</param>
		/// <param name="linkMethod">링크를 거는 방법</param>
		/// <param name="targetFrame">타겟 프레임</param>
		/// <param name="scriptParam">스크립트 파라미터</param>
		/// <param name="modalFeature">모달 속성</param>
		/// <param name="scriptLanguage">스크립트의 언어</param>
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

							//"Column5"의 형태로 칼럼을 지정한 경우
							if(paramValues[j].Length > 6 && paramValues[j].Substring(0,6).ToUpper() == "COLUMN")
							{
								string strColParam = paramValues[j];
								int startIndex = 6;
								int endIndex = paramValues[j].Length - 6;
								strColParam = strColParam.Substring(startIndex, endIndex);

								//칼럼을 잘못지정해서 칼럼이 없을 수도 있기 때문에
								if(grid.Columns.Count > Convert.ToInt32(strColParam))
								{
									//FillGrid 에서 각 셀에 Value 와 똑같이 Tag 를 넣었지만
									//나중에 추가한 셀도 있기때문에....
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
									//throw new Exception(KumhoAsiana.Telepia.FrameWork.TelepiaDictionary.GetMessage(SubSystemType.Common,"0046")[3]);//파라미터로 지정한 칼럼이 없습니다
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

								//칼럼을 잘못지정해서 칼럼이 없을 수도 있기 때문에
								if(grid.Columns.Count > Convert.ToInt32(strColParam))
								{
									//FillGrid 에서 각 셀에 Value 와 똑같이 Tag 를 넣었지만
									//나중에 추가한 셀도 있기때문에....
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
									throw new Exception("파라미터로 지정한 칼럼이 없습니다");
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

		#region 지정한 셀에 링크를 겁니다(+1)
		
		/// <summary>
		/// 지정한 셀에 링크를 겁니다
		/// </summary>
		/// <param name="grid">그리드개체의 ID</param>
		/// <param name="column">칼럼의 위치(정수)</param>
		/// <param name="row">로우의 위치</param>
		/// <param name="linkURL">링크할 페이지의 URL</param>
		/// <param name="paramNames">링크에 추가될 파라미터의 이름</param>
		/// <param name="paramValues">파라미터에 해당하는 값</param>
		/// <param name="targetFrame">타겟 프레임</param>
		public static void SetLinkCell(GridView grid, int column, int row, string linkURL, string[] paramNames, 
			string[] paramValues, string targetFrame)
		{
			SetLinkCell(grid, column, row, linkURL, paramNames, paramValues, GridEnum.LinkMethod.URL, targetFrame, 
				null, null, GridEnum.ScriptLanguage.None);
		}

		#endregion

		#region 지정한 셀에 링크를 겁니다(+2)
		/// <summary>
		/// 지정한 셀에 링크를 겁니다
		/// </summary>
		/// <param name="grid">그리드개체의 ID</param>
		/// <param name="column">칼럼의 위치(정수)</param>
		/// <param name="row">로우의 위치</param>
		/// <param name="linkURL">링크할 페이지의 URL</param>
		/// <param name="paramNames">링크에 추가될 파라미터의 이름</param>
		/// <param name="paramValues">파라미터에 해당하는 값</param>
		/// <param name="linkMethod">링크를 거는 방법</param>
		public static void SetLinkCell(GridView grid, int column, int row, string linkURL, string[] paramNames, 
			string[] paramValues, GridEnum.LinkMethod linkMethod)
		{
			SetLinkCell(grid, column, row, linkURL, paramNames, paramValues, linkMethod, 
				null, null, null, GridEnum.ScriptLanguage.None);
		}

		#endregion

		#region 지정한 셀에 링크를 겁니다(+3)
		/// <summary>
		/// 지정한 셀에 링크를 겁니다
		/// </summary>
		/// <param name="grid">그리드개체의 ID</param>
		/// <param name="column">칼럼의 위치(정수)</param>
		/// <param name="row">로우의 위치</param>
		/// <param name="linkURL">링크할 페이지의 URL</param>
		/// <param name="paramNames">링크에 추가될 파라미터의 이름</param>
		/// <param name="paramValues">파라미터에 해당하는 값</param>
		public static void SetLinkCell(GridView grid, int column, int row, string linkURL, string[] paramNames, 
			string[] paramValues)
		{
			SetLinkCell(grid, column, row, linkURL, paramNames, paramValues, GridEnum.LinkMethod.URL, null, null, null, 
				GridEnum.ScriptLanguage.None);
		}

		#endregion

		#region 지정한 셀에 링크를 겁니다(+4)
		/// <summary>
		/// 지정한 셀에 링크를 겁니다
		/// </summary>
		/// <param name="grid">그리드개체의 ID</param>
		/// <param name="column">칼럼의 위치(정수)</param>
		/// <param name="row">로우의 위치</param>
		/// <param name="linkURL">링크할 페이지의 URL</param>
		public static void SetLinkCell(GridView grid, int column, int row, string linkURL)
		{
			SetLinkCell(grid, column, row, linkURL, null, null, GridEnum.LinkMethod.URL, null, null, null, GridEnum.ScriptLanguage.None);
		}

		#endregion

		#region 지정한 셀에 링크를 겁니다(모달윈도우)
		/// <summary>
		/// 지정한 셀에 링크를 겁니다(모달윈도우)
		/// </summary>
		/// <param name="grid">그리드개체의 ID</param>
		/// <param name="column">칼럼의 위치(정수)</param>
		/// <param name="row">로우의 위치</param>
		/// <param name="linkURL">링크할 페이지의 URL</param>
		/// <param name="paramNames">링크에 추가될 파라미터의 이름</param>
		/// <param name="paramValues">파라미터에 해당하는 값</param>
		/// <param name="modalFeature">모달 속성</param>
		public static void SetModalLinkCell(GridView grid, int column, int row, string linkURL, string[] paramNames, 
			string[] paramValues, string modalFeature)
		{
			SetLinkCell(grid, column, row, linkURL, paramNames, paramValues, GridEnum.LinkMethod.Modal, null, null, 
				modalFeature, GridEnum.ScriptLanguage.None);
		}

		#endregion

		#region 지정한 셀에 링크를 겁니다(스크립트)

		/// <summary>
		/// 지정한 셀에 링크를 겁니다(스크립트)
		/// </summary>		
		/// <param name="grid">그리드개체의 ID</param>
		/// <param name="column">칼럼의 위치(정수)</param>
		/// <param name="row">로우의 위치</param>
		/// <param name="linkURL">링크할 페이지의 URL</param>
		/// <param name="scriptParam">스크립트 파라미터</param>
		/// <param name="scriptLanguage">스크립트의 언어</param>

		public static void SetScriptLinkCell(GridView grid, int column, int row, string linkURL, string[] scriptParam, 
			GridEnum.ScriptLanguage scriptLanguage)
		{
			SetLinkCell(grid, column, row, linkURL, null, null, GridEnum.LinkMethod.Script, null, scriptParam, null, scriptLanguage);
		}

		#endregion

		#region 지정한 셀에 링크를 겁니다(스크립트)(+1)
		/// <summary>
		/// 지정한 셀에 링크를 겁니다(스크립트)
		/// </summary>
		/// <param name="grid">그리드개체의 ID</param>
		/// <param name="column">칼럼의 위치(정수)</param>
		/// <param name="row">로우의 위치</param>
		/// <param name="linkURL">링크할 페이지의 URL</param>
		/// <param name="scriptParam">스크립트 파라미터</param>
		public static void SetScriptLinkCell(GridView grid, int column, int row, string linkURL, string[] scriptParam)
		{
			SetLinkCell(grid, column, row, linkURL, null, null, GridEnum.LinkMethod.Script, null, scriptParam, null, 
				GridEnum.ScriptLanguage.Javascript);
		}

		#endregion

		#region 지정한 칼럼에 링크를 겁니다
		/// <summary>
		/// 지정한 칼럼에 링크를 겁니다
		/// </summary>
		/// <param name="grid">그리드개체의 ID</param>
		/// <param name="column">칼럼의 위치(정수)</param>
		/// <param name="linkURL">링크할 페이지의 URL</param>
		/// <param name="paramNames">링크에 추가될 파라미터의 이름</param>
		/// <param name="paramValues">파라미터에 해당하는 값</param>
		/// <param name="linkMethod">링크를 거는 방법</param>
		/// <param name="targetFrame">타겟 프레임</param>
		/// <param name="scriptParam">스크립트 파라미터</param>
		/// <param name="modalFeature">모달 속성</param>
		/// <param name="scriptLanguage">스크립트의 언어</param>
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
        
		#region 지정한 칼럼에 링크를 겁니다(+1)
		/// <summary>
		/// 지정한 칼럼에 링크를 겁니다
		/// </summary>		
		/// <param name="grid">그리드개체의 ID</param>
		/// <param name="column">칼럼의 위치(정수)</param>
		/// <param name="linkURL">링크할 페이지의 URL</param>
		/// <param name="paramNames">링크에 추가될 파라미터의 이름</param>
		/// <param name="paramValues">파라미터에 해당하는 값</param>
		/// <param name="targetFrame">타겟 프레임</param>
		public static void SetLinkColumn(GridView grid, int column, string linkURL, string[] paramNames, string[] paramValues, string targetFrame)
		{
			SetLinkColumn(grid, column, linkURL, paramNames, paramValues, GridEnum.LinkMethod.URL, targetFrame, 
				null, null, GridEnum.ScriptLanguage.None);
		}

		#endregion

		#region 지정한 칼럼에 링크를 겁니다(+2)

		/// <summary>
		/// 지정한 칼럼에 링크를 겁니다
		/// </summary>
		/// <param name="grid">그리드개체의 ID</param>
		/// <param name="column">칼럼의 위치(정수)</param>
		/// <param name="linkURL">링크할 페이지의 URL</param>
		/// <param name="paramNames">링크에 추가될 파라미터의 이름</param>
		/// <param name="paramValues">파라미터에 해당하는 값</param>
		/// <param name="linkMethod">링크를 거는 방법</param>
		public static void SetLinkColumn(GridView grid, int column, string linkURL, string[] paramNames, string[] paramValues, 
			GridEnum.LinkMethod linkMethod)
		{
			
			SetLinkColumn(grid, column, linkURL, paramNames, paramValues, linkMethod, null, null, null, GridEnum.ScriptLanguage.None);
		}

		#endregion

		#region 지정한 칼럼에 링크를 겁니다(+3)

		/// <summary>
		/// 지정한 칼럼에 링크를 겁니다
		/// </summary>
		/// <param name="grid">그리드개체의 ID</param>
		/// <param name="column">칼럼의 위치(정수)</param>
		/// <param name="linkURL">링크할 페이지의 URL</param>
		/// <param name="paramNames">링크에 추가될 파라미터의 이름</param>
		/// <param name="paramValues">파라미터에 해당하는 값</param>
		public static void SetLinkColumn(GridView grid, int column, string linkURL, string[] paramNames, string[] paramValues)
		{
			SetLinkColumn(grid, column, linkURL, paramNames, paramValues, GridEnum.LinkMethod.URL, null, null, null, GridEnum.ScriptLanguage.None);
		}

		#endregion

		#region 지정한 칼럼에 링크를 겁니다(+4)

		/// <summary>
		/// 지정한 칼럼에 링크를 겁니다
		/// </summary>
		/// <param name="grid">그리드개체의 ID</param>
		/// <param name="column">칼럼의 위치(정수)</param>
		/// <param name="linkURL">링크할 페이지의 URL</param>
		public static void SetLinkColumn(GridView grid, int column, string linkURL)
		{
			SetLinkColumn(grid, column, linkURL, null, null, GridEnum.LinkMethod.URL, null, null, null, GridEnum.ScriptLanguage.None);
		}

		#endregion

		#region 지정한 칼럼에 링크를 겁니다(모달윈도우)

		/// <summary>
		/// 지정한 칼럼에 링크를 겁니다(모달윈도우)
		/// </summary>
		/// <param name="grid">그리드개체의 ID</param>
		/// <param name="column">칼럼의 위치(정수)</param>
		/// <param name="linkURL">링크할 페이지의 URL</param>
		/// <param name="paramNames">링크에 추가될 파라미터의 이름</param>
		/// <param name="paramValues">파라미터에 해당하는 값</param>
		/// <param name="modalFeature">모달 속성</param>
		public static void SetModalLinkColumn(GridView grid, int column, string linkURL, string[] paramNames, string[] paramValues, 
			string modalFeature)
		{
			SetLinkColumn(grid, column, linkURL, paramNames, paramValues, GridEnum.LinkMethod.Modal, null, null, modalFeature, GridEnum.ScriptLanguage.None);
		}

		#endregion

		#region 지정한 칼럼에 링크를 겁니다(팝업윈도우)
		/// <summary>
		/// 지정한 칼럼에 링크를 겁니다(팝업윈도우)
		/// </summary>
		/// <param name="grid">그리드개체의 ID</param>
		/// <param name="column">칼럼의 위치(정수)</param>
		/// <param name="linkURL">링크할 페이지의 URL</param>
		/// <param name="paramNames">링크에 추가될 파라미터의 이름</param>
		/// <param name="paramValues">파라미터에 해당하는 값</param>
		/// <param name="popupFeature">팝업 속성</param>
		public static void SetPopupLinkColumn(GridView grid, int column, string linkURL, string[] paramNames, string[] paramValues, 
			string popupFeature)
		{
			SetLinkColumn(grid, column, linkURL, paramNames, paramValues, GridEnum.LinkMethod.Popup, null, null, popupFeature, GridEnum.ScriptLanguage.None);
		}
		#endregion

		#region 지정한 칼럼에 링크를 겁니다(스크립트 실행)
		/// <summary>
		/// 링크형식으로 Script 실행
		/// </summary>
		/// <param name="grid">그리드 object</param>
		/// <param name="columnKey">컬럼key</param>
		/// <param name="functionName">함수명</param>
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

		#region 지정한 칼럼을 고정합니다.
		/// <summary>
		/// 지정한 칼럼을 고정합니다.
		/// </summary>
		/// <param name="grid">그리드 object</param>
		/// <param name="columnKey">컬럼key</param>
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

		#region 지정한 칼럼에 링크를 겁니다(스크립트)

		/// <summary>
		/// 지정한 칼럼에 링크를 겁니다(스크립트)
		/// </summary>
		/// <param name="grid">그리드개체의 ID</param>
		/// <param name="column">칼럼의 위치(정수)</param>
		/// <param name="linkURL">링크할 페이지의 URL</param>
		/// <param name="scriptParam">스크립트 파라미터</param>
		/// <param name="scriptLanguage">스크립트의 언어</param>
		public static void SetScriptLinkColumn(GridView grid, int column, string linkURL, string[] scriptParam, GridEnum.ScriptLanguage scriptLanguage)
		{
			SetLinkColumn(grid, column, linkURL, null, null, GridEnum.LinkMethod.Script, null, scriptParam, null, scriptLanguage);
		}

		#endregion

		#region 지정한 칼럼에 링크를 겁니다(스크립트)(+1)
		
		/// <summary>
		/// 지정한 칼럼에 링크를 겁니다(스크립트)
		/// </summary>
		/// <param name="grid">그리드개체의 ID</param>
		/// <param name="column">칼럼의 위치(정수)</param>
		/// <param name="javaScriptFunctionName">자바스크립트 함수 명</param>
		/// <param name="scriptParam">스크립트 파라미터</param>
		public static void SetScriptLinkColumn(GridView grid, int column, string javaScriptFunctionName, string[] scriptParam)
		{
			
			SetLinkColumn(grid, column, javaScriptFunctionName, null, null, GridEnum.LinkMethod.Script, null, scriptParam, null, GridEnum.ScriptLanguage.Javascript);
		}

		#endregion

		#region 지정한 칼럼을 체크박스 칼럼으로 만듭니다

	
		/// <summary>
		/// 지정한 칼럼을 체크박스 칼럼으로 만듭니다
		/// </summary>
		/// <param name="grid">그리드개체의 ID</param>
		/// <param name="columnKeys">체크박스 칼럼으로 만들 칼럼의 키값</param>
		/// <param name="selectedValue">선택 값</param>
		/// <param name="disabledValue">선택 불가 값</param>
		/// <param name="onClickClientScriptMethodName">클릭 시 실행 될 자바스크립트 함수 명</param>
		/// <param name="headerMode">헤더 표시 모드</param>
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

		

		#region 라디오버튼 칼럼을 그리드에 추가합니다

//		/// <summary>
//		/// 라디오버튼 칼럼을 그리드에 추가합니다
//		/// </summary>
//		/// <param name="grid">그리 드개체의 ID</param>
//		/// <param name="column">라디오버튼 칼럼으로 만들 칼럼의 번호</param>
//		/// <param name="selectedRow">처음 로딩될때 라디오버튼이 체크되게 할 Row의 번호</param>
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
		/// 라디오버튼 칼럼을 그리드에 추가합니다
		/// </summary>
		/// <param name="grid">그리 드개체의 ID</param>
		/// <param name="column">라디오버튼 칼럼으로 만들 칼럼의 인덱스</param>
		/// <param name="selectedRow">선택 될 로우 인덱스</param>
		/// <param name="headerText">헤더 텍스트</param>
		/// <param name="onClickClientScriptMethodName">클릭 시 실행된 스크립트 함수 이름</param>
		// 2007.09.13 by 한주희. 시스템 사용자의 선택 언어 추가 ( langType ) 
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
		/// <param name="grid">그리 드개체의 ID</param>
		/// <param name="column">라디오버튼 칼럼으로 만들 칼럼의 인덱스</param>
		/// <param name="headerText">헤더 텍스트</param>
		/// <param name="onClickClientScriptMethodName">클릭 시 실행된 스크립트 함수 이름</param>
		/// <param name="columnKey">기준 컬럼 키값</param>
		/// <param name="selectedValue">선택 값</param>
		/// <param name="disabledValue">선택 불가 값</param>
		/// 
	    // 2007.09.13 by 한주희. 시스템 사용자의 선택 언어 추가 ( langType ) 
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
		
		#region 라디오버튼 칼럼으로 설정합니다
  

		/// <summary>
		/// 라디오버튼 칼럼으로 설정합니다.
		/// </summary>
		/// <param name="grid">그리드개체의 ID</param>
		/// <param name="columnKey">칼럼 KEY</param>
		/// <param name="selectedValue">선택 될 값</param>
		/// <param name="disabledValue">선택 불가 값</param>
		/// <param name="onClickClientScriptMethodName">클릭 시 실행된 스크립트 함수 이름</param>
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

		

		#region 마우스 오버 스타일이 지정 되어 있으면 된다

		/// <summary>
		/// 마우스 오버 스타일이 지정 되어 있으면 된다.
		/// 작성자 - 김문석(인타시스템)
		/// </summary>
		/// <param name="defaultRowCSS">일반 스타일</param>
		/// <param name="mouseOverCSS">마우스 오버 스타일</param>
		private static void GridLinkCellTRStyle(string defaultRowCSS, string mouseOverCSS)
		{
			strDefaultCss = defaultRowCSS;
			strOverCss = mouseOverCSS;
		}
		#endregion

		#region 그리드의 헤더를 보이지 않게 합니다

		/// <summary>
		/// 그리드의 헤더를 보이지 않게 합니다
		/// </summary>
		/// <param name="grid">그리드 개체의 ID</param>
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

