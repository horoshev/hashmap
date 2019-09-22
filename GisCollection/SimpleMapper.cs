using System;
using System.Runtime.CompilerServices;

namespace GisCollection.SimpleMapper
{
    public interface IKey : IComparable
    {
        int hash { get; }
    }

    public static class KeyExtention
    {
        public static int Hash(this IKey key, int range)
        {
            var hash = key.hash % range;
            Console.WriteLine($"range: {range} :: Hash: {hash}");
            return hash;
        }
    }
    
    public class Mapper<TKey1, TKey2, TValue>
        where TKey1 : IKey // id
        where TKey2 : IKey // name
        where TValue : struct
    {
        private readonly int _colCount = 0;
        private readonly int _rowCount = 0;

        private readonly TValue[] _dataTable; 

        public Mapper(int cols, int rows)
        {
            this._colCount = cols;
            this._rowCount = rows;
            
            _dataTable = new TValue[cols * rows];
        }

        public TValue this[TKey1 key1, TKey2 key2]
        {
            set => _dataTable[key1.Hash(_rowCount) * _colCount + key2.Hash(_colCount)] = value;
            get => _dataTable[key1.Hash(_rowCount) * _colCount + key2.Hash(_colCount)];
        }
        
        
        public TValue[] this[TKey1 key1]
        {
            get
            {
                var res = new TValue[_colCount];
                var hash = key1.Hash(_rowCount);
                var offset = hash * _colCount;
                
                for (int i = 0; i < _colCount; i++)
                    res[i] = _dataTable[offset + i];
                
                return res;
            }
        }


        public TValue[] this[TKey2 key2]
        {
            get
            {
                var res = new TValue[_rowCount];
                var hash = key2.Hash(_colCount);
                
                for (int i = 0; i < _rowCount; i++)
                    res[i] = _dataTable[i * _colCount + hash];
                
                return res;
            }
        }
    }
}