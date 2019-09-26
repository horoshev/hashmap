using System;
using System.Collections.Generic;
using GisCollection;
using GisCollection.SimpleMapper;
using NUnit.Framework;

namespace Tests
{
    public class ClassTests
    {
        // setup values
        private static readonly AKey[] KeyData = 
        {
            new AKey() { Id = 1, Name = "Kali Byrd"},
            new AKey() { Id = 1, Name = "Drew Hopkins"},
            new AKey() { Id = 2, Name = "Nelson Eaton"},
            new AKey() { Id = 2, Name = "Sally Dickens"},
            new AKey() { Id = 3, Name = "Kirsten Watt"},
            new AKey() { Id = 3, Name = "Izaak Griffiths"},
            new AKey() { Id = 4, Name = "Jimmy Merritt"},
            new AKey() { Id = 4, Name = "Hattie Glass"},
            new AKey() { Id = 5, Name = "Jimmy Merritt"},
            new AKey() { Id = 5, Name = "Hattie Glass"},
        };
            
        private static readonly AValue[] ValData = 
        {
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
        
        private static readonly (AKey, AValue)[] Data =
        {
            (KeyData[0], ValData[0]),
            (KeyData[1], ValData[1]),
            (KeyData[2], ValData[2]),
            (KeyData[3], ValData[3]),
            (KeyData[4], ValData[4]),
            (KeyData[5], ValData[5]),
            (KeyData[6], ValData[6]),
            (KeyData[7], ValData[7]),
            (KeyData[8], ValData[8]),
            (KeyData[9], ValData[9]),
        };

        private Mapper<AKey, AValue> TestMapper()
        {
            var mapper = new Mapper<AKey, AValue>();
            
            for (int i = 0; i < KeyData.Length; i++)
                mapper[KeyData[i]] = ValData[i];
            
            return mapper;
        }
        
        [SetUp]
        public void Setup()
        {
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
        public void AddElements()
        {
            var mapper = new Mapper<Id, Name, int>(10, 100);

            var id = new Id(1);
            var name = new Name("hi");

            for (int i = 1; i < 5; i++)
            {
                id = new Id(i);            
                mapper[id, name] = 5 + i * 10;
            }
            
            for (int i = 1; i < 5; i++)
            {
                id = new Id(i);            
                Assert.AreEqual(5 + i * 10, mapper[id, name]);
            }
        }
        
        [Test]
        public void GetByName()
        {
            Console.WriteLine("name".GetHashCode());
            var mapper = new Mapper<UKey, UValue>();
            
            var key = new UKey {Id = 1, Name = "name"};
            var key1 = new UKey {Id = 2, Name = "name2"};
            var key2 = new UKey {Id = 28516, Name = "really random number"};
            var val = new UValue { Value = 0, Description = "zero" };
                
            Console.WriteLine($"key: {key}, key1: {key1}, key2: {key2}");
            
            mapper[key] = val;
            mapper[key1] = val;
            mapper[key2] = val;
            Console.WriteLine(mapper[key]);
            Console.WriteLine(mapper[key1]);
            
            var sameKey = new UKey {Id = 1, Name = "name"}; 
            Console.WriteLine(mapper[sameKey]);
        }

        [Test]
        public void AddDifferentKeys()
        {
            var mapper = new Mapper<UKey, UValue>();
            // add values
        }
        
        [Test]
        public void AddWithSameKey() // should replace
        {
            var mapper = new Mapper<UKey, UValue>();
            // add values
        }

        [Test]
        public void RemoveExistingKeys()
        {
            
        }
        
        [Test]
        public void RemoveNonExistingKeys()
        {
            
        }
        
        [Test]
        public void ThreadsUnsafeWrite()
        {
            
        }

        [Test]
        public void GetValuesById()
        {
            var mapper = TestMapper();

            var res = mapper[KeyData[1], "Id"];
            foreach (var val in res)
            {
                Console.WriteLine(val.ToString());
            }
        }
        
        [Test]
        public void GetValuesByName()
        {
            var mapper = TestMapper();

            var res = mapper[KeyData[1], "Name"];
            foreach (var val in res)
            {
                Console.WriteLine(val.ToString());
            }
        }

        [Test]
        public void ReturnClass()
        {
            UKey[] keys = {
                new UKey() {Id = 1, Name = "Name"}
            };
            
            Console.WriteLine($"before: {keys[0].ToString()}");

            var key = keys[0];
            key.Id = 2;
            key.Name += "2";
            Console.WriteLine($"list: {keys[0].ToString()}");
            Console.WriteLine($"after: {key.ToString()}");
        }
    }
}