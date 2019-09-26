namespace GisCollection
{
    public class AValue
    {
        public int Value { get; set;}
        public string Description { get; set; }

        public override string ToString()
        {
            return $"{Description}: {Value}";
        }
    }
}