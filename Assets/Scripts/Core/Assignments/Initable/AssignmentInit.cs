using System;
using System.Threading;
using UnityEngine.Assertions;

namespace Core.Assignments.Initable
{
	/// <summary>
	/// The utility Assignment to initialize the IInitable object.
	/// </summary>
	public class AssignmentInit : IAssignment, IDisposable
	{
		private bool _completed;
		private readonly IInitable _initable;
		private readonly object[] _args;

		private bool _isDisposed;
		private bool _isStarted;

		private readonly object _lock = new object();
		private SynchronizationContext _context;

		public AssignmentInit(IInitable initable, object[] args = null)
		{
			_initable = initable ?? throw new ArgumentNullException();
			_args = args ?? Array.Empty<object>();
		}

		// IAssignment

		public bool Completed
		{
			get => _completed;
			private set
			{
				lock (_lock)
				{
					if (value == _completed) return;
					_completed = value;
				}

				Assert.IsTrue(_completed);
				if (_context != null)
				{
					_context.Post(state => CompleteEvent?.Invoke(this), null);
					_context = null;
				}
				else
				{
					CompleteEvent?.Invoke(this);
				}
			}
		}

		public event AssignmentCompleteHandler CompleteEvent;

		public void Start()
		{
			if (_isStarted || _isDisposed)
			{
				return;
			}

			_isStarted = true;
			_context = SynchronizationContext.Current;

			if (_initable.IsInited)
			{
				UnityEngine.Debug.LogError("The initializable object is ready when the Assignment starts.");
				Completed = true;
				return;
			}

			_initable.InitCompleteEvent += OnInitComplete;
			_initable.Init(_args);
		}

		private void OnInitComplete(IInitable initable)
		{
			_initable.InitCompleteEvent -= OnInitComplete;
			Completed = true;
		}

		// \IAssignment

		// IDisposable

		public void Dispose()
		{
			if (_isDisposed)
			{
				return;
			}

			_isDisposed = true;

			CompleteEvent = null;
			_context = null;

			if (_isStarted)
			{
				_initable.InitCompleteEvent -= OnInitComplete;
			}
		}

		// \IDisposable
	}
}