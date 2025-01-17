using Newtonsoft.Json;

namespace Models
{
	public struct PlayerModelRecord
	{
		[JsonIgnore] public string name;
		public uint scores;
		[JsonProperty(PropertyName = "last_level")] public int lastLevel;
	}
}