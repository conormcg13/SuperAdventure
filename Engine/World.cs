using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public static class World
    {
        public static readonly List<Item> Items = new List<Item>();
        public static readonly List<Monster> Monsters = new List<Monster>();
        public static readonly List<Quest> Quests = new List<Quest>();
        public static readonly List<Location> Locations = new List<Location>();

        public enum ItemTypes
        {
            RustySword,
            RatTail,
            PieceOfFur,
            SnakeFang,
            Snakeskin,
            Club,
            HealingPotion,
            SpiderFang,
            SpiderSilk,
            AdventurerPass
        }

        public enum MonsterTypes
        {
            Rat,
            Snake,
            GiantSpider
        }

        public enum QuestTypes
        {
            ClearAlchemistGarden,
            ClearFarmersField
        }

        public enum LocationTypes
        {
            Home,
            TownSquare,
            GuardPost,
            AlchemistHut,
            AlchemistsGarden,
            Farmhouse,
            FarmField,
            Bridge,
            SpiderField
        }

        static World()
        {
            PopulateItems();
            PopulateMonsters();
            PopulateQuests();
            PopulateLocations();
        }

        private static void PopulateItems()
        {
            Items.Add(new Weapon((int)ItemTypes.RustySword, "Rusty Sword", "Rusty Swords", 0, 5 ));
            Items.Add(new Item((int)ItemTypes.RatTail, "Rat tail", "Rat tails"));
            Items.Add(new Item((int)ItemTypes.PieceOfFur, "Piece of fur", "Pieces of fur"));
            Items.Add(new Item((int)ItemTypes.SnakeFang, "Snake fang", "Snake fangs"));
            Items.Add(new Item((int)ItemTypes.Snakeskin, "Snakeskin", "Snakeskins"));
            Items.Add(new Weapon((int)ItemTypes.Club, "Club", "Clubs", 3, 10));
            Items.Add(new HealingPotion((int)ItemTypes.HealingPotion, "Healing Potion", "Healing Potions", 5));
            Items.Add(new Item((int)ItemTypes.SpiderFang, "Spider fang", "Spider fangs"));
            Items.Add(new Item((int)ItemTypes.SpiderSilk, "Spider silk", "Spider silks"));
            Items.Add(new Item((int)ItemTypes.AdventurerPass, "Adventurer Pass", "Adventurer Passes"));
        }

        private static void PopulateMonsters()
        {
            Monster rat = new Monster( (int) MonsterTypes.Rat, "Rat", 5, 3, 10, 3, 3 );
            rat.LootTable.Add(new LootItem(ItemById((int)ItemTypes.RatTail), 75, isDefaultItem: false));
            rat.LootTable.Add(new LootItem(ItemById((int)ItemTypes.PieceOfFur), 75, isDefaultItem: false));

            Monster snake = new Monster((int)MonsterTypes.Snake, "Snake", 5, 3, 10, 3, 3);
            snake.LootTable.Add(new LootItem(ItemById((int)ItemTypes.SnakeFang), 75, isDefaultItem: false));
            snake.LootTable.Add(new LootItem(ItemById((int)ItemTypes.Snakeskin), 75, isDefaultItem: true));

            Monster giantSpider = new Monster((int)MonsterTypes.GiantSpider, "Giant Spider", 20, 5, 40, 10, 10);
            giantSpider.LootTable.Add(new LootItem(ItemById((int)ItemTypes.SpiderFang), 75, isDefaultItem: true));
            giantSpider.LootTable.Add(new LootItem(ItemById((int)ItemTypes.SpiderSilk), 75, isDefaultItem: false));

            Monsters.Add(rat);
            Monsters.Add(snake);
            Monsters.Add(giantSpider);
        }

        private static void PopulateQuests()
        {
            Quest clearAlchemistGarden = new Quest((int)QuestTypes.ClearAlchemistGarden, "Clear the alchemist's garden",
                                                "Kill rats in the alchemist's garden and bring back 3 rat tails. " +
                                                " You will receive a healing potion and 10 gold pieces", 20, 10);

            clearAlchemistGarden.QuestCompletionItems.Add(new QuestCompletionItem(ItemById((int)ItemTypes.RatTail), 3));
            clearAlchemistGarden.RewardItem = ItemById((int)ItemTypes.HealingPotion);

            Quest clearFarmersField = new Quest((int)QuestTypes.ClearFarmersField, "Clear the farmer's field", "Kill" +
                                                   "snakes in the farmer's field and bring back 3 snake fangs. You will" +
                                                        "receive an adventurer's pass and 20 gold pieces.", 20, 20);

            clearFarmersField.QuestCompletionItems.Add(new QuestCompletionItem(ItemById((int)ItemTypes.SnakeFang), 3));

            clearFarmersField.RewardItem = ItemById((int)ItemTypes.AdventurerPass);

            Quests.Add(clearAlchemistGarden);
            Quests.Add(clearFarmersField);
        }

        private static void PopulateLocations()
        {
            Location home = new Location((int)LocationTypes.Home, "Home", 
                "Your House. You really need to clean up the place.");

            Location townSquare = new Location((int)LocationTypes.TownSquare, "Town Square", 
                "You see a fountain.");

            Location alchemistHut = new Location((int)LocationTypes.AlchemistHut, "Alchemist's Hut", 
                "There are many strange plants on the shelves.");
            alchemistHut.QuestAvailableHere = QuestById((int) QuestTypes.ClearAlchemistGarden);

            Location alchemistsGarden = new Location((int)LocationTypes.AlchemistsGarden, "Alchemist's garden", 
                "Many plants are growing here.");
            alchemistsGarden.MonsterLivingHere = MonsterById((int) MonsterTypes.Rat);

            Location farmhouse = new Location((int) LocationTypes.TownSquare, "Farmhouse",
                "There is a small farmhouse, with a farmer in front");
            farmhouse.QuestAvailableHere = QuestById((int) QuestTypes.ClearFarmersField);

            Location farmersField = new Location((int)LocationTypes.FarmField, "Farmer's field", "You see rows of vegetables growing here.");
            farmersField.MonsterLivingHere = MonsterById((int) MonsterTypes.Snake);

            Location guardPost = new Location((int) LocationTypes.GuardPost, "Guard post",
                "There is a large, tough-looking guard here.", ItemById((int)ItemTypes.AdventurerPass));

            Location bridge = new Location((int)LocationTypes.Bridge, "Bridge", "A stone bridge that crosses a wide river.");

            Location spiderField = new Location((int)LocationTypes.SpiderField, "Forest", "You see spider webs covering the trees in this forest.");
            spiderField.MonsterLivingHere = MonsterById((int) MonsterTypes.GiantSpider);

            home.LocationToNorth = townSquare;

            townSquare.LocationToNorth = alchemistHut;
            townSquare.LocationToSouth = home;
            townSquare.LocationToEast = guardPost;
            townSquare.LocationToWest = farmhouse;

            farmhouse.LocationToEast = townSquare;
            farmhouse.LocationToWest = farmersField;

            farmersField.LocationToEast = farmhouse;

            alchemistHut.LocationToSouth = townSquare;
            alchemistHut.LocationToNorth = alchemistsGarden;

            alchemistsGarden.LocationToSouth = alchemistHut;

            guardPost.LocationToEast = bridge;
            guardPost.LocationToWest = townSquare;

            bridge.LocationToWest = guardPost;
            bridge.LocationToEast = spiderField;

            spiderField.LocationToWest = bridge;

            Locations.Add(home);
            Locations.Add(townSquare);
            Locations.Add(guardPost);
            Locations.Add(alchemistHut);
            Locations.Add(alchemistsGarden);
            Locations.Add(farmhouse);
            Locations.Add(farmersField);
            Locations.Add(bridge);
            Locations.Add(spiderField);


        }

        public static Item ItemById(int id)
        {
            foreach (var item in Items)
            {
                if (item.Id == id)
                {
                    return item;
                }
            }
            return null;
        }

        public static Monster MonsterById(int id)
        {
            foreach (var monster in Monsters)
            {
                if (monster.Id == id)
                {
                    return monster;
                }
            }
            return null;
        }

        public static Quest QuestById(int id)
        {
            foreach (var quest in Quests)
            {
                if (quest.Id == id)
                {
                    return quest;
                }
            }
            return null;
        }

        public static Location LocationById(int id)
        {
            foreach (var location in Locations)
            {
                if (location.Id == id)
                {
                    return location;
                }
            }
            return null;
        }
    }
}
