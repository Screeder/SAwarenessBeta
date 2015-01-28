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
    class Turret
    {
        public static Menu.MenuItemSettings TurretRange = new Menu.MenuItemSettings(typeof(Ranges.Turret));

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
            return Range.Ranges.GetActive() && TurretRange.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            TurretRange.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("RANGES_TURRET_MAIN"), "SAwarenessRangesTurret"));
            TurretRange.MenuItems.Add(
                TurretRange.Menu.AddItem(new MenuItem("SAwarenessRangesTurretMode", Language.GetString("RANGES_ALL_MODE")).SetValue(new StringList(new[]
                {
                    Language.GetString("RANGES_ALL_MODE_ME"), 
                    Language.GetString("RANGES_ALL_MODE_ENEMY"), 
                    Language.GetString("RANGES_ALL_MODE_BOTH")
                }))));
            TurretRange.MenuItems.Add(
                TurretRange.Menu.AddItem(new MenuItem("SAwarenessRangesTurretColorMe", Language.GetString("RANGES_ALL_COLORME")).SetValue(Color.LawnGreen)));
            TurretRange.MenuItems.Add(
                TurretRange.Menu.AddItem(new MenuItem("SAwarenessRangesTurretColorEnemy", Language.GetString("RANGES_ALL_COLORENEMY")).SetValue(Color.DarkRed)));
            TurretRange.MenuItems.Add(
                TurretRange.Menu.AddItem(new MenuItem("SAwarenessRangesTurretRange", Language.GetString("RANGES_TURRET_RANGE")).SetValue(new Slider(2000, 10000, 0))));
            TurretRange.MenuItems.Add(
                TurretRange.Menu.AddItem(new MenuItem("SAwarenessRangesTurretActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return TurretRange;
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!IsActive())
                return;

            var mode = TurretRange.GetMenuItem("SAwarenessRangesTurretMode").GetValue<StringList>();
            switch (mode.SelectedIndex)
            {
                case 0:
                    foreach (Obj_AI_Turret turret in ObjectManager.Get<Obj_AI_Turret>())
                    {
                        if (turret.IsVisible && !turret.IsDead && !turret.IsEnemy && turret.IsValid && turret.Position.IsOnScreen() &&
                            ObjectManager.Player.ServerPosition.Distance(turret.ServerPosition) < TurretRange.GetMenuItem("SAwarenessRangesTurretRange").GetValue<Slider>().Value)
                        {
                            Utility.DrawCircle(turret.Position, 900f, TurretRange.GetMenuItem("SAwarenessRangesTurretColorMe").GetValue<Color>());
                        }
                    }
                    break;
                case 1:
                    foreach (Obj_AI_Turret turret in ObjectManager.Get<Obj_AI_Turret>())
                    {
                        if (turret.IsVisible && !turret.IsDead && turret.IsEnemy && turret.IsValid && turret.Position.IsOnScreen() &&
                            ObjectManager.Player.ServerPosition.Distance(turret.ServerPosition) < TurretRange.GetMenuItem("SAwarenessRangesTurretRange").GetValue<Slider>().Value)
                        {
                            Utility.DrawCircle(turret.Position, 900f, TurretRange.GetMenuItem("SAwarenessRangesTurretColorEnemy").GetValue<Color>());
                        }
                    }
                    break;
                case 2:
                    foreach (Obj_AI_Turret turret in ObjectManager.Get<Obj_AI_Turret>())
                    {
                        if (turret.IsVisible && !turret.IsDead && !turret.IsEnemy && turret.IsValid && turret.Position.IsOnScreen() &&
                            ObjectManager.Player.ServerPosition.Distance(turret.ServerPosition) < TurretRange.GetMenuItem("SAwarenessRangesTurretRange").GetValue<Slider>().Value)
                        {
                            Utility.DrawCircle(turret.Position, 900f, TurretRange.GetMenuItem("SAwarenessRangesTurretColorMe").GetValue<Color>());
                        }
                    }
                    foreach (Obj_AI_Turret turret in ObjectManager.Get<Obj_AI_Turret>())
                    {
                        if (turret.IsVisible && !turret.IsDead && turret.IsEnemy && turret.IsValid && turret.Position.IsOnScreen() &&
                            ObjectManager.Player.ServerPosition.Distance(turret.ServerPosition) < TurretRange.GetMenuItem("SAwarenessRangesTurretRange").GetValue<Slider>().Value)
                        {
                            Utility.DrawCircle(turret.Position, 900f, TurretRange.GetMenuItem("SAwarenessRangesTurretColorEnemy").GetValue<Color>());
                        }
                    }
                    break;
            }
        }
    }
}
