using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SAwareness.Miscs
{
    class TurnAround
    {
        public static Menu.MenuItemSettings TurnAroundMisc = new Menu.MenuItemSettings(typeof(TurnAround));

        private Vector2 _lastMove = ObjectManager.Player.ServerPosition.To2D();
        private float _lastTime = Game.Time;

        public TurnAround()
        {
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            Obj_AI_Base.OnIssueOrder += ObjAiBase_OnIssueOrder;
        }

        ~TurnAround()
        {
            Obj_AI_Base.OnProcessSpellCast -= Obj_AI_Hero_OnProcessSpellCast;
            Obj_AI_Base.OnIssueOrder -= ObjAiBase_OnIssueOrder;
            _lastMove = new Vector2();
            _lastTime = 0;
        }

        public bool IsActive()
        {
            return Misc.Miscs.GetActive() && TurnAroundMisc.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            TurnAroundMisc.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("MISCS_TURNAROUND_MAIN"), "SAwarenessMiscsTurnAround"));
            TurnAroundMisc.MenuItems.Add(
                TurnAroundMisc.Menu.AddItem(new MenuItem("SAwarenessMiscsTurnAroundActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return TurnAroundMisc;
        }

        private void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!IsActive())
                return;
            if (!sender.IsEnemy)
                return;
            if (args.SData.Name.Contains("CassiopeiaPetrifyingGaze"))
            {
                if (ObjectManager.Player.ServerPosition.Distance(sender.ServerPosition) <= 750)
                {
                    var pos =
                        new Vector2(
                            ObjectManager.Player.ServerPosition.X +
                            ((sender.ServerPosition.X - ObjectManager.Player.ServerPosition.X) * (-100) /
                             ObjectManager.Player.ServerPosition.Distance(sender.ServerPosition)),
                            ObjectManager.Player.ServerPosition.
                                Y +
                            ((sender.ServerPosition.Y - ObjectManager.Player.ServerPosition.Y) * (-100) /
                             ObjectManager.Player.ServerPosition.Distance(sender.ServerPosition)));
                    ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, pos.To3D2());
                    _lastTime = Game.Time;
                    Utility.DelayAction.Add(750,
                        () => ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, _lastMove.To3D2()));
                }
            }
            else if (args.SData.Name.Contains("MockingShout"))
            {
                if (ObjectManager.Player.ServerPosition.Distance(sender.ServerPosition) <= 850)
                {
                    var pos =
                        new Vector2(
                            ObjectManager.Player.ServerPosition.X +
                            ((sender.ServerPosition.X - ObjectManager.Player.ServerPosition.X) * (100) /
                             ObjectManager.Player.ServerPosition.Distance(sender.ServerPosition)),
                            ObjectManager.Player.ServerPosition.
                                Y +
                            ((sender.ServerPosition.Y - ObjectManager.Player.ServerPosition.Y) * (100) /
                             ObjectManager.Player.ServerPosition.Distance(sender.ServerPosition)));
                    ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, pos.To3D2());
                    _lastTime = Game.Time;
                    Utility.DelayAction.Add(750,
                        () => ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, _lastMove.To3D2()));
                }
            }
            else if (args.SData.Name.Contains("TwoShivPoison"))
            {
                if (ObjectManager.Player.ServerPosition.Distance(sender.ServerPosition) <= 625 && args.Target == ObjectManager.Player)
                {
                    var pos =
                        new Vector2(
                            ObjectManager.Player.ServerPosition.X +
                            ((sender.ServerPosition.X - ObjectManager.Player.ServerPosition.X) * (100) /
                             ObjectManager.Player.ServerPosition.Distance(sender.ServerPosition)),
                            ObjectManager.Player.ServerPosition.
                                Y +
                            ((sender.ServerPosition.Y - ObjectManager.Player.ServerPosition.Y) * (100) /
                             ObjectManager.Player.ServerPosition.Distance(sender.ServerPosition)));
                    ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, pos.To3D2());
                    _lastTime = Game.Time;
                    Utility.DelayAction.Add(750,
                        () => ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, _lastMove.To3D2()));
                }
            }
        }

        private void ObjAiBase_OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            if (!IsActive())
                return;

            if (sender.IsMe && args.Order == GameObjectOrder.MoveTo)
            {
                _lastMove = new Vector2(args.TargetPosition.X, args.TargetPosition.Y);
                if (!_lastTime.Equals(Game.Time) && _lastTime + 1 > Game.Time)
                    args.Process = false;
            }
        }

        //private void Game_OnGameSendPacket(GamePacketEventArgs args)
        //{
        //    if (!IsActive())
        //        return;

        //    try
        //    {
        //        decimal milli = DateTime.Now.Ticks / (decimal)TimeSpan.TicksPerMillisecond;
        //        var reader = new BinaryReader(new MemoryStream(args.PacketData));
        //        byte packetId = reader.ReadByte();
        //        if (packetId != Packet.C2S.Move.Header)
        //            return;
        //        Packet.C2S.Move.Struct move = Packet.C2S.Move.Decoded(args.PacketData);
        //        if (move.MoveType == 2)
        //        {
        //            if (move.SourceNetworkId == ObjectManager.Player.NetworkId)
        //            {
        //                _lastMove = new Vector2(move.X, move.Y);
        //                if (_lastTime + 1 > Game.Time)
        //                    args.Process = false;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("MovementSend: " + ex);
        //    }
        //}
    }
}
