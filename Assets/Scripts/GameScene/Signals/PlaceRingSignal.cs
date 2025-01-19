using Models;

namespace GameScene.Signals
{
	public class PlaceRingSignal
	{
		public RingColor RingType { get; }
		public int TowerIndex { get; }
		public int PinIndex { get; }

		public PlaceRingSignal(RingColor ringType, int towerIndex, int pinIndex)
		{
			RingType = ringType;
			TowerIndex = towerIndex;
			PinIndex = pinIndex;
		}
	}
}