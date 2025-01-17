using UniRx;

namespace Models
{
	public interface IPlayerModel
	{
		string Name { get; }
		IReadOnlyReactiveProperty<uint> Scores { get; }
		IReadOnlyReactiveProperty<int> LastLevel { get; }
	}
}