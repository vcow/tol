using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Models
{
	public enum RingColor
	{
		Red,
		Green,
		Blue,
		Yellow,
		White,
		Black
	}

	public class LevelModel
	{
		public int Index { get; }
		public (IReadOnlyList<RingColor> tower1, IReadOnlyList<RingColor> tower2, IReadOnlyList<RingColor> tower3) InitialState { get; }
		public (IReadOnlyList<RingColor> tower1, IReadOnlyList<RingColor> tower2, IReadOnlyList<RingColor> tower3) GoalState { get; }
		public int MinStepsNum { get; }
		public int MaxStepsNum { get; }

		public LevelModel(int index, string rawData)
		{
			Index = index;

			var levelRecord = JsonConvert.DeserializeObject<LevelRecord>(rawData);
			InitialState = (levelRecord.initialState.tower1, levelRecord.initialState.tower2, levelRecord.initialState.tower3);
			GoalState = (levelRecord.goalState.tower1, levelRecord.goalState.tower2, levelRecord.goalState.tower3);
			MinStepsNum = levelRecord.minSteps;
			MaxStepsNum = levelRecord.maxSteps;
		}

		private class StateRecord
		{
			[JsonProperty(ItemConverterType = typeof(StringEnumConverter))] public RingColor[] tower1;
			[JsonProperty(ItemConverterType = typeof(StringEnumConverter))] public RingColor[] tower2;
			[JsonProperty(ItemConverterType = typeof(StringEnumConverter))] public RingColor[] tower3;
		}

		private class LevelRecord
		{
			[JsonProperty(PropertyName = "initial_state")] public StateRecord initialState;
			[JsonProperty(PropertyName = "goal_state")] public StateRecord goalState;
			[JsonProperty(PropertyName = "min_steps")] public int minSteps;
			[JsonProperty(PropertyName = "max_steps")] public int maxSteps;
		}
	}
}