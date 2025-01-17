using Core.WindowManager;
using Models;
using StartScene.Signals;
using Zenject;

namespace StartScene
{
	public sealed class StartSceneInstaller : MonoInstaller<StartSceneInstaller>
	{
		public override void InstallBindings()
		{
			Container.BindInterfacesTo<WindowManager>().AsSingle();
			Container.Bind<IGameModel>().FromResolveGetter<GameModelController>(controller => controller.Model).AsSingle();

			Container.DeclareSignal<AddPlayerSignal>();
			Container.DeclareSignal<RemovePlayerSignal>();
			Container.DeclareSignal<StartPlayGameSignal>();
		}
	}
}