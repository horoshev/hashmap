using System;

namespace GisCollection
{
    public class AValue
    {
        public int Value { get; set;}
        public string Description { get; set; }

        public bool Equals(AValue item)
        {
            return (Value == item.Value) && (Description == item.Description);
        }
        
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
                    
            return Equals(obj as AValue ?? throw new ArgumentException(nameof(obj) + " has wrong type"));
        }
        public override string ToString()
        {
            return $"{Description}: {Value}";
        }

      
        
    }
}