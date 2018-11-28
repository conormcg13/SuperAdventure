using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Engine
{
    public class Player : LivingCreature
    {
        private int _gold;
        private int _experiencePoints; 
        public int Level => (ExperiencePoints / 100) + 1;

        public int Gold
        {
            get => _gold;
            set
            {
                _gold = value;
                OnPropertyChanged("Gold");
            }
        }

        public int ExperiencePoints
        {
            get => _experiencePoints;
            set
            {
                _experiencePoints = value;
                OnPropertyChanged("ExperiencePoints");
                OnPropertyChanged("Level");
            }
        }
        public BindingList<InventoryItem> Inventory { get; set; }
        public BindingList<PlayerQuest> Quests { get; set; }
        public Location CurrentLocation { get; set; }
        public Weapon CurrentWeapon { get; set; }

        public List<Weapon> Weapons
        {
            get { return Inventory.Where(
                x => x.Details is Weapon).Select(
                x => x.Details as Weapon).ToList(); }
        }

        public List<HealingPotion> Potions
        {
            get
            {
                return Inventory.Where(
                        x => x.Details is HealingPotion).Select(
                        x => x.Details as HealingPotion)
                    .ToList();
            }
        }

        private Player( int currentHitPoints = 10, int maximumHitPoints = 10, int gold = 20,
                            int experiencePoints = 0 ) :
                                base( currentHitPoints, maximumHitPoints )
        {
            Gold = gold;
            ExperiencePoints = experiencePoints;

            Inventory = new BindingList<InventoryItem>();
            Quests = new BindingList<PlayerQuest>();
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

                int currentLocationId =
                    Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentLocation").InnerText);
                player.CurrentLocation = World.LocationById(currentLocationId);

                if (playerData.SelectSingleNode("/Player/Stats/CurrentWeapon") != null)
                {
                    int currentWeaponId = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentWeapon").InnerText);
                    player.CurrentWeapon = (Weapon) World.ItemById(currentWeaponId);
                }

                foreach (XmlNode node in playerData.SelectNodes("/Player/InventoryItems/InventoryItem"))
                {
                    int id = Convert.ToInt32(node.Attributes["Id"].Value);
                    int quantity = Convert.ToInt32(node.Attributes["Quantity"].Value);

                    for (int i = 0; i < quantity; i++)
                    {
                        player.AddItemToInventory(World.ItemById(id));
                    }
                }

                foreach (XmlNode node in playerData.SelectNodes("/Player/PlayerQuests/PlayerQuest"))
                {
                    int id = Convert.ToInt32(node.Attributes["Id"].Value);
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

            return Inventory.Any(ii => ii.Details.Id == theLocation.ItemRequiredToEnter.Id);
        }

        public bool HasThisQuest(Quest theQuest)
        {
            return Quests.Any(pq => pq.Details.Id == theQuest.Id);
        }

        public bool CompletedThisQuest(Quest theQuest)
        {
            foreach (var playerQuest in Quests)
            {
                if (playerQuest.Details.Id == theQuest.Id)
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
                if (!Inventory.Any(jj => jj.Details.Id == ii.Details.Id
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
                InventoryItem item = Inventory.SingleOrDefault(jj => jj.Details.Id == ii.Details.Id);
                if (item != null)
                {
                    RemoveItemFromInventory(item.Details, ii.Quantity);
                }
            });
        }

        public void AddItemToInventory(Item itemToAdd)
        {
            InventoryItem item = Inventory.FirstOrDefault(ii => ii.Details.Id == itemToAdd.Id);

            if (item == null)
            {
                Inventory.Add(new InventoryItem(itemToAdd, 1));
            }
            else
            {
                item.Quantity++;
            }
            RaiseInventoryChangedEvent(itemToAdd);

        }

        public void MarkQuestCompleted(Quest quest)
        {
            
            PlayerQuest playerQuest = Quests.FirstOrDefault(ii => ii.Details.Id == quest.Id);
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
            currentLocation.AppendChild(playerData.CreateTextNode(CurrentLocation.Id.ToString()));
            stats.AppendChild(currentLocation);

            if (CurrentWeapon != null)
            {
                XmlNode currentWeapon = playerData.CreateElement("CurrentWeapon");
                currentWeapon.AppendChild(playerData.CreateTextNode(CurrentWeapon.Id.ToString()));
                stats.AppendChild(currentWeapon);
            }

            XmlNode inventoryItems = playerData.CreateElement("InventoryItems");
            player.AppendChild(inventoryItems);

            foreach (var item in Inventory)
            {
                XmlNode inventoryItem = playerData.CreateElement("InventoryItem");

                XmlAttribute idAttribute = playerData.CreateAttribute("Id");
                idAttribute.Value = item.Details.Id.ToString();
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

                XmlAttribute idAttribute = playerData.CreateAttribute("Id");
                idAttribute.Value = quest.Details.Id.ToString();
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

        private void RaiseInventoryChangedEvent(Item item)
        {
            if (item is Weapon)
            {
                OnPropertyChanged("Weapon");
            }

            if (item is HealingPotion)
            {
                OnPropertyChanged("Potions");
            }
        }

        public void RemoveItemFromInventory(Item itemToRemove, int quantity = 1)
        {
            InventoryItem item = Inventory.SingleOrDefault(x => x.Details.Id == itemToRemove.Id);

            if (item != null)
            {
                item.Quantity -= quantity;

                if (item.Quantity < 0)
                {
                    item.Quantity = 0;
                }

                if (item.Quantity == 0)
                {
                    Inventory.Remove(item);
                }

                RaiseInventoryChangedEvent(itemToRemove);
            }
        }
    }
}
