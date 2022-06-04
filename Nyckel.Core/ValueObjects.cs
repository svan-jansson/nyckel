namespace Nyckel.Core.ValueObjects
{
    public record Key
    {
        private string _key;
        private Key() { _key = string.Empty; }
        public static Key Create(string key) => new() { _key = key };
        public string Get() => _key;
        public override string ToString() => _key;
    }

    public record Value
    {
        private object _value;

        private Value() { _value = new {}; }

        public static Value Create(object value) => new() { _value = value };
        public object Get() => _value;
        public T Get<T>() where T : class => _value as T;

        public override string ToString()
        {
            return _value.ToString();
        }
    }
}
