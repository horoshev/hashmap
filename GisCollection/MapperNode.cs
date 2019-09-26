using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using GisCollection.SimpleMapper;

namespace GisCollection
{
    public class MapperNode<TKey, TValue>
    {
        internal readonly TKey _key;
        internal TValue _value;
        internal volatile MapperNode<TKey, TValue> _next;
        internal readonly int[] _hashes;

        internal MapperNode(
            TKey key,
            TValue value,
            int[] hashes,
            MapperNode<TKey, TValue> next)
        {
            _key = key;
            _value = value;
            _next = next;
            _hashes = hashes;
        }

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

        public void SetNext(MapperNode<TKey, TValue> next)
        {
            _next = next;
        }
        
//        public List<TValue> GetChain()
//        {
//            var chain = new List<TValue>();
//            var next = this;
//
//            while (next != null && next._value != null)
//            {
//                chain.Add(next._value);
//                next = next._next;
//            }
//
//            return chain;
//        }

//        public List<TValue> GetValues(TKey key, string propertyName)
//        {
//            throw new NotImplementedException();
//        }
    }

}