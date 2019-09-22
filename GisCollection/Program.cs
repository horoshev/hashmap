using System;
using System.Linq;

namespace GisCollection.SimpleMapper
{
    public class Id : IKey
    {
//        public int hash => Value * GetHashCode() + 11047;
        public int hash => Value * 61583 + 11047;
        private int Value { get; }
        
        public Id(int value)
        {
            Value = value;
        }
        
        public int CompareTo(object id)
        {
            if (id == null)
                return 1;

            if (id is Id otherId) 
                return Value.CompareTo(otherId.Value);
        
            throw new ArgumentException("Object is not a Id");
        }
    }
    
    public class Name : IKey
    {
        public int hash => Value.Aggregate(0, (res, item) => res += (int) item) * 37511 + 79063;
        private string Value { get; }
        
        public Name(string value)
        {
            Value = value;
        }
        
        public int CompareTo(object name)
        {
            if (name == null)
                return 1;

            if (name is Name otherName) 
                return String.Compare(Value, otherName.Value, StringComparison.Ordinal);
        
            throw new ArgumentException("Object is not a Name");
        }
    }
    
    class Program
    {
        static void Main()
        {
            var mapper = new Mapper<Id, Name, int>(10, 100);

            var id = new Id(1);
            var name = new Name("hi");
            
//            mapper[id, name] = 5;
//            Console.WriteLine(mapper[id, name]);
            
            for (int i = 1; i < 5; i++)
            {
                id = new Id(i);            
                mapper[id, name] = 5 + i * 10;
            }
            
            Console.WriteLine(mapper[name].Where(x => x != 0).Count());       
        }
    }
}