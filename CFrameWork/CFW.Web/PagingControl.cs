using System;
using System.ComponentModel;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Specialized;

namespace CFW.Web
{

	#region 페이징Enum/클릭이벤트Args
	/// <summary>
	/// 페이징 클릭 이벤트 매개변수입니다
	/// </summary>
	public class PagingEventArgs : EventArgs
	{
		/// <summary>
		/// 페이지번호를 매개변수로 받는 생성자입니다
		/// </summary>
		/// <param name="iCurrPage"></param>
		public PagingEventArgs(int iCurrPage)
		{
			this._iCurrPage = iCurrPage;
		}

		private int _iCurrPage;

		/// <summary>
		/// 이벤트가 일어난 페이지(클릭한 페이지)의 번호
		/// </summary>
		public int iCurrPage
		{
			get
			{
				return this._iCurrPage;
			}
		}
	}

	/// <summary>
	/// 페이징 방법 열거형
	/// </summary>
	public enum PagingMethod
	{
		/// <summary>
		/// GET 방식(링크)
		/// </summary>
		Get = 0,
		/// <summary>
		/// POST 방식(이벤트)
		/// </summary>
		Post = 1
	}
	#endregion
	

	/// <summary>
	/// PagingControl에 대한 요약 설명입니다.
	/// </summary>
	[DefaultProperty("Text"),ToolboxData("<{0}:PagingControl runat=server></{0}:PagingControl>")]
	public class PagingControl : System.Web.UI.WebControls.WebControl, IPostBackEventHandler
	{
	
		#region 변수
		private string strText = string.Empty;
		private string strPrevBlockImage = string.Empty;
		private string strNextBlockImage = string.Empty;
		private string strGotoFirstImage = string.Empty;
		private string strGotoLastImage = string.Empty;
		private bool bHidePrevNextBlockButtons = false;
		private bool bHideGotoFirstLastButtons = false;
		private bool bMustShowButtons = false;
		private int nTotalCount = 0;
		private int nCountPerPage = 10;
		private int nCountPerBlock = 10;
		private string strPageParameters = string.Empty;
		private int nCurPage = 0;
		private int nPagingTableWidth = 0;
		private PagingMethod oPagingMethod;
		private string strPageURL = string.Empty;
		private int nTotalPageCount = 0;
		
		#endregion
	
		#region 이벤트구현 부분

		private static readonly object EventClick = new object();

		/// <summary>
		/// 
		/// </summary>
		public event ClickEventHandler Click
		{
			add 
			{
				Events.AddHandler(EventClick, value);
			}
			remove 
			{
				Events.RemoveHandler(EventClick, value);
			}

		}

		/// <summary>
		/// ClickEventHandler 대리자
		/// </summary>
		public delegate void ClickEventHandler(object sender, PagingEventArgs pe);

