using System;
using System.Text;
using System.Data;
using System.Web.UI;
//using CFW.Web;
using CFW.Common;
//using CFW.Common.Messaging;

namespace CFW.Web
{
	/// <summary>
	/// ControlUtil에 대한 설명입니다.
	/// </summary>
    public class ControlUtil: System.Web.UI.Page
    {

		#region 페이지를 포함한 컨테이너의 컨트롤 속성을 초기화합니다.
        /// <summary>
        /// 페이지를 포함한 컨테이너의 컨트롤 속성을 초기화합니다.
        /// </summary>
        /// <param name="control">초기화할 컨테이터</param>
        public static void Clear(System.Web.UI.Control control)
        {
            string strType = string.Empty;
            foreach (System.Web.UI.Control ctrl in control.Controls)
            {
                strType = ctrl.GetType().Name;
                if (strType.Equals("HtmlInputText"))
                {
                    System.Web.UI.HtmlControls.HtmlInputText htxt = (System.Web.UI.HtmlControls.HtmlInputText)ctrl;
                    htxt.Value = null;
                }
                else if (strType.Equals("HtmlTextArea"))
                {
                    System.Web.UI.HtmlControls.HtmlTextArea htxta = (System.Web.UI.HtmlControls.HtmlTextArea)ctrl;
                    htxta.Value = null;
                }
                else if (strType.Equals("HtmlInputCheckBox"))
                {
                    System.Web.UI.HtmlControls.HtmlInputCheckBox hchk = (System.Web.UI.HtmlControls.HtmlInputCheckBox)ctrl;
                    hchk.Checked = false;
                }
                else if (strType.Equals("HtmlInputRadioButton"))
                {
                    System.Web.UI.HtmlControls.HtmlInputRadioButton hchk = (System.Web.UI.HtmlControls.HtmlInputRadioButton)ctrl;
                    hchk.Checked = false;
                }
                else if (strType.Equals("ListBox"))
                {
                    System.Web.UI.WebControls.ListBox lst = (System.Web.UI.WebControls.ListBox)ctrl;
                    lst.SelectedIndex = -1;
                }
                else if (strType.Equals("DropDownList"))
                {
                    System.Web.UI.WebControls.DropDownList ddl = (System.Web.UI.WebControls.DropDownList)ctrl;
                    ddl.SelectedIndex = -1;
                }/*
                else if (strType.Equals("WebDateTimeEdit"))
                {
                    Infragistics.WebUI.WebDataInput.WebDateTimeEdit udt = (Infragistics.WebUI.WebDataInput.WebDateTimeEdit)ctrl;
                    udt.Text = null;
                }
                else if (strType.Equals("WebDateChooser"))
                {
                    Infragistics.WebUI.WebSchedule.WebDateChooser udc = (Infragistics.WebUI.WebSchedule.WebDateChooser)ctrl;
                    udc.Value = null;
                    udc.NullDateLabel = "";
                }
                else if (strType.Equals("WebNumericEdit"))
                {
                    Infragistics.WebUI.WebDataInput.WebNumericEdit une = (Infragistics.WebUI.WebDataInput.WebNumericEdit)ctrl;
                    une.Text = null;
                }
                else if (strType.Equals("WebMaskEdit"))
                {
                    Infragistics.WebUI.WebDataInput.WebMaskEdit ume = (Infragistics.WebUI.WebDataInput.WebMaskEdit)ctrl;
                    ume.Text = null;
                }
                else if (strType.Equals("WebCombo"))
                {
                    Infragistics.WebUI.WebCombo.WebCombo ucb = (Infragistics.WebUI.WebCombo.WebCombo)ctrl;
                    ucb.SelectedIndex = -1;
                }*/
                else if (ctrl.Controls.Count > 0)
                {
                    Clear(ctrl);
                }
            }
        }
		#endregion


