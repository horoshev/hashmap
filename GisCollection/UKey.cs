using System;

namespace GisCollection
{
    public struct UKey
    {
        [Key(order: 1)]
        public int Id { get; set; }
        
        [Key(order: 2)]
        public string Name { get; set; }
        
        // This property will not affect to behaviour of key
        // because it has no Key Attribute
        public int Age { get; set; } 

        public bool Equals(UKey key)
        {
            return (Id == key.Id) && (Name == key.Name);
        }
        
        public override bool Equals(object obj)
        {
            return obj != null && 
                   Equals(obj as UKey? ?? throw new ArgumentException(nameof(obj) + " has wrong type"));
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Id * 197) ^ (Name != null ? Name.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return $"{Id}: {Name}";
        }
    }
}