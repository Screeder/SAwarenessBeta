using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAwareness.Ranges
{
    class Shop
    {
        public static Menu.MenuItemSettings ShopRange = new Menu.MenuItemSettings(typeof(Ranges.Shop));

        public Shop()
        {
            Drawing.OnDraw += Drawing_OnDraw;
        }

        ~Shop()
        {
            Drawing.OnDraw -= Drawing_OnDraw;
        }

        public bool IsActive()
        {
            return Range.Ranges.GetActive() && ShopRange.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            ShopRange.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("RANGES_SHOP_MAIN"), "SAwarenessRangesShop"));
            ShopRange.MenuItems.Add(
                ShopRange.Menu.AddItem(new MenuItem("SAwarenessRangesShopMode", Language.GetString("RANGES_ALL_MODE")).SetValue(new StringList(new[]
                {
                    Language.GetString("RANGES_ALL_MODE_ME"), 
                    Language.GetString("RANGES_ALL_MODE_ENEMY"), 
                    Language.GetString("RANGES_ALL_MODE_BOTH")
                }))));
            ShopRange.MenuItems.Add(
                ShopRange.Menu.AddItem(new MenuItem("SAwarenessRangesShopColorMe", Language.GetString("RANGES_ALL_COLORME")).SetValue(Color.MidnightBlue)));
            ShopRange.MenuItems.Add(
                ShopRange.Menu.AddItem(new MenuItem("SAwarenessRangesShopColorEnemy", Language.GetString("RANGES_ALL_COLORENEMY")).SetValue(Color.MidnightBlue)));
            ShopRange.MenuItems.Add(
                ShopRange.Menu.AddItem(new MenuItem("SAwarenessRangesShopActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return ShopRange;
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!IsActive())
                return;
            var mode = ShopRange.GetMenuItem("SAwarenessRangesShopMode").GetValue<StringList>();
            switch (mode.SelectedIndex)
            {
                case 0:
                    foreach (Obj_Shop shop in ObjectManager.Get<Obj_Shop>())
                    {
                        if (shop.IsValid && shop.IsValid && ObjectManager.Player.Team == shop.Team && shop.Position.IsOnScreen())
                        {
                            Utility.DrawCircle(shop.Position, 1250, ShopRange.GetMenuItem("SAwarenessRangesShopColorMe").GetValue<Color>());
                        }
                    }
                    break;
                case 1:
                    foreach (Obj_Shop shop in ObjectManager.Get<Obj_Shop>())
                    {
                        if (shop.IsValid && shop.IsValid && ObjectManager.Player.Team != shop.Team && shop.Position.IsOnScreen())
                        {
                            Utility.DrawCircle(shop.Position, 1250, ShopRange.GetMenuItem("SAwarenessRangesShopColorEnemy").GetValue<Color>());
                        }
                    }
                    break;
                case 2:
                    foreach (Obj_Shop shop in ObjectManager.Get<Obj_Shop>())
                    {
                        if (shop.IsValid && shop.IsValid)
                        {
                            if (ObjectManager.Player.Team == shop.Team && shop.Position.IsOnScreen())
                            {
                                Utility.DrawCircle(shop.Position, 1250, ShopRange.GetMenuItem("SAwarenessRangesShopColorMe").GetValue<Color>());
                            }
                            if (ObjectManager.Player.Team != shop.Team && shop.Position.IsOnScreen())
                            {
                                Utility.DrawCircle(shop.Position, 1250, ShopRange.GetMenuItem("SAwarenessRangesShopColorEnemy").GetValue<Color>());
                            }
                        }
                    }
                    break;
            }
        }
    }
}
