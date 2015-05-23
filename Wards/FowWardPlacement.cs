using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAssemblies.Wards
{
    class FowWardPlacement
    {
        public static Menu.MenuItemSettings FowWardPlacementWard = new Menu.MenuItemSettings(typeof(FowWardPlacement));

		Dictionary<Obj_AI_Hero, List<ExpandedWardItem>> _wards = new Dictionary<Obj_AI_Hero, List<ExpandedWardItem>>();
		
		private int lastGameUpdateTime = 0;

        public class ExpandedWardItem : SAssemblies.Ward.WardItem
        {
            public int Stacks;
            public int Charges;
            public int Cd;

            public ExpandedWardItem(int id, string name, string spellName, int range, int duration, SAssemblies.Ward.WardType type, int stacks, int charges)
                : base(id, name, spellName, range, duration, type)
            {
                Stacks = stacks;
                Charges = charges;
            }

            public ExpandedWardItem(SAssemblies.Ward.WardItem ward, int stacks, int charges)
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
                if (hero.IsMe)
                {
                    List<ExpandedWardItem> wards = new List<ExpandedWardItem>();
					wards = GetWardItems(hero);
                    _wards.Add(hero, wards);
                }
            }
            //Game.OnGameUpdate += Game_OnGameUpdate;
            ThreadHelper.GetInstance().Called += Game_OnGameUpdate;
        }

        ~FowWardPlacement()
        {
            //Game.OnGameUpdate -= Game_OnGameUpdate;
            ThreadHelper.GetInstance().Called -= Game_OnGameUpdate;
        }

        public bool IsActive()
        {
#if WARDS
            return Ward.Wards.GetActive() && FowWardPlacementWard.GetActive();
#else
            return FowWardPlacementWard.GetActive();
#endif
        }

        private static void SetupMainMenu()
        {
            var menu = new LeagueSharp.Common.Menu("SAssembliesFowWardPlacement", "SAssembliesWardsFowWardPlacement", true);
            SetupMenu(menu);
            menu.AddToMainMenu();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            FowWardPlacementWard.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("WARDS_FOWWARDPLACEMENT_MAIN"), "SAssembliesWardsFowWardPlacement"));
            FowWardPlacementWard.MenuItems.Add(
                FowWardPlacementWard.Menu.AddItem(new MenuItem("SAssembliesWardsFowWardPlacementActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return FowWardPlacementWard;
        }
		
		private void Game_OnGameUpdate(object sender, EventArgs args)
        {
            if (!IsActive() || lastGameUpdateTime + new Random().Next(500, 1000) > Environment.TickCount)
                return;

            lastGameUpdateTime = Environment.TickCount;

            foreach (var enemy in _wards.ToArray())
            {
                Obj_AI_Hero hero = enemy.Key;
				List<ExpandedWardItem> allWards = new List<ExpandedWardItem>(GetWardItems(hero));

                List<ExpandedWardItem> soldWards = allWards.Except(enemy.Value, new ExpandedWardItemComparer()).ToList();
                foreach (var wardItem in soldWards)
                {
                    Game.PrintChat("{0} has used {1}", enemy.Key.ChampionName, wardItem.Name);
                }

                List<ExpandedWardItem> boughtWards = enemy.Value.Except(allWards, new ExpandedWardItemComparer()).ToList();
				foreach (var wardItem in boughtWards)
                {
                    Game.PrintChat("{0} got {1}", enemy.Key.ChampionName, wardItem.Name);
                }
				
				foreach (var item in allWards)
				{
					foreach (var wardItem in enemy.Value.ToArray())
					{
						if (item.Id == wardItem.Id && wardItem.Type != SAssemblies.Ward.WardType.Temp && wardItem.Type != SAssemblies.Ward.WardType.TempVision)
                        {
                            if ((item.Charges > 0 ? item.Charges < wardItem.Charges : false) || (item.Stacks < wardItem.Stacks)) //Check for StackItems etc fail
                            {
                                Game.PrintChat("{0} has used {1}", enemy.Key.ChampionName, wardItem.Name);
                            }
                        }
					}
				}

				foreach (var item in hero.InventoryItems)
                {
                    foreach (var wardItem in enemy.Value.ToArray())
                    {
                        if ((int)item.Id == wardItem.Id && ((item.Charges > wardItem.Charges || item.Stacks > wardItem.Stacks) && hero.Spellbook.GetSpell(item.SpellSlot).CooldownExpires <= Game.Time) &&
                            wardItem.Type != SAssemblies.Ward.WardType.Temp && wardItem.Type != SAssemblies.Ward.WardType.TempVision)
                        {
                            Game.PrintChat("{0} got {1}", enemy.Key.ChampionName, wardItem.Name);
                        }
                    }
                    foreach (var ward in SAssemblies.Ward.WardItems)
                    {
                        if ((int)item.Id == ward.Id && ward.Type != SAssemblies.Ward.WardType.Temp && ward.Type != SAssemblies.Ward.WardType.TempVision &&
                            (enemy.Value.Find(wardItem => wardItem.Id == ward.Id) == null))
                        {
                            Game.PrintChat("{0} got {1}", enemy.Key.ChampionName, ward.Name);
                        }
                    }
                }

                _wards[enemy.Key] = allWards;
            }
        }
		
		private List<ExpandedWardItem> GetWardItems(Obj_AI_Hero hero)
        {
            List<ExpandedWardItem> wards = new List<ExpandedWardItem>();
            foreach (var item in hero.InventoryItems)
            {
                foreach (var wardItem in SAssemblies.Ward.WardItems)
                {
                    if ((int)item.Id == wardItem.Id && wardItem.Type != SAssemblies.Ward.WardType.Temp && wardItem.Type != SAssemblies.Ward.WardType.TempVision)
                    {
                        wards.Add(new ExpandedWardItem(wardItem, item.Stacks, item.Charges));
                    }
                }
            }
            return wards;
        }

        class ExpandedWardItemComparer : IEqualityComparer<ExpandedWardItem>
        {
            // Products are equal if their names and product numbers are equal.
            public bool Equals(ExpandedWardItem x, ExpandedWardItem y)
            {

                //Check whether the compared objects reference the same data.
                if (Object.ReferenceEquals(x, y)) return true;

                //Check whether any of the compared objects is null.
                if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                    return false;

                //Check whether the products' properties are equal.
                return x.Id == y.Id;
            }

            // If Equals() returns true for a pair of objects 
            // then GetHashCode() must return the same value for these objects.

            public int GetHashCode(ExpandedWardItem product)
            {
                //Check whether the object is null
                if (Object.ReferenceEquals(product, null)) return 0;

                //Get hash code for the Name field if it is not null.
                int hashProductName = product.Name == null ? 0 : product.Name.GetHashCode();

                //Get hash code for the Code field.
                int hashProductCode = product.Id.GetHashCode();

                //Calculate the hash code for the product.
                return hashProductName ^ hashProductCode;
            }

        }
    }
}
