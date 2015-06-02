using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace SAssemblies.Miscs
{
    class MinionBars
    {
        public static Menu.MenuItemSettings MinionBarsMisc = new Menu.MenuItemSettings(typeof(MinionBars));

        public MinionBars()
        {
            Drawing.OnDraw += Drawing_OnDraw;
        }

        ~MinionBars()
        {
            Drawing.OnDraw -= Drawing_OnDraw;
        }

        public bool IsActive()
        {
#if MISCS
            return Misc.Miscs.GetActive() && MinionBarsMisc.GetActive();
#else
            return MinionBarsMisc.GetActive();
#endif
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            MinionBarsMisc.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("MISCS_MINIONBARS_MAIN"), "SAssembliesMiscsMinionBars"));
            MinionBarsMisc.MenuItems.Add(
                MinionBarsMisc.Menu.AddItem(new MenuItem("SAssembliesMiscsMinionBarsActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return MinionBarsMisc;
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!IsActive())
                return;

            foreach (var minion in ObjectManager.Get<Obj_AI_Minion>())
            {
                if (!minion.IsVisible || minion.IsDead || minion.IsAlly || minion.Health == 0 || !minion.IsHPBarRendered)
                    continue;

                var attackToKill = Math.Ceiling(minion.MaxHealth / ObjectManager.Player.GetAutoAttackDamage(minion, true));
                var hpBarPosition = minion.HPBarPosition;
                var lineSize = 1;
                var xOffset = 45;
                var yOffsetStart = 18;
                var yOffsetEnd = 23;
                var barWidth = 50;
                switch (minion.BaseSkinName)
                {
                    //Summoners Rift
                    case "SRU_ChaosMinionMelee":
                    case "SRU_OrderMinionMelee":
                        barWidth = 70;
                        xOffset = 45;
                        yOffsetStart = 18;
                        yOffsetEnd = 23;
                        break;

                    case "SRU_ChaosMinionRanged":
                    case "SRU_OrderMinionRanged":
                        barWidth = 75;
                        xOffset = 45;
                        yOffsetStart = 18;
                        yOffsetEnd = 23;
                        break;

                    case "SRU_ChaosMinionSiege":
                    case "SRU_OrderMinionSiege":
                        barWidth = 65;
                        xOffset = 45;
                        yOffsetStart = 18;
                        yOffsetEnd = 23;
                        break;

                    case "SRU_ChaosMinionSuper":
                    case "SRU_OrderMinionSuper":
                        barWidth = 80;
                        xOffset = 45;
                        yOffsetStart = 18;
                        yOffsetEnd = 23;
                        break;

                    case "SRU_Murkwolf":
                        barWidth = 79;
                        xOffset = 53;
                        yOffsetStart = 19;
                        yOffsetEnd = 23;
                        break;

                    case "SRU_MurkwolfMini":
                        barWidth = 65;
                        xOffset = 40;
                        yOffsetStart = 19;
                        yOffsetEnd = 23;
                        break;

                    case "SRU_Razorbeak":
                        barWidth = 79;
                        xOffset = 53;
                        yOffsetStart = 19;
                        yOffsetEnd = 23;
                        break;

                    case "SRU_RazorbeakMini":
                        barWidth = 65;
                        xOffset = 35;
                        yOffsetStart = 19;
                        yOffsetEnd = 23;
                        break;

                    case "SRU_Krug":
                        barWidth = 79;
                        xOffset = 58;
                        yOffsetStart = 19;
                        yOffsetEnd = 23;
                        break;

                    case "SRU_KrugMini":
                        barWidth = 65;
                        xOffset = 35;
                        yOffsetStart = 19;
                        yOffsetEnd = 23;
                        break;

                    case "SRU_Blue":
                        barWidth = 147;
                        xOffset = 3;
                        yOffsetStart = 19;
                        yOffsetEnd = 28;
                        break;

                    case "SRU_BlueMini":
                    case "SRU_BlueMini2":
                        barWidth = 55;
                        xOffset = 37;
                        yOffsetStart = 19;
                        yOffsetEnd = 23;
                        break;

                    case "SRU_Red":
                        barWidth = 147;
                        xOffset = 3;
                        yOffsetStart = 19;
                        yOffsetEnd = 28;
                        break;

                    case "SRU_RedMini":
                        barWidth = 55;
                        xOffset = 37;
                        yOffsetStart = 19;
                        yOffsetEnd = 23;
                        break;

                    case "SRU_Gromp":
                        barWidth = 92;
                        xOffset = 60;
                        yOffsetStart = 18;
                        yOffsetEnd = 23;
                        break;

                    case "Sru_Crab":
                        barWidth = 65;
                        xOffset = 45;
                        yOffsetStart = 34;
                        yOffsetEnd = 38;
                        break;

                    case "SRU_Dragon":
                        barWidth = 147;
                        xOffset = 3;
                        yOffsetStart = 18;
                        yOffsetEnd = 27;
                        lineSize = 2;
                        break;

                    case "SRU_Baron":
                        barWidth = 190;
                        xOffset = -20;
                        yOffsetStart = 16;
                        yOffsetEnd = 28;
                        lineSize = 2;
                        break;

                    //TwistedTreeline

                    case "Red_Minion_Basic":
                    case "Blue_Minion_Basic":
                        barWidth = 70;
                        xOffset = 45;
                        yOffsetStart = 18;
                        yOffsetEnd = 23;
                        break;

                    case "Red_Minion_Wizard":
                    case "Blue_Minion_Wizard":
                        barWidth = 75;
                        xOffset = 45;
                        yOffsetStart = 18;
                        yOffsetEnd = 23;
                        break;

                    case "Red_Minion_MechCannon":
                    case "Blue_Minion_MechCannon":
                        barWidth = 65;
                        xOffset = 45;
                        yOffsetStart = 18;
                        yOffsetEnd = 23;
                        break;

                    case "Red_Minion_MechMelee":
                    case "Blue_Minion_MechMelee":
                        barWidth = 80;
                        xOffset = 45;
                        yOffsetStart = 18;
                        yOffsetEnd = 23;
                        break;

                    case "TT_NWraith":
                        barWidth = 68;
                        xOffset = 44;
                        yOffsetStart = 19;
                        yOffsetEnd = 23;
                        break;

                    case "TT_NWraith2":
                        barWidth = 65;
                        xOffset = 42;
                        yOffsetStart = 19;
                        yOffsetEnd = 23;
                        break;

                    case "TT_NGolem":
                        barWidth = 68;
                        xOffset = 44;
                        yOffsetStart = 19;
                        yOffsetEnd = 23;
                        break;

                    case "TT_NGolem2":
                        barWidth = 65;
                        xOffset = 42;
                        yOffsetStart = 19;
                        yOffsetEnd = 23;
                        break;

                    case "TT_NWolf":
                        barWidth = 68;
                        xOffset = 44;
                        yOffsetStart = 19;
                        yOffsetEnd = 23;
                        break;

                    case "TT_NWolf2":
                        barWidth = 65;
                        xOffset = 42;
                        yOffsetStart = 19;
                        yOffsetEnd = 23;
                        break;

                    case "TT_Spiderboss":
                        barWidth = 125;
                        xOffset = 88;
                        yOffsetStart = 19;
                        yOffsetEnd = 23;
                        lineSize = 2;
                        break;

                    //CrystalScare

                    case "Odin_Red_Minion_Caster":
                    case "Odin_Blue_Minion_Caster":
                        barWidth = 75;
                        xOffset = 40;
                        yOffsetStart = 18;
                        yOffsetEnd = 23;
                        break;

                    case "OdinRedSuperminion":
                    case "OdinBlueSuperminion":
                        barWidth = 65;
                        xOffset = 45;
                        yOffsetStart = 18;
                        yOffsetEnd = 23;
                        break;

                    //Howling Abyss

                    case "HA_ChaosMinionMelee":
                    case "HA_OrderMinionMelee":
                        barWidth = 70;
                        xOffset = 45;
                        yOffsetStart = 18;
                        yOffsetEnd = 23;
                        break;

                    case "HA_ChaosMinionRanged":
                    case "HA_OrderMinionRanged":
                        barWidth = 75;
                        xOffset = 45;
                        yOffsetStart = 18;
                        yOffsetEnd = 23;
                        break;

                    case "HA_ChaosMinionSiege":
                    case "HA_OrderMinionSiege":
                        barWidth = 65;
                        xOffset = 45;
                        yOffsetStart = 18;
                        yOffsetEnd = 23;
                        break;

                    case "HA_ChaosMinionSuper":
                    case "HA_OrderMinionSuper":
                        barWidth = 80;
                        xOffset = 45;
                        yOffsetStart = 18;
                        yOffsetEnd = 23;
                        break;

                    default:
                        return;
                }
                var barDistance = barWidth / attackToKill;
                for (var i = 0; i < attackToKill; i++)
                {
                    if (i != 0)
                    {
                        if (attackToKill > 20 && attackToKill < 50)
                        {
                            if (i % 5 != 0)
                            {
                                continue;
                            }
                        }
                        else if (attackToKill >= 50)
                        {
                            if (i % 10 != 0)
                            {
                                continue;
                            }
                        }
                        var start = new Vector2(
                            hpBarPosition.X + xOffset + (float) (barDistance) * i, hpBarPosition.Y + yOffsetStart);
                        var end = new Vector2(
                            hpBarPosition.X + xOffset + ((float) (barDistance) * i), hpBarPosition.Y + yOffsetEnd);
                        if(Common.IsOnScreen(start) && Common.IsOnScreen(end))
                        {
                            Drawing.DrawLine(start, end, lineSize,
                                (minion.Health <= ObjectManager.Player.GetAutoAttackDamage(minion, true) ? System.Drawing.Color.Red : System.Drawing.Color.Black));
                        }
                    }
                }
            }
        }
    }
}
