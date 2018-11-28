using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
            lblHitPoints.DataBindings.Add(
                "Text", _player, "CurrentHitPoints");
            lblGold.DataBindings.Add(
                "Text", _player, "Gold");
            lblExperience.DataBindings.Add(
                "Text", _player, "ExperiencePoints");
            lblLevel.DataBindings.Add(
                "Text", _player, "Level");

            cboWeapons.DataSource = _player.Weapons;
            cboWeapons.DisplayMember = "Name";
            cboWeapons.ValueMember = "Id";

            if (_player.CurrentWeapon != null)
            {
                cboWeapons.SelectedItem = _player.CurrentWeapon;
            }

            cboWeapons.SelectedIndexChanged += cboWeapons_SelectedIndexChanged;

            cboPotions.DataSource = _player.Potions;
            cboPotions.DisplayMember = "Name";
            cboPotions.ValueMember = "Id";

            _player.PropertyChanged += PlayerOnPropertyChanged;

            MoveTo(_player.CurrentLocation);

            dgvInventory.RowHeadersVisible = false;
            dgvInventory.AutoGenerateColumns = false;

            dgvInventory.DataSource = _player.Inventory;

            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Name",
                Width = 197,
                DataPropertyName = "Description"
            });

            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Quantity",
                DataPropertyName = "Quantity"
            });

            dgvQuests.RowHeadersVisible = false;
            dgvQuests.AutoGenerateColumns = false;

            dgvQuests.DataSource = _player.Quests;

            dgvQuests.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Name",
                Width = 197,
                DataPropertyName = "Name"
            });

            dgvQuests.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Done?",
                DataPropertyName = "IsCompleted"
            });

        }

        private void PlayerOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Weapons")
            {
                cboWeapons.DataSource = _player.Weapons;

                if (!_player.Weapons.Any())
                {
                    cboWeapons.Visible = false;
                    btnUseWeapon.Visible = false;
                }
            }

            if (e.PropertyName == "Potions")
            {
                cboPotions.DataSource = _player.Potions;

                if (!_player.Potions.Any())
                {
                    cboPotions.Visible = false;
                    btnUsePotion.Visible = false;
                }
            }
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

                _player.AddExperiencePoints(_currentMonster.RewardExperiencePoints);
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

                if (_player.CurrentHitPoints <= 0)
                {
                    rtbMessages.Text += "The " + _currentMonster.Name + " killed you." +
                                        Environment.NewLine;

                    MoveTo(World.LocationById((int)World.LocationTypes.Home));
                }
                ScrollToBottomOfMessages();
            }
        }

        private void btnUsePotion_Click(object sender, System.EventArgs e)
        {
            var healingPotion = cboPotions.SelectedItem as HealingPotion;

            _player.CurrentHitPoints += healingPotion.AmountToHeal;

            if (_player.CurrentHitPoints > _player.MaximumHitPoints)
            {
                _player.CurrentHitPoints = _player.MaximumHitPoints;
            }

            _player.RemoveItemFromInventory(healingPotion);

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

                                _player.AddExperiencePoints(theLocation.QuestAvailableHere.RewardExperiencePoints);
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

                Monster standardMonster = World.MonsterById(theLocation.MonsterLivingHere.Id);

                _currentMonster = new Monster(standardMonster);

                foreach (var lootItem in standardMonster.LootTable)
                {
                    _currentMonster.LootTable.Add(lootItem);
                }

                cboWeapons.Visible = _player.Weapons.Any();
                cboPotions.Visible = _player.Potions.Any();
                btnUsePotion.Visible = _player.Potions.Any();
                btnUseWeapon.Visible = _player.Weapons.Any();
            }
            else
            {
                _currentMonster = null;
                cboWeapons.Visible = false;
                cboPotions.Visible = false;
                btnUsePotion.Visible = false;
                btnUseWeapon.Visible = false;
            }

            ScrollToBottomOfMessages();
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

        private void cboWeapons_SelectedIndexChanged(object sender, EventArgs e)
        {
            _player.CurrentWeapon = (Weapon) cboWeapons.SelectedItem;
        }
    }
}
