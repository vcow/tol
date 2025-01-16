using System.Collections.Generic;

namespace Core.ScreenLocker
{
	public interface IScreenLockerSettings
	{
		IReadOnlyList<BaseScreenLocker> ScreenLockers { get; }
	}
}