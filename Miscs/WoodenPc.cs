﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SAssemblies;
using SAssemblies.Miscs;
using Menu = SAssemblies.Menu;

namespace SAssemblies.Miscs
{
    class WoodenPc
    {
        public static Menu.MenuItemSettings WoodenPcMisc = new Menu.MenuItemSettings(typeof(WoodenPc));

        private MemoryStream packet;
        private bool packetSent = false;
        private Notification notification = null;
        private Notification notificationRemaining = null;
        private long ms = -1;
        private DrawingDraw drawingEvent = null;
        private int delay = 420;
        private bool disabledBecauseOfBrokerAPI = true;

        public WoodenPc()
        {
            if (Game.Mode == GameMode.Running)
            {
                return;
            }
            SetupMenu();
            WoodenPcMisc.GetMenuItem("SAssembliesMiscsWoodenPcActive").ValueChanged += Active_OnValueChanged;
            notification = Common.ShowNotification("Waiting for the packet", Color.LawnGreen, -1);
            Game.OnSendPacket += Game_OnSendPacket;
            Game.OnWndProc += Game_OnWndProc;
            GameUpdate updateEvent = null;
            updateEvent = delegate
            {
                if (Game.Mode == GameMode.Running)
                {
                    LeagueSharp.Common.Menu.RootMenus.Remove(Assembly.GetCallingAssembly().GetName().Name + "." + WoodenPcMisc.Menu.Name);
                    Game.OnUpdate -= updateEvent;
                }
            };
            
        }

        ~WoodenPc()
        {
            Game.OnSendPacket -= Game_OnSendPacket;
            Game.OnWndProc -= Game_OnWndProc;
            if (notification != null)
            {
                notification.Dispose();
                Notifications.RemoveNotification(notification);
            }
            if (notificationRemaining != null)
            {
                notificationRemaining.Dispose();
                Notifications.RemoveNotification(notificationRemaining);
            }
        }

        public bool IsActive()
        {
            return WoodenPcMisc.GetActive() && !disabledBecauseOfBrokerAPI;
        }

        public static Menu.MenuItemSettings SetupMenu()
        {
            Language.SetLanguage();
            WoodenPcMisc.Menu = new LeagueSharp.Common.Menu("SAwarenessWoodenPc", "SAwarenessWoodenPc", true);
            WoodenPcMisc.MenuItems.Add(
                WoodenPcMisc.Menu.AddItem(new MenuItem("SAssembliesMiscsWoodenPcActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(true)));
            WoodenPcMisc.Menu.AddItem(new MenuItem("By Screeder", "By Screeder V" + Assembly.GetExecutingAssembly().GetName().Version));
            WoodenPcMisc.Menu.AddToMainMenu();
            return WoodenPcMisc;
        }

        private void Active_OnValueChanged(object sender, OnValueChangeEventArgs onValueChangeEventArgs)
        {
            Game.OnSendPacket -= Game_OnSendPacket;
            Game.OnWndProc -= Game_OnWndProc;
            if (packet != null)
            {
                Game.SendPacket(packet.ToArray(), PacketChannel.C2S, PacketProtocolFlags.Reliable);
            }
            if (notification != null)
            {
                Notifications.RemoveNotification(notification);
            }
            if (notificationRemaining != null)
            {
                Notifications.RemoveNotification(notificationRemaining);
            }
        }

        private void Game_OnSendPacket(GamePacketEventArgs args)
        {
            try
            {
                if (Game.Mode != GameMode.Running && IsActive())
                {
                    //Console.Write("Packet Sent: ");
                    //args.PacketData.ForEach(x => Console.Write(x + " "));
                    //Console.WriteLine();
                    if (args.PacketData.Length != 6 || packetSent || packet != null)
                        return;
                    args.Process = false;
                    packet = new MemoryStream(args.PacketData, 0, args.PacketData.Length);
                    Console.Write("Got Packet: " + args.PacketData[0] + "; Length: " + args.PacketData.Length + "; ");
                    Notifications.RemoveNotification(notification);
                    notification.Dispose();
                    notification = Common.ShowNotification("Press spacebar to continue.", Color.YellowGreen, -1);
                    ms = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                    drawingEvent = delegate
                    {
                        if (!packetSent && (ms + (delay * 1000)) < (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond))
                        {
                            Game.SendPacket(packet.ToArray(), PacketChannel.C2S, PacketProtocolFlags.Reliable);
                            packetSent = true;
                            if (notification != null)
                            {
                                Notifications.RemoveNotification(notification);
                                notification.Dispose();
                            }
                            if (notificationRemaining != null)
                            {
                                Notifications.RemoveNotification(notificationRemaining);
                                notificationRemaining.Dispose();
                            }
                            notification = Common.ShowNotification("Game starts now. Prepare!", Color.OrangeRed, 1000);
                            Drawing.OnDraw -= drawingEvent;
                        }
                        else
                        {
                            if (notificationRemaining == null)
                            {
                                notificationRemaining =
                                Common.ShowNotification(
                                    "Remaining: " +
                                    (ms + (delay * 1000) - DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond).ToString(),
                                    Color.YellowGreen, delay * 1000);
                            }
                            else
                            {
                                notificationRemaining.Text = "Remaining: " +
                                    ((ms + (delay * 1000) - DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) / 1000).ToString() + "s";
                            }
                        }
                    };
                    Drawing.OnDraw += drawingEvent;
                }
                else
                {
                    if (notification != null)
                    {
                        Notifications.RemoveNotification(notification);
                        notification.Dispose();
                    }
                    if (notificationRemaining != null)
                    {
                        Notifications.RemoveNotification(notificationRemaining);
                        notificationRemaining.Dispose();
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void Game_OnWndProc(WndEventArgs args)
        {
            if ((WindowsMessages)args.Msg != WindowsMessages.WM_KEYUP || args.WParam != 32 || packetSent || !IsActive())
                return;
            Game.SendPacket(packet.ToArray(), PacketChannel.C2S, PacketProtocolFlags.Reliable);
            packetSent = true;
            if (notification != null)
            {
                Notifications.RemoveNotification(notification);
                notification.Dispose();
            }
            if (notificationRemaining != null)
            {
                Notifications.RemoveNotification(notificationRemaining);
                notificationRemaining.Dispose();
            }
            if (drawingEvent != null)
            {
                Drawing.OnDraw -= drawingEvent;
            }
            notification = Common.ShowNotification("Game starts now. Prepare!", Color.OrangeRed, 1000);
        }
    }
}
