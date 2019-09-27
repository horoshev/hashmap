using System;

namespace GisCollection
{
    public struct UValue
    {
        public int Value { get; set; }
        public string Description { get; set; }

        private bool Equals(UValue item)
        {
            return (Value == item.Value) && (Description == item.Description);
        }
        
        public override bool Equals(object obj)
        {
            return obj != null && 
                   Equals(obj as UValue? ?? throw new ArgumentException(nameof(obj) + " has wrong type"));
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Value * 171) ^ (Description != null ? Description.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return $"{Description}: {Value}";
        }
    }
}