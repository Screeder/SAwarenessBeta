using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAwareness.Miscs
{
    class AutoLatern
    {
        public static Menu.MenuItemSettings AutoLaternMisc = new Menu.MenuItemSettings(typeof(AutoLatern));

        public AutoLatern()
        {
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        ~AutoLatern()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
        }

        public bool IsActive()
        {
            return Misc.Miscs.GetActive() && AutoLaternMisc.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            AutoLaternMisc.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("MISCS_AUTOLATERN_MAIN"), "SAwarenessMiscsAutoLatern"));
            AutoLaternMisc.MenuItems.Add(
                AutoLaternMisc.Menu.AddItem(new MenuItem("SAwarenessMiscsAutoLaternKey", Language.GetString("GLOBAL_KEY")).SetValue(new KeyBind(84, KeyBindType.Press))));
            AutoLaternMisc.MenuItems.Add(
                AutoLaternMisc.Menu.AddItem(new MenuItem("SAwarenessMiscsAutoLaternActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return AutoLaternMisc;
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive() || !AutoLaternMisc.GetMenuItem("SAwarenessMiscsAutoLaternKey").GetValue<KeyBind>().Active)
                return;

            foreach (GameObject gObject in ObjectManager.Get<GameObject>())
            {
                if (gObject.Name.Contains("ThreshLantern") && gObject.IsAlly &&
                    gObject.Position.Distance(ObjectManager.Player.ServerPosition) < 400 &&
                    !ObjectManager.Player.ChampionName.Contains("Thresh"))
                {
                    GamePacket gPacket =
                        Packet.C2S.InteractObject.Encoded(
                            new Packet.C2S.InteractObject.Struct(ObjectManager.Player.NetworkId,
                                gObject.NetworkId));
                    gPacket.Send();
                }
            }
        }
    }
}
