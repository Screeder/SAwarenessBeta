using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SAwareness.Wards;

namespace SAwareness.Wards
{
    class FowWardPlacement
    {
        public static Menu.MenuItemSettings FowWardPlacementWard = new Menu.MenuItemSettings(typeof(FowWardPlacement));

        Dictionary<Obj_AI_Hero, List<ExpandedWardItem>> enemiesUsed = new Dictionary<Obj_AI_Hero, List<ExpandedWardItem>>();
        Dictionary<Obj_AI_Hero, List<ExpandedWardItem>> enemiesRefilled = new Dictionary<Obj_AI_Hero, List<ExpandedWardItem>>();

        private int lastGameUpdateTime = 0;

        public class ExpandedWardItem : Ward.WardItem
        {
            public int Stacks;
            public int Charges;
            public bool Cd;

            public ExpandedWardItem(int id, string name, string spellName, int range, int duration, Ward.WardType type, int stacks, int charges)
                : base(id, name, spellName, range, duration, type)
            {
                Stacks = stacks;
                Charges = charges;
            }

            public ExpandedWardItem(Ward.WardItem ward, int stacks, int charges)
                : base(ward.Id, ward.Name, ward.SpellName, ward.Range, ward.Duration, ward.Type)
            {
                Stacks = stacks;
                Charges = charges;
            }
        }

        public FowWardPlacement()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsEnemy)
                {
                    List<ExpandedWardItem> wards = GetWardItemsUsed(hero);
                    enemiesUsed.Add(hero, wards);
                    wards = GetWardItemsRefilled(hero);
                    enemiesRefilled.Add(hero, wards);
                }
            }
            //Game.OnGameUpdate += Game_OnGameUpdate;
            ThreadHelper.GetInstance().Called += Game_OnGameUpdate;
        }

        ~FowWardPlacement()
        {
            //Game.OnGameUpdate -= Game_OnGameUpdate;
            ThreadHelper.GetInstance().Called -= Game_OnGameUpdate;
            enemiesUsed = null;
            enemiesRefilled = null;
        }

        public bool IsActive()
        {
            return Ward.Wards.GetActive() && FowWardPlacementWard.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            FowWardPlacementWard.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("WARDS_FOWWARDPLACEMENT_MAIN"), "SAwarenessWardsFowWardPlacement"));
            FowWardPlacementWard.MenuItems.Add(
                FowWardPlacementWard.Menu.AddItem(new MenuItem("SAwarenessWardsFowWardPlacementActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return FowWardPlacementWard;
        }

        private void Game_OnGameUpdate(object sender, EventArgs args)
        {
            if (!IsActive() || lastGameUpdateTime + new Random().Next(500, 1000) > Environment.TickCount)
                return;

            lastGameUpdateTime = Environment.TickCount;

            foreach (var enemy in enemiesUsed.ToArray())
            {
                Obj_AI_Hero hero = enemy.Key;
                List<ExpandedWardItem> wards = new List<ExpandedWardItem>(enemy.Value.ToArray());
                foreach (var item in hero.InventoryItems)
                {
                    foreach (var wardItem in enemy.Value.ToArray())
                    {
                        if ((int)item.Id == wardItem.Id && wardItem.Type != Ward.WardType.Temp && wardItem.Type != Ward.WardType.TempVision && hero.Spellbook.CanUseSpell(item.SpellSlot) == SpellState.Ready)
                        {
                            /*if (item.Charges < wardItem.Charges || item.Stacks < wardItem.Stacks)
                                Console.Write("");*/
                            if (item.Charges > 0 ? item.Charges >= wardItem.Charges : false || item.Stacks >= wardItem.Stacks) //Check for StackItems etc fail
                            {
                                enemy.Value.Remove(wardItem);
                            }
                        }
                    }
                }
                foreach (var wardItem in enemy.Value)
                {
                    Game.PrintChat("{0} has used {1}", enemy.Key.ChampionName, wardItem.Name);
                }
                enemiesUsed[enemy.Key] = GetWardItemsUsed(hero);
            }

            foreach (var enemy in enemiesRefilled.ToArray())
            {
                Obj_AI_Hero hero = enemy.Key;

                //Refill
                List<ExpandedWardItem> wards = new List<ExpandedWardItem>();
                foreach (var item in hero.InventoryItems)
                {
                    List<int> checkedWards = new List<int>();
                    foreach (var wardItem in enemy.Value.ToArray())
                    {
                        if ((int)item.Id == wardItem.Id && (item.Charges > wardItem.Charges || item.Stacks > wardItem.Stacks) &&
                            wardItem.Type != Ward.WardType.Temp && wardItem.Type != Ward.WardType.TempVision && hero.Spellbook.CanUseSpell(item.SpellSlot) == SpellState.Ready)
                        {
                            wards.Add(wardItem);
                        }
                        checkedWards.Add(wardItem.Id);
                    }
                    foreach (var ward in Ward.WardItems)
                    {
                        if ((int)item.Id == ward.Id && ward.Type != Ward.WardType.Temp && ward.Type != Ward.WardType.TempVision && hero.Spellbook.CanUseSpell(item.SpellSlot) == SpellState.Ready &&
                            (enemy.Value.Find(wardItem => wardItem.Id == ward.Id) == null))
                        {
                            wards.Add(new ExpandedWardItem(ward, item.Stacks, item.Charges));
                        }
                    }
                }
                foreach (var wardItem in wards)
                {
                    Game.PrintChat("{0} got {1}", enemy.Key.ChampionName, wardItem.Name);
                }
                enemiesRefilled[enemy.Key] = GetWardItemsRefilled(hero);
            }
        }

        private List<ExpandedWardItem> GetWardItemsRefilled(Obj_AI_Hero hero)
        {
            List<ExpandedWardItem> wards = new List<ExpandedWardItem>();
            foreach (var item in hero.InventoryItems)
            {
                foreach (var wardItem in Ward.WardItems)
                {
                    if ((int)item.Id == wardItem.Id && wardItem.Type != Ward.WardType.Temp && wardItem.Type != Ward.WardType.TempVision)
                    {
                        wards.Add(new ExpandedWardItem(wardItem, item.Stacks, item.Charges));
                    }
                }
            }
            return wards;
        }

        private List<ExpandedWardItem> GetWardItemsUsed(Obj_AI_Hero hero)
        {
            List<ExpandedWardItem> wards = new List<ExpandedWardItem>();
            foreach (var item in hero.InventoryItems)
            {
                foreach (var wardItem in Ward.WardItems)
                {
                    if ((int)item.Id == wardItem.Id && wardItem.Type != Ward.WardType.Temp && wardItem.Type != Ward.WardType.TempVision && hero.Spellbook.CanUseSpell(item.SpellSlot) == SpellState.Ready)
                    {
                        wards.Add(new ExpandedWardItem(wardItem, item.Stacks, item.Charges));
                    }
                }
            }
            return wards;
        }
    }
}
