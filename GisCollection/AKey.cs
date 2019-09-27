namespace GisCollection
{
    public class AKey
    {
        [Key(order: 1)]
        public int Id { get; set; }
        
        [Key(order: 2)]
        public string Name { get; set; }
        
        private bool Equals(AKey key)
        {
            return (key != null) && (Id == key.Id) && (Name == key.Name);
        }
        
        public override bool Equals(object obj)
        {
            return obj != null && Equals(obj as AKey);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Id * 397) ^ (Name != null ? Name.GetHashCode() : 0);
            }
        }
        
        public override string ToString()
        {
            return $"{Id}: {Name}";
        }
    }
}