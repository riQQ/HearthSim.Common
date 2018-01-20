namespace HearthSim.Core.Util
{
	public static class Math
	{
		public static double DrawProbability(int copies, int deck, int draw)
		{
			if(draw >= deck || copies >= deck)
				return 1;
			return 1 - (BinomialCoefficient(deck - copies, draw) / BinomialCoefficient(deck, draw));
		}

		public static double BinomialCoefficient(int n, int k)
		{
			double result = 1;
			for(var i = 1; i <= k; i++)
			{
				result *= n - (k - i);
				result /= i;
			}
			return result;
		}
	}
}
