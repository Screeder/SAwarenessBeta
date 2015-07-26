using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAssemblies.Ranges
{
    class Attack
    {
        public static Menu.MenuItemSettings AttackRange = new Menu.MenuItemSettings(typeof(Attack));

        public Attack()
        {
            Drawing.OnDraw += Drawing_OnDraw;
        }

        ~Attack()
        {
            Drawing.OnDraw -= Drawing_OnDraw;
        }

        public bool IsActive()
        {
#if RANGES
            return Range.Ranges.GetActive() && AttackRange.GetActive();
#else
            return AttackRange.GetActive();
#endif
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            AttackRange.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("RANGES_ATTACK_MAIN"), "SAssembliesRangesAttack"));
            AttackRange.MenuItems.Add(
                AttackRange.Menu.AddItem(new MenuItem("SAssembliesRangesAttackMode", Language.GetString("RANGES_ALL_MODE")).SetValue(new StringList(new[]
                {
                    Language.GetString("RANGES_ALL_MODE_ME"), 
                    Language.GetString("RANGES_ALL_MODE_ENEMY"), 
                    Language.GetString("RANGES_ALL_MODE_BOTH")
                }))));
            AttackRange.MenuItems.Add(
                AttackRange.Menu.AddItem(new MenuItem("SAssembliesRangesAttackColorMe", Language.GetString("RANGES_ALL_COLORME")).SetValue(Color.LawnGreen)));
            AttackRange.MenuItems.Add(
                AttackRange.Menu.AddItem(new MenuItem("SAssembliesRangesAttackColorEnemy", Language.GetString("RANGES_ALL_COLORENEMY")).SetValue(Color.IndianRed)));
            AttackRange.MenuItems.Add(AttackRange.CreateActiveMenuItem("SAssembliesRangesAttackActive", () => new Attack()));
            return AttackRange;
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!IsActive())
                return;

            var mode = AttackRange.GetMenuItem("SAssembliesRangesAttackMode").GetValue<StringList>();
            switch (mode.SelectedIndex)
            {
                case 0:
                    if (ObjectManager.Player.Position.IsOnScreen())
                    {
                        Utility.DrawCircle(ObjectManager.Player.Position, ObjectManager.Player.AttackRange, AttackRange.GetMenuItem("SAssembliesRangesAttackColorMe").GetValue<Color>());
                    }
                    break;
                case 1:
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead && enemy.Position.IsOnScreen())
                        {
                            Utility.DrawCircle(enemy.Position, enemy.AttackRange, AttackRange.GetMenuItem("SAssembliesRangesAttackColorEnemy").GetValue<Color>());
                        }
                    }
                    break;
                case 2:
                    Utility.DrawCircle(ObjectManager.Player.Position, ObjectManager.Player.AttackRange, AttackRange.GetMenuItem("SAssembliesRangesAttackColorMe").GetValue<Color>());
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead && enemy.Position.IsOnScreen())
                        {
                            Utility.DrawCircle(enemy.Position, enemy.AttackRange, AttackRange.GetMenuItem("SAssembliesRangesAttackColorEnemy").GetValue<Color>());
                        }
                    }
                    break;
            }
        }
    }
}
