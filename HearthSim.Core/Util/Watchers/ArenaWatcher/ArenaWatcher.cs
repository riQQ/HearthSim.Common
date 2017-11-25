using System;
using System.Linq;
using System.Threading.Tasks;
using HearthMirror.Objects;
using HearthSim.Core.Util.EventArgs;

namespace HearthSim.Core.Util.Watchers.ArenaWatcher
{
	internal class ArenaWatcher
	{
		private const int MaxDeckSize = 30;
		private readonly IArenaProvider _arenaProvider;
		private readonly int _delay;
		private Card[] _prevChoices;
		private ArenaInfo _prevInfo;
		private int _prevSlot = -1;
		private bool _running;
		private bool _sameChoices;
		private bool _watch;

		public ArenaWatcher(IArenaProvider arenaProvider = null, int delay = 500)
		{
			_arenaProvider = arenaProvider ?? throw new ArgumentNullException(nameof(arenaProvider));
			_delay = delay;
		}

		public event Action<ArenaChoicesChangedEventArgs> ChoicesChanged;
		public event Action<ArenaCardPickedEventArgs> CardPicked;
		public event Action<ArenaDeckComlpeteEventArgs> DeckComplete;
		public event Action<ArenaRunCompleteEventArgs> RunComplete;

		public void Run()
		{
			_watch = true;
			if(!_running)
				Watch();
		}

		public void Stop() => _watch = false;

		private async void Watch()
		{
			_running = true;
			_prevSlot = -1;
			_prevInfo = null;
			while(_watch)
			{
				await Task.Delay(_delay);
				if(!_watch)
					break;
				if(Update())
					break;
			}
			_running = false;
		}

		public bool Update()
		{
			var arenaInfo = _arenaProvider.GetArenaInfo();
			if(arenaInfo == null)
				return false;
			var numCards = arenaInfo.Deck.Cards.Sum(x => x.Count);
			if(numCards == MaxDeckSize)
			{
				if(_prevSlot == MaxDeckSize)
					OnCardPicked(arenaInfo);
				DeckComplete?.Invoke(new ArenaDeckComlpeteEventArgs(arenaInfo));
				if(arenaInfo.Rewards?.Any() ?? false)
					RunComplete?.Invoke(new ArenaRunCompleteEventArgs(arenaInfo));
				_watch = false;
				return true;
			}
			if(HasChanged(arenaInfo, arenaInfo.CurrentSlot))
			{
				var choices = _arenaProvider.GetDraftChoices();
				if(choices == null || choices.Length == 0)
					return false;
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
						return false;
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
			return false;
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
