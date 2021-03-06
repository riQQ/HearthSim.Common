﻿
using System;
using HearthDb.Deckstrings;
using HearthSim.Core.Hearthstone.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HearthSim.Core.Test
{
	[TestClass]
	public class DeckTests
	{
		[TestMethod]
		public void GetDeckstring()
		{
			const string deckstring = "AAEBAYDTAgSyAq8EkbwCnOICDbQB7QKoBdQF3QiStgKBwgLrwgKbywLKywKmzgL70wLb4wIA";
			var hdbDeck = DeckSerializer.Deserialize(deckstring);
			var deck = new Hearthstone.Deck(DeckType.Constructed, hdbDeck);
			Assert.AreEqual(deckstring, deck.GetDeckstring());
		}
	}
}
