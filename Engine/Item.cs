namespace Engine
{
    public class Item
    {
        public Item( int id, string name, string namePlural )
        {
            Id = id;
            Name = name;
            NamePlural = namePlural;
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string NamePlural { get; set; }
    }
}
