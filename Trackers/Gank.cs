using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SAwareness.Trackers
{
    class Gank
    {
        public static Menu.MenuItemSettings GankTracker = new Menu.MenuItemSettings(typeof(Gank));

        private Dictionary<Obj_AI_Hero, InternalGankTracker> _enemies = new Dictionary<Obj_AI_Hero, InternalGankTracker>();
        private int lastGameUpdateTime = 0;

        public Gank()
        {
            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsEnemy)
                {
                    Render.Line line = new Render.Line(new Vector2(0,0), new Vector2(0,0), 2, Color.LightGreen);
                    InternalGankTracker gank = new InternalGankTracker(line);
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
                        return Tracker.Trackers.GetActive() && GankTracker.GetActive() &&
                                GankTracker.GetMenuItem("SAwarenessTrackersGankDraw").GetValue<bool>() &&
                               hero.ServerPosition.Distance(ObjectManager.Player.ServerPosition) <
                               GankTracker.GetMenuItem("SAwarenessTrackersGankTrackRange").GetValue<Slider>().Value &&
                               hero.IsVisible && !hero.IsDead &&
                               (GankTracker.GetMenuItem("SAwarenessTrackersGankKillable").GetValue<bool>() && gank.Damage > hero.Health ||
                               !GankTracker.GetMenuItem("SAwarenessTrackersGankKillable").GetValue<bool>());
                    };
                    line.Add();
                    _enemies.Add(hero, gank);
                }
            }
            ThreadHelper.GetInstance().Called += Game_OnGameUpdate;
            //Game.OnGameUpdate += Game_OnGameUpdate;
        }

        ~Gank()
        {
            ThreadHelper.GetInstance().Called -= Game_OnGameUpdate;
            //Game.OnGameUpdate -= Game_OnGameUpdate;
            _enemies = null;
        }

        public bool IsActive()
        {
            return Tracker.Trackers.GetActive() && GankTracker.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            GankTracker.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("TRACKERS_GANK_MAIN"), "SAwarenessTrackersGank"));
            GankTracker.MenuItems.Add(
                GankTracker.Menu.AddItem(new MenuItem("SAwarenessTrackersGankTrackRange", Language.GetString("TRACKERS_GANK_RANGE")).SetValue(new Slider(1, 20000, 1))));
            GankTracker.MenuItems.Add(
               GankTracker.Menu.AddItem(new MenuItem("SAwarenessTrackersGankKillable", Language.GetString("TRACKERS_GANK_KILLABLE")).SetValue(false)));
            GankTracker.MenuItems.Add(
               GankTracker.Menu.AddItem(new MenuItem("SAwarenessTrackersGankDraw", Language.GetString("TRACKERS_GANK_LINES")).SetValue(false)));
            GankTracker.MenuItems.Add(
               GankTracker.Menu.AddItem(new MenuItem("SAwarenessTrackersGankPing", Language.GetString("TRACKERS_GANK_PING")).SetValue(false)));
            GankTracker.MenuItems.Add(
                GankTracker.Menu.AddItem(new MenuItem("SAwarenessTrackersGankVoice", Language.GetString("GLOBAL_VOICE")).SetValue(false)));
            GankTracker.MenuItems.Add(
                GankTracker.Menu.AddItem(new MenuItem("SAwarenessTrackersGankActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return GankTracker;
        }

        private void Game_OnGameUpdate(object sender, EventArgs args)
        {
            if (!IsActive() || lastGameUpdateTime + new Random().Next(500, 1000) > Environment.TickCount)
                return;

            lastGameUpdateTime = Environment.TickCount;
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
                if (enemy.Value.Damage < enemy.Key.Health && !GankTracker.GetMenuItem("SAwarenessTrackersGankKillable").GetValue<bool>())
                {
                    _enemies[enemy.Key].Line.Color = Color.GreenYellow;
                }
                else if (enemy.Key.Health/enemy.Key.MaxHealth < 0.1)
                {
                    _enemies[enemy.Key].Line.Color = Color.Red;
                    if (!_enemies[enemy.Key].Pinged)
                    {
                        if (GankTracker.GetMenuItem("SAwarenessTrackersGankVoice").GetValue<bool>())
                        {
                            Speech.Speak("Gankable " + enemy.Key.ChampionName);
                        }
                    }
                    if (!_enemies[enemy.Key].Pinged && GankTracker.GetMenuItem("SAwarenessTrackersGankPing").GetValue<bool>())
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
}
