using PreloaderScene.Assignments;
using Zenject;

namespace PreloaderScene
{
	public sealed class PreloaderSceneInstaller : MonoInstaller<PreloaderSceneInstaller>
	{
		public override void InstallBindings()
		{
			Container.Bind<RestoreGameStateAssignment>().AsSingle();
			Container.Bind<RestoreGameModelAssignment>().AsSingle();
		}
	}
}