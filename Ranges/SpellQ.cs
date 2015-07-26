﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAssemblies.Ranges
{
    class SpellQ
    {
        public static Menu.MenuItemSettings SpellQRange = new Menu.MenuItemSettings(typeof(SpellQ));

        public SpellQ()
        {
            Drawing.OnDraw += Drawing_OnDraw;
        }

        ~SpellQ()
        {
            Drawing.OnDraw -= Drawing_OnDraw;
        }

        public bool IsActive()
        {
#if RANGES
            return Range.Ranges.GetActive() && SpellQRange.GetActive();
#else
            return SpellQRange.GetActive();
#endif
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            SpellQRange.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("RANGES_SPELLQ_MAIN"), "SAssembliesRangesSpellQ"));
            SpellQRange.MenuItems.Add(
                SpellQRange.Menu.AddItem(new MenuItem("SAssembliesRangesSpellQMode", Language.GetString("RANGES_ALL_MODE")).SetValue(new StringList(new[]
                {
                    Language.GetString("RANGES_ALL_MODE_ME"), 
                    Language.GetString("RANGES_ALL_MODE_ENEMY"), 
                    Language.GetString("RANGES_ALL_MODE_BOTH")
                }))));
            SpellQRange.MenuItems.Add(
                SpellQRange.Menu.AddItem(new MenuItem("SAssembliesRangesSpellQColorMe", Language.GetString("RANGES_ALL_COLORME")).SetValue(Color.LawnGreen)));
            SpellQRange.MenuItems.Add(
                SpellQRange.Menu.AddItem(new MenuItem("SAssembliesRangesSpellQColorEnemy", Language.GetString("RANGES_ALL_COLORENEMY")).SetValue(Color.IndianRed)));
            SpellQRange.MenuItems.Add(SpellQRange.CreateActiveMenuItem("SAssembliesRangesSpellQActive", () => new SpellQ()));
            return SpellQRange;
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!IsActive())
                return;

            var mode = SpellQRange.GetMenuItem("SAssembliesRangesSpellQMode").GetValue<StringList>();
            switch (mode.SelectedIndex)
            {
                case 0:
                    if (ObjectManager.Player.Position.IsOnScreen())
                    {
                        Utility.DrawCircle(ObjectManager.Player.Position,
                            ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).SData.CastRange, SpellQRange.GetMenuItem("SAssembliesRangesSpellQColorMe").GetValue<Color>());
                    }
                    break;
                case 1:
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead && enemy.Position.IsOnScreen())
                        {
                            Utility.DrawCircle(enemy.Position,
                                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).SData.CastRange, SpellQRange.GetMenuItem("SAssembliesRangesSpellQColorEnemy").GetValue<Color>());
                        }
                    }
                    break;
                case 2:
                    if (ObjectManager.Player.Position.IsOnScreen())
                    {
                        Utility.DrawCircle(ObjectManager.Player.Position,
                            ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).SData.CastRange, SpellQRange.GetMenuItem("SAssembliesRangesSpellQColorMe").GetValue<Color>());
                    }
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead && enemy.Position.IsOnScreen())
                        {
                            Utility.DrawCircle(enemy.Position,
                                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).SData.CastRange, SpellQRange.GetMenuItem("SAssembliesRangesSpellQColorEnemy").GetValue<Color>());
                        }
                    }
                    break;
            }
        }
    }
}
