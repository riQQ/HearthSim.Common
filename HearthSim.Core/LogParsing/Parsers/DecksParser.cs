using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HearthDb.Deckstrings;
using HearthSim.Core.LogParsing.Interfaces;
using HearthSim.Core.LogReading.Data;

namespace HearthSim.Core.LogParsing.Parsers
{
	public class DecksParser : ILogParser
	{
		private readonly List<Deck> _current;
		private readonly List<string> _currentDeck;
		private State _state;

		public DecksParser()
		{
			_current = new List<Deck>();
			_currentDeck = new List<string>();
			_state = State.None;
		}

		public string LogName { get; } = "Decks";

		public void Parse(Line line)
		{
			if(line.Text == "Deck Contents Received:")
			{
				_state = State.ReceivingDecks;
				InvokeFoundDecksAsync();
			}
			else if(line.Text == "Finding Game With Deck:" || line.Text == "Starting Arena Game With Deck:")
			{
				if(_state == State.ReceivingDecks)
					InvokeFoundDecks();
				_state = State.FindingGame;
			}
			else
			{
				_currentDeck.Add(line.Text);
				if(_currentDeck.Count == 3)
				{
					var deck = DeckSerializer.Deserialize(string.Join("\n", _currentDeck));
					if(_state == State.FindingGame)
						FindingGame?.Invoke(deck);
					else
						_current.Add(deck);
					_currentDeck.Clear();
				}
			}
		}

		private async void InvokeFoundDecksAsync()
		{
			await Task.Delay(1000);
			InvokeFoundDecks();
		}

		private void InvokeFoundDecks()
		{
			if(_state != State.ReceivingDecks || _current.Count <= 0)
				return;
			FoundDecks?.Invoke(_current);
			_current.Clear();
		}

		public event Action<List<Deck>> FoundDecks;
		public event Action<Deck> FindingGame;

		private enum State
		{
			None,
			ReceivingDecks,
			FindingGame
		}
	}
}
