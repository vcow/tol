using System;
using UnityEngine.Assertions;

namespace Core.Assignments
{
	/// <summary>
	/// Deferred Assignment. Accepts a closure that will be called when the Assignment starts.
	/// </summary>
	public class LazyAssignment : IAssignment, IDisposable
	{
		private bool _isDisposed;
		private bool _completed;
		private readonly Func<IAssignment> _closure;
		private IAssignment _assignment;

		public LazyAssignment(Func<IAssignment> closure)
		{
			_closure = closure;
		}

		// IAssignment

		public bool Completed
		{
			get => _completed;
			private set
			{
				if (value == _completed) return;
				_completed = value;

				Assert.IsTrue(_completed);
				CompleteEvent?.Invoke(this);
			}
		}

		public void Dispose()
		{
			if (_isDisposed)
			{
				return;
			}

			_isDisposed = true;

			if (_assignment != null)
			{
				_assignment.CompleteEvent -= SubAssignmentCompleteHandler;
				(_assignment as IDisposable)?.Dispose();
				_assignment = null;
			}
		}

		public void Start()
		{
			if (_isDisposed || _assignment != null || Completed)
			{
				return;
			}

			_assignment = _closure?.Invoke();
			if (_assignment != null)
			{
				if (_assignment.Completed)
				{
					Completed = true;
				}
				else
				{
					_assignment.CompleteEvent += SubAssignmentCompleteHandler;
					_assignment.Start();
				}
			}
			else
			{
				Completed = true;
			}
		}

		public event AssignmentCompleteHandler CompleteEvent;

		// \IAssignment

		private void SubAssignmentCompleteHandler(IAssignment assignment)
		{
			assignment.CompleteEvent -= SubAssignmentCompleteHandler;
			if (_assignment == assignment)
			{
				_assignment = null;
			}

			Completed = true;
		}
	}
}