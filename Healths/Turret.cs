using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SAwareness.Healths
{
    class Turret
    {
        public static Menu.MenuItemSettings TurretHealth = new Menu.MenuItemSettings(typeof(Turret));

        List<Health.HealthConf> healthConf = new List<Health.HealthConf>();
        private int lastGameUpdateTime = 0;

        public Turret()
        {
            InitTurrentHealth();
            //Game.OnGameUpdate += Game_OnGameUpdate;
            ThreadHelper.GetInstance().Called += Game_OnGameUpdate;
        }

        ~Turret()
        {
            //Game.OnGameUpdate -= Game_OnGameUpdate;
            ThreadHelper.GetInstance().Called -= Game_OnGameUpdate;
            healthConf = null;
        }

        public bool IsActive()
        {
            return Health.Healths.GetActive() && TurretHealth.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            TurretHealth.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("HEALTHS_TURRET_MAIN"), "SAwarenessHealthsTurret"));
            TurretHealth.MenuItems.Add(
                TurretHealth.Menu.AddItem(new MenuItem("SAwarenessHealthsTurretActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return TurretHealth;
        }

        void Game_OnGameUpdate(object sender, EventArgs args)
        {
            if (!IsActive() || lastGameUpdateTime + new Random().Next(500, 1000) > Environment.TickCount)
                return;

            lastGameUpdateTime = Environment.TickCount;

            foreach (Health.HealthConf health in healthConf.ToArray())
            {
                Obj_AI_Turret objAiTurret = health.Obj as Obj_AI_Turret;
                if (objAiTurret != null)
                {
                    if (objAiTurret.IsValid)
                    {
                        if (((objAiTurret.Health / objAiTurret.MaxHealth) * 100) > 75)
                            health.Text.Color = Color.LightGreen;
                        else if (((objAiTurret.Health / objAiTurret.MaxHealth) * 100) <= 75)
                            health.Text.Color = Color.LightYellow;
                        else if (((objAiTurret.Health / objAiTurret.MaxHealth) * 100) <= 50)
                            health.Text.Color = Color.Orange;
                        else if (((objAiTurret.Health / objAiTurret.MaxHealth) * 100) <= 25)
                            health.Text.Color = Color.IndianRed;
                    }
                    else
                    {
                        healthConf.Remove(health);
                    }
                }
            }
        }

        private void InitTurrentHealth() //TODO: Draw HP above BarPos
        {
            if (!IsActive())
                return;

            foreach (Obj_AI_Turret turret in ObjectManager.Get<Obj_AI_Turret>())
            {
                int health = 0;
                var mode =
                    Health.Healths.GetMenuItem("SAwarenessHealthsMode")
                        .GetValue<StringList>();
                Render.Text Text = new Render.Text(0, 0, "", 14, new ColorBGRA(Color4.White));
                Text.TextUpdate = delegate
                {
                    if (!turret.IsValid)
                        return "";
                    switch (mode.SelectedIndex)
                    {
                        case 0:
                            health = (int)((turret.Health / turret.MaxHealth) * 100);
                            break;

                        case 1:
                            health = (int)turret.Health;
                            break;
                    }
                    return health.ToString();
                };
                Text.PositionUpdate = delegate
                {
                    if (!turret.IsValid)
                        return new Vector2(0,0);
                    Vector2 pos = Drawing.WorldToMinimap(turret.Position);
                    return new Vector2(pos.X, pos.Y);
                };
                Text.VisibleCondition = sender =>
                {
                    if (!turret.IsValid)
                        return false;
                    return Health.Healths.GetActive() && TurretHealth.GetActive() && turret.IsValid && !turret.IsDead && turret.IsValid && turret.Health != 9999 &&
                    ((turret.Health / turret.MaxHealth) * 100) != 100;
                };
                Text.OutLined = true;
                Text.Centered = true;
                Text.Add();

                healthConf.Add(new Health.HealthConf(turret, Text));
            }
        }
    }
}
