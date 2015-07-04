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
    class Turret
    {
        public static Menu.MenuItemSettings TurretRange = new Menu.MenuItemSettings(typeof(Turret));

        public Turret()
        {
            Drawing.OnDraw += Drawing_OnDraw;
        }

        ~Turret()
        {
            Drawing.OnDraw -= Drawing_OnDraw;
        }

        public bool IsActive()
        {
#if RANGES
            return Range.Ranges.GetActive() && TurretRange.GetActive();
#else
            return TurretRange.GetActive();
#endif
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            TurretRange.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("RANGES_TURRET_MAIN"), "SAssembliesRangesTurret"));
            TurretRange.MenuItems.Add(
                TurretRange.Menu.AddItem(new MenuItem("SAssembliesRangesTurretMode", Language.GetString("RANGES_ALL_MODE")).SetValue(new StringList(new[]
                {
                    Language.GetString("RANGES_ALL_MODE_ME"), 
                    Language.GetString("RANGES_ALL_MODE_ENEMY"), 
                    Language.GetString("RANGES_ALL_MODE_BOTH")
                }))));
            TurretRange.MenuItems.Add(
                TurretRange.Menu.AddItem(new MenuItem("SAssembliesRangesTurretColorMe", Language.GetString("RANGES_ALL_COLORME")).SetValue(Color.LawnGreen)));
            TurretRange.MenuItems.Add(
                TurretRange.Menu.AddItem(new MenuItem("SAssembliesRangesTurretColorEnemy", Language.GetString("RANGES_ALL_COLORENEMY")).SetValue(Color.DarkRed)));
            TurretRange.MenuItems.Add(
                TurretRange.Menu.AddItem(new MenuItem("SAssembliesRangesTurretRange", Language.GetString("RANGES_TURRET_RANGE")).SetValue(new Slider(2000, 10000, 0))));
            TurretRange.MenuItems.Add(
                TurretRange.Menu.AddItem(new MenuItem("SAssembliesRangesTurretActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return TurretRange;
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!IsActive())
                return;

            var mode = TurretRange.GetMenuItem("SAssembliesRangesTurretMode").GetValue<StringList>();
            switch (mode.SelectedIndex)
            {
                case 0:
                    foreach (Obj_AI_Turret turret in ObjectManager.Get<Obj_AI_Turret>())
                    {
                        if (turret.IsVisible && !turret.IsDead && !turret.IsEnemy && turret.IsValid && turret.Position.IsOnScreen() &&
                            ObjectManager.Player.ServerPosition.Distance(turret.ServerPosition) < TurretRange.GetMenuItem("SAssembliesRangesTurretRange").GetValue<Slider>().Value)
                        {
                            Utility.DrawCircle(turret.Position, 900f, TurretRange.GetMenuItem("SAssembliesRangesTurretColorMe").GetValue<Color>());
                        }
                    }
                    break;
                case 1:
                    foreach (Obj_AI_Turret turret in ObjectManager.Get<Obj_AI_Turret>())
                    {
                        if (turret.IsVisible && !turret.IsDead && turret.IsEnemy && turret.IsValid && turret.Position.IsOnScreen() &&
                            ObjectManager.Player.ServerPosition.Distance(turret.ServerPosition) < TurretRange.GetMenuItem("SAssembliesRangesTurretRange").GetValue<Slider>().Value)
                        {
                            Utility.DrawCircle(turret.Position, 900f, TurretRange.GetMenuItem("SAssembliesRangesTurretColorEnemy").GetValue<Color>());
                        }
                    }
                    break;
                case 2:
                    foreach (Obj_AI_Turret turret in ObjectManager.Get<Obj_AI_Turret>())
                    {
                        if (turret.IsVisible && !turret.IsDead && !turret.IsEnemy && turret.IsValid && turret.Position.IsOnScreen() &&
                            ObjectManager.Player.ServerPosition.Distance(turret.ServerPosition) < TurretRange.GetMenuItem("SAssembliesRangesTurretRange").GetValue<Slider>().Value)
                        {
                            Utility.DrawCircle(turret.Position, 900f, TurretRange.GetMenuItem("SAssembliesRangesTurretColorMe").GetValue<Color>());
                        }
                    }
                    foreach (Obj_AI_Turret turret in ObjectManager.Get<Obj_AI_Turret>())
                    {
                        if (turret.IsVisible && !turret.IsDead && turret.IsEnemy && turret.IsValid && turret.Position.IsOnScreen() &&
                            ObjectManager.Player.ServerPosition.Distance(turret.ServerPosition) < TurretRange.GetMenuItem("SAssembliesRangesTurretRange").GetValue<Slider>().Value)
                        {
                            Utility.DrawCircle(turret.Position, 900f, TurretRange.GetMenuItem("SAssembliesRangesTurretColorEnemy").GetValue<Color>());
                        }
                    }
                    break;
            }
        }
    }
}
