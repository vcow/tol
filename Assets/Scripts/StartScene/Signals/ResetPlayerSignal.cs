namespace StartScene.Signals
{
	public sealed class ResetPlayerSignal
	{
		public string Name { get; }

		public ResetPlayerSignal(string name)
		{
			Name = name;
		}
	}
}