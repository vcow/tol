using Core.ScreenLocker;
using Zenject;

public class ProjectInstaller : MonoInstaller<ProjectInstaller>
{
	public override void InstallBindings()
	{
		Container.Bind<IScreenLockerManager>().To<ScreenLockerManager>().AsSingle();
	}
}