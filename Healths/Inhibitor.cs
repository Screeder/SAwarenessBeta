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
    class Inhibitor
    {
        public static Menu.MenuItemSettings InhibitorHealth = new Menu.MenuItemSettings(typeof(Inhibitor));

        List<Health.HealthConf> healthConf = new List<Health.HealthConf>();
        private int lastGameUpdateTime = 0;

        public Inhibitor()
        {
            InitInhibitorHealth();
            //Game.OnGameUpdate += Game_OnGameUpdate;
            ThreadHelper.GetInstance().Called += Game_OnGameUpdate;
        }

        ~Inhibitor()
        {
            //Game.OnGameUpdate -= Game_OnGameUpdate;
            ThreadHelper.GetInstance().Called -= Game_OnGameUpdate;
            healthConf = null;
        }

        public bool IsActive()
        {
            return Health.Healths.GetActive() && InhibitorHealth.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            InhibitorHealth.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("HEALTHS_INHIBITOR_MAIN"), "SAwarenessHealthsInhibitor"));
            InhibitorHealth.MenuItems.Add(
                InhibitorHealth.Menu.AddItem(new MenuItem("SAwarenessHealthsInhibitorActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return InhibitorHealth;
        }

        void Game_OnGameUpdate(object sender, EventArgs args)
        {
            if (!IsActive() || lastGameUpdateTime + new Random().Next(500, 1000) > Environment.TickCount)
                return;

            lastGameUpdateTime = Environment.TickCount;

            foreach (Health.HealthConf health in healthConf.ToArray())
            {
                Obj_BarracksDampener objBarracks = health.Obj as Obj_BarracksDampener;
                if (objBarracks != null)
                {
                    if (objBarracks.IsValid)
                    {
                        if (((objBarracks.Health / objBarracks.MaxHealth) * 100) > 75)
                            health.Text.Color = Color.LightGreen;
                        else if (((objBarracks.Health / objBarracks.MaxHealth) * 100) <= 75)
                            health.Text.Color = Color.LightYellow;
                        else if (((objBarracks.Health / objBarracks.MaxHealth) * 100) <= 50)
                            health.Text.Color = Color.Orange;
                        else if (((objBarracks.Health / objBarracks.MaxHealth) * 100) <= 25)
                            health.Text.Color = Color.IndianRed;
                    }
                    else
                    {
                        healthConf.Remove(health);
                    }
                }
            }
        }

        private void InitInhibitorHealth()
        {
            if (!IsActive())
                return;
            var baseB = new List<Obj_Barracks>();

            foreach (Obj_Barracks inhibitor in baseB)
            {
                int health = 0;
                var mode =
                    Health.Healths.GetMenuItem("SAwarenessHealthsMode")
                        .GetValue<StringList>();
                Render.Text Text = new Render.Text(0, 0, "", 14, new ColorBGRA(Color4.White));
                Text.TextUpdate = delegate
                {
                    if (!inhibitor.IsValid)
                        return "";
                    switch (mode.SelectedIndex)
                    {
                        case 0:
                            health = (int)((inhibitor.Health / inhibitor.MaxHealth) * 100);
                            break;

                        case 1:
                            health = (int)inhibitor.Health;
                            break;
                    }
                    return health.ToString();
                };
                Text.PositionUpdate = delegate
                {
                    if (!inhibitor.IsValid)
                        return new Vector2(0, 0);
                    Vector2 pos = Drawing.WorldToMinimap(inhibitor.Position);
                    return new Vector2(pos.X, pos.Y);
                };
                Text.VisibleCondition = sender =>
                {
                    if (!inhibitor.IsValid)
                        return false;
                    return Health.Healths.GetActive() && InhibitorHealth.GetActive() && inhibitor.IsValid && !inhibitor.IsDead && inhibitor.IsValid && inhibitor.Health > 0.1f &&
                    ((inhibitor.Health / inhibitor.MaxHealth) * 100) != 100;
                };
                Text.OutLined = true;
                Text.Centered = true;
                Text.Add();

                healthConf.Add(new Health.HealthConf(inhibitor, Text));
            }

            foreach (Obj_BarracksDampener inhibitor in ObjectManager.Get<Obj_BarracksDampener>())
            {
                int health = 0;
                var mode =
                    Health.Healths.GetMenuItem("SAwarenessHealthsMode")
                        .GetValue<StringList>();
                Render.Text Text = new Render.Text(0, 0, "", 14, new ColorBGRA(Color4.White));
                Text.TextUpdate = delegate
                {
                    if (!inhibitor.IsValid)
                        return "";
                    switch (mode.SelectedIndex)
                    {
                        case 0:
                            health = (int)((inhibitor.Health / inhibitor.MaxHealth) * 100);
                            break;

                        case 1:
                            health = (int)inhibitor.Health;
                            break;
                    }
                    return health.ToString();
                };
                Text.PositionUpdate = delegate
                {
                    if (!inhibitor.IsValid)
                        return new Vector2(0, 0);
                    Vector2 pos = Drawing.WorldToMinimap(inhibitor.Position);
                    return new Vector2(pos.X, pos.Y);
                };
                Text.VisibleCondition = sender =>
                {
                    if (!inhibitor.IsValid)
                        return false;
                    return Health.Healths.GetActive() && InhibitorHealth.GetActive() && inhibitor.IsValid && !inhibitor.IsDead && inhibitor.IsValid && inhibitor.Health > 0.1f &&
                    ((inhibitor.Health / inhibitor.MaxHealth) * 100) != 100;
                };
                Text.OutLined = true;
                Text.Centered = true;
                Text.Add();

                healthConf.Add(new Health.HealthConf(inhibitor, Text));
            }
        }
    }
}
