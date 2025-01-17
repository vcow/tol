using System;
using Core.PersistentManager;
using Core.ScreenLocker;
using Core.SoundManager;
using Models;
using PreloaderScene.Assignments;
using Zenject;

public class ProjectInstaller : MonoInstaller<ProjectInstaller>
{
	public override void InstallBindings()
	{
		Container.Bind<IScreenLockerManager>().To<ScreenLockerManager>().AsSingle();
		Container.Bind<IPersistentManager>().To<PlayerPrefsPersistentManager>().AsSingle();
		Container.Bind<ISoundManager>().FromComponentInNewPrefabResource("SoundManager").AsSingle();
		Container.BindInterfacesAndSelfTo<GameModelController>().AsSingle();
		Container.Bind(typeof(WatchForGameModelChangesAssignment), typeof(IDisposable)).To<WatchForGameModelChangesAssignment>().AsSingle();

		SignalBusInstaller.Install(Container);
	}
}