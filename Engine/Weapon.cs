namespace Engine
{
    public class Weapon : Item
    {
        public Weapon( int id, string name, string namePlural,
                        int minimumDamage, int maximumDamage ) :
                            base( id, name, namePlural )
        {
            MinimumDamage = minimumDamage;
            MaximumDamage = maximumDamage;
        }
        public int MinimumDamage { get; set; }
        public int MaximumDamage { get; set; }
    }
}
