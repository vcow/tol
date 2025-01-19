using Models;

namespace GameScene.Signals
{
	public class CatchRingSignal
	{
		public RingColor RingType { get; }

		public CatchRingSignal(RingColor ringType)
		{
			RingType = ringType;
		}
	}
}