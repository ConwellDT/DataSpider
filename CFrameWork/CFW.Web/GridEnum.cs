using System;
using System.Text;

namespace  CFW.Web
{
	/// <summary>
	/// GridEnum에 대한 설명입니다.
	/// </summary>
	public class GridEnum
	{
		/// <summary>
		/// List 컨트롤에 추가 아이템 항목을 지정합니다.
		/// </summary>
		public enum SelectionMode
		{
			/// <summary>
			/// 아무것도 추가하지 않습니다.
			/// </summary>
			None = -1,
			/// <summary>
			/// "전체선택"을 항목에 추가합니다.
			/// </summary>
			/// <remarks>기본값은 "전체선택" 이며 고객사에 따라 Message.xml의 값을 변경하여 텍스트를 변경할 수 있습니다.</remarks>
			All = 0,
			/// <summary>
			/// "선택하세요"를 항목에 추가합니다.
			/// </summary>
			/// <remarks>기본값은 "선택하세요" 이며 고객사에 따라 Message.xml의 값을 변경하여 텍스트를 변경할 수 있습니다.</remarks>
			Select = 1
		};

		/// <summary>
		/// SortIndicator
		/// </summary>
		public enum SortIndicator
		{
			/// <summary>
			/// Ascending
			/// </summary>
			Asc,
			
			/// <summary>
			///Descending
			/// </summary>
			Dsc,
			
			/// <summary>
			/// None
			/// </summary>
			None
		};





		/// <summary>
		/// 데이터 유효성 검사 항목을 지정합니다.
		/// </summary>
		public enum ValidationMode
		{
			/// <summary>
			/// 필수 항목 여부를 확인합니다.
			/// </summary>
			NonBlank,
			/// <summary>
			/// 필수 항목 여부와 이메일 형식 검사 여부를 지정합니다.
			/// </summary>
			NonBlankAndEmail,
			/// <summary>
			/// 필수 항목 여부와 주민등록번호 형식 검사 여부를 지정합니다.
			/// </summary>
			NonBlankAndJuminNumber,
			/// <summary>
			/// 필수 항목 여부와 전화번호 형식 검사 여부를 지정합니다.
			/// </summary>
			NonBlankAndPhoneNumber,
			/// <summary>
			/// 이메일 형식 검사 여부를 지정합니다.
			/// </summary>
			Email,
			/// <summary>
			/// 주민등록번호 형식 검사 여부를 지정합니다.
			/// </summary>
			JuminNumber,
			/// <summary>
			/// 전화번호 형식 검사 여부를 지정합니다.
			/// </summary>
			PhoneNumber
		};

		#region 그리드의 기본적인 레이아웃 설정 구조체에 사용될 열거형


		/// <summary>
		/// 그리드의 데이터 표시 스타일 
		/// </summary>
		public enum GridStyle
		{
			/// <summary>
			/// 일반적인 표(table) 구조 
			/// </summary>
			Normal,
			/// <summary>
			/// 데이터의 계층구조 표시
			/// </summary>
			OutLookGroupBy

		}

		/// <summary>
		/// 컬럼 헤더 클릭시 반응을 설정 합니다.
		/// </summary>
		public enum HeaderClickAction
		{
			/// <summary>
			/// 정렬
			/// </summary>
			ColumnSort,
			/// <summary>
			/// 해당 컬럼 전체 선택
			/// </summary>
			ColumnSelect,
			/// <summary>
			/// 무반응
			/// </summary>
			None
		}

		/// <summary>
		/// 사이즈 단위 열거형
		/// </summary>
		public enum SizeUnit
		{
			/// <summary>
			/// 픽셀
			/// </summary>
			Pixel = 0,
			/// <summary>
			/// 퍼센트
			/// </summary>
			Percent = 1
		}
		/// <summary>
		/// GridUtil 클래스의 SetColumnValue 등에서
		/// 칼럼의 값을 바꿀때 방법을 정의한 열거형
		/// </summary>
		public enum SetValueMethod
		{
			/// <summary>
			/// 기존 값에 더하는 경우
			/// </summary>
			AddValue = 0,
			/// <summary>
			/// 완전히 새로운 값을 넣는 경우
			/// </summary>
			SetNewValue = 1
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
		/// <summary>
		/// 메뉴 컨트롤을 사용할 때,
		/// 메뉴의 유형을 나타내는 열거형입니다
		/// </summary>
		public enum MenuTarget
		{
			/// <summary>
			/// 트리 등의 다른 컨트롤이나, 페이지에서
			/// 팝업메뉴(마우스 버튼)로 사용할 때 선택합니다
			/// </summary>
			Popup = 0,
			/// <summary>
			/// 수직으로 나타내는 메뉴로
			/// 사용할 때 씁니다
			/// </summary>
			Vertical = 1,
			/// <summary>
			/// 수평으로 나타나는 메뉴로
			/// 사용할 때 씁니다
			/// </summary>
			Horizontal = 2
		}

