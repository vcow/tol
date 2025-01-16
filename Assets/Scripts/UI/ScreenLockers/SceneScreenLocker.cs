using Core.Activatable;
using Core.ScreenLocker;
using UnityEngine;

namespace UI.ScreenLockers
{
	[DisallowMultipleComponent]
	public sealed class SceneScreenLocker : BaseScreenLocker
	{
		public override void Activate(bool immediately = false)
		{
			ActivatableState = ActivatableState.Active;
		}

		public override void Deactivate(bool immediately = false)
		{
			ActivatableState = ActivatableState.Inactive;
		}

		public override LockerType LockerType => LockerType.SceneLoader;

		public override void Force()
		{
		}
	}
}