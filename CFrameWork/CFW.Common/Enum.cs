using System;

namespace CFW.Common
{
	/// <summary>
	/// DropDownList 컨트롤에 추가 아이템 항목을 지정합니다.
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
		/// <remarks>기본값은 "전체선택" 이며 Message.xml의 값을 변경하여 텍스트를 변경할 수 있습니다.</remarks>
		All = 0,
		/// <summary>
		/// "선택하세요"를 항목에 추가합니다.
		/// </summary>
		/// <remarks>기본값은 "선택하세요" 이며 Message.xml의 값을 변경하여 텍스트를 변경할 수 있습니다.</remarks>
		Select = 1
	};
}
