namespace GisCollection
{
    internal class MapperNode<TKey, TValue>
    {
        internal readonly TKey _key;
        internal TValue _value;
        internal readonly int[] _hashes;
        internal volatile MapperNode<TKey, TValue> _next;

        internal MapperNode(
            TKey key,
            TValue value,
            int[] hashes,
            MapperNode<TKey, TValue> next)
        {
            _key = key;
            _value = value;
            _hashes = hashes;
            _next = next;
        }

        /// <summary>
        /// Add a new node or update current if keys are equal
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <param name="hashes">Hashes of key properties</param>
        /// <param name="added">Out parameter. Represents number of new elements added (0 or 1)</param>
        public void Add(TKey key, TValue value, int[] hashes, out int added)
        {
            if (Mapper<TKey, TValue>.IsKeysEqual(_key, key))
            {
                _value = value;
                added = 0;
            }
            else if (_next != null)
                _next.Add(key, value, hashes, out added);
            else
            {
                _next = new MapperNode<TKey, TValue>(key, value, hashes, null);
                added = 1;
            }
        }

        /// <summary>
        /// Method sets <see cref="_next"/> - link to next node
        /// </summary>
        /// <param name="next">Represents next node</param>
        public void SetNext(MapperNode<TKey, TValue> next)
        {
            _next = next;
        }
    }
}