using System;
using System.Collections.Generic;
using System.Linq;
using GisCollection;
using NUnit.Framework;

namespace Tests
{
    public class ClassTests
    {
        private AKey[] KeyData;
        private AValue[] ValData;
        private (AKey, AValue)[] Data;
        private AKey[] NotAddedKeys;

        private Mapper<AKey, AValue> TestMapper()
        {
            var mapper = new Mapper<AKey, AValue>(10);
            
            for (int i = 0; i < KeyData.Length; i++)
                mapper[KeyData[i]] = ValData[i];
            
            return mapper;
        }
        
        [SetUp]
        public void Setup()
        {
            NotAddedKeys = new[]
            {
                new AKey(){ Id = -28, Name = "Martin"},
                new AKey(){ Id = -91, Name = "Oliver"},
                new AKey(){ Id = 10, Name = "Ron"},
                new AKey(){ Id = 11, Name = "Jason"},
                new AKey(){ Id = 22, Name = "Gregory"},
                new AKey(){ Id = 32, Name = "Chuck"},
                new AKey(){ Id = 34, Name = "Jim"},
                new AKey(){ Id = 53, Name = "Finn"},
                new AKey(){ Id = 46, Name = "Otto"},
                new AKey(){ Id = 74, Name = "Camila"},
                new AKey(){ Id = 58, Name = "Kevin"},
                new AKey(){ Id = 95, Name = "Jake"},
            };
            
            KeyData = new []
            {
                new AKey(){ Id = -2, Name = "Steward Wise"},
                new AKey(){ Id = -1, Name = "Matias Strong"},
                new AKey(){ Id = 1, Name = "Kali Byrd"},
                new AKey(){ Id = 1, Name = "Drew Hopkins"},
                new AKey(){ Id = 2, Name = "Nelson Eaton"},
                new AKey(){ Id = 2, Name = "Sally Dickens"},
                new AKey(){ Id = 3, Name = "Kirsten Watt"},
                new AKey(){ Id = 3, Name = "Izaak Griffiths"},
                new AKey(){ Id = 4, Name = "Jimmy Merritt"},
                new AKey(){ Id = 4, Name = "Hattie Glass"},
                new AKey(){ Id = 5, Name = "Jimmy Merritt"},
                new AKey(){ Id = 5, Name = "Hattie Glass"},
            };
            
            ValData = new []
            {
                new AValue() { Value = -2, Description = "Minus Two" },
                new AValue() { Value = -1, Description = "Minus One" },
                new AValue() { Value = 1, Description = "One" },
                new AValue() { Value = 2, Description = "Two" },
                new AValue() { Value = 3, Description = "Three" },
                new AValue() { Value = 4, Description = "Four" },
                new AValue() { Value = 5, Description = "Five" },
                new AValue() { Value = 6, Description = "Six" },
                new AValue() { Value = 7, Description = "Seven" },
                new AValue() { Value = 8, Description = "Eight" },
                new AValue() { Value = 9, Description = "Nine" },
                new AValue() { Value = 10, Description = "Ten" },
            };

            Data = new (AKey, AValue)[KeyData.Length];
            for (var i = 0; i < KeyData.Length; i++)
                Data[i] = (Key: KeyData[i], Value: ValData[i]);
        }

        [TestCase(20, 400)]
        public void InitCorrectly(int range, int expected)
        {
            var mapper = new Mapper<AKey, AValue>(range);
            Assert.AreEqual(expected, mapper.Capacity);
        }

        [TestCase(-5)]
        public void InitFailed(int range)
        {
            Assert.Catch<Exception>(() => { var mapper = new Mapper<AKey, AValue>(range); });
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
            var mapper = new Mapper<AKey, AValue>();

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
            var wrongKey = new AKey() { Id = 345, Name = "Vova"};
            Assert.AreEqual(default(AValue), mapper[wrongKey]);
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
                var key = new AKey() { Name = Data[i].Item1.Name };
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
                var key = new AKey() { Id = Data[i].Item1.Id, Name = "" };
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
                var key = new AKey() { Name = Data[i].Item1.Name };
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
                var key = new AKey() { Id = Data[i].Item1.Id, Name = "" };
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
            Assert.AreEqual(new List<AValue>(), mapper.GetValuesById(id));
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
            Assert.AreEqual(new List<AValue>(), mapper.GetValuesByName(name));
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
                Assert.AreEqual(default(AValue), mapper[key]);
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

        [Test]
        public void GetValueOfNonExistingKeys()
        {
            var mapper = TestMapper();
            foreach (var _key in NotAddedKeys)
                Assert.AreEqual(default(AValue), mapper[_key]);
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
            var mapper = new Mapper<AKey, AValue>(10);
            var keys = new List<AKey>();
            var capacity = mapper.Capacity;
            var id = 0;
            while (!mapper.Full)
            {
                var key = new AKey() { Id = id, Name = id.ToString() };
                var value = new AValue() { Value = id, Description = $"id: {id}" };
                mapper[key] = value;
                keys.Add(key);
                id++;
            }
            
            // one more key for resize
            var lastKey = new AKey() { Id = id, Name = id.ToString() };
            var lastValue = new AValue() { Value = id, Description = $"id: {id}" };
            mapper[lastKey] = lastValue;
            keys.Add(lastKey);

            foreach (var key in keys)
            {
                Assert.AreEqual(new AValue() { Value = key.Id, Description = $"id: {key.Id}" }, mapper[key]);
            }
            
            Assert.AreEqual(capacity * Math.Pow(2, mapper.keySize), mapper.Capacity, 1e-7);
        }

        [Test]
        public void EnumeratorImplementedCorrectly()
        {
            CollectionAssert.AreEquivalent(ValData, TestMapper());
        }
    }
}