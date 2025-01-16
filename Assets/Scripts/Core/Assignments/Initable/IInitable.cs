namespace Core.Assignments.Initable
{
	/// <summary>
	/// Delegate for the init complete event of IInitable.
	/// </summary>
	public delegate void InitCompleteHandler(IInitable initable);

	/// <summary>
	/// The interface of the entity, that has initialization.
	/// </summary>
	public interface IInitable
	{
		/// <summary>
		/// The start initialization.
		/// </summary>
		/// <param name="args">The initialization arguments.</param>
		void Init(params object[] args);

		/// <summary>
		/// A flag indicating the initialization is complete.
		/// </summary>
		bool IsInited { get; }

		/// <summary>
		/// The initialization complete event.
		/// </summary>
		event InitCompleteHandler InitCompleteEvent;
	}
}