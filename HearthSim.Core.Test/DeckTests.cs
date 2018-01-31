
using System;
using HearthDb.Deckstrings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HearthSim.Core.Test
{
	[TestClass]
	public class DeckTests
	{
		[TestMethod]
		public void GetDeckstring()
		{
			const string deckstring = "AAECAYDTAgSyAq8EkbwCnOICDbQB7QKoBdQF3QiStgKBwgLrwgKbywLKywKmzgL70wLb4wIA";
			var hdbDeck = DeckSerializer.Deserialize(deckstring);
			var deck = new Hearthstone.Deck(hdbDeck);
			Assert.AreEqual(deckstring, deck.GetDeckstring());
		}
	}
}
