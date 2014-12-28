using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;

namespace SAwareness
{
    internal class GankPotentialTracker
    {
        private Dictionary<Obj_AI_Hero, InternalGankTracker> _enemies = new Dictionary<Obj_AI_Hero, InternalGankTracker>();

        public GankPotentialTracker()
        {
            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsEnemy)
                {
                    Render.Line line = new Render.Line(new Vector2(0,0), new Vector2(0,0), 2, Color.LightGreen);
                    line.StartPositionUpdate = delegate
                    {
                        return Drawing.WorldToScreen(ObjectManager.Player.Position);
                    };
                    line.EndPositionUpdate = delegate
                    {
                        return Drawing.WorldToScreen(hero.Position);
                    };
                    line.VisibleCondition = delegate
                    {
                        return Menu.GankTracker.GetMenuItem("SAwarenessGankTrackerDraw").GetValue<bool>();
                    };
                    _enemies.Add(hero, new InternalGankTracker(line));
                }
            }
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        ~GankPotentialTracker()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
            _enemies = null;
        }

        public bool IsActive()
        {
            return Menu.Ganks.GetActive() && Menu.GankTracker.GetActive();
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive())
                return;
            Obj_AI_Hero player = ObjectManager.Player;
            foreach (var enemy in _enemies.ToList())
            {
                double dmg = 0;
                try
                {
                    if (player.Spellbook.CanUseSpell(SpellSlot.Q) == SpellState.Ready)
                        dmg += player.GetSpellDamage(enemy.Key, SpellSlot.Q);
                }
                catch (InvalidOperationException)
                {
                }
                try
                {
                    if (player.Spellbook.CanUseSpell(SpellSlot.W) == SpellState.Ready)
                        dmg += player.GetSpellDamage(enemy.Key, SpellSlot.W);
                }
                catch (InvalidOperationException)
                {
                }
                try
                {
                    if (player.Spellbook.CanUseSpell(SpellSlot.E) == SpellState.Ready)
                        dmg += player.GetSpellDamage(enemy.Key, SpellSlot.E);
                }
                catch (InvalidOperationException)
                {
                }
                try
                {
                    if (player.Spellbook.CanUseSpell(SpellSlot.R) == SpellState.Ready)
                        dmg += player.GetSpellDamage(enemy.Key, SpellSlot.R);
                }
                catch (InvalidOperationException)
                {
                }
                try
                {
                    dmg += player.GetAutoAttackDamage(enemy.Key);
                }
                catch (InvalidOperationException)
                {
                }
                _enemies[enemy.Key].Damage = dmg;
                if (enemy.Value.Damage > enemy.Key.Health)
                {
                    _enemies[enemy.Key].Line.Color = Color.OrangeRed;
                }
                if (enemy.Value.Damage < enemy.Key.Health && !Menu.GankTracker.GetMenuItem("SAwarenessGankTrackerKillable").GetValue<bool>())
                {
                    _enemies[enemy.Key].Line.Color = Color.GreenYellow;
                }
                else if (enemy.Key.Health/enemy.Key.MaxHealth < 0.1)
                {
                    _enemies[enemy.Key].Line.Color = Color.Red;
                    if (!_enemies[enemy.Key].Pinged && Menu.GankTracker.GetMenuItem("SAwarenessGankTrackerPing").GetValue<bool>())
                    {
                        Packet.S2C.Ping.Encoded(new Packet.S2C.Ping.Struct(enemy.Key.ServerPosition[0],
                            enemy.Key.ServerPosition[1], 0, 0, Packet.PingType.Normal)).Process();
                        _enemies[enemy.Key].Pinged = true;
                    }
                }
                else if (enemy.Key.Health / enemy.Key.MaxHealth > 0.1)
                {
                    _enemies[enemy.Key].Pinged = false;
                }
            }
        }

        public class InternalGankTracker
        {
            public double Damage = 0;
            public Render.Line Line;
            public bool Pinged = false;

            public InternalGankTracker(Render.Line line)
            {
                Line = line;
            }
        }
    }

    internal class GankDetector
    {
        private static Dictionary<Obj_AI_Hero, InternalGankDetector> Enemies = new Dictionary<Obj_AI_Hero, InternalGankDetector>();

        public GankDetector()
        {
            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsEnemy)
                {
                    Render.Text text = new Render.Text(new Vector2(0,0), "Enemy jungler approaching", 28, Color.Red);
                    text.PositionUpdate = delegate
                    {
                        return Drawing.WorldToScreen(ObjectManager.Player.ServerPosition);
                    };
                    text.VisibleCondition = sender =>
                    {
                        bool hasSmite = false;
                        foreach (SpellDataInst spell in hero.Spellbook.Spells)
                        {
                            if (spell.Name.ToLower().Contains("smite"))
                            {
                                hasSmite = true;
                                break;
                            }
                        }
                        return IsActive() &&
                               Menu.GankDetector.GetMenuItem("SAwarenessGankDetectorShowJungler").GetValue<bool>() &&
                               hero.IsVisible && !hero.IsDead &&
                                Vector3.Distance(ObjectManager.Player.ServerPosition, hero.ServerPosition) >
                                Menu.GankDetector.GetMenuItem("SAwarenessGankDetectorTrackRangeMin").GetValue<Slider>().Value &&
                                Vector3.Distance(ObjectManager.Player.ServerPosition, hero.ServerPosition) <
                                Menu.GankDetector.GetMenuItem("SAwarenessGankDetectorTrackRangeMax").GetValue<Slider>().Value &&
                                hasSmite;
                    };
                    text.OutLined = true;
                    text.Centered = true;
                    Enemies.Add(hero, new InternalGankDetector(text));
                }
            }
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        ~GankDetector()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
            Enemies = null;
        }

        public bool IsActive()
        {
            return Menu.Ganks.GetActive() && Menu.GankDetector.GetActive() &&
                Game.Time < (Menu.GankDetector.GetMenuItem("SAwarenessGankDetectorDisableTime").GetValue<Slider>().Value * 60); ;
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive())
                return;
            foreach (var enemy in Enemies)
            {
                UpdateTime(enemy);
            }
        }

        private void ChatAndPing(KeyValuePair<Obj_AI_Hero, InternalGankDetector> enemy)
        {
            Obj_AI_Hero hero = enemy.Key;
            var pingType = Packet.PingType.Normal;
            var t = Menu.GankDetector.GetMenuItem("SAwarenessGankDetectorPingType").GetValue<StringList>();
            pingType = (Packet.PingType) t.SelectedIndex + 1;
            Vector3 pos = hero.ServerPosition;
            GamePacket gPacketT;
            for (int i = 0;
                i < Menu.GankDetector.GetMenuItem("SAwarenessGankDetectorPingTimes").GetValue<Slider>().Value;
                i++)
            {
                if (Menu.GankDetector.GetMenuItem("SAwarenessGankDetectorLocalPing").GetValue<bool>())
                {
                    gPacketT = Packet.S2C.Ping.Encoded(new Packet.S2C.Ping.Struct(pos[0], pos[1], 0, 0, pingType));
                    gPacketT.Process();
                }
                else if (!Menu.GankDetector.GetMenuItem("SAwarenessGankDetectorLocalPing").GetValue<bool>() &&
                         Menu.GlobalSettings.GetMenuItem("SAwarenessGlobalSettingsServerChatPingActive")
                             .GetValue<bool>())
                {
                    gPacketT = Packet.C2S.Ping.Encoded(new Packet.C2S.Ping.Struct(pos[0], pos[1], 0, pingType));
                    gPacketT.Send();
                }
            }

            if (
                Menu.GankDetector.GetMenuItem("SAwarenessGankDetectorChatChoice").GetValue<StringList>().SelectedIndex ==
                1)
            {
                Game.PrintChat("Gank: {0}", hero.ChampionName);
            }
            else if (
                Menu.GankDetector.GetMenuItem("SAwarenessGankDetectorChatChoice")
                    .GetValue<StringList>()
                    .SelectedIndex == 2 &&
                Menu.GlobalSettings.GetMenuItem("SAwarenessGlobalSettingsServerChatPingActive").GetValue<bool>())
            {
                Game.Say("Gank: {0}", hero.ChampionName);
            }

            //TODO: Check for Teleport etc.                    
        }

        private void HandleGank(KeyValuePair<Obj_AI_Hero, InternalGankDetector> enemy)
        {
            Obj_AI_Hero hero = enemy.Key;
            if (enemy.Value.Time.InvisibleTime > 5)
            {
                if (!enemy.Value.Time.CalledInvisible && hero.IsValid && !hero.IsDead && hero.IsVisible &&
                    Vector3.Distance(ObjectManager.Player.ServerPosition, hero.ServerPosition) >
                    Menu.GankDetector.GetMenuItem("SAwarenessGankDetectorTrackRangeMin").GetValue<Slider>().Value &&
                    Vector3.Distance(ObjectManager.Player.ServerPosition, hero.ServerPosition) <
                    Menu.GankDetector.GetMenuItem("SAwarenessGankDetectorTrackRangeMax").GetValue<Slider>().Value)
                {
                    ChatAndPing(enemy);
                    enemy.Value.Time.CalledInvisible = true;
                }
            }
            if (!enemy.Value.Time.CalledVisible && hero.IsValid && !hero.IsDead &&
                enemy.Key.GetWaypoints().Last().Distance(ObjectManager.Player.ServerPosition) >
                Menu.GankDetector.GetMenuItem("SAwarenessGankDetectorTrackRangeMin").GetValue<Slider>().Value &&
                enemy.Key.GetWaypoints().Last().Distance(ObjectManager.Player.ServerPosition) <
                Menu.GankDetector.GetMenuItem("SAwarenessGankDetectorTrackRangeMax").GetValue<Slider>().Value)
            {
                ChatAndPing(enemy);
                enemy.Value.Time.CalledVisible = true;
            }
        }

        private void UpdateTime(KeyValuePair<Obj_AI_Hero, InternalGankDetector> enemy)
        {
            Obj_AI_Hero hero = enemy.Key;
            if (!hero.IsValid)
                return;
            if (hero.IsVisible)
            {
                HandleGank(enemy);
                Enemies[hero].Time.InvisibleTime = 0;
                Enemies[hero].Time.VisibleTime = (int) Game.Time;
                enemy.Value.Time.CalledInvisible = false;
            }
            else
            {
                if (Enemies[hero].Time.VisibleTime != 0)
                {
                    Enemies[hero].Time.InvisibleTime = (int) (Game.Time - Enemies[hero].Time.VisibleTime);
                }
                else
                {
                    Enemies[hero].Time.InvisibleTime = 0;
                }
                enemy.Value.Time.CalledVisible = false;
            }
        }

        public class InternalGankDetector
        {
            public Time Time = new Time();
            public Render.Text Text;

            public InternalGankDetector(Render.Text text)
            {
                Text = text;
            }
        }

        public class Time
        {
            public bool CalledInvisible;
            public bool CalledVisible;
            public int InvisibleTime;
            public int VisibleTime;
        }
    }
}
