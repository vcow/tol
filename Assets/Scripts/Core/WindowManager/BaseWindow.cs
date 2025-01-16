using Core.Activatable;
using UnityEngine;
using UnityEngine.Assertions;

namespace Core.WindowManager
{
	/// <summary>
	/// The final base Window component.
	/// </summary>
	/// <typeparam name="TDerived">Self type.</typeparam>
	/// <typeparam name="TResult">Returned result type.</typeparam>
	public abstract class BaseWindow<TDerived, TResult> : WindowImpl where TDerived : BaseWindow<TDerived, TResult>
	{
		/// <summary>
		/// Delegate for close event of specific Window with the result of TResult type.
		/// </summary>
		public delegate void CloseWindowHandler(IWindow window, TResult result);

		/// <summary>
		/// Delegate for destroy event of specific Window with the result of TResult type.
		/// </summary>
		public delegate void DestroyWindowHandler(IWindow window, TResult result);

		private bool _isClosed;
		private ActivatableState _activatableState = ActivatableState.Inactive;

		protected TResult Result = default;
		protected bool IsDisposed { get; private set; }

		public event CloseWindowHandler CloseWindowEvent;
		public event DestroyWindowHandler DestroyWindowEvent;

		public override ActivatableState ActivatableState
		{
			get => _activatableState;
			protected set
			{
				if (value == _activatableState || IsDisposed)
				{
					return;
				}

				_activatableState = value;
				InvokeActivatableStateChangedEvent(_activatableState);
			}
		}

		public override bool Close(bool immediately = false)
		{
			if (_isClosed || IsDisposed || this.IsInactiveOrDeactivated())
			{
				return false;
			}

			if (ActivatableState == ActivatableState.ToActive)
			{
				Debug.LogWarningFormat("Trying to close window {0} before it was activated.", GetType().FullName);

				ActivatableStateChangedHandler autoCloseHandler = null;
				autoCloseHandler = (_, state) =>
				{
					if (state != ActivatableState.Active)
					{
						return;
					}

					ActivatableStateChangedEvent -= autoCloseHandler;
					Close(immediately);
				};

				ActivatableStateChangedEvent += autoCloseHandler;
				return true;
			}

			_isClosed = true;
			InvokeCloseWindowEvent();

			Deactivate(immediately);
			return true;
		}

		protected virtual void OnDestroy()
		{
			if (IsDisposed)
			{
				return;
			}

			InvokeDestroyWindowEvent();
			Dispose();
		}

		public override void Dispose()
		{
			if (IsDisposed)
			{
				return;
			}

			IsDisposed = true;

			base.Dispose();

			CloseWindowEvent = null;
			DestroyWindowEvent = null;
		}

		private void InvokeCloseWindowEvent()
		{
			Assert.IsFalse(IsDisposed, "Window was disposed before CloseWindowEvent invoked.");

			CloseWindowEvent?.Invoke(this, Result);
			base.InvokeCloseWindowEvent(Result);
		}

		private void InvokeDestroyWindowEvent()
		{
			Assert.IsFalse(IsDisposed, "Window was disposed before DestroyWindowEvent invoked.");

			DestroyWindowEvent?.Invoke(this, Result);
			base.InvokeDestroyWindowEvent(Result);
		}

		public override bool IsClosed => _isClosed;

#if DEBUG_DESTRUCTION
		~Window()
		{
			Debug.LogFormat("The window {0} was successfully destroyed.", WindowId);
		}
#endif
	}
}