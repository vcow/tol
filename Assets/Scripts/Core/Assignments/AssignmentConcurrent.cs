using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine.Assertions;

namespace Core.Assignments
{
	/// <summary>
	/// A Parallel execution of the Assignments.
	/// </summary>
	public class AssignmentConcurrent : IAssignment, IDisposable
	{
		private bool _completed;
		private bool _isStarted;
		private readonly HashSet<IAssignment> _assignments = new HashSet<IAssignment>();

		private bool _isDisposed;

		private SynchronizationContext _context;

		// ITask

		public void Start()
		{
			if (_isStarted || Completed || _isDisposed)
			{
				return;
			}

			_isStarted = true;
			_context = SynchronizationContext.Current;

			List<IAssignment> assignments, assignmentsToStart = new List<IAssignment>();
			lock (_assignments)
			{
				assignments = _assignments.ToList();
			}

			if (assignments.Count > 0)
			{
				assignments.ForEach(task =>
				{
					if (task.Completed)
					{
						UnityEngine.Debug.LogWarning("Task in Concurrent already completed.");
						lock (_assignments)
						{
							_assignments.Remove(task);
						}
					}
					else
					{
						assignmentsToStart.Add(task);
					}
				});

				if (assignmentsToStart.Count > 0)
				{
					assignmentsToStart.ForEach(assignment =>
					{
						assignment.CompleteEvent += OnAssignmentComplete;
						assignment.Start();
					});
				}
				else
				{
					Completed = true;
				}
			}
			else
			{
				Completed = true;
			}
		}

		private void OnAssignmentComplete(IAssignment assignment)
		{
			assignment.CompleteEvent -= OnAssignmentComplete;

			bool completed;
			lock (_assignments)
			{
				_assignments.Remove(assignment);
				completed = _assignments.Count <= 0;
			}

			if (completed)
			{
				Completed = true;
			}
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

		// \ITask

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

			lock (_assignments)
			{
				foreach (var assignment in _assignments)
				{
					assignment.CompleteEvent -= OnAssignmentComplete;
					(assignment as IDisposable)?.Dispose();
				}

				_assignments.Clear();
			}
		}

#if DEBUG_DESTRUCTION
		~AssignmentConcurrent()
		{
			Debug.Log("The Assignments Concurrent completely destroyed.");
		}
#endif

		// \IDisposable

		/// <summary>
		/// Clear Concurrent.
		/// </summary>
		public void Clear()
		{
			if (_isDisposed)
			{
				return;
			}

			lock (_assignments)
			{
				foreach (var assignment in _assignments)
				{
					assignment.CompleteEvent -= OnAssignmentComplete;
					(assignment as IDisposable)?.Dispose();
				}

				_assignments.Clear();
			}
		}

		/// <summary>
		/// Add Assignment to the Concurrent.
		/// </summary>
		/// <param name="assignment">Added assignment.</param>
		public void Add(IAssignment assignment)
		{
			Assert.IsNotNull(assignment, "Assignment can't be null.");

			if (_isDisposed)
			{
				return;
			}

			if (_isStarted)
			{
				UnityEngine.Debug.LogError("Can't add Assignment. Concurrent already started.");
				return;
			}

			if (Completed)
			{
				UnityEngine.Debug.LogError("Concurrent already completed, added Assignment will have no effect.");
			}

			lock (_assignments)
			{
				_assignments.Add(assignment);
			}
		}
	}
}