using System;
using Core.Activatable;
using UnityEngine;

namespace Core.WindowManager
{
	/// <summary>
	/// Delegate for close Window event.
	/// </summary>
	public delegate void CloseWindowHandler(IWindow window, object result);

	/// <summary>
	/// Delegate for destroy Window event.
	/// </summary>
	public delegate void DestroyWindowHandler(IWindow window, object result);

	/// <summary>
	/// Window interface.
	/// </summary>
	public interface IWindow : IActivatable, IDisposable
	{
		/// <summary>
		/// Unique identifier of the Window.
		/// </summary>
		string WindowId { get; }

		/// <summary>
		/// The group to which the Window belongs. Flags Unique and Overlap are working within a
		/// group when calling a Window.
		/// </summary>
		string WindowGroup { get; }

		/// <summary>
		/// Flag indicating that the Window should be opened exclusively, i. e. the Window will not be open while
		/// there are other open windows from the same group, and other windows from the same group will not be opened
		/// while an exclusive Window is open. 
		/// </summary>
		bool IsUnique { get; }

		/// <summary>
		/// Flag indicating that the Window overlaps an underlying window from the same group, i. e. the underlying
		/// window will be deactivated, and returned to its original state (activated) after the overlapping Window
		/// is closed.
		/// </summary>
		bool Overlap { get; }

		/// <summary>
		/// The base canvas of Window.
		/// </summary>
		Canvas Canvas { get; }

		/// <summary>
		/// Close Window.
		/// </summary>
		/// <param name="immediately">Flag indicating that the Window close immediately (without visual effects).</param>
		/// <returns>Returns false if Window can't be closed.</returns>
		bool Close(bool immediately = false);

		/// <summary>
		/// When calling the WindowManager's ShowWindow() method, received arguments are passed to this method.
		/// </summary>
		/// <param name="args">Arguments list.</param>
		void SetArgs(object[] args);

		/// <summary>
		/// Flag indicating that the Window is closed.
		/// </summary>
		bool IsClosed { get; }

		/// <summary>
		/// Close Window event. Occurs when Window closed with Close() method.
		/// </summary>
		event CloseWindowHandler CloseWindowEvent;

		/// <summary>
		/// Destroy Window event. Occurs when Window destroyed.
		/// </summary>
		event DestroyWindowHandler DestroyWindowEvent;
	}
}