using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAssemblies.Activators
{
    class AutoUlt
    {
        private static Ult _ult;
        public static Menu.MenuItemSettings AutoUltActivator = new Menu.MenuItemSettings(typeof(AutoUlt));

        public AutoUlt()
        {
            if (_ult == null)
            {
                Init();
            }
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        ~AutoUlt()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
        }

        public bool IsActive()
        {
            return Activator.Activators.GetActive() && AutoUltActivator.GetActive();
        }

        private static void SetupMainMenu()
        {
            var menu = new LeagueSharp.Common.Menu("SAutoUlt", "SAssembliesSActivatorsAutoUlt", true);
            SetupMenu(menu);
            menu.AddToMainMenu();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            AutoUltActivator.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("ACTIVATORS_AUTOULT_MAIN"), "SAssembliesActivatorsAutoUlt"));
            AutoUltActivator.MenuItems.Add(
                AutoUltActivator.Menu.AddItem(new MenuItem("SAssembliesActivatorsAutoUltAlly", Language.GetString("ACTIVATORS_AUTOULT_ALLY")).SetValue(false)));
            AutoUltActivator.MenuItems.Add(
                AutoUltActivator.Menu.AddItem(new MenuItem("SAssembliesActivatorsAutoUltActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return AutoUltActivator;
        }

        private static void Init()
        {
            switch (ObjectManager.Player.ChampionName)
            {
                case "Kayle":
                    _ult = new Ult(UltType.Invincible, SpellSlot.E, 900);
                    break;

                case "Lissandra":
                    _ult = new Ult(UltType.Invincible, SpellSlot.R, 0);
                    break;

                case "Lulu":
                    _ult = new Ult(UltType.Invincible, SpellSlot.R, 900);
                    break;

                case "Shen":
                    _ult = new Ult(UltType.Global, SpellSlot.R, 90000);
                    break;

                case "Soraka":
                    _ult = new Ult(UltType.Global, SpellSlot.R, 90000);
                    break;

                case "Tryndamere":
                    _ult = new Ult(UltType.Invincible, SpellSlot.R, 0, false);
                    break;

                case "Yorick":
                    _ult = new Ult(UltType.Invincible, SpellSlot.R, 900);
                    break;

                case "Zilean":
                    _ult = new Ult(UltType.Invincible, SpellSlot.R, 900);
                    break;

                default:
                    return;
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive() || _ult == null || ObjectManager.Player.Spellbook.GetSpell(_ult.SpellSlot).State != SpellState.Ready)
                return;

            var tempDamages =
                new Dictionary<Obj_AI_Hero, List<ActivatorOld.IncomingDamage>>(ActivatorOld.Damages);
            foreach (var damage in tempDamages)
            {
                if (ActivatorOld.CalcMaxDamage(damage.Key) > damage.Key.Health &&
                    (damage.Key.Distance(ObjectManager.Player.ServerPosition) < _ult.Range))
                {
                    if (!AutoUltActivator.GetMenuItem("SAssembliesActivatorsAutoUltAlly").GetValue<bool>() &&
                        damage.Key.NetworkId != ObjectManager.Player.NetworkId)
                    {
                        continue;
                    }
                    if (_ult.Target)
                    {
                        Packet.C2S.Cast.Encoded(new Packet.C2S.Cast.Struct(damage.Key.NetworkId, _ult.SpellSlot)).Send();
                        return;
                    }
                    if (damage.Key.NetworkId == ObjectManager.Player.NetworkId)
                    {
                        Packet.C2S.Cast.Encoded(new Packet.C2S.Cast.Struct(damage.Key.NetworkId, _ult.SpellSlot)).Send();
                        return;
                    }
                }
            }
        }

        enum UltType
        {
            Invincible,
            Global
        }

        private class Ult
        {
            public UltType UltType;
            public SpellSlot SpellSlot;
            public double Range;
            public bool Target;

            public Ult(UltType ultType, SpellSlot spellSlot, double range, bool target = true)
            {
                UltType = ultType;
                SpellSlot = spellSlot;
                Range = range;
                Target = target;
            }
        }
    }
}
