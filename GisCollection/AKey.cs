namespace GisCollection
{
    public class AKey
    {
        [Key(order: 0)]
        public int Id { get; set; }
        
        [Key(order: 1)]
        public string Name { get; set; }
        
        public override bool Equals(object obj)
        {
            return obj != null && Equals(obj as AKey);
        }

        private bool Equals(AKey key)
        {
            return (key != null) && (Id == key.Id) && (Name == key.Name);
        }

        // TODO: Check later
        /*public override int GetHashCode()
        {
            unchecked
            {
                return (Id * 397) ^ (Name != null ? Name.GetHashCode() : 0);
            }
        }*/
        
        public override string ToString()
        {
            return $"{Id}: {Name}";
        }
    }
}