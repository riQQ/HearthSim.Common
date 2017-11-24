using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HearthDb.Deckstrings;
using HearthSim.Core.LogParsing.Interfaces;
using HearthSim.Core.LogReading.Data;
using HearthSim.Core.Util.EventArgs;

namespace HearthSim.Core.LogParsing.Parsers
{
	public class DecksParser : ILogParser
	{
		private readonly List<Deck> _current;
		private readonly List<string> _currentDeck;

		private readonly string[] _findingGame =
		{
			"Finding Game With Deck:",
			"Finding Friendly Game With Deck:",
			"Starting Arena Game With Deck:"
		};

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
			const string findingGameWithHero = "Finding Game With Hero: ";
			if(line.Text == "Deck Contents Received:")
			{
				_state = State.ReceivingDecks;
				InvokeFoundDecksAsync();
			}
			else if(line.Text == "Finished Editing Deck:")
				UpdateState(State.FinishedEditingDeck);
			else if(_findingGame.Contains(line.Text))
				UpdateState(State.FindingGameWithDeck);
			else if(line.Text == findingGameWithHero)
			{
				UpdateState(State.FindingGameWithHero);
				if(int.TryParse(line.Text.Substring(findingGameWithHero.Length), out var dbfId))
					FindingGame?.Invoke(new QueuedForGameEventArgs(dbfId));
			}
			else
			{
				_currentDeck.Add(line.Text);
				if(_currentDeck.Count == 3)
				{
					var deck = DeckSerializer.Deserialize(string.Join("\n", _currentDeck));
					if(_state == State.FindingGameWithDeck)
						FindingGame?.Invoke(new QueuedForGameEventArgs(deck));
					else if(_state == State.FinishedEditingDeck)
						EditedDeck?.Invoke(new DeckEditedEventArgs(deck));
					else
						_current.Add(deck);
					_currentDeck.Clear();
				}
			}
		}

		private void UpdateState(State state)
		{
			if(_state == State.ReceivingDecks)
				InvokeFoundDecks();
			_state = state;
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
			FoundDecks?.Invoke(new ConstructedDeckFoundEventArgs(_current));
			_current.Clear();
		}

		public event Action<DeckEditedEventArgs> EditedDeck;
		public event Action<ConstructedDeckFoundEventArgs> FoundDecks;
		public event Action<QueuedForGameEventArgs> FindingGame;

		private enum State
		{
			None,
			ReceivingDecks,
			FindingGameWithDeck,
			FindingGameWithHero,
			FinishedEditingDeck
		}
	}
}
