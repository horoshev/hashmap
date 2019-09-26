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
    /// Implements thread-safe version of hash table with multiple keys
    /// </summary>
    /// <typeparam name="TKey">Type of keys in collection</typeparam>
    /// <typeparam name="TValue">Type of values in collection</typeparam>
    /// <remarks>
    /// Actually collection is not thread-safe for the moment
    /// </remarks>
    public class Mapper<TKey, TValue> : IEnumerable<TValue>
    {
        // only for properties with attribute Key
        /// <summary>
        /// Determine type of TKey
        /// </summary>
        private static readonly Type KeyType = typeof(TKey);
        /// <summary>
        /// Hold array of TKey properties which got <see cref="KeyAttribute"/>
        /// </summary>
        private static readonly PropertyInfo[] KeyProperties;
        /// <summary>
        /// Number of TKey properties which got <see cref="KeyAttribute"/>
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
        /// List of keys which was added to collection
        /// </summary>
        private List<TKey> _keys { get; set; }
        /// <summary>
        /// Index range for each key property in TKey
        /// </summary>
        private int _rangePerKey;
        /// <summary>
        /// Multidimensional table with index for each key property in TKey
        /// Represented as one-dimensional array
        /// </summary>
        private MapperNode<TKey, TValue>[] _table;

        /// <summary>
        /// Number of elements in collection
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
        /// Initialize new Mapper instance
        /// </summary>
        /// <param name="initRangePerKey">The initial range of indices for each key property in <see cref="TKey"/>.
        /// This value affects the <see cref="Capacity"/> of the <see cref="_table"/></param>
        public Mapper(int initRangePerKey = 32)
        {
            if (initRangePerKey <= 0)
                throw new ArgumentException($"{nameof(initRangePerKey)} should be > 0");
            
            _rangePerKey = initRangePerKey;
            _table = new MapperNode<TKey, TValue>[(int) Math.Pow(_rangePerKey, KeySize)];
            _keys = new List<TKey>();
            Size = NonEmptyNodes = 0;
        }

        // TODO: Validate index
        /// <summary>
        /// Verifies that key has the correct value 
        /// </summary>
        /// <param name="key">Key to verify</param>
        /// <exception cref="ArgumentNullException"></exception>
        private void ValidateKey(TKey key)
        {
            if (key == null)
                throw new ArgumentNullException (nameof (key));
        }

        /// <summary>
        /// Indexer, which allow to get and also set values of collection by key.
        /// </summary>
        /// <param name="key">Received key value</param>
        /// <exception cref="ArgumentNullException"></exception>
        public TValue this[TKey key]
        {
            set
            {
                if (key == null) throw new ArgumentNullException(nameof(key));
                if (value == null) throw new ArgumentNullException(nameof(value));

                _keys.Add(key); // should be unique but cause performance
                
                if (NonEmptyNodes == Capacity) ExpandTable();
                
                var hashes = HashesOfKey(key);
                var index = HashesToTableIndex(hashes, _rangePerKey);
                
//                Console.WriteLine($"set index: {index}");
                
                if (_table[index] == null)
                {
                    _table[index] = new MapperNode<TKey, TValue>(key, value, hashes, null);
                    NonEmptyNodes++;
                    Size++;
                }
                else
                {
                    _table[index].Add(key, value, hashes, out var added);
                    Size += added;
                }
            }
            get
            {
                var index = KeyToTableIndex(key, _rangePerKey);
                var node = _table[index];
                
                while (node != null)
                    if (IsKeysEqual(node._key, key))
                        return node._value;
                    else //if (node._next != null)
                        node = node._next;

                return default(TValue);
            }
        }

        /// <summary>
        /// Experimental indexer which allow to get values by property name.
        /// In future should inverse behavior. Get values by key without received property name.
        /// </summary>
        /// <param name="key">Received key</param>
        /// <param name="propertyName">Property name</param>
        public List<TValue> this[TKey key, string propertyName]
        {
            get // only for 2 dims
            {
                var slice = new List<TValue>(_rangePerKey);

                // index = hash1 * dimSize + hash2
                var propertyMultiplier = KeyType
                    .GetProperties()
                    .Select((x, iter) =>
                    {
                        if (x.Name == propertyName)
                        {
                            return (
                                Name: x.Name, 
                                Hash: x.GetValue(key).GetHashCode().Abs().Fit(_rangePerKey),
                                Power: KeySize - iter - 1,
                                IsTarget: true);
                        }

                        return (
                            Name: x.Name, 
                            Hash: x.GetValue(key).GetHashCode().Abs().Fit(_rangePerKey),
                            Power: KeySize - iter - 1,
                            IsTarget: false);
                    })
                    .ToArray();

//                Console.WriteLine($"propertyMultiplier: {propertyMultiplier.Select(x => x.ToString())}");
                var m = propertyMultiplier.Select(x => { Console.WriteLine($"\n{nameof(x.Name)}:{x.Name}" +
                                                                   $"\n{nameof(x.Hash)}:{x.Hash}" +
                                                                   $"\n{nameof(x.Power)}:{x.Power}" +
                                                                   $"\n{nameof(x.IsTarget)}:{x.IsTarget}"); return 0; });

                var a = 0;
                foreach (var val in m)
                {
                    var v = val;
                    a += v;
                }
                
                var mul = 0;
                var hash = 0;
                var offset = propertyMultiplier
                    .Aggregate(0, (res, item) => {
                        if (!item.IsTarget)
                        {
                            return res + item.Hash * _rangePerKey.Pow(item.Power);
                        }
                        else
                        {
                            hash = item.Hash;
                            mul = item.Power;
                            return res;
                        }
                    });

                Console.WriteLine($"\n{nameof(offset)}: {offset}" +
                                  $"\n{nameof(hash)}: {hash}" +
                                  $"\n{nameof(mul)}: {mul}");

                for (int i = 0; i < _rangePerKey; i++)
                {
                    var index = offset + i * _rangePerKey.Pow(mul);
                    Console.WriteLine($"{nameof(index)} : {index}");
                    
                    // TODO
                    // if (_table[index] != null)
                    //     slice.AddRange(_table[index].GetChain());
                }

                return slice;
            }
        }

        // TODO: later
        /// <summary>
        /// Find elements in collection with given id
        /// </summary>
        /// <param name="id">Id to key should contain</param>
        /// <returns>Collection of elements with received id in key</returns>
        public List<TValue> GetValuesById(int id) // only for key with 2 dims
        {
            var res = new List<TValue>();
            var idIndex = id.GetHashCode().Abs().Fit(_rangePerKey);
            // Console.WriteLine($"{nameof(idIndex)}: {idIndex}");
            
            var data = KeyProperties.Select((value, index) => (index: index + 1, name: value.Name));
            var powerId = KeySize - data.First(value => value.name == "Id").index;
            var powerName = KeySize - data.First(value => value.name == "Name").index;
            var offset = idIndex * _rangePerKey.Pow(powerId);
            
            for (var i = 0; i < _rangePerKey; i++)
            {
                var index = offset + i * _rangePerKey.Pow(powerName);
                
                if (_table[index] == null) continue;
                
                var key = _table[index]._key;
                var isIdSame = KeyProperties
                    .Where(property => property.Name == "Id")
                    .Select(property => property.GetValue(key).Equals(id))
                    .First();

                if (isIdSame) res.Add(_table[index]._value);
            }
            
            return res;
        }

        /// <summary>
        /// Find elements in collection with given name. Not implemented.
        /// </summary>
        /// <param name="name">Name to key should contain</param>
        /// <returns>Collection of elements with received name in key</returns>
        public List<TValue> GetValuesByName(string name)
        {
            if (name == null)
                throw new ArgumentNullException($"{nameof(name)} should be not null value");

            var res = new List<TValue>();
            var nameIndex = name.GetHashCode().Abs().Fit(_rangePerKey);
            // Console.WriteLine($"{nameof(idIndex)}: {idIndex}");
            
            var data = KeyProperties.Select((value, index) => (index: index + 1, name: value.Name));
            var powerId = KeySize - data.First(value => value.name == "Id").index;
            var powerName = KeySize - data.First(value => value.name == "Name").index;
            var offset = nameIndex * _rangePerKey.Pow(powerName);
            
            for (var i = 0; i < _rangePerKey; i++)
            {
                var index = offset + i * _rangePerKey.Pow(powerId);
                
                if (_table[index] == null) continue;
                
                var key = _table[index]._key;
                var isNameSame = KeyProperties
                    .Where(property => property.Name == "Name")
                    .Select(property => property.GetValue(key).Equals(name))
                    .First();

                if (isNameSame) res.Add(_table[index]._value);
            }
            
            return res;
        }

        /// <summary>
        /// Remove from collection by key
        /// </summary>
        /// <param name="key">Key to remove</param>
        public void RemoveKey(TKey key)
        {
            var index = KeyToTableIndex(key, _rangePerKey);
            if (_table[index] == null) return;
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

        /// <summary>
        /// Delete value in collection. Not implemented
        /// </summary>
        /// <param name="value">Value to remove</param>
        public void RemoveValue(TValue value) { }

        private void RemoveKeyFromArray(TKey key)
        {
            _keys.RemoveAll(_key => IsKeysEqual(_key, key));
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
            
            for (int i = 0; i < _table.Length; i++)
            {
                MapperNode<TKey, TValue> current = _table[i];
                //Thread.Sleep(1);
                while (current != null)
                {
                    var index = HashesToTableIndex(current._hashes, newRangePerKey);
                    
                    if (newTable[index] == null)
                    {
                        newTable[index] = new MapperNode<TKey, TValue>(current._key, current._value, current._hashes, null);
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

        /// <summary>
        /// Reset data of collection
        /// </summary>
        /// <param name="initTableDimSize">The initial range of indices for each key property in <see cref="TKey"/>.
        /// This value affects the <see cref="Capacity"/> of the <see cref="_table"/></param>
        public void Clear(int initTableDimSize = 32)
        {
            _rangePerKey = initTableDimSize;
            _table = new MapperNode<TKey, TValue>[(int) Math.Pow(_rangePerKey, KeySize)];
            _keys = new List<TKey>();
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
//            Console.WriteLine($"{nameof(key1)}:{key1} | {nameof(key2)}:{key2}");
            return KeyProperties
                .All(property =>
                {
                    /*var pkey1 = Convert.ChangeType(property.GetValue(key1), property.PropertyType);
                    var pkey2 = Convert.ChangeType(property.GetValue(key2), property.PropertyType);
                    
                    Console.WriteLine($"{nameof(pkey1)}:{pkey1} | {nameof(pkey2)}:{pkey2}");
                    Console.WriteLine($"is {pkey1.Equals(pkey2)}");

                    return pkey1 == pkey2;*/
                    return property.GetValue(key1).Equals(property.GetValue(key2));
                    return property.GetValue(key1) == property.GetValue(key2);
                });
        }

        /// <summary>
        /// Calculate hash values for received key
        /// </summary>
        /// <param name="key">Key for which the hashes values is calculated</param>
        /// <returns>Hash values of each key properties</returns>
        private static int[] HashesOfKey(TKey key)
        {
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
        /// Convert key property values to <see cref="_table"/> index 
        /// </summary>
        /// <param name="key">Key to convert</param>
        /// <param name="range">Fit hash to this range to get table index</param>
        /// <returns>Index of input key in <see cref="_table"/></returns>
        private static int KeyToTableIndex(TKey key, int range)
        {
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