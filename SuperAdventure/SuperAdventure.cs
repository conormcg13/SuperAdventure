using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Engine;
using System.Windows.Forms;

namespace SuperAdventure
{
    public partial class SuperAdventure : Form
    {
        private readonly Player _player;
        private Monster _currentMonster;
        private const string PLAYER_DATA_FILE_NAME = "PlayerData.Xml";

        public SuperAdventure()
        {
            InitializeComponent();
            if (File.Exists(PLAYER_DATA_FILE_NAME))
            {
                _player = Player.CreatePlayerFromXmlString(File.ReadAllText(PLAYER_DATA_FILE_NAME));
            }
            else
            {
                _player = Player.CreateDefaultPlayer();
            }
            MoveTo(_player.CurrentLocation);
            UpdatePlayerStats();
        }

        private void btnNorth_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToNorth);
        }

        private void btnSouth_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToSouth);
        }

        private void btnEast_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToEast);
        }

        private void btnWest_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToWest);
        }

        private void btnUseWeapon_Click(object sender, EventArgs e)
        {
            var currentWeapon = cboWeapons.SelectedItem as Weapon;
            var amountOfDamage =
                RandomNumberGenerator.NumberBetween(currentWeapon.MinimumDamage, currentWeapon.MaximumDamage);

            _currentMonster.CurrentHitPoints -= amountOfDamage;

            rtbMessages.Text += "You hit the " + _currentMonster.Name + " for " +
                                amountOfDamage + " points." + Environment.NewLine;
            ScrollToBottomOfMessages();

            if (_currentMonster.CurrentHitPoints <= 0)
            {
                rtbMessages.Text += Environment.NewLine;
                rtbMessages.Text += "You defeated the " + _currentMonster.Name + Environment.NewLine;

                _player.ExperiencePoints += _currentMonster.RewardExperiencePoints;
                rtbMessages.Text += "You receive " + _currentMonster.RewardExperiencePoints +
                                    " experience points" + Environment.NewLine;

                _player.Gold += _currentMonster.RewardGold;
                rtbMessages.Text += "You receive " + _currentMonster.RewardGold +
                                    " gold" + Environment.NewLine;

                List<InventoryItem> lootedItems = new List<InventoryItem>();

                foreach (var lootItem in _currentMonster.LootTable)
                {
                    if (RandomNumberGenerator.NumberBetween(1, 100) <= lootItem.DropPercentage)
                    {
                        lootedItems.Add(new InventoryItem(lootItem.Details, 1));
                    }
                }

                if (lootedItems.Count == 0)
                {
                    foreach (var lootItem in _currentMonster.LootTable)
                    {
                        if (lootItem.IsDefaultItem)
                        {
                            lootedItems.Add(new InventoryItem(lootItem.Details, 1));
                        }
                    }
                }

                foreach (var inventoryItem in lootedItems)
                {
                    _player.AddItemToInventory(inventoryItem.Details);

                    if (inventoryItem.Quantity == 1)
                    {
                        rtbMessages.Text += "You loot " +
                                            inventoryItem.Quantity + " " +
                                            inventoryItem.Details.Name + Environment.NewLine;
                    }
                    else
                    {
                        rtbMessages.Text += "You loot " +
                                            inventoryItem.Quantity + " " +
                                            inventoryItem.Details.NamePlural + Environment.NewLine;
                    }
                }

                UpdatePlayerStats();

                UpdateInventoryListInUI();
                UpdateWeaponListinUI();
                UpdatePotionListInUI();

                rtbMessages.Text += Environment.NewLine;

                MoveTo(_player.CurrentLocation);
                ScrollToBottomOfMessages();
            }
            else
            {
                int damageToPlayer = RandomNumberGenerator.NumberBetween(0, _currentMonster.MaximumDamage);

                rtbMessages.Text += "The " + _currentMonster.Name + " did " + damageToPlayer + " points of damage." +
                                    Environment.NewLine;

                _player.CurrentHitPoints -= damageToPlayer;

                lblHitPoints.Text = _player.CurrentHitPoints.ToString();

                if (_player.CurrentHitPoints <= 0)
                {
                    rtbMessages.Text += "The " + _currentMonster.Name + " killed you." +
                                        Environment.NewLine;

                    MoveTo(World.LocationById((int)World.LocationTypes.Home));
                }
                ScrollToBottomOfMessages();
            }
        }

        private void UpdatePlayerStats()
        {
            lblHitPoints.Text = _player.CurrentHitPoints.ToString();
            lblGold.Text = _player.Gold.ToString();
            lblExperience.Text = _player.ExperiencePoints.ToString();
            lblLevel.Text = _player.Level.ToString();
        }

        private void btnUsePotion_Click(object sender, System.EventArgs e)
        {
            var healingPotion = cboPotions.SelectedItem as HealingPotion;

            _player.CurrentHitPoints += healingPotion.AmountToHeal;

            if (_player.CurrentHitPoints > _player.MaximumHitPoints)
            {
                _player.CurrentHitPoints = _player.MaximumHitPoints;
            }

            foreach (var inventoryItem in _player.Inventory)
            {
                if (inventoryItem.Details.ID == healingPotion.ID)
                {
                    inventoryItem.Quantity--;
                    break;
                }
            }

            rtbMessages.Text += "You drink a " + healingPotion.Name + Environment.NewLine;

            int damageToPlayer = RandomNumberGenerator.NumberBetween(0, _currentMonster.MaximumDamage);

            rtbMessages.Text += "The " + _currentMonster.Name + " did " +
                                damageToPlayer + " points of damage." + Environment.NewLine;

            _player.CurrentHitPoints -= damageToPlayer;

            if (_player.CurrentHitPoints <= 0)
            {
                rtbMessages.Text += "The " + _currentMonster.Name + " killed you." +
                                    Environment.NewLine;

                MoveTo(World.LocationById((int)World.LocationTypes.Home));
            }

            lblHitPoints.Text = _player.CurrentHitPoints.ToString();
            UpdateInventoryListInUI();
            UpdatePotionListInUI();
            ScrollToBottomOfMessages();
        }

        private void MoveTo(Location theLocation)
        {
            if (!_player.HasRequiredItemToEnterThisLocation(theLocation))
            {
                StringBuilder message = new StringBuilder();
                message.Append("You must have a ");
                message.Append(theLocation.ItemRequiredToEnter.Name);
                message.Append(" to enter this location.");
                message.Append(Environment.NewLine);
                rtbMessages.Text += message;
                return;
            }

            _player.CurrentLocation = theLocation;

            btnNorth.Visible = theLocation.LocationToNorth != null;
            btnEast.Visible = theLocation.LocationToEast != null;
            btnSouth.Visible = theLocation.LocationToSouth != null;
            btnWest.Visible = theLocation.LocationToWest != null;

            rtbLocation.Text = theLocation.Name + Environment.NewLine;
            rtbLocation.Text += theLocation.Description + Environment.NewLine;

            _player.CurrentHitPoints = _player.MaximumHitPoints;

            lblHitPoints.Text = _player.CurrentHitPoints.ToString();

            if (theLocation.QuestAvailableHere != null)
            {
                bool alreadyHasQuest = _player.HasThisQuest(theLocation.QuestAvailableHere);
                bool questCompleted = _player.CompletedThisQuest(theLocation.QuestAvailableHere);

                if (alreadyHasQuest)
                {
                    if (!questCompleted)
                    {
                        bool playerHasAllItemsToCompleteQuest = _player.HasAllQuestCompletionItems(theLocation.QuestAvailableHere);

                        if (playerHasAllItemsToCompleteQuest)
                            {
                                rtbMessages.Text += Environment.NewLine;
                                rtbMessages.Text += "You complete the " +
                                                    theLocation.QuestAvailableHere.Name + " quest." +
                                                    Environment.NewLine;

                                _player.RemoveQuestCompletionItems(theLocation.QuestAvailableHere);

                                rtbMessages.Text += "You receive: " + Environment.NewLine;
                                rtbMessages.Text += theLocation.QuestAvailableHere.RewardExperiencePoints.ToString() +
                                                    " experience points" + Environment.NewLine;
                                rtbMessages.Text += theLocation.QuestAvailableHere.RewardGold.ToString() +
                                                    " gold" + Environment.NewLine;
                                rtbMessages.Text +=
                                    theLocation.QuestAvailableHere.RewardItem.Name + Environment.NewLine;
                                rtbMessages.Text += Environment.NewLine;

                                _player.ExperiencePoints += theLocation.QuestAvailableHere.RewardExperiencePoints;
                                _player.Gold += theLocation.QuestAvailableHere.RewardGold;

                                _player.AddItemToInventory(theLocation.QuestAvailableHere.RewardItem);

                                _player.MarkQuestCompleted(theLocation.QuestAvailableHere);
                            }
                    }
                }
                else
                {
                    rtbMessages.Text += "You receive the " +
                                        theLocation.QuestAvailableHere.Name +
                                        " quest." + Environment.NewLine;
                    rtbMessages.Text += theLocation.QuestAvailableHere.Description +
                                        Environment.NewLine;
                    rtbMessages.Text += "To complete it, return with:" +
                                        Environment.NewLine;
                    foreach (var questCompletionItem in theLocation.QuestAvailableHere.QuestCompletionItems)
                    {
                        if (questCompletionItem.Quantity == 1)
                        {
                            rtbMessages.Text += questCompletionItem.Quantity + " " +
                                                questCompletionItem.Details.Name + Environment.NewLine;
                        }
                        else
                        {
                            rtbMessages.Text += questCompletionItem.Quantity + " " +
                                                questCompletionItem.Details.NamePlural + Environment.NewLine;
                        }
                    }

                    rtbMessages.Text += Environment.NewLine;

                    _player.Quests.Add(new PlayerQuest(theLocation.QuestAvailableHere));
                }
            }

            if (theLocation.MonsterLivingHere != null)
            {
                rtbMessages.Text += "You see a " + theLocation.MonsterLivingHere.Name +
                                    Environment.NewLine;

                Monster standardMonster = World.MonsterById(theLocation.MonsterLivingHere.ID);

                _currentMonster = new Monster(standardMonster);

                foreach (var lootItem in standardMonster.LootTable)
                {
                    _currentMonster.LootTable.Add(lootItem);
                }

                cboWeapons.Visible = true;
                cboPotions.Visible = true;
                btnUsePotion.Visible = true;
                btnUseWeapon.Visible = true;
            }
            else
            {
                _currentMonster = null;
                cboWeapons.Visible = false;
                cboPotions.Visible = false;
                btnUsePotion.Visible = false;
                btnUseWeapon.Visible = false;
            }

            UpdateInventoryListInUI();
            UpdateQuestListInUI();
            UpdateWeaponListinUI();
            UpdatePotionListInUI();
            ScrollToBottomOfMessages();
        }

        private void UpdateInventoryListInUI()
        {
            dgvInventory.RowHeadersVisible = false;

            dgvInventory.ColumnCount = 2;
            dgvInventory.Columns[0].Name = "Name";
            dgvInventory.Columns[0].Width = 197;
            dgvInventory.Columns[1].Name = "Quantity";

            dgvInventory.Rows.Clear();

            foreach (var inventoryItem in _player.Inventory)
            {
                if (inventoryItem.Quantity > 0)
                {
                    dgvInventory.Rows.Add(new[] { inventoryItem.Details.Name, inventoryItem.Quantity.ToString() });
                }
            }
        }

        private void UpdateQuestListInUI()
        {
            dgvQuests.RowHeadersVisible = false;

            dgvQuests.ColumnCount = 2;
            dgvQuests.Columns[0].Name = "Name";
            dgvQuests.Columns[0].Width = 197;
            dgvQuests.Columns[1].Name = "Done?";

            dgvQuests.Rows.Clear();

            foreach (var playerQuest in _player.Quests)
            {
                dgvQuests.Rows.Add(new[] { playerQuest.Details.Name, playerQuest.IsCompleted.ToString() });
            }
        }

        private void UpdateWeaponListinUI()
        {
            List<Weapon> weapons = new List<Weapon>();

            foreach (var inventoryItem in _player.Inventory)
            {
                if (inventoryItem.Details is Weapon weapon)
                {
                    if (inventoryItem.Quantity > 0)
                    {
                        weapons.Add(weapon);
                    }
                }
            }

            if (weapons.Count == 0)
            {
                cboWeapons.Visible = false;
                btnUseWeapon.Visible = false;
            }
            else
            {
                cboWeapons.DataSource = weapons;
                cboWeapons.DisplayMember = "Name";
                cboWeapons.ValueMember = "ID";

                cboWeapons.SelectedIndex = 0;
            }
        }

        private void UpdatePotionListInUI()
        {
            List<HealingPotion> healingPotions = new List<HealingPotion>();

            foreach (var inventoryItem in _player.Inventory)
            {
                if (inventoryItem.Details is HealingPotion potion)
                {
                    if (inventoryItem.Quantity > 0)
                    {
                        healingPotions.Add(potion);
                    }
                }
            }

            if (healingPotions.Count == 0)
            {
                cboPotions.Visible = false;
                btnUsePotion.Visible = false;
            }
            else
            {
                cboPotions.DataSource = healingPotions;
                cboPotions.DisplayMember = "Name";
                cboPotions.ValueMember = "ID";

                cboPotions.SelectedIndex = 0;
            }
        }

        private void ScrollToBottomOfMessages()
        {
            rtbMessages.SelectionStart = rtbMessages.Text.Length;
            rtbMessages.ScrollToCaret();
        }

        private void SuperAdventure_FormClosing(object sender, FormClosingEventArgs e)
        {
            File.WriteAllText(PLAYER_DATA_FILE_NAME, _player.ToXmlString());
        }
    }
}
