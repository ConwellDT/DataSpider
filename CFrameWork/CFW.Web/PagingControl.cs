using System;
using System.ComponentModel;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Specialized;

namespace CFW.Web
{

	#region ����¡Enum/Ŭ���̺�ƮArgs
	/// <summary>
	/// ����¡ Ŭ�� �̺�Ʈ �Ű������Դϴ�
	/// </summary>
	public class PagingEventArgs : EventArgs
	{
		/// <summary>
		/// ��������ȣ�� �Ű������� �޴� �������Դϴ�
		/// </summary>
		/// <param name="iCurrPage"></param>
		public PagingEventArgs(int iCurrPage)
		{
			this._iCurrPage = iCurrPage;
		}

		private int _iCurrPage;

		/// <summary>
		/// �̺�Ʈ�� �Ͼ ������(Ŭ���� ������)�� ��ȣ
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
	/// ����¡ ��� ������
	/// </summary>
	public enum PagingMethod
	{
		/// <summary>
		/// GET ���(��ũ)
		/// </summary>
		Get = 0,
		/// <summary>
		/// POST ���(�̺�Ʈ)
		/// </summary>
		Post = 1
	}
	#endregion
	

	/// <summary>
	/// PagingControl�� ���� ��� �����Դϴ�.
	/// </summary>
	[DefaultProperty("Text"),ToolboxData("<{0}:PagingControl runat=server></{0}:PagingControl>")]
	public class PagingControl : System.Web.UI.WebControls.WebControl, IPostBackEventHandler
	{
	
		#region ����
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
	
		#region �̺�Ʈ���� �κ�

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
		/// ClickEventHandler �븮��
		/// </summary>
		public delegate void ClickEventHandler(object sender, PagingEventArgs pe);

		/// <summary>
		/// ��Ŭ�� �̺�Ʈ
		/// </summary>
		/// <param name="e">�̺�Ʈ �μ�</param>
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

		#region ������Ƽ


	
		/// <summary>
		/// �� ��Ʈ�ѿ��� ���������� ȭ�鿡 �ѷ��� HTML ���ڿ�
		/// </summary>
		[Bindable(true), Category("Appearance"), DefaultValue(""), 
		Description("�� ��Ʈ�ѿ��� ���������� ȭ�鿡 �ѷ��� HTML ���ڿ�")] 
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
		/// ���� �� ��ư �̹��� ���
		/// </summary>
		[Bindable(true), Category("Images"), DefaultValue(""),
		Description("���� �� ��ư �̹��� ��θ� ����� �Ǵ� �����η� ����")]
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
		/// ���� �� ��ư �̹��� ���
		/// </summary>
		[Bindable(true),Category("Images"), DefaultValue(""),
		Description("���� �� ��ư �̹��� ��θ� ����� �Ǵ� �����η� ����")]
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
		/// ó������ ��ư �̹��� ���
		/// </summary>
		[Bindable(true), Category("Images"), DefaultValue(""),
		Description("ó������ ��ư �̹��� ���")]
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
		/// ���������� ��ư �̹��� ���
		/// </summary>
		[Bindable(true), Category("Images"),  DefaultValue(""),
		Description("���������� ��ư �̹��� ���")]
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
		/// ���� ��, ���� �� ��ư�� ������� ����(����)
		/// </summary>
		[Bindable(true), Category("Appearance"), DefaultValue(false),
		Description("���� ��, ���� �� ��ư�� ������� ����(����)")]
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
		/// ó������, ���������� ��ư�� ������� ����(����)
		/// </summary>
		[Bindable(true), Category("Appearance"), DefaultValue(true),
		Description("ó������, ���������� ��ư�� ������� ����(����)")]
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
		/// ������,����,����,������ �̹����� Disable�ÿ� ȭ�鿡 ǥ������ ����
		/// </summary>
		[Bindable(true), Category("Appearance"), DefaultValue(true),
		Description("������,����,����,������ �̹����� Disable�ÿ� ȭ�鿡 ǥ������ ����")]
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
		/// �� �������� ������ ����
		/// </summary>
		[Bindable(true), Category("Appearance"), DefaultValue(""),
		Description("�� �������� ������ ����")]
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
		/// �� �������� ������ ������ ����
		/// </summary>
		[Bindable(true), Category("Appearance"), DefaultValue(10),
		Description("�� �������� ������ ������ ����")]
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
		/// �� ���� ������ �������� ������ ����
		/// </summary>
		[Bindable(true), Category("Appearance"), DefaultValue(10),
		Description("�� ���� ������ �������� ������ ����")]
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
		/// ���� �������� ����
		/// </summary>
		[Bindable(true), Category("Appearance"), DefaultValue(1),Description("���� ������")]
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
		/// ����¡�� �ʿ��� ��Ÿ(page������ ����) �Ķ���͵��� ����
		/// �� ������Ƽ�� ����ϸ� ��� ������ GET ������� �Ѱ��־�� �մϴ�
		/// �� ������Ƽ�� ������� ������ Post ������� ����� ���� �ֽ��ϴ�
		/// </summary>
		[Bindable(true), Category("Appearance"), DefaultValue(""), 
		Description("����¡�� �ʿ��� ��Ÿ(page������ ����) �Ķ���͵��� ����")]
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
		/// ����¡���� ��ü ���̺��� Width �� �����մϴ�
		/// </summary>
		[Bindable(true), Category("Appearance"), DefaultValue("250"),
		Description("����¡���� ��ü ���̺��� Width�� �����մϴ�")]
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
		/// ����¡ ����Դϴ�(Post/Get)
		/// </summary>
		[Bindable(true), Category("Appearance"), Description("����¡ ����Դϴ�(Post/Get)")]
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
		/// ��ũ�� �ɱ����� ������ URL
		/// </summary>
		[Bindable(true), Category("Appearance"), Description("��ũ�� �ɱ����� ������ URL")]
		public string PageURL
		{
			set
			{
				this.strPageURL = value;
			}
		}