		/// <summary>
		/// 온클릭 이벤트
		/// </summary>
		/// <param name="e">이벤트 인수</param>
		protected virtual void OnClick(PagingEventArgs e)
		{
			ClickEventHandler handler = (ClickEventHandler)Events[EventClick];
			//ekw
			if (handler != null)
			{
				handler(this,e);
			}

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="postDataKey"></param>
		/// <param name="values"></param>
		/// <returns></returns>
		public virtual bool LoadPostData(string postDataKey, NameValueCollection values)
		{
			int iPresent = this.nCurPage;
			int iNew = Convert.ToInt32(values[UniqueID].ToString());
			
			//ekw
			if (iPresent != iNew)
			{
				this.nCurPage = Convert.ToInt32(values[UniqueID].ToString());
				return true;
			}
			return false;
		}
         
		/// <summary>
		/// 
		/// </summary>
		public virtual void RaisePostDataChangedEvent() 
		{
			OnClick(new PagingEventArgs(this.nCurPage));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventargument"></param>
		public void RaisePostBackEvent(string eventargument)
		{
			OnClick(new PagingEventArgs(Convert.ToInt32(eventargument)));
		}

		#endregion

		#region 프로퍼티


	
		/// <summary>
		/// 이 컨트롤에서 최종적으로 화면에 뿌려줄 HTML 문자열
		/// </summary>
		[Bindable(true), Category("Appearance"), DefaultValue(""), 
		Description("이 컨트롤에서 최종적으로 화면에 뿌려줄 HTML 문자열")] 
		public string Text 
		{
			get
			{
				if(this.strText == null)
				{
					this.strText = "Paging Custom Control";
				}
				return strText;
			}

			set
			{
				this.strText = value;
			}
		}

		/// <summary>
		/// 이전 블럭 버튼 이미지 경로
		/// </summary>
		[Bindable(true), Category("Images"), DefaultValue(""),
		Description("이전 블럭 버튼 이미지 경로를 상대경로 또는 절대경로로 기입")]
		public string PrevBlockImage
		{
			get
			{
				if(this.strPrevBlockImage == string.Empty)
				{
					strPrevBlockImage = "/WebCommon/image/button/arr_prev.gif";						
				}
				
				return this.strPrevBlockImage;
			}
			set
			{
				this.strPrevBlockImage = value;
			}
		}

		/// <summary>
		/// 다음 블럭 버튼 이미지 경로
		/// </summary>
		[Bindable(true),Category("Images"), DefaultValue(""),
		Description("다음 블럭 버튼 이미지 경로를 상대경로 또는 절대경로로 기입")]
		public string NextBlockImage
		{
			get
			{
				if(this.strNextBlockImage == string.Empty)
				{
					//					if(Component.Site.DesignMode)
					strNextBlockImage = "/WebCommon/image/button/arr_next.gif";
					//					else
					//                        this.strNextBlockImage = Constant.PAGECONTROL_NEXTBLOCK_IMAGE;
				}
				return this.strNextBlockImage;
			}
			set
			{
				this.strNextBlockImage = value;
			}
		}

		/// <summary>
		/// 처음으로 버튼 이미지 경로
		/// </summary>
		[Bindable(true), Category("Images"), DefaultValue(""),
		Description("처음으로 버튼 이미지 경로")]
		public string GotoFirstImage
		{
			get
			{
				if(this.strGotoFirstImage == string.Empty)
				{
					//					if(Component.Site.DesignMode)
					strGotoFirstImage = "/WebCommon/image/button/arr_first.gif";
					//					else
					//                        this.strGotoFirstImage = Constant.PAGECONTROL_GOTOFIRST_IMAGE;
				}
				return this.strGotoFirstImage;
			}
			set
			{
				this.strGotoFirstImage = value;
			}
		}

		/// <summary>
		/// 마지막으로 버튼 이미지 경로
		/// </summary>
		[Bindable(true), Category("Images"),  DefaultValue(""),
		Description("마지막으로 버튼 이미지 경로")]
		public string GotoLastImage
		{
			get
			{
				if(this.strGotoLastImage == string.Empty)
				{
					//					if(Component.Site.DesignMode)
					strGotoLastImage = "/WebCommon/image/button/arr_end.gif";
					//					else
					//                        this.strGotoLastImage = Constant.PAGECONTROL_GOTOLAST_IMAGE;
				}
				return this.strGotoLastImage;
			}
			set
			{
				this.strGotoLastImage = value;
			}
		}

		/// <summary>
		/// 이전 블럭, 다음 블럭 버튼을 사용하지 않음(감춤)
		/// </summary>
		[Bindable(true), Category("Appearance"), DefaultValue(false),
		Description("이전 블럭, 다음 블럭 버튼을 사용하지 않음(감춤)")]
		public bool HidePrevNextBlockButtons
		{
			get
			{
				
				return this.bHidePrevNextBlockButtons;
			}
			set
			{
				this.bHidePrevNextBlockButtons = value;
			}
		}

		/// <summary>
		/// 처음으로, 마지막으로 버튼을 사용하지 않음(감춤)
		/// </summary>
		[Bindable(true), Category("Appearance"), DefaultValue(true),
		Description("처음으로, 마지막으로 버튼을 사용하지 않음(감춤)")]
		public bool HideGotoFirstLastButtons
		{
			get
			{
				return this.bHideGotoFirstLastButtons;
			}
			set
			{
				this.bHideGotoFirstLastButtons = value;
			}
		}

		/// <summary>
		/// 이전블럭,이전,다음,다음블럭 이미지가 Disable시에 화면에 표시할지 여부
		/// </summary>
		[Bindable(true), Category("Appearance"), DefaultValue(true),
		Description("이전블럭,이전,다음,다음블럭 이미지가 Disable시에 화면에 표시할지 여부")]
		public bool MustShowButtons
		{
			get
			{
				return this.bMustShowButtons;
			}
			set
			{
				this.bMustShowButtons = value;
			}
		}

		/// <summary>
		/// 총 데이터의 갯수를 설정
		/// </summary>
		[Bindable(true), Category("Appearance"), DefaultValue(""),
		Description("총 데이터의 갯수를 설정")]
		public int TotalCount
		{
			get
			{
				return this.nTotalCount;
			}
			set
			{
				this.nTotalCount = value;
			}
		}

		/// <summary>
		/// 한 페이지당 보여질 갯수를 설정
		/// </summary>
		[Bindable(true), Category("Appearance"), DefaultValue(10),
		Description("한 페이지당 보여질 갯수를 설정")]
		public int CountPerPage
		{
			get
			{
				return this.nCountPerPage;
			}
			set
			{
				this.nCountPerPage = value;
			}
		}

		/// <summary>
		/// 한 블럭당 보여질 페이지의 갯수를 설정
		/// </summary>
		[Bindable(true), Category("Appearance"), DefaultValue(10),
		Description("한 블럭당 보여질 페이지의 갯수를 설정")]
		public int CountPerBlock
		{
			get
			{
				return this.nCountPerBlock;
			}
			set
			{
				this.nCountPerBlock = value;
			}
		}

		/// <summary>
		/// 현재 페이지를 세팅
		/// </summary>
		[Bindable(true), Category("Appearance"), DefaultValue(1),Description("현재 페이지")]
		public int CurPage
		{
			get
			{
				if(this.nCurPage == 0)
				{
					this.nCurPage = 1;
				}
				return this.nCurPage;
			}
			set
			{
				this.nCurPage = value;
			}
		}

		/// <summary>
		/// 페이징에 필요한 기타(page정보는 제외) 파라미터들을 세팅
		/// 이 프로퍼티를 사용하면 모든 정보를 GET 방식으로 넘겨주어야 합니다
		/// 이 프로퍼티를 사용하지 않으면 Post 방식으로 사용할 수가 있습니다
		/// </summary>
		[Bindable(true), Category("Appearance"), DefaultValue(""), 
		Description("페이징에 필요한 기타(page정보는 제외) 파라미터들을 세팅")]
		public string EtcPageParameters
		{
			get
			{
				if(this.strPageParameters == null)
				{
					this.strPageParameters = string.Empty;
				}
				else
				{
					if(this.strPageParameters.Length>1)
					{
						if(this.strPageParameters.Substring(0,1)!="&")
						{
							this.strPageParameters = "&" + this.strPageParameters;
						}
					}
				}
				

				return this.strPageParameters;
			}
			set
			{
				this.strPageParameters = value;
			}
		}

		/// <summary>
		/// 페이징영역 전체 테이블의 Width 를 지정합니다
		/// </summary>
		[Bindable(true), Category("Appearance"), DefaultValue("250"),
		Description("페이징영역 전체 테이블의 Width를 지정합니다")]
		public int PagingTableWidth
		{
			get
			{
				return this.nPagingTableWidth;
			}
			set
			{
				this.nPagingTableWidth = value;
			}
		}

		/// <summary>
		/// 페이징 방법입니다(Post/Get)
		/// </summary>
		[Bindable(true), Category("Appearance"), Description("페이징 방법입니다(Post/Get)")]
		public PagingMethod PagingMethod
		{
			get
			{
				return this.oPagingMethod;
			}
			set
			{
				this.oPagingMethod = value;
			}
		}

		/// <summary>
		/// 링크를 걸기위한 페이지 URL
		/// </summary>
		[Bindable(true), Category("Appearance"), Description("링크를 걸기위한 페이지 URL")]
		public string PageURL
		{
			set
			{
				this.strPageURL = value;
			}
		}

		/// <summary>
		/// 총갯수와 페이지당 글의 갯수를 가지고
		/// 계산되어 나오는 총 페이지 수입니다
		/// </summary>
		public int TotalPageCount
		{
			get
			{
				if(this.nTotalCount != 0 && this.nCountPerPage != 0)
				{
					this.nTotalPageCount = this.nTotalCount / this.nCountPerPage;

					if((this.nTotalCount%this.nCountPerPage) > 0) 
					{
						this.nTotalPageCount = this.nTotalPageCount + 1;
					}
				}
				return this.nTotalPageCount;
			}
		}

		#endregion

		#region 실제로 페이징을 계산하고 세팅하는 부분

		/// <summary>
		///  페이지 번호를 계산하고 이전,다음 등 버튼들을 세팅하는 함수
		/// </summary>
		private void setPageData()
		{
			//현재 블럭의 시작 페이지
			int nPageStart = 0;
																	
			//현재 블럭의 끝 페이지
			int nPageEnd = 0;		
																
			//총 페이지의 갯수
			int nTotalPageCount = 0;	
														
			//총 블럭의 갯수
			int nTotalBlockCount = 0;

			//현재 페이지의 블럭 위치												
			int nCurBlock = 0;																		


			//총 페이지수 계산 부분
			nTotalPageCount = this.TotalCount / this.CountPerPage;
			//마지막페이지 잔여글 존재시..
			if((this.TotalCount%this.nCountPerPage) > 0) 
			{
				nTotalPageCount++;	
			}

			//총 블럭수 계산 부분
			nTotalBlockCount = nTotalPageCount / this.CountPerBlock;
			//마지막 블럭 잔여 페이지 존재시 ..
			if((nTotalPageCount % this.CountPerBlock) > 0) 
			{
				nTotalBlockCount++;
			}

			//현재 몇번째 블럭인지 계산
			nCurBlock = Convert.ToInt32(this.CurPage / this.CountPerBlock)+1;	
			if(this.CurPage%this.CountPerBlock == 0) 
			{
				nCurBlock--;
			}

			string strFirst = string.Empty;					//처음으로 버튼 세팅
			string strLast = string.Empty;					//마지막으로 버튼 세팅
			string strPrevBlock = string.Empty;			//이전블럭 버튼 세팅
			string strNextBlock = string.Empty;			//다음블럭 버튼 세팅
			string strPage = string.Empty;					//페이지번호 세팅 변수

			//========================================================
			//										"처음으로","마지막으로" 버튼 설정
			//========================================================
			if(!this.HideGotoFirstLastButtons)								//처음으로,마지막으로 버튼을 보여줄지 여부 검사
			{
				//"처음으로"  버튼 세팅
				if(this.GotoFirstImage.Length > 3)
				{
					if((this.GotoFirstImage.Substring(this.GotoFirstImage.Length-3,3).ToUpper() == "GIF")
						|| (this.GotoFirstImage.Substring(this.GotoFirstImage.Length-3,3).ToUpper() == "JPG")
						|| (this.GotoFirstImage.Substring(this.GotoFirstImage.Length-3,3).ToUpper() == "BMP"))
					{
						strFirst = "<img src='"+ this.GotoFirstImage +"' border=0 align=absmiddle>";
					}
					else
					{
						strFirst = this.GotoFirstImage;
					}
				}
				else
				{
					strFirst = this.GotoFirstImage;
				}


				if(this.CurPage > 1)												//현재 페이지가 1페이지가 아니면
				{
					//strFirst = makeLink( 1, this.EtcPageParameters, TelepiaDictionary.Dic("FirstPage"), strFirst );
					strFirst = makeLink( 1, this.EtcPageParameters, CFW.Common.Dictionary.SearchStaticDic("First"), strFirst );
				}
				else																	//현재 1페이지이면
				{
					if(!this.MustShowButtons)								//비활성화 되었을때도 보여줄지 여부를 결정
						strFirst = string.Empty;	//이미지 없앰

				}

				

				if(this.GotoLastImage.Length > 3)
				{
					if((this.GotoLastImage.Substring(this.GotoLastImage.Length-3,3).ToUpper() == "GIF")
						|| (this.GotoLastImage.Substring(this.GotoLastImage.Length-3,3).ToUpper() == "JPG")
						|| (this.GotoLastImage.Substring(this.GotoLastImage.Length-3,3).ToUpper() == "BMP"))
					{
						strLast = "<img src='"+ this.GotoLastImage +"' border=0 align=absmiddle>";
					}
					else
					{
						strLast = this.GotoLastImage;
					}
				}
				else
				{
					strLast = this.GotoLastImage;
				}

				if(this.CurPage < nTotalPageCount)						//마지막 페이지가 아니면
				{
					strLast = makeLink(nTotalPageCount, this.EtcPageParameters, CFW.Common.Dictionary.SearchStaticDic("Last"), strLast);
				}
				else																	//마지막 페이지 이면
				{
					if(!this.MustShowButtons)								//비활성화 되었을때도 보여줄지 여부를 결정
						strLast = string.Empty;//이미지 없앰
				}
			}

	

			//========================================================
			//										"이전 블럭","다음 블럭" 버튼 설정
			//========================================================
			if(!this.HidePrevNextBlockButtons)
			{
				

				if(this.PrevBlockImage.Length > 3)
				{
					if((this.PrevBlockImage.Substring(this.PrevBlockImage.Length-3,3).ToUpper() == "GIF")
						|| (this.PrevBlockImage.Substring(this.PrevBlockImage.Length-3,3).ToUpper() == "JPG")
						|| (this.PrevBlockImage.Substring(this.PrevBlockImage.Length-3,3).ToUpper() == "BMP"))
					{
						strPrevBlock = "<img src='"+ this.PrevBlockImage +"' border=0 align=absmiddle>";
					}
					else
					{
						strPrevBlock = this.PrevBlockImage;
					}
				}
				else
				{
					strPrevBlock = this.PrevBlockImage;
				}

				if(nCurBlock>1)										//현재 블럭이 1블럭이 아니면
				{
					//strPrevBlock = makeLink((nCurBlock-2)*this.CountPerBlock+1,this.EtcPageParameters, TelepiaDictionary.Dic("Previous")+this.CountPerBlock.ToString()+ TelepiaDictionary.Dic("Page"),strPrevBlock);
					strPrevBlock = makeLink((nCurBlock-2)*this.CountPerBlock+1,this.EtcPageParameters, CFW.Common.Dictionary.SearchStaticDic("Prev") + this.CountPerBlock.ToString()+ CFW.Common.Dictionary.SearchStaticDic("Page"),strPrevBlock);
				}
				else														//현재 1블럭이면
				{
					if(!this.MustShowButtons)		//비활성화 되었을때도 보여줄지 여부
						strPrevBlock=string.Empty;
				}
				
				if(this.NextBlockImage.Length > 3)
				{
					if((this.NextBlockImage.Substring(this.NextBlockImage.Length-3,3).ToUpper() == "GIF")
						|| (this.NextBlockImage.Substring(this.NextBlockImage.Length-3,3).ToUpper() == "JPG")
						|| (this.NextBlockImage.Substring(this.NextBlockImage.Length-3,3).ToUpper() == "BMP"))
					{
						strNextBlock = "<img src='"+ this.NextBlockImage +"' border=0 align=absmiddle>";
					}
					else
					{
						strNextBlock = this.NextBlockImage;
					}
				}
				else
				{
					strNextBlock = this.NextBlockImage;
				}

				if(nCurBlock<nTotalBlockCount)				//현재 마지막 블럭이 아니면
				{
					strNextBlock = makeLink(nCurBlock*this.CountPerBlock+1,this.EtcPageParameters, CFW.Common.Dictionary.SearchStaticDic("Next") +this.CountPerBlock.ToString()+CFW.Common.Dictionary.SearchStaticDic("Page"),strNextBlock);
				}
				else
				{
					if(!this.MustShowButtons)		//비활성화 되었을때도 보여줄지 여부
						strNextBlock=string.Empty;
				}

			}

			//========================================================
			//										페이지 번호 조합 부분
			//========================================================
			nPageStart = (nCurBlock*this.CountPerBlock)-this.CountPerBlock+1;
			nPageEnd	 = (nCurBlock*this.CountPerBlock);
			if(nPageEnd>=nTotalPageCount) nPageEnd = nTotalPageCount;

			for(int i=nPageStart;i<=nPageEnd;i++)
			{
				//현재 페이지는 링크없이 진하게 표기
				if(i==this.CurPage) 
				{
					//2004-08-09
					//BugID #19 수정
					//i가 마지막일때는 |를 붙이지 않고, 아닐때는 붙여서
					// << < 1 | 2 | 3 | ....10 > >> 이렇게 보이게 함
					//2004-08-30
					//Raid BugID #189 수정
					//페이징이  |2 |3 | 이렇게 되던 문제를 수정해서
					// | 2 | 3 | 이렇게 되도록 조정함
					if(i == nPageEnd)
					{
						strPage += "<b>&nbsp;"+i+"&nbsp;</b>";
					}
					else
					{
						strPage += "<b>&nbsp;"+i+"&nbsp;</b>|";
					}
				}
					//나머지 페이지는 링크 걸기
				else
				{
					//2004-08-09
					//BugID #19 수정
					//i가 마지막일때는 |를 붙이지 않고, 아닐때는 붙여서
					// << < 1 | 2 | 3 | ....10 > >> 이렇게 보이게 함
					if(i == nPageEnd)
					{
						strPage += makeLink(i, this.EtcPageParameters, i.ToString()+CFW.Common.Dictionary.SearchStaticDic("Page"), i.ToString())+"";
					}
					else
					{
						strPage += makeLink(i, this.EtcPageParameters, i.ToString()+CFW.Common.Dictionary.SearchStaticDic("Page"), i.ToString())+"|";
					}
				}
			}

			//최종 Return String 조합
			this.Text = strFirst + strPrevBlock + strPage + strNextBlock + strLast;
		}

		#endregion

		#region 링크 생성

		/// <summary>
		///  입력된 파라미터로 페이징에 필요한 링크를 생성해 주는 함수
		/// </summary>
		/// <param name="page">페이지 번호</param>
		/// <param name="etcParams">페이지 번호외에 링크 생성에 필요한 다른 파라미터들(GET 방식일때만 필요)</param>
		/// <param name="title">링크에 관한 설명상자에 나타날 문자열</param>
		/// <param name="linkStr">링크를 걸어줄 이미지나 문자열</param>
		/// <returns>링크를 걸어준 문자열</returns>
		private string makeLink(int page, string etcParams, string title, string linkStr)
		{
			//현재 이 컨트롤이 사용되고 있는 URL 의 절대경로
			string strCurrentURL = this.Context.Request.Url.AbsolutePath;
			string strReturn = "";

			if((int)this.oPagingMethod == 0)
			{
				strReturn = "<a alt=\"" + title +"\" title=\"" + title +"\" id=\"" + this.UniqueID + "\" href=\"javascript:" + Page.GetPostBackEventReference(this, page.ToString()) +"\">";
				strReturn += "&nbsp;" + linkStr + "&nbsp;</a>";	
			}
			else
			{
				if(this.strPageURL != null || this.strPageURL != string.Empty)
				{
					strReturn = String.Format( "<a href=\"{0}\" title=\"{1}\">&nbsp;{2}&nbsp;</a>",  String.Format("{0}?page={1}{2}", strCurrentURL, page, etcParams), title, linkStr );
				}
				else
				{
					strReturn = String.Format( "<a href=\"{0}\" title=\"{1}\">&nbsp;{2}&nbsp;</a>",  String.Format("?page={0}{1}", page, etcParams), title, linkStr );
				}
			}
			
			return strReturn;
		}

		#endregion
		
		#region 렌더링

		/// <summary> 
		/// 이 컨트롤을 지정한 출력 매개 변수로 렌더링합니다.
		/// </summary>
		/// <param name="output"> 출력을 쓸 HTML 작성기 </param>
		protected override void Render(HtmlTextWriter output)
		{
			setPageData();					//페이징에 필요한 데이터 조합
			output.Write(this.Text);
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
				object[] stateArr = (object[])savedState;
				
				base.LoadViewState(stateArr[0]);
				
				if (stateArr[1] != null)
					this.nTotalCount = (int)stateArr[1];
				this.oPagingMethod = (PagingMethod)stateArr[2];
				this.nCurPage = (int)stateArr[3];
			}
		}

		/// <summary>
		/// View State 속성을 Save합니다.
		/// </summary>
		/// <returns></returns>
		protected override object SaveViewState()
		{
			object[] stateArr = new object[4];
			
			stateArr[0] = base.SaveViewState();
			
			stateArr[1] = this.nTotalCount;
			stateArr[2] = this.oPagingMethod;
			stateArr[3] = this.nCurPage;
			return (object)stateArr;
		}
		#endregion

	}
}
