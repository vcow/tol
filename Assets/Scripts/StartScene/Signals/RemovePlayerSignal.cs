namespace StartScene.Signals
{
	public sealed class RemovePlayerSignal
	{
		public string Name { get; }

		public RemovePlayerSignal(string name)
		{
			Name = name;
		}
	}
}