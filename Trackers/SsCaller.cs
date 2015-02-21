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
    class SsCaller
    {
        public static Menu.MenuItemSettings SsCallerTracker = new Menu.MenuItemSettings(typeof(SsCaller));

        public static Dictionary<Obj_AI_Hero, Time> Enemies = new Dictionary<Obj_AI_Hero, Time>();
        private int lastGameUpdateTime = 0;

        public SsCaller()
        {
            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsEnemy)
                {
                    Enemies.Add(hero, new Time());
                }
            }
            //Game.OnGameUpdate += Game_OnGameUpdate;
            ThreadHelper.GetInstance().Called += Game_OnGameUpdate;
            Drawing.OnEndScene += Drawing_OnEndScene;
            Obj_AI_Base.OnTeleport += Obj_AI_Base_OnTeleport;
        }

        ~SsCaller()
        {
            //Game.OnGameUpdate -= Game_OnGameUpdate;
            ThreadHelper.GetInstance().Called -= Game_OnGameUpdate;
            Drawing.OnEndScene -= Drawing_OnEndScene;
            Obj_AI_Base.OnTeleport -= Obj_AI_Base_OnTeleport;
            Enemies = null;
        }

        public bool IsActive()
        {
            return Tracker.Trackers.GetActive() && SsCallerTracker.GetActive() &&
                   Game.Time < (SsCallerTracker.GetMenuItem("SAwarenessTrackersSsCallerDisableTime").GetValue<Slider>().Value * 60);
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            SsCallerTracker.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("TRACKERS_SSCALLER_MAIN"), "SAwarenessTrackersSsCaller"));
            SsCallerTracker.MenuItems.Add(
                SsCallerTracker.Menu.AddItem(new MenuItem("SAwarenessTrackersSsCallerPingTimes", Language.GetString("GLOBAL_PING_TIMES")).SetValue(new Slider(0, 5, 0))));
            SsCallerTracker.MenuItems.Add(
                SsCallerTracker.Menu.AddItem(new MenuItem("SAwarenessTrackersSsCallerPingType", Language.GetString("GLOBAL_PING_TYPE")).SetValue(
                        new StringList(new[]
                        {
                            Language.GetString("GLOBAL_PING_TYPE_NORMAL"), 
                            Language.GetString("GLOBAL_PING_TYPE_DANGER"), 
                            Language.GetString("GLOBAL_PING_TYPE_ENEMYMISSING"), 
                            Language.GetString("GLOBAL_PING_TYPE_ONMYWAY"), 
                            Language.GetString("GLOBAL_PING_TYPE_FALLBACK"), 
                            Language.GetString("GLOBAL_PING_ASSISTME") 
                        }))));
            SsCallerTracker.MenuItems.Add(
                SsCallerTracker.Menu.AddItem(new MenuItem("SAwarenessTrackersSsCallerLocalPing", Language.GetString("GLOBAL_PING_LOCAL")).SetValue(false)));
            SsCallerTracker.MenuItems.Add(
                SsCallerTracker.Menu.AddItem(new MenuItem("SAwarenessTrackersSsCallerChatChoice", Language.GetString("GLOBAL_CHAT_CHOICE")).SetValue(new StringList(new[] 
                { 
                    Language.GetString("GLOBAL_CHAT_CHOICE_NONE"),  
                    Language.GetString("GLOBAL_CHAT_CHOICE_LOCAL"), 
                    Language.GetString("GLOBAL_CHAT_CHOICE_SERVER")
                }))));
            SsCallerTracker.MenuItems.Add(
                SsCallerTracker.Menu.AddItem(new MenuItem("SAwarenessTrackersSsCallerDisableTime", Language.GetString("TRACKERS_SSCALLER_DISABLETIME")).SetValue(new Slider(20, 180, 1))));
            SsCallerTracker.MenuItems.Add(
                SsCallerTracker.Menu.AddItem(new MenuItem("SAwarenessTrackersSsCallerCircleRange", Language.GetString("TRACKERS_SSCALLER_CIRCLE_RANGE")).SetValue(new Slider(2000, 15000, 100))));
            SsCallerTracker.MenuItems.Add(
                SsCallerTracker.Menu.AddItem(new MenuItem("SAwarenessTrackersSsCallerCircleActive", Language.GetString("TRACKERS_SSCALLER_CIRCLE_ACTIVE")).SetValue(false)));
            SsCallerTracker.MenuItems.Add(
                SsCallerTracker.Menu.AddItem(new MenuItem("SAwarenessTrackersSsCallerSpeech", Language.GetString("GLOBAL_VOICE")).SetValue(false)));
            SsCallerTracker.MenuItems.Add(
                SsCallerTracker.Menu.AddItem(new MenuItem("SAwarenessTrackersSsCallerActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return SsCallerTracker;
        }

        void Drawing_OnEndScene(EventArgs args)
        {
            foreach (var enemy in Enemies)
            {
                Obj_AI_Hero hero = enemy.Key;
                if (!hero.IsVisible && !hero.IsDead)
                {
                    if (SsCallerTracker.GetMenuItem("SAwarenessTrackersSsCallerCircleActive").GetValue<bool>() && enemy.Value.Teleport.Status != Packet.S2C.Teleport.Status.Start)
                    {
                        float radius = Math.Abs(enemy.Value.LastPosition.X - enemy.Value.PredictedPosition.X);
                        if (radius < SsCallerTracker.GetMenuItem("SAwarenessTrackersSsCallerCircleRange").GetValue<Slider>().Value)
                        {
                            Utility.DrawCircle(enemy.Value.LastPosition, radius, System.Drawing.Color.Goldenrod, 1, 30, true);
                            if (enemy.Value.LastPosition.IsOnScreen())
                            {
                                Utility.DrawCircle(enemy.Value.LastPosition, radius, System.Drawing.Color.Goldenrod);
                            }
                        } 
                        else if (radius >= SsCallerTracker.GetMenuItem("SAwarenessTrackersSsCallerCircleRange").GetValue<Slider>().Value)
                        {
                            radius = SsCallerTracker.GetMenuItem("SAwarenessTrackersSsCallerCircleRange").GetValue<Slider>().Value;
                            Utility.DrawCircle(enemy.Value.LastPosition, radius, System.Drawing.Color.Goldenrod, 1, 30, true);
                            if (enemy.Value.LastPosition.IsOnScreen())
                            {
                                Utility.DrawCircle(enemy.Value.LastPosition, radius, System.Drawing.Color.Goldenrod);
                            }
                        }
                    }
                }
            }
        }

        private void Game_OnGameUpdate(object sender, EventArgs args)
        {
            if (!IsActive() || lastGameUpdateTime + new Random().Next(10, 50) > Environment.TickCount)
                return;

            lastGameUpdateTime = Environment.TickCount;
            foreach (var enemy in Enemies)
            {
                UpdateTime(enemy);
                HandleSs(enemy);
            }
        }

        private void HandleSs(KeyValuePair<Obj_AI_Hero, Time> enemy)
        {
            Obj_AI_Hero hero = enemy.Key;
            if (enemy.Value.InvisibleTime > 5 && !enemy.Value.Called && Game.Time - enemy.Value.LastTimeCalled > 30)
            {
                var pos = new Vector2(hero.Position.X, hero.Position.Y);
                var pingType = Packet.PingType.Normal;
                var t = SsCallerTracker.GetMenuItem("SAwarenessTrackersSsCallerPingType").GetValue<StringList>();
                pingType = (Packet.PingType)t.SelectedIndex + 1;
                GamePacket gPacketT;
                for (int i = 0;
                    i < SsCallerTracker.GetMenuItem("SAwarenessTrackersSsCallerPingTimes").GetValue<Slider>().Value;
                    i++)
                {
                    if (SsCallerTracker.GetMenuItem("SAwarenessTrackersSsCallerLocalPing").GetValue<bool>())
                    {
                        gPacketT =
                            Packet.S2C.Ping.Encoded(new Packet.S2C.Ping.Struct(pos[0], pos[1], 0, 0, pingType));
                        gPacketT.Process();
                    }
                    else if (!SsCallerTracker.GetMenuItem("SAwarenessTrackersSsCallerLocalPing").GetValue<bool>() &&
                             Menu.GlobalSettings.GetMenuItem("SAwarenessGlobalSettingsServerChatPingActive")
                                 .GetValue<bool>())
                    {
                        gPacketT =
                            Packet.C2S.Ping.Encoded(new Packet.C2S.Ping.Struct(enemy.Value.LastPosition.X,
                                enemy.Value.LastPosition.Y, 0, pingType));
                        gPacketT.Send();
                    }
                }
                if (SsCallerTracker.GetMenuItem("SAwarenessTrackersSsCallerChatChoice").GetValue<StringList>().SelectedIndex == 1)
                {
                    Game.PrintChat("ss {0}", hero.ChampionName);
                }
                else if (
                    SsCallerTracker.GetMenuItem("SAwarenessTrackersSsCallerChatChoice").GetValue<StringList>().SelectedIndex ==
                    2 &&
                    Menu.GlobalSettings.GetMenuItem("SAwarenessGlobalSettingsServerChatPingActive").GetValue<bool>())
                {
                    Game.Say("ss {0}", hero.ChampionName);
                }
                if (SsCallerTracker.GetMenuItem("SAwarenessTrackersSsCallerSpeech").GetValue<bool>())
                {
                    Speech.Speak("Miss " + hero.ChampionName);
                }
                enemy.Value.LastTimeCalled = (int)Game.Time;
                enemy.Value.Called = true;
            }
        }

        private void UpdateTime(KeyValuePair<Obj_AI_Hero, Time> enemy)
        {
            Obj_AI_Hero hero = enemy.Key;
            if (hero.IsVisible)
            {
                Enemies[hero].InvisibleTime = 0;
                Enemies[hero].VisibleTime = (int)Game.Time;
                enemy.Value.Called = false;
                Enemies[hero].LastPosition = hero.ServerPosition;
            }
            else
            {
                if (Enemies[hero].VisibleTime != 0)
                {
                    Enemies[hero].InvisibleTime = (int)(Game.Time - Enemies[hero].VisibleTime);
                }
                else
                {
                    Enemies[hero].InvisibleTime = 0;
                }
                if (enemy.Value.Teleport.Status != Packet.S2C.Teleport.Status.Start)
                {
                    Enemies[hero].PredictedPosition = new Vector3(enemy.Value.LastPosition.X + ((Game.ClockTime - enemy.Value.VisibleTime) * hero.MoveSpeed), 
                        enemy.Value.LastPosition.Y, enemy.Value.LastPosition.Z);
                }
            }
            if (hero.ServerPosition != enemy.Value.LastPosition)
            {
                Enemies[hero].LastPosition = hero.ServerPosition;
                Enemies[hero].PredictedPosition = hero.ServerPosition;
            }
        }

        private void Obj_AI_Base_OnTeleport(GameObject sender, GameObjectTeleportEventArgs args)
        {
            Packet.S2C.Teleport.Struct teleport = Packet.S2C.Teleport.Decoded(sender, args);
            foreach (var enemy in Enemies)
            {
                if (teleport.UnitNetworkId == enemy.Key.NetworkId && teleport.Type == Packet.S2C.Teleport.Type.Recall)
                {
                    Enemies[enemy.Key].Teleport = teleport;
                    if (teleport.Status == Packet.S2C.Teleport.Status.Finish)
                    {
                        Vector3 spawnPos = ObjectManager.Get<GameObject>().First(spawnPoint => spawnPoint is Obj_SpawnPoint &&
                            spawnPoint.Team != ObjectManager.Player.Team).Position;
                        Enemies[enemy.Key].PredictedPosition = spawnPos;
                        Enemies[enemy.Key].LastPosition = spawnPos;
                        Enemies[enemy.Key].VisibleTime = (int)Game.ClockTime;
                    }
                }
            }
        }

        public class Time
        {
            public bool Called;
            public int InvisibleTime;
            public Vector3 LastPosition;
            public Vector3 PredictedPosition;
            public Packet.S2C.Teleport.Struct Teleport;
            public int LastTimeCalled;
            public int VisibleTime;
        }
    }
}
