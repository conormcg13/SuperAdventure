using System.Collections.Generic;

namespace Engine
{
    public class Monster : LivingCreature
    {
        public Monster( int id, string name, int maximumDamage, int rewardExperiencePoints,
                            int rewardGold, int currentHitPoints, int maximumHitPoints) :
                                base( currentHitPoints, maximumHitPoints )
        {
            Id = id;
            Name = name;
            MaximumDamage = maximumDamage;
            RewardExperiencePoints = rewardExperiencePoints;
            RewardGold = rewardGold;
            LootTable = new List<LootItem>();
        }

        public Monster(Monster monsterObject) : base(monsterObject.CurrentHitPoints, monsterObject.MaximumHitPoints)
        {
            Id = monsterObject.Id;
            Name = monsterObject.Name;
            MaximumDamage = monsterObject.MaximumDamage;
            RewardExperiencePoints = monsterObject.RewardExperiencePoints;
            RewardGold = monsterObject.RewardGold;
            LootTable = new List<LootItem>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public int MaximumDamage { get; set; }
        public int RewardExperiencePoints { get; set; }
        public int RewardGold { get; set; }
        public List<LootItem> LootTable { get; set; }
    }
}
