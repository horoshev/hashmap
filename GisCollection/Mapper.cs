using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace GisCollection
{
    public interface IKey
    {
        int Dims { get; }
    }

    public class Key
    {
        public Type Type { get; }
        public int Size { get; }
        public string Name { get; }

        public Key(Type type, int size, string name)
        {
            Type = type;
            Size = size;
            Name = name;
        }
    }

    public class TableKey : IKey
    {
        public Key[] keys = new Key[]
        {
            new Key(typeof(int), 100, "Id"), 
            new Key(typeof(string), 10, "Name"), 
        };
        
        public int Dims { get => keys.Length; }
        public int Size
        {
            get => keys.Aggregate(1, (res, val) => res *= val.Size);
        }
    }
    
    public class Mapper<TKey, TValue>
        where TKey : IKey
        where TValue : struct
    {
        private TValue[] dataTable; 

        public Mapper(TableKey tableKey)
        {
            dataTable = new TValue[tableKey.Size];
        }
        
        public TValue this[TableKey index]
        {
            set => dataTable[0] = value;
            get => dataTable[0];
        }

        public TValue this[TKey index]
        {
            set => dataTable[0] = value;
            get => dataTable[0];
        }
    }
}