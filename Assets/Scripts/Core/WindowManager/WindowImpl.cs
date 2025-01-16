using Core.Activatable;
using UnityEngine;
using UnityEngine.UI;

namespace Core.WindowManager
{
	/// <summary>
	/// Common abstract base Window component for references, used in the WindowManager.
	/// </summary>
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster))]
	public abstract class WindowImpl : MonoBehaviour, IWindow
	{
		private Canvas _canvas;
		private CanvasScaler _canvasScaler;
		private GraphicRaycaster _graphicRaycaster;

		// ReSharper disable InconsistentNaming
		private event CloseWindowHandler _closeWindowEvent;
		private event DestroyWindowHandler _destroyWindowEvent;
		// ReSharper restore InconsistentNaming

		public event ActivatableStateChangedHandler ActivatableStateChangedEvent;

		event CloseWindowHandler IWindow.CloseWindowEvent
		{
			add => _closeWindowEvent += value;
			remove => _closeWindowEvent -= value;
		}

		event DestroyWindowHandler IWindow.DestroyWindowEvent
		{
			add => _destroyWindowEvent += value;
			remove => _destroyWindowEvent -= value;
		}

		[SerializeField] private string _windowGroup;
		[SerializeField] private bool _isUnique;
		[SerializeField] private bool _overlap;

		protected void InvokeActivatableStateChangedEvent(ActivatableState state)
		{
			ActivatableStateChangedEvent?.Invoke(this, state);
		}

		protected void InvokeCloseWindowEvent(object result)
		{
			_closeWindowEvent?.Invoke(this, result);
		}

		protected void InvokeDestroyWindowEvent(object result)
		{
			_destroyWindowEvent?.Invoke(this, result);
		}

		protected CanvasScaler CanvasScaler =>
			_canvasScaler ? _canvasScaler : _canvasScaler = GetComponent<CanvasScaler>();

		public GraphicRaycaster GraphicRaycaster =>
			_graphicRaycaster ? _graphicRaycaster : _graphicRaycaster = GetComponent<GraphicRaycaster>();

		public Canvas Canvas => _canvas ? _canvas : _canvas = GetComponent<Canvas>();
		public abstract string WindowId { get; }
		public virtual string WindowGroup => _windowGroup ?? string.Empty;
		public bool IsUnique => _isUnique;
		public bool Overlap => _overlap;
		public abstract void Activate(bool immediately = false);
		public abstract void Deactivate(bool immediately = false);
		public abstract ActivatableState ActivatableState { get; protected set; }
		public abstract bool Close(bool immediately = false);
		public abstract void SetArgs(object[] args);
		public abstract bool IsClosed { get; }

		public virtual void Dispose()
		{
			_closeWindowEvent = null;
			_destroyWindowEvent = null;
			ActivatableStateChangedEvent = null;
		}
	}
}