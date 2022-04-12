using System;
using System.ComponentModel;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Specialized;

namespace CFW.Web
{
	/// <summary>
	/// ImageTextButton에 대한 요약 설명입니다.
	/// </summary>
	[DefaultProperty("Text"), ToolboxData("<{0}:ImageTextButton runat=server></{0}:ImageTextButton>")]
	public class ImageTextButton : System.Web.UI.WebControls.WebControl, IPostBackEventHandler
	{
		#region 변수

		private string m_Text = "Button";
		private string m_ClientClick = string.Empty;
		private string m_Class = "btn01";
		private bool m_RightEnable = true;
		private Unit m_Width = 0;
		private System.Web.UI.WebControls.HorizontalAlign m_TextAlign = System.Web.UI.WebControls.HorizontalAlign.Left;
		
		#endregion

		#region 이벤트구현 부분

		private static readonly object EventClick = new object();

		/// <summary>
		/// 클릭 이벤트
		/// </summary>
		public event ClickEventHandler Click
		{
			add { Events.AddHandler(EventClick, value); }
			remove { Events.RemoveHandler(EventClick, value); }
		}

		/// <summary>
		/// ClickEventHandler 대리자
		/// </summary>
		public delegate void ClickEventHandler(object sender, EventArgs e);

		/// <summary>
		/// 온클릭 이벤트
		/// </summary>
		/// <param name="e">이벤트 인수</param>
		protected virtual void OnClick(EventArgs e)
		{
			ClickEventHandler handler = (ClickEventHandler)Events[EventClick];
			if (handler != null) handler(this,e);
		}

	
		/// <summary>
		/// 포스트 백 이벤트
		/// </summary>
		/// <param name="eventArgument"></param>
		public void RaisePostBackEvent(string eventArgument)
		{
			OnClick(new EventArgs());
		}

		#endregion

		#region 프로퍼티

		/// <summary>
		/// 버튼 Text
		/// </summary>
		[Bindable(true), Category("Appearance"), DefaultValue("Button"), Description("버튼 Text")] 
		public string Text
		{
			get
			{
				if(this.m_Text == null) { this.m_Text = "Button"; }
				return m_Text;
			}

			set { this.m_Text = value; }
		}

		/// <summary>
		/// 버튼 너비
		/// </summary>
		[Bindable(true), Category("Appearance"), DefaultValue(0), Description("ImageTextButton의 너비입니다.")] 
		public override Unit  Width
		{
			get	{return m_Width;}

			set { this.m_Width = value; }
		}


		/// <summary>
		/// 버튼 Text Align
		/// </summary>
		[Bindable(true), Category("Appearance"), DefaultValue(System.Web.UI.WebControls.HorizontalAlign.Left), Description("Text 정렬입니다.")] 
		public System.Web.UI.WebControls.HorizontalAlign ImageTextAlign
		{
			get	{return m_TextAlign;}

			set { this.m_TextAlign = value; }
		}


        

		/// <summary>
		/// 버튼 권한 활성 여부
		/// </summary>
		[Bindable(true), Category("Appearance"), DefaultValue(true), Description("버튼 활성화")] 
		public bool RightEnable
		{
			get{ return this.m_RightEnable;	}

			set { this.m_RightEnable = value; }
		}


		/// <summary>
		/// 버튼 스타일 시트
		/// </summary>
		[Bindable(true), Category("Appearance"), DefaultValue("btn01"), Description("ImageTextButton에 적용되는 CSS 클래스 이름입니다.")] 
		public string CssClass
		{
			get
			{
				if(this.m_Class == null) { this.m_Class = "btn01"; }
				return m_Class;
			}

			set { this.m_Class = value; }
		}
        

		/// <summary>
		/// 버튼 클라이언트 스크립트
		/// </summary>
		[Bindable(true), Category("Behavior"), DefaultValue(""), Description("클라이언트 스크립트")] 
		public string ClientClick
		{
			get
			{
				if(this.m_ClientClick == null) { this.m_ClientClick = string.Empty; }
				return m_ClientClick;
			}

			set { this.m_ClientClick = value; }
		}

		#endregion

		#region 렌더링

		/// <summary> 
		/// 이 컨트롤을 지정한 출력 매개 변수로 렌더링합니다.
		/// </summary>
		/// <param name="writer"> 출력을 쓸 HTML 작성기 </param>
		protected override void RenderContents(HtmlTextWriter writer)
		{

			if(this.m_RightEnable)
			{

					
					writer.Write("<table border=\"0\" cellspacing=\"0\" cellpadding=\"0\">\r\n");
					writer.Write("  <tr onclick=\"javascript:");
					if (this.m_ClientClick.Length > 0)
					{
						writer.Write("if (");
						writer.Write(this.m_ClientClick);
						writer.Write(")\r\n");
						writer.Write(Page.GetPostBackEventReference(this));
					}
					else
					writer.Write(Page.GetPostBackEventReference(this));
					writer.Write("\"");		
					writer.Write(" onmouseover=\"style.cursor='hand'\" onmouseout=\"style.cursor='auto';\">\r\n");
					writer.Write("    <td class=\"");
					writer.Write(this.m_Class);
					writer.Write("\" width = \"");//Width설정
					writer.Write(this.m_Width);
					writer.Write("\" align = \"");//TextAlign 설정
					writer.Write(this.m_TextAlign);
					writer.Write("\" onmouseover=\"style.color='blue';\" onmouseout=\"style.color='black';\">");
					writer.Write(this.m_Text);
					writer.Write("</td>\r\n");
					writer.Write("<td class=\"btn_back \"></td>");
					writer.Write("  </tr>\r\n");
					writer.Write("</table>");
				}
				else
				{
					writer.Write("<table border=\"0\" cellspacing=\"0\" cellpadding=\"0\">\r\n");
					writer.Write("  <tr style.cursor='auto'; >\r\n");
					writer.Write("    <td class=\"");
					writer.Write(this.m_Class);
					writer.Write("\" width = \"");//Width설정
					writer.Write(this.m_Width);
					writer.Write("\" align = \"");//TextAlign 설정
					writer.Write(this.m_TextAlign);
					writer.Write("\" style.filter = 'progid:DXImageTransform.Microsoft.Alpha(opacity=30)';>");
					writer.Write(this.m_Text);
					writer.Write("</td>\r\n");
					writer.Write("<td class=\"btn_back \"></td>");
					writer.Write("  </tr>\r\n");
					writer.Write("</table>");
				}			

			base.RenderContents(writer);
		}

		#endregion

		#region  ViewState에 속성 SAVE & LOAD

		/// <summary>
		/// View State 속성을 Load합니다.
		/// </summary>
		/// <param name="savedState"></param>
		protected override void LoadViewState(object savedState)
		{
			if (savedState != null) 
			{
				object[] state = (object[])savedState;
				
				base.LoadViewState(state[0]);
				this.m_Text = (string)state[1];
				this.m_ClientClick = (string)state[2];
				this.m_Class = (string)state[3];
			}
		}

		/// <summary>
		/// View State 속성을 Save합니다.
		/// </summary>
		/// <returns></returns>
		protected override object SaveViewState()
		{
			object[] state = new object[6];
			
			state[0] = base.SaveViewState();
			state[1] = this.m_Text;
			state[2] = this.m_ClientClick;
			state[3] = this.m_Class;

			return (object)state;
		}

		#endregion
	}
}
