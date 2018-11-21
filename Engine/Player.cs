using System.Collections.Generic;

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

            foreach (var inventoryItem in Inventory)
            {
                if (inventoryItem.Details.ID == theLocation.ItemRequiredToEnter.ID)
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasThisQuest(Quest theQuest)
        {
            foreach (var playerQuest in Quests)
            {
                if (playerQuest.Details.ID == theQuest.ID)
                {
                    return true;
                }
            }

            return false;
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
            foreach (var theQuestQuestCompletionItem in theQuest.QuestCompletionItems)
            {
                bool foundItemInPlayersInventory = false;

                foreach (var inventoryItem in Inventory)
                {
                    if (inventoryItem.Details.ID == theQuestQuestCompletionItem.Details.ID)
                    {
                        foundItemInPlayersInventory = true;
                        if (inventoryItem.Quantity < theQuestQuestCompletionItem.Quantity)
                        {
                            return false;
                        }
                    }
                }

                if (!foundItemInPlayersInventory)
                {
                    return false;
                }
            }

            return true;
        }

        public void RemoveQuestCompletionItems(Quest quest)
        {
            foreach (var questQuestCompletionItem in quest.QuestCompletionItems)
            {
                foreach (var inventoryItem in Inventory)
                {
                    if (inventoryItem.Details.ID == questQuestCompletionItem.Details.ID)
                    {
                        inventoryItem.Quantity -= questQuestCompletionItem.Quantity;
                        break;
                    }
                }
            }
        }

        public void AddItemToInventory(Item itemToAdd)
        {
            foreach (var inventoryItem in Inventory)
            {
                if (inventoryItem.Details == itemToAdd)
                {
                    inventoryItem.Quantity++;
                    return;
                }
            }

            Inventory.Add(new InventoryItem(itemToAdd, 1));
        }

        public void MarkQuestCompleted(Quest quest)
        {
            foreach (var playerQuest in Quests)
            {
                if (playerQuest.Details.ID == quest.ID)
                {
                    playerQuest.IsCompleted = true;
                    return;
                }
            }
        }
    }
}
