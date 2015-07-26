using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace SAssemblies.Timers
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
            var menu = new LeagueSharp.Common.Menu("STimers", "SAssembliesSTimers", true);
            SetupMenu(menu);
            menu.AddToMainMenu();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu, bool useExisitingMenu = false)
        {
            Language.SetLanguage();
            if (!useExisitingMenu)
            {
                Timers.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("TIMERS_TIMER_MAIN"), "SAssembliesTimers"));
            }
            else
            {
                Timers.Menu = menu;
            }
            Timers.MenuItems.Add(
                Timers.Menu.AddItem(new MenuItem("SAssembliesTimersPingTimes", Language.GetString("GLOBAL_PING_TIMES")).SetValue(new Slider(0, 5, 0))));
            Timers.MenuItems.Add(
                Timers.Menu.AddItem(new MenuItem("SAssembliesTimersRemindTime", Language.GetString("TIMERS_REMIND_TIME")).SetValue(new Slider(0, 50, 0))));
            Timers.MenuItems.Add(
                Timers.Menu.AddItem(new MenuItem("SAssembliesTimersLocalPing", Language.GetString("GLOBAL_PING_LOCAL")).SetValue(true)));
            Timers.MenuItems.Add(
                Timers.Menu.AddItem(new MenuItem("SAssembliesTimersChat", Language.GetString("GLOBAL_CHAT")).SetValue(false)));
            Timers.MenuItems.Add(
                Timers.Menu.AddItem(new MenuItem("SAssembliesTimersNotification", Language.GetString("GLOBAL_NOTIFICATION")).SetValue(false)));
            Timers.MenuItems.Add(
                Timers.Menu.AddItem(new MenuItem("SAssembliesTimersTextScale", Language.GetString("TIMERS_TIMER_SCALE")).SetValue(new Slider(12, 8, 20))));
            if (!useExisitingMenu)
            {
                Timers.MenuItems.Add(Timers.CreateActiveMenuItem("SAssembliesTimersActive"));
            }
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

        public static bool PingAndCall(String text, Vector3 pos, bool call = true, bool ping = true, bool notification = true)
        {
            if (ping)
            {
                for (int i = 0; i < Timers.GetMenuItem("SAssembliesTimersPingTimes").GetValue<Slider>().Value; i++)
                {
                    if (Timers.GetMenuItem("SAssembliesTimersLocalPing").GetValue<bool>())
                    {
                        Game.ShowPing(PingCategory.Normal, pos, true);
                    }
                    else if (!Timers.GetMenuItem("SAssembliesTimersLocalPing").GetValue<bool>() &&
                             Menu.GlobalSettings.GetMenuItem("SAssembliesGlobalSettingsServerChatPingActive")
                                 .GetValue<bool>())
                    {
                        Game.SendPing(PingCategory.Normal, pos);
                    }
                }
            }
            if (call)
            {
                if (Timers.GetMenuItem("SAssembliesTimersChat").GetValue<bool>() &&
                         Menu.GlobalSettings.GetMenuItem("SAssembliesGlobalSettingsServerChatPingActive").GetValue<bool>())
                {
                    Console.WriteLine("CALL!!!");
                    Game.Say(text);
                }
            }
            if (notification)
            {
                if (Timers.GetMenuItem("SAssembliesTimersNotification").GetValue<bool>())
                {
                    Common.ShowNotification(text, Color.White, 3);
                }
            }
            return true;
        }
    }
}