		#region 컨테이너에 속한 컨트롤에 데이터를 표시합니다.
        /// <summary>
        /// 컨테이너에 속한 컨트롤에 데이터를 표시합니다.
        /// </summary>
        /// <param name="dataRow">바인딩할 데이터 로우</param>
        /// <param name="container">바인딩할 컨테이너</param>
        public static void DataBind(DataRow dataRow, System.Web.UI.Control container)
        {
            foreach (System.Web.UI.Control ctrl in container.Controls)
            {
                if (ctrl.GetType() == typeof(System.Web.UI.HtmlControls.HtmlInputText))
                {
                    ((System.Web.UI.HtmlControls.HtmlInputText)ctrl).Value = dataRow[ctrl.ID.Substring(4)].ToString();  //htxt
                }
                else if (ctrl.GetType() == typeof(System.Web.UI.HtmlControls.HtmlTextArea))
                {
                    ((System.Web.UI.HtmlControls.HtmlTextArea)ctrl).Value = dataRow[ctrl.ID.Substring(5)].ToString(); //htxta
                }
                else if (ctrl.GetType() == typeof(System.Web.UI.HtmlControls.HtmlInputCheckBox))
                {
                    ((System.Web.UI.HtmlControls.HtmlInputCheckBox)ctrl).Value = dataRow[ctrl.ID.Substring(4)].ToString(); //hchk
                }
                else if (ctrl.GetType() == typeof(System.Web.UI.HtmlControls.HtmlInputRadioButton))
                {
                    ((System.Web.UI.HtmlControls.HtmlInputRadioButton)ctrl).Value = dataRow[ctrl.ID.Substring(4)].ToString(); //hrdo
                }
                else if (ctrl.GetType() == typeof(System.Web.UI.WebControls.ListBox))
                {
                    ((System.Web.UI.WebControls.ListBox)ctrl).SelectedValue = dataRow[ctrl.ID.Substring(3)].ToString(); //lst
                }
                else if (ctrl.GetType() == typeof(System.Web.UI.WebControls.DropDownList))
                {
                    ((System.Web.UI.WebControls.DropDownList)ctrl).SelectedValue = dataRow[ctrl.ID.Substring(3)].ToString(); //ddl
                }
                else if (ctrl.Controls.Count > 0)       //하위그룹박스가 존재하면 안으로 들어간다.
                {
                    DataBind(dataRow, ctrl);
                }
            }
        }

		#region ListBox Databinding
        /// <summary>
        /// System.Web.UI.WebControls.ListBox 컨트롤에 데이터를 바인딩합니다.
        /// </summary>
        /// <param name="dataTable">바인딩할 데이터 테이블입니다.</param>
        /// <param name="listBox">System.Web.UI.WebControls.ListBox 컨트롤</param>
        /// <param name="selectedValue">초기 선택된 값</param>
        public static void DataBind(DataTable dataTable, System.Web.UI.WebControls.ListBox listBox, string selectedValue)
        {
            if (listBox.DataValueField == null || listBox.DataValueField == "" || !dataTable.Columns.Contains(listBox.DataValueField))
                listBox.DataValueField = dataTable.Columns[0].ColumnName;
            if (listBox.DataTextField == null || listBox.DataTextField == "" || !dataTable.Columns.Contains(listBox.DataTextField))
                listBox.DataTextField = dataTable.Columns[1].ColumnName;
            listBox.DataSource = dataTable;
            listBox.DataBind();
            listBox.SelectedValue = selectedValue;
        }
		#endregion

		#region DropDownList Databinding
        /// <summary>
        /// System.Web.UI.WebControls.DropDownList 컨트롤에 데이터를 바인딩합니다.
        /// </summary>
        /// <param name="dataTable">바인딩할 데이터 테이블</param>
        /// <param name="dropDownList">System.Web.UI.WebControls.DropDownList 컨트롤</param>
        /// <param name="selectedValue">초기 선택 값</param>
        /// <param name="selectionMode">선택 모드</param>
        public static void DataBind(DataTable dataTable, System.Web.UI.WebControls.DropDownList dropDownList, string selectedValue, SelectionMode selectionMode,string langtype)
        {
            string strValueMember = dataTable.Columns[0].ColumnName;
            string strDisplayMember = dataTable.Columns[1].ColumnName;

			DataBind(dataTable,strValueMember,strDisplayMember,dropDownList,selectedValue,selectionMode, langtype);
        }
		
		/// <summary>
		/// System.Web.UI.WebControls.DropDownList 컨트롤에 데이터를 바인딩합니다.
		/// </summary>
		/// <param name="dataTable">바인딩할 데이터 테이블</param>
		/// <param name="ValueMember">ValueMember Name</param>
		/// <param name="DisplayMember">DisplayMember Name</param>
		/// <param name="dropDownList">System.Web.UI.WebControls.DropDownList 컨트롤</param>
		/// <param name="selectedValue">초기 선택 값</param>
		/// <param name="selectionMode">선택 모드</param>
		public static void DataBind(DataTable dataTable,string ValueMember,string DisplayMember, System.Web.UI.WebControls.DropDownList dropDownList, string selectedValue, SelectionMode selectionMode, string langtype)
		{
			string strValueMember = ValueMember;
			string strDisplayMember = DisplayMember;

			if (dropDownList.DataValueField == null || dropDownList.DataValueField == "" || !dataTable.Columns.Contains(dropDownList.DataValueField))
				dropDownList.DataValueField = strValueMember;
			if (dropDownList.DataTextField == null || dropDownList.DataTextField == "" || !dataTable.Columns.Contains(dropDownList.DataTextField))
				dropDownList.DataTextField = strDisplayMember;

			if ( selectionMode != SelectionMode.None )
			{
				SetSelectionMode(dataTable, strDisplayMember, strValueMember, selectionMode , langtype);
			}  

			dropDownList.DataSource = dataTable;
			dropDownList.DataBind();

			for (int i = 0; i < dropDownList.Items.Count; i++)
			{
				if (dropDownList.Items[i].Value.Equals(selectedValue))
				{
					dropDownList.SelectedValue = selectedValue;
					break;
				}
			}
		}
		#endregion
  