		/// <summary>
		/// �Ѱ����� �������� ���� ������ ������
		/// ���Ǿ� ������ �� ������ ���Դϴ�
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

		#region ������ ����¡�� ����ϰ� �����ϴ� �κ�

		/// <summary>
		///  ������ ��ȣ�� ����ϰ� ����,���� �� ��ư���� �����ϴ� �Լ�
		/// </summary>
		private void setPageData()
		{
			//���� ���� ���� ������
			int nPageStart = 0;
																	
			//���� ���� �� ������
			int nPageEnd = 0;		
																
			//�� �������� ����
			int nTotalPageCount = 0;	
														
			//�� ���� ����
			int nTotalBlockCount = 0;

			//���� �������� �� ��ġ												
			int nCurBlock = 0;																		


			//�� �������� ��� �κ�
			nTotalPageCount = this.TotalCount / this.CountPerPage;
			//������������ �ܿ��� �����..
			if((this.TotalCount%this.nCountPerPage) > 0) 
			{
				nTotalPageCount++;	
			}

			//�� ���� ��� �κ�
			nTotalBlockCount = nTotalPageCount / this.CountPerBlock;
			//������ �� �ܿ� ������ ����� ..
			if((nTotalPageCount % this.CountPerBlock) > 0) 
			{
				nTotalBlockCount++;
			}

			//���� ���° ������ ���
			nCurBlock = Convert.ToInt32(this.CurPage / this.CountPerBlock)+1;	
			if(this.CurPage%this.CountPerBlock == 0) 
			{
				nCurBlock--;
			}

			string strFirst = string.Empty;					//ó������ ��ư ����
			string strLast = string.Empty;					//���������� ��ư ����
			string strPrevBlock = string.Empty;			//������ ��ư ����
			string strNextBlock = string.Empty;			//������ ��ư ����
			string strPage = string.Empty;					//��������ȣ ���� ����

			//========================================================
			//										"ó������","����������" ��ư ����
			//========================================================
			if(!this.HideGotoFirstLastButtons)								//ó������,���������� ��ư�� �������� ���� �˻�
			{
				//"ó������"  ��ư ����
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


				if(this.CurPage > 1)												//���� �������� 1�������� �ƴϸ�
				{
					//strFirst = makeLink( 1, this.EtcPageParameters, TelepiaDictionary.Dic("FirstPage"), strFirst );
					strFirst = makeLink( 1, this.EtcPageParameters, CFW.Common.Dictionary.SearchStaticDic("First"), strFirst );
				}
				else																	//���� 1�������̸�
				{
					if(!this.MustShowButtons)								//��Ȱ��ȭ �Ǿ������� �������� ���θ� ����
						strFirst = string.Empty;	//�̹��� ����

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

				if(this.CurPage < nTotalPageCount)						//������ �������� �ƴϸ�
				{
					strLast = makeLink(nTotalPageCount, this.EtcPageParameters, CFW.Common.Dictionary.SearchStaticDic("Last"), strLast);
				}
				else																	//������ ������ �̸�
				{
					if(!this.MustShowButtons)								//��Ȱ��ȭ �Ǿ������� �������� ���θ� ����
						strLast = string.Empty;//�̹��� ����
				}
			}

	

			//========================================================
			//										"���� ��","���� ��" ��ư ����
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

				if(nCurBlock>1)										//���� ���� 1���� �ƴϸ�
				{
					//strPrevBlock = makeLink((nCurBlock-2)*this.CountPerBlock+1,this.EtcPageParameters, TelepiaDictionary.Dic("Previous")+this.CountPerBlock.ToString()+ TelepiaDictionary.Dic("Page"),strPrevBlock);
					strPrevBlock = makeLink((nCurBlock-2)*this.CountPerBlock+1,this.EtcPageParameters, CFW.Common.Dictionary.SearchStaticDic("Prev") + this.CountPerBlock.ToString()+ CFW.Common.Dictionary.SearchStaticDic("Page"),strPrevBlock);
				}
				else														//���� 1���̸�
				{
					if(!this.MustShowButtons)		//��Ȱ��ȭ �Ǿ������� �������� ����
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

				if(nCurBlock<nTotalBlockCount)				//���� ������ ���� �ƴϸ�
				{
					strNextBlock = makeLink(nCurBlock*this.CountPerBlock+1,this.EtcPageParameters, CFW.Common.Dictionary.SearchStaticDic("Next") +this.CountPerBlock.ToString()+CFW.Common.Dictionary.SearchStaticDic("Page"),strNextBlock);
				}
				else
				{
					if(!this.MustShowButtons)		//��Ȱ��ȭ �Ǿ������� �������� ����
						strNextBlock=string.Empty;
				}

			}

			//========================================================
			//										������ ��ȣ ���� �κ�
			//========================================================
			nPageStart = (nCurBlock*this.CountPerBlock)-this.CountPerBlock+1;
			nPageEnd	 = (nCurBlock*this.CountPerBlock);
			if(nPageEnd>=nTotalPageCount) nPageEnd = nTotalPageCount;

			for(int i=nPageStart;i<=nPageEnd;i++)
			{
				//���� �������� ��ũ���� ���ϰ� ǥ��
				if(i==this.CurPage) 
				{
					//2004-08-09
					//BugID #19 ����
					//i�� �������϶��� |�� ������ �ʰ�, �ƴҶ��� �ٿ���
					// << < 1 | 2 | 3 | ....10 > >> �̷��� ���̰� ��
					//2004-08-30
					//Raid BugID #189 ����
					//����¡��  |2 |3 | �̷��� �Ǵ� ������ �����ؼ�
					// | 2 | 3 | �̷��� �ǵ��� ������
					if(i == nPageEnd)
					{
						strPage += "<b>&nbsp;"+i+"&nbsp;</b>";
					}
					else
					{
						strPage += "<b>&nbsp;"+i+"&nbsp;</b>|";
					}
				}
					//������ �������� ��ũ �ɱ�
				else
				{
					//2004-08-09
					//BugID #19 ����
					//i�� �������϶��� |�� ������ �ʰ�, �ƴҶ��� �ٿ���
					// << < 1 | 2 | 3 | ....10 > >> �̷��� ���̰� ��
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

			//���� Return String ����
			this.Text = strFirst + strPrevBlock + strPage + strNextBlock + strLast;
		}

		#endregion

		#region ��ũ ����

		/// <summary>
		///  �Էµ� �Ķ���ͷ� ����¡�� �ʿ��� ��ũ�� ������ �ִ� �Լ�
		/// </summary>
		/// <param name="page">������ ��ȣ</param>
		/// <param name="etcParams">������ ��ȣ�ܿ� ��ũ ������ �ʿ��� �ٸ� �Ķ���͵�(GET ����϶��� �ʿ�)</param>
		/// <param name="title">��ũ�� ���� ������ڿ� ��Ÿ�� ���ڿ�</param>
		/// <param name="linkStr">��ũ�� �ɾ��� �̹����� ���ڿ�</param>
		/// <returns>��ũ�� �ɾ��� ���ڿ�</returns>
		private string makeLink(int page, string etcParams, string title, string linkStr)
		{
			//���� �� ��Ʈ���� ���ǰ� �ִ� URL �� ������
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
		
		#region ������

		/// <summary> 
		/// �� ��Ʈ���� ������ ��� �Ű� ������ �������մϴ�.
		/// </summary>
		/// <param name="output"> ����� �� HTML �ۼ��� </param>
		protected override void Render(HtmlTextWriter output)
		{
			setPageData();					//����¡�� �ʿ��� ������ ����
			output.Write(this.Text);
		}

		#endregion

		#region  ViewState�� �Ӽ� SAVE & LOAD

		/// <summary>
		/// View State �Ӽ��� Load�մϴ�.
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
		/// View State �Ӽ��� Save�մϴ�.
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
