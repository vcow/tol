namespace Core.Assignments
{
	/// <summary>
	/// Delegate for complete event of IAssignment.
	/// </summary>
	public delegate void AssignmentCompleteHandler(IAssignment assignment);

	/// <summary>
	/// Assignment.
	/// </summary>
	public interface IAssignment
	{
		/// <summary>
		/// Launch the Assignment for execution.
		/// </summary>
		void Start();

		/// <summary>
		/// Flag indicated the Assignment is finished.
		/// </summary>
		bool Completed { get; }

		/// <summary>
		/// Assignment complete event. Completed flag changes to true.
		/// </summary>
		event AssignmentCompleteHandler CompleteEvent;
	}
}