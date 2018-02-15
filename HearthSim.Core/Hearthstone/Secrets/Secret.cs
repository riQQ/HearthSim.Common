using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using HearthSim.Core.Hearthstone.Entities;

namespace HearthSim.Core.Hearthstone.Secrets
{
	public class Secret
	{
		public Entity Entity { get; }
		public List<string> PossibleSecrets { get; }
		public List<string> SolvedSecrets { get; }
		public List<string> RemainingSecrets => PossibleSecrets.Where(x => !SolvedSecrets.Contains(x)).ToList();

		public Secret(Entity entity)
		{
			Entity = entity;
			PossibleSecrets = SecretList.Get(entity.GetTag(GameTag.CLASS)).Select(x => x.Id).ToList();
			SolvedSecrets = new List<string>();
		}

		public void Solve(IEnumerable<string> solved) 
			=> SolvedSecrets.AddRange(solved.Where(x => !SolvedSecrets.Contains(x)));
	}
}
