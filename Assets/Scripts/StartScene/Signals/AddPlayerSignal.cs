namespace StartScene.Signals
{
	public sealed class AddPlayerSignal
	{
		public string Name { get; }
		public uint Scores { get; }
		public int LastLevel { get; }

		public AddPlayerSignal(string name, uint scores = 0u, int lastLevel = -1)
		{
			Name = name;
			Scores = scores;
			LastLevel = lastLevel;
		}
	}
}