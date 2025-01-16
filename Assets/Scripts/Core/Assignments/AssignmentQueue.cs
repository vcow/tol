using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.Assertions;

namespace Core.Assignments
{
	/// <summary>
	/// The Queue of the Assignments.
	/// </summary>
	public class AssignmentQueue : IAssignment, IDisposable
	{
		private bool _completed;
		private readonly Queue<IAssignment> _queue = new Queue<IAssignment>();

		private IAssignment _currentAssignment;

		private bool _isDisposed;

		private SynchronizationContext _context;

		// IAssignment

		public void Start()
		{
			if (_currentAssignment != null || Completed || _isDisposed)
			{
				return;
			}

			_context = SynchronizationContext.Current;

			StartNextAssignment();
		}

		public bool Completed
		{
			get => _completed;
			private set
			{
				if (value == _completed) return;
				_completed = value;

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

			lock (_queue)
			{
				foreach (var assignment in _queue)
				{
					(assignment as IDisposable)?.Dispose();
				}

				_queue.Clear();
			}

			if (_currentAssignment != null)
			{
				_currentAssignment.CompleteEvent -= OnAssignmentComplete;
				(_currentAssignment as IDisposable)?.Dispose();
				_currentAssignment = null;
			}
		}

#if DEBUG_DESTRUCTION
		~AssignmentQueue()
		{
			Debug.Log("The Assignments Queue completely destroyed.");
		}
#endif

		// \IDisposable

		/// <summary>
		/// Clear the Queue.
		/// </summary>
		public void Clear()
		{
			if (_isDisposed)
			{
				return;
			}

			lock (_queue)
			{
				_queue.Clear();
			}

			if (_currentAssignment != null)
			{
				_currentAssignment.CompleteEvent -= OnAssignmentComplete;
				_currentAssignment = null;
			}
		}

		/// <summary>
		/// Add Assignment to the Queue.
		/// </summary>
		/// <param name="assignment">Added Assignment.</param>
		public void Add(IAssignment assignment)
		{
			Assert.IsNotNull(assignment, "Assignment can't be null.");

			if (_isDisposed)
			{
				return;
			}

			if (Completed)
			{
				UnityEngine.Debug.LogError("Queue already completed, added Assignment will have no effect.");
			}

			lock (_queue)
			{
				_queue.Enqueue(assignment);
			}
		}

		private void StartNextAssignment()
		{
			lock (_queue)
			{
				_currentAssignment = _queue.Count > 0 ? _queue.Dequeue() : null;
			}

			if (_currentAssignment == null)
			{
				Completed = true;
			}
			else if (_currentAssignment.Completed)
			{
				UnityEngine.Debug.LogWarning("Assignment in Queue already completed.");

				_currentAssignment = null;
				StartNextAssignment();
			}
			else
			{
				_currentAssignment.CompleteEvent += OnAssignmentComplete;
				_currentAssignment.Start();
			}
		}

		private void OnAssignmentComplete(IAssignment assignment)
		{
			assignment.CompleteEvent -= OnAssignmentComplete;
			if (_currentAssignment == assignment)
			{
				_currentAssignment = null;
			}

			StartNextAssignment();
		}
	}
}