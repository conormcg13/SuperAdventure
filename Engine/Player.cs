using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;

namespace Engine
{
    public class Player : LivingCreature
    {
        public int Gold { get; set; }
        public int ExperiencePoints { get; set; }
        public int Level => (ExperiencePoints / 100) + 1;
        
        public List<InventoryItem> Inventory { get; set; }
        public List<PlayerQuest> Quests { get; set; }
        public Location CurrentLocation { get; set; }

        public Player( int currentHitPoints = 10, int maximumHitPoints = 10, int gold = 20,
                            int experiencePoints = 0 ) :
                                base( currentHitPoints, maximumHitPoints )
        {
            Gold = gold;
            ExperiencePoints = experiencePoints;
            Inventory = new List<InventoryItem>();
            Quests = new List<PlayerQuest>();
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
    }
}
