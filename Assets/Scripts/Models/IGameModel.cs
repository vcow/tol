using UniRx;

namespace Models
{
	public interface IGameModel
	{
		IReadOnlyReactiveCollection<IPlayerModel> Players { get; }
	}
}