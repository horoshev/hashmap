using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GisCollection;
using NUnit.Framework;

namespace Tests
{
    public class Tests
    {
        private UKey[] KeyData;
        private UValue[] ValData;
        private (UKey, UValue)[] Data;
        private UKey[] NotAddedKeys;

        private Mapper<UKey, UValue> TestMapper()
        {
            var mapper = new Mapper<UKey, UValue>(10);
            
            for (int i = 0; i < KeyData.Length; i++)
                mapper[KeyData[i]] = ValData[i];
            
            return mapper;
        }
        
        [SetUp]
        public void Setup()
        {
            NotAddedKeys = new[]
            {
                new UKey(){ Id = -28, Name = "Martin"},
                new UKey(){ Id = -91, Name = "Oliver"},
                new UKey(){ Id = 10, Name = "Ron"},
                new UKey(){ Id = 11, Name = "Jason"},
                new UKey(){ Id = 22, Name = "Gregory"},
                new UKey(){ Id = 32, Name = "Chuck"},
                new UKey(){ Id = 34, Name = "Jim"},
                new UKey(){ Id = 53, Name = "Finn"},
                new UKey(){ Id = 46, Name = "Otto"},
                new UKey(){ Id = 74, Name = "Camila"},
                new UKey(){ Id = 58, Name = "Kevin"},
                new UKey(){ Id = 95, Name = "Jake"},
            };
            
            KeyData = new []
            {
                new UKey(){ Id = -2, Name = "Steward Wise"},
                new UKey(){ Id = -1, Name = "Matias Strong"},
                new UKey(){ Id = 1, Name = "Kali Byrd"},
                new UKey(){ Id = 1, Name = "Drew Hopkins"},
                new UKey(){ Id = 2, Name = "Nelson Eaton"},
                new UKey(){ Id = 2, Name = "Sally Dickens"},
                new UKey(){ Id = 3, Name = "Kirsten Watt"},
                new UKey(){ Id = 3, Name = "Izaak Griffiths"},
                new UKey(){ Id = 4, Name = "Jimmy Merritt"},
                new UKey(){ Id = 4, Name = "Hattie Glass"},
                new UKey(){ Id = 5, Name = "Jimmy Merritt"},
                new UKey(){ Id = 5, Name = "Hattie Glass"},
            };
            
            ValData = new []
            {
                new UValue() { Value = -2, Description = "Minus Two" },
                new UValue() { Value = -1, Description = "Minus One" },
                new UValue() { Value = 1, Description = "One" },
                new UValue() { Value = 2, Description = "Two" },
                new UValue() { Value = 3, Description = "Three" },
                new UValue() { Value = 4, Description = "Four" },
                new UValue() { Value = 5, Description = "Five" },
                new UValue() { Value = 6, Description = "Six" },
                new UValue() { Value = 7, Description = "Seven" },
                new UValue() { Value = 8, Description = "Eight" },
                new UValue() { Value = 9, Description = "Nine" },
                new UValue() { Value = 10, Description = "Ten" },
            };

            Data = new (UKey, UValue)[KeyData.Length];
            for (var i = 0; i < KeyData.Length; i++)
                Data[i] = (Key: KeyData[i], Value: ValData[i]);
        }

        [TestCase(20, 400)]
        public void InitCorrectly(int range, int expected)
        {
            var mapper = new Mapper<UKey, UValue>(range);
            Assert.AreEqual(expected, mapper.Capacity);
        }

        [TestCase(-5)]
        public void InitFailed(int range)
        {
            Assert.Catch<Exception>(() => { var mapper = new Mapper<UKey, UValue>(range); });
        }

        [Test]
        public void ImplementKeyComparerCorrectly()
        {
            for (int i = 0; i < KeyData.Length; i++)
                for (int j = 0; j < KeyData.Length; j++)
                    Assert.AreEqual(i == j, KeyData[i].Equals(KeyData[j]), $"Error on {i} {j}");
        }
        
        [Test]
        public void AddCorrectly()
        {
            var mapper = new Mapper<UKey, UValue>();

            foreach (var (key, value) in Data)
            {
                mapper[key] = value;
            }

            foreach (var (key, value) in Data)
            {
                Assert.AreEqual(value, mapper[key]);
            }
        }

        [Test]
        public void GetByWrongKey()
        {
            var mapper = TestMapper();
            var wrongKey = new UKey() { Id = 345, Name = "Vova"};
            Assert.AreEqual(default(UValue), mapper[wrongKey]);
        }
        
