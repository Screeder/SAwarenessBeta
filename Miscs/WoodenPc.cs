using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SAssemblies;
using SAssemblies.Miscs;
using Menu = SAssemblies.Menu;

namespace SAwareness.Miscs
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

        public WoodenPc()
        {
            notification = Common.ShowNotification("Waiting for the packet", Color.LawnGreen, -1);
            Game.OnSendPacket += Game_OnSendPacket;
            Game.OnWndProc += Game_OnWndProc;
        }

        ~WoodenPc()
        {
            Game.OnSendPacket -= Game_OnSendPacket;
            Game.OnWndProc -= Game_OnWndProc;
            Notifications.RemoveNotification(notification);
            notification.Dispose();
            if (notificationRemaining != null)
            {
                Notifications.RemoveNotification(notificationRemaining);
                notificationRemaining.Dispose();
            }
        }

        public bool IsActive()
        {
#if MISCS
            return Misc.Miscs.GetActive() && WoodenPcMisc.GetActive();
#else
            return WoodenPcMisc.GetActive();
#endif
        }

        private void Game_OnSendPacket(GamePacketEventArgs args)
        {
            try
            {
                if (Game.Mode != GameMode.Running)
                {
                    //Console.Write("Packet Sent: ");
                    //args.PacketData.ForEach(x => Console.Write(x + " "));
                    //Console.WriteLine();
                    if (args.PacketData.Length != 6 || packetSent || packet != null)
                        return;
                    args.Process = false;
                    packet = new MemoryStream(args.PacketData, 0, args.PacketData.Length);
                    Notifications.RemoveNotification(notification);
                    notification.Dispose();
                    notification = Common.ShowNotification("Press spacebar to continue.", Color.YellowGreen, -1);
                    ms = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                    drawingEvent = delegate
                    {
                        if (!packetSent && (ms + (250 * 1000)) < (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond))
                        {
                            Console.WriteLine((ms + (250 * 1000)) + " " + DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
                            Game.SendPacket(packet.ToArray(), PacketChannel.C2S, PacketProtocolFlags.Reliable);
                            packetSent = true;
                            Notifications.RemoveNotification(notification);
                            notification.Dispose();
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
                                    (ms + (250 * 1000) - DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond).ToString(),
                                    Color.YellowGreen, 250 * 1000);
                            }
                            else
                            {
                                notificationRemaining.Text = "Remaining: " +
                                    ((ms + (250 * 1000) - DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) / 1000).ToString() + "s";
                            }
                        }
                    };
                    Drawing.OnDraw += drawingEvent;
                }
            }
            catch (Exception)
            {
            }
        }

        private void Game_OnWndProc(WndEventArgs args)
        {
            if ((WindowsMessages)args.Msg != WindowsMessages.WM_KEYUP || args.WParam != 32 || packetSent)
                return;
            Game.SendPacket(packet.ToArray(), PacketChannel.C2S, PacketProtocolFlags.Reliable);
            packetSent = true;
            Notifications.RemoveNotification(notification);
            notification.Dispose();
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
