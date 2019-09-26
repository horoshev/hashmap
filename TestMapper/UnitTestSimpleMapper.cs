using GisCollection.SimpleMapper;
using NUnit.Framework;

namespace Tests
{
    public class UnitTestSimpleMapper
    {
        [Test]
        public void AddElements()
        {
            var mapper = new Mapper<Id, Name, int>(10, 100);

            var id = new Id(1);
            var name = new Name("hi");

            for (int i = 1; i < 5; i++)
            {
                id = new Id(i);            
                mapper[id, name] = 5 + i * 10;
            }
            
            for (int i = 1; i < 5; i++)
            {
                id = new Id(i);            
                Assert.AreEqual(5 + i * 10, mapper[id, name]);
            }
        }
    }
}