        [Test]
        public void GetByCorrectId()
        {
            var mapper = TestMapper();
            for (var i = 0; i < Data.Length; i++)
            {
                var actual = mapper.GetValuesById(Data[i].Item1.Id);
                var expected = Data
                    .Where(val => val.Item1.Id == Data[i].Item1.Id)
                    .Select(val => val.Item2)
                    .ToList();
                
                CollectionAssert.AreEquivalent(expected, actual);
            }
        }
        
        [Test]
        public void GetByKeyExcludeId()
        {
            var mapper = TestMapper();
            for (var i = 0; i < Data.Length; i++)
            {
                Console.WriteLine($"i: {i}");
                
                var key = new UKey() { Name = Data[i].Item1.Name };
                var actual = mapper[key, 1];
                var expected = Data
                    .Where(val => val.Item1.Name == Data[i].Item1.Name)
                    .Select(val => val.Item2)
                    .ToList();
                
                CollectionAssert.AreEquivalent(expected, actual);
            }
        }
        
        [Test]
        public void GetByKeyExcludeName()
        {
            var mapper = TestMapper();
            for (var i = 0; i < Data.Length; i++)
            {
                var key = new UKey() { Id = Data[i].Item1.Id, Name = "" };
                var actual = mapper[key, 2];
                var expected = Data
                    .Where(val => val.Item1.Id == Data[i].Item1.Id)
                    .Select(val => val.Item2)
                    .ToList();
                
                CollectionAssert.AreEquivalent(expected, actual);
            }
        }
     
        [Test]
        public void GetByKeyExcludePropertyNameId()
        {
            var mapper = TestMapper();
            for (var i = 0; i < Data.Length; i++)
            {
                var key = new UKey() { Name = Data[i].Item1.Name };
                var actual = mapper[key, "Id"];
                var expected = Data
                    .Where(val => val.Item1.Name == Data[i].Item1.Name)
                    .Select(val => val.Item2)
                    .ToList();
                
                CollectionAssert.AreEquivalent(expected, actual);
            }
        }

        [Test]
        public void GetByKeyExcludePropertyNameName()
        {
            var mapper = TestMapper();
            for (var i = 0; i < Data.Length; i++)
            {
                var key = new UKey() { Id = Data[i].Item1.Id, Name = "" };
                var actual = mapper[key, "Name"];
                var expected = Data
                    .Where(val => val.Item1.Id == Data[i].Item1.Id)
                    .Select(val => val.Item2)
                    .ToList();
                
                CollectionAssert.AreEquivalent(expected, actual);
            }
        }

        [TestCase(-20)]
        [TestCase(40)]
        public void GetByWrongId(int id)
        {
            var mapper = TestMapper();
            Assert.AreEqual(new List<UValue>(), mapper.GetValuesById(id));
        }
        
        [Test]
        public void GetByCorrectName()
        {
            var mapper = TestMapper();
            for (var i = 0; i < Data.Length; i++)
            {
                var actual = mapper.GetValuesByName(Data[i].Item1.Name);
                var expected = Data
                    .Where(val => val.Item1.Name == Data[i].Item1.Name)
                    .Select(val => val.Item2)
                    .ToList();
                
                CollectionAssert.AreEquivalent(expected, actual);
            }
        }

        [TestCase("")]
        [TestCase("Vova")]
        public void GetByWrongName(string name)
        {
            var mapper = TestMapper();
            Assert.AreEqual(new List<UValue>(), mapper.GetValuesByName(name));
        }
        
        [Test]
        public void GetByNullName()
        {
            var mapper = TestMapper();
            Assert.Catch<ArgumentNullException>(() => mapper.GetValuesByName(null));
        }

        [Test]
        public void UpdateValues()
        {
            var mapper = TestMapper();
            var newValues = ValData.Reverse().ToArray();

            for (var i = 0; i < Data.Length; i++)
            {
                mapper[Data[i].Item1] = newValues[i];
            }
            
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(newValues[i], mapper[Data[i].Item1]);
            }
        }

        [Test]
        public void RemoveExistingKeys()
        {
            var mapper = TestMapper();
            
            foreach (var (key, value) in Data)
            {
                mapper.RemoveKey(key);
                Assert.AreEqual(default(UValue), mapper[key]);
            }
        }
        
