using System.Collections.Generic;
using Core.WindowManager;
using UnityEngine;

namespace Settings
{
	[CreateAssetMenu(fileName = "CommonWindowProvider", menuName = "Window Manager/Common Window Provider")]
	public class CommonWindowProvider : BaseWindowProvider
	{
		[SerializeField] private WindowImpl[] _windows;

		public override IReadOnlyList<WindowImpl> Windows => _windows;
	}
}