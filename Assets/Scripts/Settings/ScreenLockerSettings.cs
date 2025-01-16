using System.Collections.Generic;
using Core.ScreenLocker;
using UnityEngine;
using Zenject;

namespace Settings
{
	[CreateAssetMenu(fileName = "ScreenLockerSettings", menuName = "Screen Locker/Screen Locker Settings")]
	public class ScreenLockerSettings : ScriptableObjectInstaller<ScreenLockerSettings>, IScreenLockerSettings
	{
		[SerializeField] private BaseScreenLocker[] _screenLockers;

		public override void InstallBindings()
		{
			Container.Bind<IScreenLockerSettings>().FromInstance(this).AsSingle();
		}

		public IReadOnlyList<BaseScreenLocker> ScreenLockers => _screenLockers;
	}
}