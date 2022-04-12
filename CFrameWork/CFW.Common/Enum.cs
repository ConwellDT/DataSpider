using System;

namespace CFW.Common
{
	/// <summary>
	/// DropDownList ��Ʈ�ѿ� �߰� ������ �׸��� �����մϴ�.
	/// </summary>
	public enum SelectionMode
	{
		/// <summary>
		/// �ƹ��͵� �߰����� �ʽ��ϴ�.
		/// </summary>
		None = -1,
		/// <summary>
		/// "��ü����"�� �׸� �߰��մϴ�.
		/// </summary>
		/// <remarks>�⺻���� "��ü����" �̸� Message.xml�� ���� �����Ͽ� �ؽ�Ʈ�� ������ �� �ֽ��ϴ�.</remarks>
		All = 0,
		/// <summary>
		/// "�����ϼ���"�� �׸� �߰��մϴ�.
		/// </summary>
		/// <remarks>�⺻���� "�����ϼ���" �̸� Message.xml�� ���� �����Ͽ� �ؽ�Ʈ�� ������ �� �ֽ��ϴ�.</remarks>
		Select = 1
	};
}