        [Test]
        public void RemoveNonExistingKeys()
        {
            var mapper = TestMapper();

            foreach (var _key in NotAddedKeys)
            {
                mapper.RemoveKey(_key);
                foreach (var (key, value) in Data)
                {
                    Assert.AreEqual(value, mapper[key]);
                }    
            }
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(5)]
        [TestCase(7)]
        [TestCase(8)]
        public void GetKeysImplementedCorrect(int n)
        {
            var mapper = TestMapper();
            CollectionAssert.AreEquivalent(KeyData, mapper.GetKeys());
            
            // Remove first n
            var remove = KeyData.Take(n).ToArray();
            foreach (var key in remove)
                mapper.RemoveKey(key);

            // Check again
            var contain = KeyData.Skip(n);
            CollectionAssert.AreEquivalent(contain, mapper.GetKeys());
        }
        
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(5)]
        [TestCase(7)]
        [TestCase(8)]
        public void ContainKeyImplementedCorrect(int n)
        {
            var mapper = TestMapper();
            foreach (var key in KeyData)
                Assert.True(mapper.ContainsKey(key));
            
            // Remove first n
            var remove = KeyData.Take(n).ToArray();
            foreach (var key in remove)
                mapper.RemoveKey(key);

            foreach (var key in remove)
                Assert.False(mapper.ContainsKey(key));
            
            // Check again
            var contain = KeyData.Skip(n);
            foreach (var key in contain)
                Assert.True(mapper.ContainsKey(key));
            
            foreach (var key in NotAddedKeys)
                Assert.False(mapper.ContainsKey(key));
        }

        [Test]
        public void CollectionResizeCorrectly()
        {
            var mapper = new Mapper<UKey, UValue>(10);
            var keys = new List<UKey>();
            var capacity = mapper.Capacity;
            var id = 0;
            while (!mapper.Full)
            {
                var key = new UKey() { Id = id, Name = id.ToString() };
                var value = new UValue() { Value = id, Description = $"id: {id}" };
                mapper[key] = value;
                keys.Add(key);
                id++;
            }
            
            // one more key for resize
            var lastKey = new UKey() { Id = id, Name = id.ToString() };
            var lastValue = new UValue() { Value = id, Description = $"id: {id}" };
            mapper[lastKey] = lastValue;
            keys.Add(lastKey);

            foreach (var key in keys)
            {
                Assert.AreEqual(new UValue() { Value = key.Id, Description = $"id: {key.Id}" }, mapper[key]);
            }
            
            Assert.AreEqual(capacity * Math.Pow(2, mapper.keySize), mapper.Capacity, 1e-7);
        }

        [Test]
        public void EnumeratorImplementedCorrectly()
        {
            CollectionAssert.AreEquivalent(ValData, TestMapper());
        }
        
        private readonly Mapper<UKey, UValue> _shared = new Mapper<UKey, UValue>(32);
        
        [Test]
        public async Task ThreadsUnsafeWrite()
        {
            /*
             * Idea:
             * 
             *   (Full table start resize)
             *   |
             *   |    (Other Thread write new values)
             *   |    |
             *   |    |    (Table stop resizing)
             *   |    |    |
             *   |    |    |    (Other Thread write values to resized table)
             * __|____|____|____|______________________________
             * Timeline
             *
             * In resized table some first values from old table which
             * not changed by other thread
             * Which means data in table inconsistent
             * (But it did not work)
             */
            
            var tasks = new List<Task>
            {
                AddingValues(),
                ChangingValues(),
            };

            await Task.WhenAll(tasks);
            
            for (var i = 0; i < _shared.Size; i++)
            {
                var key = new UKey() { Id = i, Name = i.ToString() };
                Console.WriteLine(_shared[key]);
            }
            // Test
        }

        private Task ChangingValues()
        {
            var task = new Task(() =>
            {
                Thread.Sleep(800);

                Console.WriteLine($"change start| size: {_shared.Size} capacity: {_shared.Capacity}");

                for (var i = 0; i < _shared.Size; i++)
                {
                    var key = new UKey() { Id = i, Name = i.ToString() };
                    var value = new UValue() { Value = 0, Description = "Zero" };
//                    Thread.Sleep(1);
                    _shared[key] = value;
                }

                Console.WriteLine($"change end| size: {_shared.Size} capacity: {_shared.Capacity}");
            });
            
            task.Start();
            return task;
        }
        
        private Task AddingValues()
        {
            var task = new Task(() =>
            {
                Console.WriteLine($"add before| size: {_shared.Size} capacity: {_shared.Capacity}");
                
                // Add values until collection start resizing
                var id = 0;
                while (!_shared.Full)
                {
                    var key = new UKey() { Id = id, Name = id.ToString() };
                    var value = new UValue() { Value = 1, Description = "One" };
                    _shared[key] = value;
                    id++;
                }
                
                Console.WriteLine($"add full| size: {_shared.Size} capacity: {_shared.Capacity}");
                
                var _key = new UKey() { Id = id, Name = id.ToString() };
                var _value = new UValue() { Value = 1, Description = "One" };
                _shared[_key] = _value;

                Console.WriteLine($"add resize| size: {_shared.Size} capacity: {_shared.Capacity}");

//                Thread.Sleep(5000);
            });
            
            task.Start();
            return task;
        }
    }
}