		#region RadioButtonList DataBinding
        /// <summary>
        /// System.Web.UI.WebControls.RadioButtonList 컨트롤에 데이터를 바인딩합니다.
        /// </summary>
        /// <param name="dataTable">바인딩할 데이터 테이블</param>
        /// <param name="radioButtonList">System.Web.UI.WebControls.RadioButtonList 컨트롤</param>
        /// <param name="selectedValue">선택된 값</param>
        public static void DataBind(DataTable dataTable, System.Web.UI.WebControls.RadioButtonList radioButtonList, string selectedValue)
        {
            if (radioButtonList.DataValueField == null || radioButtonList.DataValueField == "" || !dataTable.Columns.Contains(radioButtonList.DataValueField))
                radioButtonList.DataValueField = dataTable.Columns[0].ColumnName;
            if (radioButtonList.DataTextField == null || radioButtonList.DataTextField == "" || !dataTable.Columns.Contains(radioButtonList.DataTextField))
                radioButtonList.DataTextField = dataTable.Columns[1].ColumnName;

            radioButtonList.DataSource = dataTable;
            radioButtonList.DataBind();
            radioButtonList.SelectedValue = selectedValue;
        }

		#endregion

		#region CheckBoxList DataBinding
		/// <summary>
        /// System.Web.UI.WebControls.CheckBoxList 컨트롤에 데이터를 바인딩합니다.
        /// </summary>
        /// <param name="dataTable">바인딩할 데이터 테이블</param>
        /// <param name="checkBoxList">System.Web.UI.WebControls.CheckBoxList 컨트롤</param>
        /// <param name="selectedValue">선택된 값</param>
        public static void DataBind(DataTable dataTable, System.Web.UI.WebControls.CheckBoxList checkBoxList, string selectedValue)
        {
            if (checkBoxList.DataValueField == null || checkBoxList.DataValueField == "" || !dataTable.Columns.Contains(checkBoxList.DataValueField))
                checkBoxList.DataValueField = dataTable.Columns[0].ColumnName;
            if (checkBoxList.DataTextField == null || checkBoxList.DataTextField == "" || !dataTable.Columns.Contains(checkBoxList.DataTextField))
                checkBoxList.DataTextField = dataTable.Columns[1].ColumnName;

            checkBoxList.DataSource = dataTable;
            checkBoxList.DataBind();
            checkBoxList.SelectedValue = selectedValue;
        }
		#endregion

        
		#endregion


		#region 컨트롤에 데이터 표시할 때 선택 모드

        private static void SetSelectionMode(DataTable dataTable, string displayMember, string valueMember, SelectionMode selectionMode, string langtype)
        {
            string strAdditem = "";
            object oDefaultValue = "-1";

			
			PageBase oPage = new PageBase(); 

            switch (selectionMode)
            {
                case SelectionMode.All: strAdditem = CFW.Common.Messaging.SearchMsgWEB("COMMON.All", langtype); break;
                case SelectionMode.Select: strAdditem = CFW.Common.Messaging.SearchMsgWEB("COMMON.Select", langtype); break;
                default: return;
            }

            foreach (DataColumn col in dataTable.Columns)
                if (!col.AllowDBNull) col.AllowDBNull = true;

            DataRow row = dataTable.NewRow();

            if (displayMember != null && dataTable.Columns.Contains(displayMember))
                row[displayMember] = strAdditem;
            else
                row[1] = strAdditem;

            if (valueMember != null && dataTable.Columns.Contains(valueMember))
                row[valueMember] = oDefaultValue;
            else
                row[0] = oDefaultValue;

            dataTable.Rows.InsertAt(row, 0);
            dataTable.AcceptChanges();
        }

		#endregion


		#region Table의 컬럼에 ToolTip/말줄임표 추가

		/// <summary>
		/// Table의 컬럼에 ToolTip/말줄임표 추가
		/// </summary>
		/// <param name="strValue">컬럼의 Text값</param>
		/// <param name="intWidth">컬럼 Width</param>
		/// <returns></returns>
		public static string ShowToolTip(string strValue, int intWidth)
		{
			StringBuilder sbToolTip = new StringBuilder();
			if (strValue != null && strValue != string.Empty)
			{
				sbToolTip.Append("<span title=\'{0}\' style=\'text-overflow:ellipsis;overflow:hidden;width={1}px\'>");
				sbToolTip.Append("<nobr>{0}</nobr></span>");

				return string.Format(sbToolTip.ToString(), strValue, intWidth);
			}
			else
			{
				return "";
			}
		}

		#endregion
    			

	}
}
