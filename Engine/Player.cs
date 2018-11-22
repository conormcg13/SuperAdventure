using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Engine
{
    public class Player : LivingCreature
    {
        public int Gold { get; set; }
        public int ExperiencePoints { get; private set; }
        public int Level => (ExperiencePoints / 100) + 1;
        
        public List<InventoryItem> Inventory { get; set; }
        public List<PlayerQuest> Quests { get; set; }
        public Location CurrentLocation { get; set; }
        public Weapon CurrentWeapon { get; set; }

        private Player( int currentHitPoints = 10, int maximumHitPoints = 10, int gold = 20,
                            int experiencePoints = 0 ) :
                                base( currentHitPoints, maximumHitPoints )
        {
            Gold = gold;
            ExperiencePoints = experiencePoints;

            Inventory = new List<InventoryItem>();
            Quests = new List<PlayerQuest>();
        }

        public static Player CreateDefaultPlayer()
        {
            Player player = new Player();
            player.CurrentLocation = (World.LocationById((int)World.LocationTypes.Home));
            player.Inventory.Add(new InventoryItem(World.ItemById((int)World.ItemTypes.RustySword), 1));
            return player;
        }

        public static Player CreatePlayerFromXmlString(string xmlPlayerData)
        {
            try
            {
                XmlDocument playerData = new XmlDocument();

                playerData.LoadXml(xmlPlayerData);

                int currentHitPoints =
                    Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentHitPoints").InnerText);
                int maximumHitPoints =
                    Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/MaximumHitPoints").InnerText);
                int gold =
                    Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/Gold").InnerText);
                int experiencePoints =
                    Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/ExperiencePoints").InnerText);

                Player player = new Player(currentHitPoints, maximumHitPoints, gold, experiencePoints);

                int currentLocationID =
                    Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentLocation").InnerText);
                player.CurrentLocation = World.LocationById(currentLocationID);

                if (playerData.SelectSingleNode("/Player/Stats/CurrentWeapon") != null)
                {
                    int currentWeaponID = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentWeapon").InnerText);
                    player.CurrentWeapon = (Weapon) World.ItemById(currentWeaponID);
                }

                foreach (XmlNode node in playerData.SelectNodes("/Player/InventoryItems/InventoryItem"))
                {
                    int id = Convert.ToInt32(node.Attributes["ID"].Value);
                    int quantity = Convert.ToInt32(node.Attributes["Quantity"].Value);

                    for (int i = 0; i < quantity; i++)
                    {
                        player.AddItemToInventory(World.ItemById(id));
                    }
                }

                foreach (XmlNode node in playerData.SelectNodes("/Player/PlayerQuests/PlayerQuest"))
                {
                    int id = Convert.ToInt32(node.Attributes["ID"].Value);
                    bool isCompleted = Convert.ToBoolean(node.Attributes["IsCompleted"].Value);

                    player.Quests.Add(new PlayerQuest(World.QuestById(id))
                    {
                        IsCompleted = isCompleted
                    });
                }

                return player;

            }
            catch
            {
                return CreateDefaultPlayer();
            }
        }

        public bool HasRequiredItemToEnterThisLocation(Location theLocation)
        {
            if (theLocation.ItemRequiredToEnter == null)
            {
                return true;
            }

            return Inventory.Exists(ii => ii.Details.ID == theLocation.ItemRequiredToEnter.ID);
        }

        public bool HasThisQuest(Quest theQuest)
        {
            return Quests.Exists(pq => pq.Details.ID == theQuest.ID);
        }

        public bool CompletedThisQuest(Quest theQuest)
        {
            foreach (var playerQuest in Quests)
            {
                if (playerQuest.Details.ID == theQuest.ID)
                {
                    return playerQuest.IsCompleted;
                }
            }

            return false;
        }

        public bool HasAllQuestCompletionItems(Quest theQuest)
        {
            var result = theQuest.QuestCompletionItems.Exists(ii =>
            {
                if (!Inventory.Exists(jj => jj.Details.ID == ii.Details.ID
                                            && jj.Quantity >= ii.Quantity))
                {
                    return false;
                }

                return true;
            });
            return result;
        }

        public void RemoveQuestCompletionItems(Quest quest)
        {
            quest.QuestCompletionItems.ForEach(ii =>
            {
                InventoryItem item = Inventory.SingleOrDefault(jj => jj.Details.ID == ii.Details.ID);
                if (item != null)
                {
                    item.Quantity -= ii.Quantity;
                }
            });
        }

        public void AddItemToInventory(Item itemToAdd)
        {
            InventoryItem item = Inventory.FirstOrDefault(ii => ii.Details.ID == itemToAdd.ID);

            if (item == null)
            {
                Inventory.Add(new InventoryItem(itemToAdd, 1));
            }
            else
            {
                item.Quantity++;
            }

        }

        public void MarkQuestCompleted(Quest quest)
        {
            
            PlayerQuest playerQuest = Quests.FirstOrDefault(ii => ii.Details.ID == quest.ID);
            if (playerQuest != null)
            {
                playerQuest.IsCompleted = true;
            }
        }

        public string ToXmlString()
        {
            XmlDocument playerData = new XmlDocument();

            XmlNode player = playerData.CreateElement("Player");
            playerData.AppendChild(player);

            XmlNode stats = playerData.CreateElement("Stats");
            player.AppendChild(stats);

            XmlNode currentHitPoints = playerData.CreateElement("CurrentHitPoints");
            currentHitPoints.AppendChild(playerData.CreateTextNode(CurrentHitPoints.ToString()));
            stats.AppendChild(currentHitPoints);

            XmlNode maximumHitPoints = playerData.CreateElement("MaximumHitPoints");
            maximumHitPoints.AppendChild(playerData.CreateTextNode(MaximumHitPoints.ToString()));
            stats.AppendChild(maximumHitPoints);

            XmlNode gold = playerData.CreateElement("Gold");
            gold.AppendChild(playerData.CreateTextNode(Gold.ToString()));
            stats.AppendChild(gold);

            XmlNode experiencePoints = playerData.CreateElement("ExperiencePoints");
            experiencePoints.AppendChild(playerData.CreateTextNode(ExperiencePoints.ToString()));
            stats.AppendChild(experiencePoints);

            XmlNode currentLocation = playerData.CreateElement("CurrentLocation");
            currentLocation.AppendChild(playerData.CreateTextNode(CurrentLocation.ID.ToString()));
            stats.AppendChild(currentLocation);

            if (CurrentWeapon != null)
            {
                XmlNode currentWeapon = playerData.CreateElement("CurrentWeapon");
                currentWeapon.AppendChild(playerData.CreateTextNode(CurrentWeapon.ID.ToString()));
                stats.AppendChild(currentWeapon);
            }

            XmlNode inventoryItems = playerData.CreateElement("InventoryItems");
            player.AppendChild(inventoryItems);

            foreach (var item in Inventory)
            {
                XmlNode inventoryItem = playerData.CreateElement("InventoryItem");

                XmlAttribute idAttribute = playerData.CreateAttribute("ID");
                idAttribute.Value = item.Details.ID.ToString();
                inventoryItem.Attributes.Append(idAttribute);

                XmlAttribute quantityAttribute = playerData.CreateAttribute("Quantity");
                quantityAttribute.Value = item.Quantity.ToString();
                inventoryItem.Attributes.Append(quantityAttribute);

                inventoryItems.AppendChild(inventoryItem);
            }

            XmlNode playerQuests = playerData.CreateElement("PlayerQuests");
            player.AppendChild(playerQuests);

            foreach (var quest in Quests)
            {
                XmlNode playerQuest = playerData.CreateElement("PlayerQuest");

                XmlAttribute idAttribute = playerData.CreateAttribute("ID");
                idAttribute.Value = quest.Details.ID.ToString();
                playerQuest.Attributes.Append(idAttribute);

                XmlAttribute isCompleteAttribute = playerData.CreateAttribute("IsCompleted");
                isCompleteAttribute.Value = quest.IsCompleted.ToString();
                playerQuest.Attributes.Append(isCompleteAttribute);

                playerQuests.AppendChild(playerQuest);
            }

            return XDocument.Parse(playerData.InnerXml).ToString();
        }

        public void AddExperiencePoints(int experiencePointsToAdd)
        {
            ExperiencePoints += experiencePointsToAdd;
            MaximumHitPoints = (Level * 10);
        }
    }
}
