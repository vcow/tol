using System.Collections.Generic;

namespace Core.WindowManager
{
	public interface IWindowManagerSettings
	{
		IReadOnlyList<string> GroupHierarchy { get; }
		IReadOnlyDictionary<string, WindowImpl> WindowsMap { get; }
		int StartCanvasSortingOrder { get; }
	}
}