		/// <summary>
		/// 그리드에서 SetLinkColumn 메서드를 호출해서 링크를 걸 때,
		/// 링크의 유형을 나타내는 열거형입니다
		/// </summary>
		public enum LinkMethod
		{
			/// <summary>
			/// 일반 URL 로 링크를 걸 때 씁니다
			/// </summary>
			URL = 0,
			/// <summary>
			/// 클라이언트 스크립트 펑션을 매핑할 때 씁니다
			/// </summary>
			Script = 1,
			/// <summary>
			/// 모달 윈도우를 호출할 때 사용합니다
			/// </summary>
			Modal = 2,
			/// <summary>
			/// 팝업 윈도우를 호출할 때 사용합니다
			/// </summary>
			Popup = 3
		}

	
		/// <summary>
		/// 그리드 칼럼 스타일 열거형입니다
		/// </summary>
		public enum GridColumnStyle
		{
			/// <summary>
			/// 일반적인 문자열 칼럼입니다(좌측정렬)
			/// </summary>
			NormalString = 0,
			/// <summary>
			/// 길이가 고정적인 문자열 칼럼입니다(중앙정렬)
			/// </summary>
			FixedString = 1,
			/// <summary>
			/// 숫자 칼럼입니다(우측정렬, 세자리마다 "," 가 붙습니다)
			/// </summary>
			Numeric = 2
		}


		/// <summary>
		/// 작업모드(Debug이면 Xml Container가 textarea로 처리되어 화면에 보이고, Release이면 Hidden Box로 처리함)
		/// </summary>
		public enum WorkModeType
		{
			/// <summary>
			/// Debug 모드 : Xml Container가 textarea로 처리되어 화면에 보입니다.
			/// </summary>
			Debug,
			/// <summary>
			/// Release 모드 : Xml Container가 Hidden Box로 처리됩니다.
			/// </summary>
			Release
		}

		/// <summary>
		/// 페이징/넘버링칼럼을 세팅할때 넘버링을 할 순서
		/// </summary>
		public enum NumberingOrder
		{
			/// <summary>
			/// 1부터 시작해서1씩 증가
			/// </summary>
			ASC = 0,
			/// <summary>
			/// Max부터 시작해서1씩 감소
			/// </summary>
			DESC = 1
		}

		/// <summary>
		/// 셀 클릭 시 일어나는 액션을 설정하기 위해 사용하는 열거형입니다.
		/// </summary>
		public enum CellClickAction
		{
			/// <summary>
			/// 셀 클릭 액션 없음
			/// </summary>
			No = 0,
			/// <summary>
			/// Edit 모드로 변경
			/// </summary>
			Edit = 1,
			/// <summary>
			/// 클릭 된 셀의 로우 선택
			/// </summary>
			RowSelect = 2,
			/// <summary>
			/// 클릭 된 셀의 선택
			/// </summary>
			CellSelect = 3
		}

    
		/// <summary>
		/// 그리드에 클라이언트 스크립트함수로 링크를 걸때 
		/// 스크립트의 언어를 지정하기 위해서
		/// 사용하는 열거형입니다
		/// </summary>
		public enum ScriptLanguage
		{
			/// <summary>
			/// 스크립트를 쓰지 않을 때
			/// </summary>
			None = 0,

			/// <summary>
			/// 자바스크립트를 사용할 때
			/// </summary>
			Javascript = 1,

			/// <summary>
			/// VB스크립트를 사용할 때
			/// </summary>
			Vbscript = 2
		}

		/// <summary>
		/// 그리드 체크박스 헤더 타입 설정할때
		/// 사용하는 열거형 입니다.
		/// </summary>
		public enum CheckBoxHeaderMode
		{
			/// <summary>
			/// 빈 박스 타입
			/// </summary>
			None        = 0,
			/// <summary>
			/// 체크박스와 헤더텍스트 사용
			/// </summary>
		    Both        = 1,
			/// <summary>
			/// 헤더 텍스트 사용
			/// </summary>
		    HeaderText  = 2,
			/// <summary>
			/// 체크박스 사용
			/// </summary>
			CheckBoxAll = 3
		    
		}




		#endregion
	}
	
}
