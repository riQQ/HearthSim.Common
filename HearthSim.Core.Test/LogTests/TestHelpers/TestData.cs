using System.IO;
using HearthMirror.Objects;
using Newtonsoft.Json;

namespace HearthSim.Core.Test.LogTests.TestHelpers
{
	public class TestData
	{
		[JsonProperty("file")]
		public string LogFile { get; set; }

		[JsonProperty("local_player")]
		public TestDataPlayer LocalPlayer { get; set; }

		[JsonProperty("opposing_player")]
		public TestDataPlayer OpposingPlayer { get; set; }

		public class TestDataPlayer
		{
			[JsonProperty("name")]
			public string Name { get; set; }

			[JsonProperty("id")]
			public int Id { get; set; }

			[JsonProperty("deck")]
			public string Deck { get; set; }

			[JsonProperty("deck_cards")]
			public string[] DeckCards { get; set; }

			public MatchInfo.Player ToMatchInfoPlayer()
				=> new MatchInfo.Player(Id, Name, 0, 0, 0, 0, 0, 0, 0, null, null);
		}

		public static TestData Load(string file)
		{
			using(var sr = new StreamReader(file))
				return JsonConvert.DeserializeObject<TestData>(sr.ReadToEnd());
		}
	}
}
