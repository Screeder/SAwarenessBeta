using System;
using System.IO;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAssemblies.Miscs
{
    internal class SafeMovement
    {
        public static Menu.MenuItemSettings SafeMovementMisc = new Menu.MenuItemSettings(typeof(SafeMovement));

        private decimal _lastSend;

        public SafeMovement()
        {
            Obj_AI_Hero.OnIssueOrder += Obj_AI_Hero_OnIssueOrder;
            //Game.OnGameSendPacket += Game_OnGameSendPacket;
        }

        ~SafeMovement()
        {
            Obj_AI_Hero.OnIssueOrder -= Obj_AI_Hero_OnIssueOrder;
            //Game.OnGameSendPacket -= Game_OnGameSendPacket;
        }

        public bool IsActive()
        {
#if MISCS
            return Misc.Miscs.GetActive() && SafeMovementMisc.GetActive();
#else
            return SafeMovementMisc.GetActive();
#endif
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            SafeMovementMisc.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("MISCS_SAFEMOVEMENT_MAIN"), "SAssembliesMiscsSafeMovement"));
            SafeMovementMisc.MenuItems.Add(
                SafeMovementMisc.Menu.AddItem(new MenuItem("SAssembliesMiscsSafeMovementBlockIntervall", Language.GetString("MISCS_SAFEMOVEMENT_BLOCKINTERVAL")).SetValue(new Slider(20, 1000, 0))));
            SafeMovementMisc.MenuItems.Add(
                SafeMovementMisc.Menu.AddItem(new MenuItem("SAssembliesMiscsSafeMovementActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return SafeMovementMisc;
        }

        void Obj_AI_Hero_OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            if (!IsActive())
                return;

            if(!sender.IsMe || args.Order != GameObjectOrder.MoveTo)
                return;

            decimal milli = DateTime.Now.Ticks / (decimal)TimeSpan.TicksPerMillisecond;
            if (milli - _lastSend <
                            SafeMovementMisc.GetMenuItem("SAssembliesMiscsSafeMovementBlockIntervall")
                                .GetValue<Slider>()
                                .Value)
            {
                args.Process = false;
            }
            else
            {
                _lastSend = milli;
            }
        }

        private void Game_OnGameSendPacket(GamePacketEventArgs args)
        {
            if (!IsActive())
                return;

            try
            {
                decimal milli = DateTime.Now.Ticks/(decimal) TimeSpan.TicksPerMillisecond;
                var reader = new BinaryReader(new MemoryStream(args.PacketData));
                byte packetId = reader.ReadByte();
                if (packetId != Packet.C2S.Move.Header)
                    return;
                Packet.C2S.Move.Struct move = Packet.C2S.Move.Decoded(args.PacketData);
                if (move.MoveType == 2)
                {
                    if (move.SourceNetworkId == ObjectManager.Player.NetworkId)
                    {
                        if (milli - _lastSend <
                            SafeMovementMisc.GetMenuItem("SAssembliesMiscsSafeMovementBlockIntervall")
                                .GetValue<Slider>()
                                .Value)
                        {
                            args.Process = false;
                        }
                        else
                        {
                            _lastSend = milli;
                        }
                    }
                }
                else if (move.MoveType == 3)
                {
                    _lastSend = 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("MovementProcess: " + ex);
            }
        }
    }
}