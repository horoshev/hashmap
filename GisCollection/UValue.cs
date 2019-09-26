namespace GisCollection
{
    public struct UValue
    {
        public int Value { get; set; }
        public string Description { get; set; }

        public UValue(int value, string description)
        {
            Value = value;
            Description = description;
        }
        
        public override string ToString()
        {
            return $"{Description}: {Value}";
        }
    }
}