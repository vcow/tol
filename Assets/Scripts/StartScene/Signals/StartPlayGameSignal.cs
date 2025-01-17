namespace StartScene.Signals
{
	public sealed class StartPlayGameSignal
	{
		public string PlayerName { get; }
		public int FromLevel { get; }

		public StartPlayGameSignal(string playerName, int fromLevel)
		{
			PlayerName = playerName;
			FromLevel = fromLevel;
		}
	}
}