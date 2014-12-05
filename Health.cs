using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;

namespace SAwareness
{
    internal class Health
    {
        List<HealthConf> healthConf = new List<HealthConf>();

        public Health()
        {
            InitTurrentHealth();
            InitInhibitorHealth();
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        ~Health()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
            healthConf = null;
        }

        public bool IsActive()
        {
            return Menu.Health.GetActive();
        }

        void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive() || !Menu.InhibitorHealth.GetActive())
                return;

            foreach (HealthConf health in healthConf.ToArray())
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

        private void InitInhibitorHealth()
        {
            if (!IsActive())
                return;
            if (!Menu.InhibitorHealth.GetActive())
                return;
            var baseB = new List<Obj_Barracks>();

            foreach (Obj_Barracks inhibitor in baseB)
            {
                int health = 0;
                var mode =
                    Menu.Health.GetMenuItem("SAwarenessHealthMode")
                        .GetValue<StringList>();
                switch (mode.SelectedIndex)
                {
                    case 0:
                        health = (int)((inhibitor.Health / inhibitor.MaxHealth) * 100);
                        break;

                    case 1:
                        health = (int)inhibitor.Health;
                        break;
                }

                Render.Text Text = new Render.Text(0, 0, "", 14, new ColorBGRA(Color4.White));
                Text.TextUpdate = delegate
                {
                    return health.ToString();
                };
                Text.PositionUpdate = delegate
                {
                    Vector2 pos = Drawing.WorldToMinimap(inhibitor.Position);
                    return new Vector2(pos.X, pos.Y);
                };
                Text.VisibleCondition = sender =>
                {
                    return Menu.Health.GetActive() && Menu.InhibitorHealth.GetActive() && inhibitor.IsValid && !inhibitor.IsDead && inhibitor.IsValid && inhibitor.Health > 0.1f &&
                    ((inhibitor.Health / inhibitor.MaxHealth) * 100) != 100;
                };
                Text.OutLined = true;
                Text.Centered = true;
                Text.Add();

                healthConf.Add(new HealthConf(inhibitor, Text));
            }

            foreach (Obj_BarracksDampener inhibitor in ObjectManager.Get<Obj_BarracksDampener>())
            {
                int health = 0;
                var mode =
                    Menu.Health.GetMenuItem("SAwarenessHealthMode")
                        .GetValue<StringList>();
                switch (mode.SelectedIndex)
                {
                    case 0:
                        health = (int)((inhibitor.Health / inhibitor.MaxHealth) * 100);
                        break;

                    case 1:
                        health = (int)inhibitor.Health;
                        break;
                }

                Render.Text Text = new Render.Text(0, 0, "", 14, new ColorBGRA(Color4.White));
                Text.TextUpdate = delegate
                {
                    return health.ToString();
                };
                Text.PositionUpdate = delegate
                {
                    Vector2 pos = Drawing.WorldToMinimap(inhibitor.Position);
                    return new Vector2(pos.X, pos.Y);
                };
                Text.VisibleCondition = sender =>
                {
                    return Menu.Health.GetActive() && Menu.InhibitorHealth.GetActive() && inhibitor.IsValid && !inhibitor.IsDead && inhibitor.IsValid && inhibitor.Health > 0.1f &&
                    ((inhibitor.Health / inhibitor.MaxHealth) * 100) != 100;
                };
                Text.OutLined = true;
                Text.Centered = true;
                Text.Add(); 

                healthConf.Add(new HealthConf(inhibitor, Text));
            }
        }

        private void InitTurrentHealth() //TODO: Draw HP above BarPos
        {
            if (!IsActive())
                return;
            if (!Menu.TowerHealth.GetActive())
                return;

            foreach (Obj_AI_Turret turret in ObjectManager.Get<Obj_AI_Turret>())
            {
                int health = 0;
                var mode =
                    Menu.Health.GetMenuItem("SAwarenessHealthMode")
                        .GetValue<StringList>();
                switch (mode.SelectedIndex)
                {
                    case 0:
                        health = (int)((turret.Health / turret.MaxHealth) * 100);
                        break;

                    case 1:
                        health = (int)turret.Health;
                        break;
                }

                Render.Text Text = new Render.Text(0, 0, "", 14, new ColorBGRA(Color4.White));
                Text.TextUpdate = delegate
                {
                    return health.ToString();
                };
                Text.PositionUpdate = delegate
                {
                    Vector2 pos = Drawing.WorldToMinimap(turret.Position);
                    return new Vector2(pos.X, pos.Y);
                };
                Text.VisibleCondition = sender =>
                {
                    return Menu.Health.GetActive() && Menu.TowerHealth.GetActive() && turret.IsValid && !turret.IsDead && turret.IsValid && turret.Health != 9999 &&
                    ((turret.Health / turret.MaxHealth) * 100) != 100;
                };
                Text.OutLined = true;
                Text.Centered = true;
                Text.Add();

                healthConf.Add(new HealthConf(turret, Text));
            }
        }

        class HealthConf
        {
            public Object Obj;
            public Render.Text Text;

            public HealthConf(object obj, Render.Text text)
            {
                Obj = obj;
                Text = text;
            }
        }
    }
}
