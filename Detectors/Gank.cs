using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SAwareness.Detectors
{
    class Gank
    {
        public static Menu.MenuItemSettings GankDetector = new Menu.MenuItemSettings(typeof(Gank));

        private static Dictionary<Obj_AI_Hero, InternalGankDetector> Enemies = new Dictionary<Obj_AI_Hero, InternalGankDetector>();
        private int lastGameUpdateTime = 0;

        public Gank()
        {
            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsEnemy)
                {
                    Render.Text text = new Render.Text(new Vector2(0, 0), Language.GetString("DETECTORS_GANK_TEXT_JUNGLER"), 28, Color.Red);
                    text.PositionUpdate = delegate
                    {
                        Speech.Speak(Language.GetString("DETECTORS_GANK_TEXT_JUNGLER"));
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
                               GankDetector.GetMenuItem("SAwarenessDetectorsGankShowJungler").GetValue<bool>() &&
                               hero.IsVisible && !hero.IsDead &&
                                Vector3.Distance(ObjectManager.Player.ServerPosition, hero.ServerPosition) >
                                GankDetector.GetMenuItem("SAwarenessDetectorsGankTrackRangeMin").GetValue<Slider>().Value &&
                                Vector3.Distance(ObjectManager.Player.ServerPosition, hero.ServerPosition) <
                                GankDetector.GetMenuItem("SAwarenessDetectorsGankTrackRangeMax").GetValue<Slider>().Value &&
                                hasSmite;
                    };
                    text.OutLined = true;
                    text.Centered = true;
                    Enemies.Add(hero, new InternalGankDetector(text));
                }
            }
            ThreadHelper.GetInstance().Called += Game_OnGameUpdate;
            //Game.OnGameUpdate += Game_OnGameUpdate;
        }

        ~Gank()
        {
            ThreadHelper.GetInstance().Called -= Game_OnGameUpdate;
            //Game.OnGameUpdate -= Game_OnGameUpdate;
            Enemies = null;
        }

        public bool IsActive()
        {
            return Detector.Detectors.GetActive() && GankDetector.GetActive() &&
                Game.Time < (GankDetector.GetMenuItem("SAwarenessDetectorsGankDisableTime").GetValue<Slider>().Value * 60); ;
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            GankDetector.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("DETECTORS_GANK_MAIN"), "SAwarenessDetectorsGank"));
            GankDetector.MenuItems.Add(
                GankDetector.Menu.AddItem(new MenuItem("SAwarenessDetectorsGankPingTimes", Language.GetString("GLOBAL_PING_TIMES")).SetValue(new Slider(0, 5, 0))));
            GankDetector.MenuItems.Add(
                GankDetector.Menu.AddItem(new MenuItem("SAwarenessDetectorsGankPingType", Language.GetString("GLOBAL_PING_TYPE")).SetValue(new StringList(new[] 
                { 
                    Language.GetString("GLOBAL_PING_TYPE_NORMAL"), 
                    Language.GetString("GLOBAL_PING_TYPE_DANGER"), 
                    Language.GetString("GLOBAL_PING_TYPE_ENEMYMISSING"), 
                    Language.GetString("GLOBAL_PING_TYPE_ONMYWAY"), 
                    Language.GetString("GLOBAL_PING_TYPE_FALLBACK"), 
                    Language.GetString("GLOBAL_PING_ASSISTME") 
                }))));
            GankDetector.MenuItems.Add(
                GankDetector.Menu.AddItem(new MenuItem("SAwarenessDetectorsGankLocalPing", Language.GetString("GLOBAL_PING_LOCAL")).SetValue(true)));
            GankDetector.MenuItems.Add(
                GankDetector.Menu.AddItem(new MenuItem("SAwarenessDetectorsGankChatChoice", Language.GetString("GLOBAL_CHAT_CHOICE")).SetValue(
                        new StringList(new[]
                        {
                            Language.GetString("GLOBAL_CHAT_CHOICE_NONE"), 
                            Language.GetString("GLOBAL_CHAT_CHOICE_LOCAL"), 
                            Language.GetString("GLOBAL_CHAT_CHOICE_SERVER")
                        }))));
            GankDetector.MenuItems.Add(
                GankDetector.Menu.AddItem(new MenuItem("SAwarenessDetectorsGankTrackRangeMin", Language.GetString("DETECTORS_GANK_RANGE_MIN")).SetValue(new Slider(1, 10000, 1))));
            GankDetector.MenuItems.Add(
                GankDetector.Menu.AddItem(new MenuItem("SAwarenessDetectorsGankTrackRangeMax", Language.GetString("DETECTORS_GANK_RANGE_MAX")).SetValue(new Slider(1, 10000, 1))));
            GankDetector.MenuItems.Add(
                GankDetector.Menu.AddItem(new MenuItem("SAwarenessDetectorsGankDisableTime", Language.GetString("DETECTORS_GANK_DISABLETIME")).SetValue(new Slider(20, 180, 1))));
            GankDetector.MenuItems.Add(
                GankDetector.Menu.AddItem(new MenuItem("SAwarenessDetectorsGankShowJungler", Language.GetString("DETECTORS_GANK_SHOWJUNGLER")).SetValue(false)));
            GankDetector.MenuItems.Add(
                GankDetector.Menu.AddItem(new MenuItem("SAwarenessDetectorsGankVoice", Language.GetString("GLOBAL_VOICE")).SetValue(false)));
            GankDetector.MenuItems.Add(
                GankDetector.Menu.AddItem(new MenuItem("SAwarenessDetectorsGankActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return GankDetector;
        }

        private void Game_OnGameUpdate(object sender, EventArgs args)
        {
            if (!IsActive() || lastGameUpdateTime + new Random().Next(500, 1000) > Environment.TickCount)
                return;

            lastGameUpdateTime = Environment.TickCount;
            foreach (var enemy in Enemies)
            {
                UpdateTime(enemy);
            }
        }

        private void ChatAndPing(KeyValuePair<Obj_AI_Hero, InternalGankDetector> enemy)
        {
            Obj_AI_Hero hero = enemy.Key;
            var pingType = Packet.PingType.Normal;
            var t = GankDetector.GetMenuItem("SAwarenessDetectorsGankPingType").GetValue<StringList>();
            pingType = (Packet.PingType)t.SelectedIndex + 1;
            Vector3 pos = hero.ServerPosition;
            GamePacket gPacketT;
            for (int i = 0;
                i < GankDetector.GetMenuItem("SAwarenessDetectorsGankPingTimes").GetValue<Slider>().Value;
                i++)
            {
                if (GankDetector.GetMenuItem("SAwarenessDetectorsGankLocalPing").GetValue<bool>())
                {
                    gPacketT = Packet.S2C.Ping.Encoded(new Packet.S2C.Ping.Struct(pos[0], pos[1], 0, 0, pingType));
                    gPacketT.Process();
                }
                else if (!GankDetector.GetMenuItem("SAwarenessDetectorsGankLocalPing").GetValue<bool>() &&
                         Menu.GlobalSettings.GetMenuItem("SAwarenessGlobalSettingsServerChatPingActive")
                             .GetValue<bool>())
                {
                    gPacketT = Packet.C2S.Ping.Encoded(new Packet.C2S.Ping.Struct(pos[0], pos[1], 0, pingType));
                    gPacketT.Send();
                }
            }

            if (
                GankDetector.GetMenuItem("SAwarenessDetectorsGankChatChoice").GetValue<StringList>().SelectedIndex ==
                1)
            {
                Game.PrintChat(Language.GetString("DETECTORS_GANK_TEXT") + ": {0}", hero.ChampionName);
            }
            else if (
                GankDetector.GetMenuItem("SAwarenessDetectorsGankChatChoice")
                    .GetValue<StringList>()
                    .SelectedIndex == 2 &&
                Menu.GlobalSettings.GetMenuItem("SAwarenessGlobalSettingsServerChatPingActive").GetValue<bool>())
            {
                Game.Say(Language.GetString("DETECTORS_GANK_TEXT") + ": {0}", hero.ChampionName);
            }
            if (GankDetector.GetMenuItem("SAwarenessDetectorsGankVoice").GetValue<bool>())
            {
                Speech.Speak(Language.GetString("DETECTORS_GANK_TEXT") + ": " + hero.ChampionName);
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
                    GankDetector.GetMenuItem("SAwarenessDetectorsGankTrackRangeMin").GetValue<Slider>().Value &&
                    Vector3.Distance(ObjectManager.Player.ServerPosition, hero.ServerPosition) <
                    GankDetector.GetMenuItem("SAwarenessDetectorsGankTrackRangeMax").GetValue<Slider>().Value)
                {
                    ChatAndPing(enemy);
                    enemy.Value.Time.CalledInvisible = true;
                }
            }
            if (!enemy.Value.Time.CalledVisible && hero.IsValid && !hero.IsDead &&
                enemy.Key.GetWaypoints().Last().Distance(ObjectManager.Player.ServerPosition) >
                GankDetector.GetMenuItem("SAwarenessDetectorsGankTrackRangeMin").GetValue<Slider>().Value &&
                enemy.Key.GetWaypoints().Last().Distance(ObjectManager.Player.ServerPosition) <
                GankDetector.GetMenuItem("SAwarenessDetectorsGankTrackRangeMax").GetValue<Slider>().Value)
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
                Enemies[hero].Time.VisibleTime = (int)Game.Time;
                enemy.Value.Time.CalledInvisible = false;
            }
            else
            {
                if (Enemies[hero].Time.VisibleTime != 0)
                {
                    Enemies[hero].Time.InvisibleTime = (int)(Game.Time - Enemies[hero].Time.VisibleTime);
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
