using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace SAwareness.Miscs
{
    class FlashJuke
    {
        public static Menu.MenuItemSettings FlashJukeMisc = new Menu.MenuItemSettings(typeof(FlashJuke));

        List<Vector3> spotsStart = new List<Vector3>();
        List<Vector3> spotsEnd = new List<Vector3>();

        private double flashedTime = Game.Time;
        private int lastGameUpdateTime = 0;

         public FlashJuke()
        {
            if (Game.MapId != (GameMapId)11)
                return;

            spotsStart.Add(new Vector3(5351, 12775, 40));
            spotsStart.Add(new Vector3(8580, 2160, 54));
            spotsStart.Add(new Vector3(6240, 11632, 53));
            spotsStart.Add(new Vector3(8560, 3333, 57));
            spotsStart.Add(new Vector3(11955, 5008, 53));
            spotsStart.Add(new Vector3(2910, 9918, 53));
            spotsStart.Add(new Vector3(10368, 6690, 55));
            spotsStart.Add(new Vector3(4548, 8254, 52));
            spotsStart.Add(new Vector3(4360, 7445, 53));
            spotsStart.Add(new Vector3(9325, 2655, 64));
            spotsStart.Add(new Vector3(6143, 3777, 53));
            spotsStart.Add(new Vector3(7477, 3498, 55));
            spotsStart.Add(new Vector3(7484, 3356, 55));
            spotsStart.Add(new Vector3(5479, 12273, 41));
            spotsStart.Add(new Vector3(7391, 11557, 53));
            spotsStart.Add(new Vector3(7384, 11691, 53));
            spotsStart.Add(new Vector3(8610, 11378, 50));
            spotsStart.Add(new Vector3(10374, 7524, 55));
            spotsStart.Add(new Vector3(11786, 4465, -62));
            spotsStart.Add(new Vector3(5625, 9465, -65));
            spotsStart.Add(new Vector3(5830, 10201, 55));
            spotsStart.Add(new Vector3(9080, 5470, -64));
            spotsStart.Add(new Vector3(9008, 4683, 55));
            spotsStart.Add(new Vector3(3213, 10479, -65));
            spotsStart.Add(new Vector3(3885, 7800, 55));
            spotsStart.Add(new Vector3(11020, 7190, 54));
            spotsStart.Add(new Vector3(6349, 5250, 51));
            spotsStart.Add(new Vector3(8427, 9735, 53));

            spotsEnd.Add(new Vector3(5946, 12786, 40));
            spotsEnd.Add(new Vector3(8962, 2175, 55));
            spotsEnd.Add(new Vector3(6534, 11435, 54));
            spotsEnd.Add(new Vector3(8292, 3568, 56));
            spotsEnd.Add(new Vector3(12310, 5208, 45));
            spotsEnd.Add(new Vector3(2545, 9794, 53));
            spotsEnd.Add(new Vector3(10019, 6594, 51));
            spotsEnd.Add(new Vector3(4871, 8423, 34));
            spotsEnd.Add(new Vector3(4698, 7255, 54));
            spotsEnd.Add(new Vector3(9300, 2275, 55));
            spotsEnd.Add(new Vector3(5800, 3568, 54));
            spotsEnd.Add(new Vector3(7816, 3572, 55));
            spotsEnd.Add(new Vector3(7154, 3186, 55));
            spotsEnd.Add(new Vector3(5507, 12649, 40));
            spotsEnd.Add(new Vector3(7015, 11420, 54));
            spotsEnd.Add(new Vector3(7728, 11775, 52));
            spotsEnd.Add(new Vector3(9005, 11436, 54));
            spotsEnd.Add(new Vector3(10090, 7772, 54));
            spotsEnd.Add(new Vector3(11860, 4095, -55));
            spotsEnd.Add(new Vector3(5325, 9215, -63));
            spotsEnd.Add(new Vector3(6197, 10308, 54));
            spotsEnd.Add(new Vector3(9360, 5692, -64));
            spotsEnd.Add(new Vector3(8692, 4690, 55));
            spotsEnd.Add(new Vector3(3110, 10842, -64));
            spotsEnd.Add(new Vector3(3525, 7809, 56));
            spotsEnd.Add(new Vector3(11361, 7140, 54));
            spotsEnd.Add(new Vector3(6526, 4896, 51));
            spotsEnd.Add(new Vector3(8305, 10120, 53));


            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnEndScene += Drawing_OnEndScene;
        }

         ~FlashJuke()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
            Drawing.OnEndScene -= Drawing_OnEndScene;
        }

        public bool IsActive()
        {
            return Misc.Miscs.GetActive() && FlashJukeMisc.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            FlashJukeMisc.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("MISCS_FLASHJUKE_MAIN"), "SAwarenessMiscsFlashJuke"));
            FlashJukeMisc.MenuItems.Add(
                FlashJukeMisc.Menu.AddItem(new MenuItem("SAwarenessMiscsFlashJukeKeyActive", Language.GetString("GLOBAL_KEY")).SetValue(new KeyBind(90, KeyBindType.Press))));
            FlashJukeMisc.MenuItems.Add(
                FlashJukeMisc.Menu.AddItem(new MenuItem("SAwarenessMiscsFlashJukeRecall", Language.GetString("MISCS_FLASHJUKE_RECALL")).SetValue(false)));
            FlashJukeMisc.MenuItems.Add(
                FlashJukeMisc.Menu.AddItem(new MenuItem("SAwarenessMiscsFlashJukeActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return FlashJukeMisc;
        }

        void Drawing_OnEndScene(EventArgs args)
        {
            if (!IsActive())
                return;

            SpellSlot spell = Activator.GetFlashSlot();
            if (ObjectManager.Player.Spellbook.CanUseSpell(spell) != SpellState.Ready)
                return;

            for (int i = 0; i < spotsStart.Count; i++)
            {
                if (ObjectManager.Player.ServerPosition.Distance(spotsStart[i]) < 2000)
                {
                    if (spotsStart[i].IsOnScreen())
                    {
                        Utility.DrawCircle(spotsStart[i], 50, System.Drawing.Color.Red);
                    }
                    if (spotsEnd[i].IsOnScreen())
                    {
                        Utility.DrawCircle(spotsEnd[i], 100, System.Drawing.Color.Green);
                    }
                    if (spotsStart[i].IsOnScreen() || spotsEnd[i].IsOnScreen())
                    {
                        Drawing.DrawLine(Drawing.WorldToScreen(spotsStart[i]), Drawing.WorldToScreen(spotsEnd[i]), 2, Color.Gold);
                    }
                }
            }
        }

        void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive() || !FlashJukeMisc.GetMenuItem("SAwarenessMiscsFlashJukeKeyActive").GetValue<KeyBind>().Active || lastGameUpdateTime + new Random().Next(500, 1000) > Environment.TickCount)
                return;

            lastGameUpdateTime = Environment.TickCount;

            SpellSlot spell = Activator.GetFlashSlot();
            if(ObjectManager.Player.Spellbook.CanUseSpell(spell) != SpellState.Ready)
                return;

            Vector3 nearestPosStart = GetNearestPos(spotsStart);

            if (Game.Time > flashedTime + 5)
            {
                ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, nearestPosStart);
            }

            if (nearestPosStart.X != 0 && ObjectManager.Player.Distance(nearestPosStart) < 50 && !NavMesh.IsWallOfGrass(ObjectManager.Player.ServerPosition, 10) && Game.Time > flashedTime + 5 &&
                !AnyEnemyInBush())
            {
                Vector3 nearestPosEnd = GetNearestPos(spotsEnd);
                if (nearestPosEnd.X != 0)
                {
                    Packet.C2S.Cast.Encoded(new Packet.C2S.Cast.Struct(0, spell, -1, nearestPosEnd.X, nearestPosEnd.Y, nearestPosEnd.X, nearestPosEnd.Y)).Send();
                    ObjectManager.Player.Spellbook.CastSpell(spell, nearestPosEnd);
                    flashedTime = Game.Time;
                    if (FlashJukeMisc.GetMenuItem("SAwarenessMiscsFlashJukeRecall").GetValue<bool>())
                    {
                        Utility.DelayAction.Add(200, () =>
                        {
                            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, nearestPosEnd);
                            Packet.C2S.Cast.Encoded(new Packet.C2S.Cast.Struct(0, SpellSlot.Recall)).Send();
                            ObjectManager.Player.Spellbook.CastSpell(SpellSlot.Recall);
                        });
                    }
                }                
            }
        }

        bool AnyEnemyInBush()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsValid && hero.IsEnemy && hero.IsVisible && !hero.IsDead)
                {
                    if (NavMesh.IsWallOfGrass(hero.ServerPosition,10) &&
                        ObjectManager.Player.ServerPosition.Distance(hero.ServerPosition) < 650)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        Vector3 GetNearestPos(List<Vector3> vecs)
        {
            Vector3 near = new Vector3();
            double lastDist = 999999999;
            foreach (var vec in vecs)
            {
                if (ObjectManager.Player.Distance(vec) < lastDist)
                {
                    lastDist = ObjectManager.Player.Distance(vec);
                    near = vec;
                }
            }
            return near;
        }
    }
}
