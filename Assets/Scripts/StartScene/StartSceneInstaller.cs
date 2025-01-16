using Core.WindowManager;
using Zenject;

namespace StartScene
{
	public sealed class StartSceneInstaller : MonoInstaller<StartSceneInstaller>
	{
		public override void InstallBindings()
		{
			Container.BindInterfacesTo<WindowManager>().AsSingle();
		}
	}
}