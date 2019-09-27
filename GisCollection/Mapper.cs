using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace GisCollection
{
    [AttributeUsage(AttributeTargets.Property)]
    public class KeyAttribute : Attribute
    {
        /// <summary>
        /// Position of a property in a array.
        /// Indexing starts from 1.
        /// </summary>
        public int Order { get; }
        public KeyAttribute(int order)
        {
            if (order < 0) throw new ArgumentException(nameof(order) + " < 0");
            Order = order;
        }
    }
    public static class IntExtension
    {
        public static int Abs(this int value) => Math.Abs(value);
        public static int Fit(this int value, int range) => value % range; 
        public static int Pow(this int basis, int power)
        {
            return Enumerable
                .Repeat(basis, power)
                .Aggregate(1, (a, b) => a * b);
        }
    }
    
    /// <summary>
    /// Implements a thread-safe version of a hash table with multiple keys
    /// </summary>
    /// <typeparam name="TKey">Type of keys in collection</typeparam>
    /// <typeparam name="TValue">Type of values in collection</typeparam>
    /// <remarks>
    /// Actually a collection is not fully thread-safe for the moment
    /// </remarks>
    public class Mapper<TKey, TValue> : IEnumerable<TValue>
    {
        // only for properties with attribute Key
        /// <summary>
        /// Determine type of TKey
        /// </summary>
        private static readonly Type KeyType = typeof(TKey);
        /// <summary>
        /// Hold array of TKey properties with <see cref="KeyAttribute"/>
        /// </summary>
        private static readonly PropertyInfo[] KeyProperties;
        /// <summary>
        /// Number of TKey properties with <see cref="KeyAttribute"/>
        /// </summary>
        private static readonly int KeySize;
        public int keySize => KeySize;
        
        /// <summary>
        /// Initialize static values <see cref="KeyProperties"/> and <see cref="KeySize"/>
        /// </summary>
        static Mapper()
        {
            KeyProperties = KeyType.GetProperties()
                .Where(x => Attribute.IsDefined(x, typeof(KeyAttribute)))
                .OrderBy(x => ((KeyAttribute) x.GetCustomAttribute(typeof(KeyAttribute), false)).Order)
                .ToArray();
            
            KeySize = KeyProperties.Length;
            
            if (KeySize == 0)
                throw new ArgumentException( $"{nameof(KeySize)} should be > 0");
        }

        /// <summary>
        /// List of keys which was added to a collection
        /// </summary>
        private List<TKey> _keys { get; set; }
        /// <summary>
        /// Index range for each key property in TKey
        /// </summary>
        private int _rangePerKey;
        /// <summary>
        /// Multidimensional table with an index for each key property in TKey
        /// Represented an one-dimensional array
        /// </summary>
        private MapperNode<TKey, TValue>[] _table;

        /// <summary>
        /// Number of elements in a collection.
        /// Can be greater than <see cref="Capacity"/> due of collisions
        /// </summary>
        public int Size { get; private set; }
        /// <summary>
        /// Number of filled cells in <see cref="_table"/>
        /// </summary>
        public int NonEmptyNodes { get; private set; }
        /// <summary>
        /// Number of cells in <see cref="_table"/>
        /// </summary>
        public int Capacity => _rangePerKey.Pow(KeySize);
        /// <summary>
        /// Tells that all cells in table is filled
        /// </summary>
        public bool Full => NonEmptyNodes == Capacity;
        
        /// <summary>
        /// Initialize a new Mapper instance
        /// </summary>
        /// <param name="initRangePerKey">The initial range of indices for each key property in <see cref="TKey"/>.
        /// This value affects the <see cref="Capacity"/> of the <see cref="_table"/></param>
        public Mapper(int initRangePerKey = 32)
        {
            if (initRangePerKey <= 0)
                throw new ArgumentException($"{nameof(initRangePerKey)} should be > 0");
            
            _rangePerKey = initRangePerKey;
            _table = new MapperNode<TKey, TValue>[_rangePerKey.Pow(KeySize)];
            _keys = new List<TKey>();
            Size = NonEmptyNodes = 0;
        }

        /// <summary>
        /// Indexer, which allows to get and also set values of a collection by a key.
        /// </summary>
        /// <param name="key">Received key value</param>
        /// <exception cref="ArgumentNullException"></exception>
        public TValue this[TKey key]
        {
            set
            {
                if (key == null) throw new ArgumentNullException(nameof(key));
                if (value == null) throw new ArgumentNullException(nameof(value));

                lock (_keys) _keys.Add(key); // should contain unique values but cause performance
                
                if (NonEmptyNodes == Capacity) ExpandTable();
                
                var hashes = HashesOfKey(key);
                var index = HashesToTableIndex(hashes, _rangePerKey);
                
                if (_table[index] == null)
                {
                    _table[index] = new MapperNode<TKey, TValue>(key, value, hashes, null);
                    NonEmptyNodes++;
                    Size++;
                }
                else
                {
                    lock (_table[index])
                    {
                        _table[index].Add(key, value, hashes, out var added);
                        Size += added;
                    }
                }
            }
            get
            {
                if (key == null) throw new ArgumentNullException(nameof(key));
                
                var index = KeyToTableIndex(key, _rangePerKey);
                var node = _table[index];
                while (node != null)
                {
                    lock (node)
                    {
                        if (IsKeysEqual(node._key, key))
                            return node._value;
                        else
                            node = node._next;
                    }
                }

                return default(TValue);
            }
        }

        /// <summary>
        /// Experimental indexer which allows to get values by a key without one key property.
        /// Get values by a key without received property name.
        /// </summary>
        /// <param name="key">Received key</param>
        /// <param name="excludePropertyName">Exclude property name</param>
        public List<TValue> this[TKey key, string excludePropertyName]
        {
            get
            {
                if (key == null) throw new ArgumentNullException(nameof(key));

                var res = new List<TValue>();
                var propertyIndex = Array.FindIndex(KeyProperties, p => p.Name == excludePropertyName);
                if (propertyIndex < 0) throw new ArgumentException($"Wrong property name");
                var property = KeyProperties[propertyIndex];
                
                var hashes = HashesOfKey(key);
                var offset = 0;
                var propertyPower = 0;

                for (var i = 0; i < hashes.Length; i++)
                {
                    if (i != propertyIndex)
                        offset += hashes[i].Fit(_rangePerKey) * _rangePerKey.Pow(KeySize - i - 1);
                    else
                        propertyPower = KeySize - i - 1;
                }
                
                for (var i = 0; i < _rangePerKey; i++)
                {
                    var index = offset + i * _rangePerKey.Pow(propertyPower);

                    var current = _table[index];
                    while (current != null)
                    {
                        lock (current)
                        {
                            if (IsKeysEqual(current._key, key, property.Name))
                                res.Add(current._value);
                        
                            current = current._next;
                        }
                    }
                }

                return res;
            }
        }

        /// <summary>
        /// Experimental indexer which allows to get values by a key without one key property.
        /// Get values by a key without received property order in Key Attribute in a key.
        /// </summary>
        /// <param name="key">Received key</param>
        /// <param name="excludePropertyOrder">Exclude property Order value of Key Attribute</param>
        public List<TValue> this[TKey key, int excludePropertyOrder]
        {
            get
            {
                if (key == null) throw new ArgumentNullException(nameof(key));
                
                var res = new List<TValue>();
                var propertyIndex = Array.FindIndex(KeyProperties,
                    p => ((KeyAttribute) p.GetCustomAttribute(typeof(KeyAttribute), false)).Order == excludePropertyOrder);

                if (propertyIndex < 0) throw new ArgumentException($"Wrong property order");
                var property = KeyProperties[propertyIndex];
                
                var hashes = HashesOfKey(key);
                var offset = 0;
                var propertyPower = 0;

                for (var i = 0; i < hashes.Length; i++)
                {
                    if (i != propertyIndex)
                        offset += hashes[i].Fit(_rangePerKey) * _rangePerKey.Pow(KeySize - i - 1);
                    else
                        propertyPower = KeySize - i - 1;
                }
                
                for (var i = 0; i < _rangePerKey; i++)
                {
                    var index = offset + i * _rangePerKey.Pow(propertyPower);

                    var current = _table[index];
                    while (current != null)
                    {
                        if (IsKeysEqual(current._key, key, property.Name))
                            res.Add(current._value);
                        
                        current = current._next;
                    }
                }

                return res;
            }
        }
        
        /// <summary>
        /// Find elements in a collection with a given id
        /// </summary>
        /// <param name="id">Id to a key should contain</param>
        /// <returns>Collection of elements with received id in key</returns>
        public List<TValue> GetValuesById(int id) // only for key with size = 2
        {
            var idIndex = id.GetHashCode().Abs().Fit(_rangePerKey);
            
            var data = KeyProperties.Select((value, index) => (index: index + 1, name: value.Name));
            var powerId = KeySize - data.First(value => value.name == "Id").index;
            var powerName = KeySize - data.First(value => value.name == "Name").index;
            
            var offset = idIndex * _rangePerKey.Pow(powerId);
            var propertyOffset = _rangePerKey.Pow(powerName);
            
            return GetValuesByPropertyName("Id", id, propertyOffset, offset);
            /*for (var i = 0; i < _rangePerKey; i++)
            {
                var index = offset + i * _rangePerKey.Pow(powerName);
                
                if (_table[index] == null) continue;
                lock (_table[index])
                {
                    var key = _table[index]._key;
                    var isIdSame = KeyProperties
                        .Where(property => property.Name == "Id")
                        .Select(property => property.GetValue(key).Equals(id))
                        .First();

                    if (isIdSame) res.Add(_table[index]._value);
                }
            }
            
            return res;*/
        }

        /// <summary>
        /// Find elements in a collection with a given name.
        /// </summary>
        /// <param name="name">Name to a key should contain</param>
        /// <returns>Collection of elements with received name in key</returns>
        public List<TValue> GetValuesByName(string name)
        {
            if (name == null)
                throw new ArgumentNullException($"{nameof(name)} should be not null value");

            var nameIndex = name.GetHashCode().Abs().Fit(_rangePerKey);
            
            var data = KeyProperties.Select((value, index) => (index: index + 1, name: value.Name));
            var powerId = KeySize - data.First(value => value.name == "Id").index;
            var powerName = KeySize - data.First(value => value.name == "Name").index;
            var offset = nameIndex * _rangePerKey.Pow(powerName);
            var propertyOffset = _rangePerKey.Pow(powerId);

            return GetValuesByPropertyName("Name", name, propertyOffset, offset);

            /*for (var i = 0; i < _rangePerKey; i++)
            {
                var index = offset + i * _rangePerKey.Pow(powerId);
                
                if (_table[index] == null) continue;
                lock (_table[index])
                {
                    var key = _table[index]._key;
                    var isNameSame = KeyProperties
                        .Where(property => property.Name == "Name")
                        .Select(property => property.GetValue(key).Equals(name))
                        .First();

                    if (isNameSame) res.Add(_table[index]._value);
                }
            }
            
            return res;*/
        }

        /// <summary>
        /// Helper method for <see cref="GetValuesById"/> and <see cref="GetValuesByName"/>
        /// </summary>
        /// <param name="propertyName">Name of property</param>
        /// <param name="propertyValue">Value of property</param>
        /// <param name="propertyOffset">Index offset of property</param>
        /// <param name="offset">Index offset of other properties</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private List<TValue> GetValuesByPropertyName<T>(
            string propertyName,
            T propertyValue,
            int propertyOffset,
            int offset)
        {
            var res = new List<TValue>();

            for (var i = 0; i < _rangePerKey; i++)
            {
                var index = offset + i * propertyOffset;
                
                var current = _table[index];
                while (current != null)
                {
                    lock (current)
                    {
                        var key = current._key;
                        var isNameSame = KeyProperties
                            .Where(property => property.Name == propertyName)
                            .Select(property => property.GetValue(key).Equals(propertyValue))
                            .First();

                        if (isNameSame) res.Add(current._value);
                        current = current._next;
                    }
                }
            }

            return res;
        }

        /// <summary>
        /// Remove in a collection by a key
        /// </summary>
        /// <param name="key">Key to remove</param>
        public void RemoveKey(TKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            var index = KeyToTableIndex(key, _rangePerKey);
            if (_table[index] == null) return;
            
            lock (_table[index])
            {
                if (IsKeysEqual(_table[index]._key, key))
                {
                    _table[index] = _table[index]._next;
                    RemoveKeyFromArray(key);
                    return;
                }

                var previous = _table[index];
                var current = _table[index]._next;

                while (current != null)
                {
                    if (IsKeysEqual(current._key, key))
                    {
                        previous.SetNext(current._next);
                        RemoveKeyFromArray(current._key);
                    }
                    else if (current._next != null)
                    {
                        previous = current;
                        current = current._next;
                    }
                }
            }
        }

        private void RemoveKeyFromArray(TKey removeKey)
        {
            if (removeKey == null) throw new ArgumentNullException(nameof(removeKey));
            lock (_keys) _keys.RemoveAll(key => IsKeysEqual(key, removeKey));
        }
        
        /// <summary>
        /// Determines that collection contains the specified key.
        /// </summary>
        /// <param name="key">Key to find</param>
        /// <returns>True - if collection contains the specified key. False otherwise.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool ContainsKey(TKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            return _keys.Any(k => IsKeysEqual(k, key));
        }

        /// <summary>
        /// Double number of indices for each key properties.
        /// Increase <see cref="_table"/> capacity 2^KeySize times
        /// </summary>
        private void ExpandTable()
        {
            var newSize = 0;
            var newNonEmptyNodes = 0;
            var newRangePerKey = _rangePerKey * 2;
            var newTable = new MapperNode<TKey, TValue>[newRangePerKey.Pow(KeySize)];

            lock (_table)
            {
                for (int i = 0; i < _table.Length; i++)
                {
                    MapperNode<TKey, TValue> current = _table[i];
                    while (current != null)
                    {
                        var index = HashesToTableIndex(current._hashes, newRangePerKey);

                        if (newTable[index] == null)
                        {
                            newTable[index] =
                                new MapperNode<TKey, TValue>(current._key, current._value, current._hashes, null);
                            newNonEmptyNodes++;
                            newSize++;
                        }
                        else
                        {
                            newTable[index].Add(current._key, current._value, current._hashes, out var added);
                            newSize += added;
                        }

                        current = current._next;
                    }
                }

                _rangePerKey = newRangePerKey;
                _table = newTable;

                NonEmptyNodes = newNonEmptyNodes;
                Size = newSize;
            }
        }

        /// <summary>
        /// Reset data of collection
        /// </summary>
        /// <param name="initTableDimSize">The initial range of indices for each key property in <see cref="TKey"/>.
        /// This value affects the <see cref="Capacity"/> of the <see cref="_table"/></param>
        public void Clear(int initTableDimSize = 32)
        {
            if (initTableDimSize <= 0) throw new ArgumentException(nameof(initTableDimSize) + "should be > 0");
            
            _rangePerKey = initTableDimSize;
            _table = new MapperNode<TKey, TValue>[(int) Math.Pow(_rangePerKey, KeySize)];
            lock (_keys) _keys = new List<TKey>();
            Size = NonEmptyNodes = 0;
        }
        
        /// <summary>
        /// Gets a list containing all the keys in the collection.
        /// </summary>
        /// <returns>Returns all keys stored in <see cref="_table"/></returns>
        public List<TKey> GetKeys()
        {
            var keys = new List<TKey>();
            foreach (var node in _table)
            {
                MapperNode<TKey, TValue> current = node;
                while (current != null)
                {
                    keys.Add(current._key);
                    current = current._next;
                }
            }
            
            return keys;
        }

        /// <summary>
        /// Comparer of key properties for TKey
        /// </summary>
        /// <param name="key1">First key</param>
        /// <param name="key2">Second key</param>
        /// <returns>True - if value of key properties of both received keys is the same. False otherwise.</returns>
        public static bool IsKeysEqual(TKey key1, TKey key2) 
        {
            if (key1 == null) throw new ArgumentNullException($"key1 value is null");
            if (key2 == null) throw new ArgumentNullException($"key2 value is null");
            
            return KeyProperties
                .All(property => property.GetValue(key1).Equals(property.GetValue(key2)));
        }

        /// <summary>
        /// Comparer of key properties 
        /// </summary>
        /// <param name="key1">First key</param>
        /// <param name="key2">Second key</param>
        /// <param name="propertyOrder">Excluded property</param>
        /// <returns>True - if values of not excluded key properties of both received keys are the same. False otherwise.</returns>
        public static bool IsKeysEqual(TKey key1, TKey key2, int propertyOrder) 
        {
            if (key1 == null) throw new ArgumentNullException($"key1 value is null");
            if (key2 == null) throw new ArgumentNullException($"key2 value is null");
            
            return KeyProperties
                .All(property =>
                {
                    var order = ((KeyAttribute) property.GetCustomAttribute(typeof(KeyAttribute), false)).Order;
                    return order == propertyOrder || property.GetValue(key1).Equals(property.GetValue(key2));
                });
        }

        /// <summary>
        /// Comparer of key properties 
        /// </summary>
        /// <param name="key1">First key</param>
        /// <param name="key2">Second key</param>
        /// <param name="propertyName">Excluded property</param>
        /// <returns>True - if value of not excluded key properties of both received keys is the same. False otherwise.</returns>
        public static bool IsKeysEqual(TKey key1, TKey key2, string propertyName)
        {
            if (key1 == null) throw new ArgumentNullException($"key1 value is null");
            if (key2 == null) throw new ArgumentNullException($"key2 value is null");

            return KeyProperties
                .All(property => property.Name == propertyName || property.GetValue(key1).Equals(property.GetValue(key2)));
        }
        
        /// <summary>
        /// Calculate hash values for received key
        /// </summary>
        /// <param name="key">Key for which the hashes values is calculated</param>
        /// <returns>Hash values of each key properties</returns>
        private static int[] HashesOfKey(TKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            var hashes = new int[KeySize];
            for (var i = 0; i < KeySize; i++)
            {
                hashes[i] = KeyProperties[i].GetValue(key).GetHashCode().Abs();
            }
            
            return hashes;
        }

        /// <summary>
        /// Convert key property hash values to <see cref="_table"/> index 
        /// </summary>
        /// <param name="hashes">Hash values of each key properties</param>
        /// <param name="range">Fit hash values to this range</param>
        /// <returns>Index in <see cref="_table"/> received from input hash values</returns>
        private static int HashesToTableIndex(int[] hashes, int range)
        {
            var tableIndex = 0;
            foreach (var hash in hashes)
            {
                tableIndex = tableIndex * range + hash.Fit(range);
            }
            
            return tableIndex;
        }

        /// <summary>
        /// Convert a key property values to <see cref="_table"/> index 
        /// </summary>
        /// <param name="key">Key to convert</param>
        /// <param name="range">Fit hash to this range to get a table index</param>
        /// <returns>Index of input key in <see cref="_table"/></returns>
        private static int KeyToTableIndex(TKey key, int range)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            var tableIndex = 0;
            foreach (var property in KeyProperties)
            {
                var offset = property.GetValue(key).GetHashCode().Abs().Fit(range);
                tableIndex = tableIndex * range + offset;
            }
            
            return tableIndex;
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            foreach (var node in _table)
            {
                var current = node;
                while (current != null)
                {
                    yield return current._value;
                    current = current._next;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}