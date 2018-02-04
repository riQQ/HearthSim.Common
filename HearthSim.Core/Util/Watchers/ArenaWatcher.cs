using System;
using System.Linq;
using HearthMirror.Objects;
using HearthSim.Core.Hearthstone;
using HearthSim.Core.Util.EventArgs;
using Card = HearthMirror.Objects.Card;

namespace HearthSim.Core.Util.Watchers
{
	internal class ArenaWatcher : Watcher
	{
		private const int MaxDeckSize = 30;
		private readonly IGameDataProvider _gameDataProvider;
		private Card[] _prevChoices;
		private ArenaInfo _prevInfo;
		private int _prevSlot = -1;
		private bool _sameChoices;

		public ArenaWatcher(IGameDataProvider gameDataProvider = null, int delay = 500) : base(delay)
		{
			_gameDataProvider = gameDataProvider ?? throw new ArgumentNullException(nameof(gameDataProvider));
		}

		public event Action<ArenaChoicesChangedEventArgs> ChoicesChanged;
		public event Action<ArenaCardPickedEventArgs> CardPicked;
		public event Action<ArenaDeckComlpeteEventArgs> DeckComplete;
		public event Action<ArenaRunCompleteEventArgs> RunComplete;

		protected override void Reset()
		{
			_prevSlot = -1;
			_prevInfo = null;
		}

		public override UpdateResult Update()
		{
			var arenaInfo = _gameDataProvider.GetArenaInfo();
			if(arenaInfo == null)
				return UpdateResult.Continue;
			var numCards = arenaInfo.Deck.Cards.Sum(x => x.Count);
			if(numCards == MaxDeckSize)
			{
				if(_prevSlot == MaxDeckSize)
					OnCardPicked(arenaInfo);
				DeckComplete?.Invoke(new ArenaDeckComlpeteEventArgs(arenaInfo));
				if(arenaInfo.Rewards?.Any() ?? false)
					RunComplete?.Invoke(new ArenaRunCompleteEventArgs(arenaInfo));
				return UpdateResult.Break;
			}
			if(HasChanged(arenaInfo, arenaInfo.CurrentSlot))
			{
				var choices = _gameDataProvider.GetDraftChoices();
				if(choices == null || choices.Length == 0)
					return UpdateResult.Continue;
				if(arenaInfo.CurrentSlot > _prevSlot)
				{
					if(ChoicesHaveChanged(choices) || _sameChoices)
					{
						_sameChoices = false;
						ChoicesChanged?.Invoke(new ArenaChoicesChangedEventArgs(choices, arenaInfo.Deck));
					}
					else
					{
						_sameChoices = true;
						return UpdateResult.Continue;
					}
				}
				if(_prevSlot == 0 && arenaInfo.CurrentSlot == 1)
					OnHeroPicked(arenaInfo);
				else if(_prevSlot > 0 && arenaInfo.CurrentSlot > _prevSlot)
					OnCardPicked(arenaInfo);
				_prevSlot = arenaInfo.CurrentSlot;
				_prevInfo = arenaInfo;
				_prevChoices = choices;
			}
			return UpdateResult.Continue;
		}

		private bool ChoicesHaveChanged(Card[] choices) => _prevChoices == null || choices[0] != _prevChoices[0]
															|| choices[1] != _prevChoices[1] || choices[2] != _prevChoices[2];

		private bool HasChanged(ArenaInfo arenaInfo, int slot)
			=> _prevInfo == null || _prevInfo.Deck.Hero != arenaInfo.Deck.Hero || slot > _prevSlot;

		private void OnHeroPicked(ArenaInfo arenaInfo)
		{
			var hero = _prevChoices.FirstOrDefault(x => x.Id == arenaInfo.Deck.Hero);
			if(hero != null)
				CardPicked?.Invoke(new ArenaCardPickedEventArgs(hero, _prevChoices));
		}

		private void OnCardPicked(ArenaInfo arenaInfo)
		{
			var pick = arenaInfo.Deck.Cards.FirstOrDefault(
				x => !_prevInfo?.Deck.Cards.Any(c => x.Id == c.Id && x.Count == c.Count) ?? false);
			if(pick != null)
				CardPicked?.Invoke(new ArenaCardPickedEventArgs(new Card(pick.Id, 1, pick.Premium), _prevChoices));
		}
	}
}
