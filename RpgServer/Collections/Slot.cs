namespace RpgServer.Collections
{
    public class Slot<T>
    {
        public Slot(T defaultValue)
        {
            _value = defaultValue;
        }

        public delegate T? Load();

        public T Get(Load load)
        {
            if (_isUpdated) { return _value; }

            var value = load();
            if (value == null) { return _value; }

            _value = value;

            _isUpdated = true;

            return _value;
        }

        public void Set(T value)
        {
            _value = value;

            _isUpdated = true;
        }

        private T _value;
        private bool _isUpdated;
    }
}
