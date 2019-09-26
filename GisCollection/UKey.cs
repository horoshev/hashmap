using System;

namespace GisCollection
{
    public struct UKey
    {
        [Key(order: 1)]
        public int Id { get; set; }
        
        [Key(order: 2)]
        public string Name { get; set; }
        
        // [Key(order: 3)]
        // public string Surname { get; set; }
        
        public int Age { get; set; } 

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
                    
            return Equals(obj as UKey? ?? throw new ArgumentException(nameof(obj) + " has wrong type"));
        }

        public bool Equals(UKey key)
        {
            return (Id == key.Id) && (Name == key.Name);
        }

        public override string ToString()
        {
            return $"{Id}: {Name}";
        }
    }
}