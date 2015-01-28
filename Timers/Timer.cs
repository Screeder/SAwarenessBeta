using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SAwareness.Timers
{
    class Timer
    {
        public static Menu.MenuItemSettings Timers = new Menu.MenuItemSettings();

        private Timer()
        {

        }

        ~Timer()
        {
            
        }

        private static void SetupMainMenu()
        {
            var menu = new LeagueSharp.Common.Menu("SAwareness", "SAwareness", true);
            SetupMenu(menu);
            menu.AddToMainMenu();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            Language.SetLanguage();
            Timers.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("TIMERS_TIMER_MAIN"), "SAwarenessTimers"));
            Timers.MenuItems.Add(
                Timers.Menu.AddItem(new MenuItem("SAwarenessTimersPingTimes", Language.GetString("GLOBAL_PING_TIMES")).SetValue(new Slider(0, 5, 0))));
            Timers.MenuItems.Add(
                Timers.Menu.AddItem(new MenuItem("SAwarenessTimersRemindTime", Language.GetString("TIMERS_REMIND_TIME")).SetValue(new Slider(0, 50, 0))));
            Timers.MenuItems.Add(
                Timers.Menu.AddItem(new MenuItem("SAwarenessTimersLocalPing", Language.GetString("GLOBAL_PING_LOCAL")).SetValue(true)));
            Timers.MenuItems.Add(
                Timers.Menu.AddItem(new MenuItem("SAwarenessTimersChatChoice", Language.GetString("GLOBAL_CHAT_CHOICE")).SetValue(new StringList(new[]
                {
                    Language.GetString("GLOBAL_CHAT_CHOICE_NONE"), 
                    Language.GetString("GLOBAL_CHAT_CHOICE_LOCAL"), 
                    Language.GetString("GLOBAL_CHAT_CHOICE_SERVER")
                }))));
            Timers.MenuItems.Add(
                Timers.Menu.AddItem(new MenuItem("SAwarenessTimersTextScale", Language.GetString("TIMERS_TIMER_SCALE")).SetValue(new Slider(12, 8, 20))));
            Timers.MenuItems.Add(Timers.Menu.AddItem(new MenuItem("SAwarenessTimersActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return Timers;
        }

        private String AlignTime(float endTime)
        {
            if (!float.IsInfinity(endTime) && !float.IsNaN(endTime))
            {
                var m = (float)Math.Floor(endTime / 60);
                var s = (float)Math.Ceiling(endTime % 60);
                String ms = (s < 10 ? m + ":0" + s : m + ":" + s);
                return ms;
            }
            return "";
        }

        public static bool PingAndCall(String text, Vector3 pos, bool call = true, bool ping = true)
        {
            if (ping)
            {
                for (int i = 0; i < Timers.GetMenuItem("SAwarenessTimersPingTimes").GetValue<Slider>().Value; i++)
                {
                    GamePacket gPacketT;
                    if (Timers.GetMenuItem("SAwarenessTimersLocalPing").GetValue<bool>())
                    {
                        gPacketT =
                            Packet.S2C.Ping.Encoded(new Packet.S2C.Ping.Struct(pos[0], pos[1], 0, 0,
                                Packet.PingType.Normal));
                        gPacketT.Process();
                    }
                    else if (!Timers.GetMenuItem("SAwarenessTimersLocalPing").GetValue<bool>() &&
                             Menu.GlobalSettings.GetMenuItem("SAwarenessGlobalSettingsServerChatPingActive")
                                 .GetValue<bool>())
                    {
                        gPacketT = Packet.C2S.Ping.Encoded(new Packet.C2S.Ping.Struct(pos.X, pos.Y));
                        gPacketT.Send();
                    }
                }
            }
            if (call)
            {
                if (Timers.GetMenuItem("SAwarenessTimersChatChoice").GetValue<StringList>().SelectedIndex == 1)
                {
                    Game.PrintChat(text);
                }
                else if (Timers.GetMenuItem("SAwarenessTimersChatChoice").GetValue<StringList>().SelectedIndex == 2 &&
                         Menu.GlobalSettings.GetMenuItem("SAwarenessGlobalSettingsServerChatPingActive").GetValue<bool>())
                {
                    Game.Say(text);
                }
            }
            return true;
        }
    }
}
