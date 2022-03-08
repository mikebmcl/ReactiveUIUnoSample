namespace ReactiveUIUnoSample
{
    public class ValueDisplayGenericPair<T>
    {
        public T Value { get; set; }
        public string Display { get; set; }
        public ValueDisplayGenericPair(T value, string display)
        {
            Value = value;
            Display = display;
        }

        public override string ToString()
        {
            return Display ?? "";
        }
    }

    public class ValueDisplayPairString : ValueDisplayGenericPair<string>
    {
        public ValueDisplayPairString(string value, string display) : base(value, display) { }
    }
}
