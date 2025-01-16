using System.Linq;
using Core.Activatable;
using UnityEngine;

namespace Core.ScreenLocker
{
	/// <summary>
	/// The base class of the Screen Locker component, which should be added to the screen locker window prefab.
	/// </summary>
	public abstract class BaseScreenLocker : MonoBehaviour, IActivatable
	{
		private ActivatableState _activatableState = ActivatableState.Inactive;
		public event ActivatableStateChangedHandler ActivatableStateChangedEvent;

		public abstract void Activate(bool immediately = false);

		public abstract void Deactivate(bool immediately = false);

		/// <summary>
		/// The type of the locker screen.
		/// </summary>
		public abstract LockerType LockerType { get; }

		/// <summary>
		/// This method immediately finished any transition process (if locker is in the ToActive or ToInactive state).
		/// </summary>
		public abstract void Force();

		public ActivatableState ActivatableState
		{
			get => _activatableState;
			protected set
			{
				if (value == _activatableState) return;
				_activatableState = value;
				ActivatableStateChangedEvent?.Invoke(this, _activatableState);
			}
		}

		protected virtual void OnDestroy()
		{
			if (ActivatableStateChangedEvent == null)
			{
				return;
			}

			var invocationList = ActivatableStateChangedEvent.GetInvocationList().Cast<ActivatableStateChangedHandler>().ToArray();
			foreach (var handler in invocationList)
			{
				ActivatableStateChangedEvent -= handler;
			}

			ActivatableStateChangedEvent = null;
		}
	}
}