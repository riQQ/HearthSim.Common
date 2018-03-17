namespace HearthSim.Core.Util.EventArgs
{
	public class ValueChangedEventArgs<T> : System.EventArgs
	{
		public ValueChangedEventArgs(T oldValue, T newValue)
		{
			OldValue = oldValue;
			NewValue = newValue;
		}

		public T OldValue { get; }
		public T NewValue { get; }
	}
}