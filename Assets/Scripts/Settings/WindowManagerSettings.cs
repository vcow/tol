using System;
using System.Collections.Generic;
using System.Linq;
using Core.WindowManager;
using UnityEngine;
using Zenject;

namespace Settings
{
	[CreateAssetMenu(fileName = "WindowManagerSettings", menuName = "Window Manager/Window Manager Settings")]
	public sealed class WindowManagerSettings : ScriptableObjectInstaller<WindowManagerSettings>, IWindowManagerSettings
	{
		private Lazy<Dictionary<string, WindowImpl>> _windowsMap;

		[SerializeField] private int _startCanvasSortingOrder;
		[SerializeField] private string[] _groupHierarchy;
		[SerializeField] private BaseWindowProvider[] _windowProviders;

		public WindowManagerSettings()
		{
			_windowsMap = new Lazy<Dictionary<string, WindowImpl>>(() =>
				_windowProviders?.SelectMany(p => p.Windows)
					.GroupBy(window => window.WindowId)
					.Select(windows =>
					{
						var window = windows.First();
#if DEBUG || UNITY_EDITOR
						var numWindows = windows.Count();
						if (numWindows > 1)
						{
							Debug.LogErrorFormat("There are {0} registered windows for the {1} Window identifier.",
								numWindows, window.WindowId);
						}
#endif
						return window;
					})
					.ToDictionary(window => window.WindowId)
			);
		}

		public override void InstallBindings()
		{
			Container.Bind<IWindowManagerSettings>().FromInstance(this).AsSingle();
		}

		public IReadOnlyList<string> GroupHierarchy => _groupHierarchy;

		public IReadOnlyDictionary<string, WindowImpl> WindowsMap => _windowsMap.Value;

		public int StartCanvasSortingOrder => _startCanvasSortingOrder;
	}
}