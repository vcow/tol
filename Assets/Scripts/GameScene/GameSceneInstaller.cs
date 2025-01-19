using GameScene.Logic;
using GameScene.Signals;
using Models;
using Zenject;

namespace GameScene
{
	public sealed class GameSceneInstaller : MonoInstaller<GameSceneInstaller>
	{
		public override void InstallBindings()
		{
			Container.Bind<IPlayerModel>().FromResolveGetter<PlayerModelController>(controller => controller.Model).AsSingle();
			Container.BindInterfacesAndSelfTo<GameLogic>().AsSingle();

			Container.DeclareSignal<ThrowRingSignal>();
			Container.DeclareSignal<CatchRingSignal>();
			Container.DeclareSignal<CatchWrongRingSignal>();
			Container.DeclareSignal<PlaceRingSignal>();
		}
	}
}