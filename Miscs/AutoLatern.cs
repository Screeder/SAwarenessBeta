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
        private int lastGameUpdateTime = 0;

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
            if (!IsActive() || !AutoLaternMisc.GetMenuItem("SAwarenessMiscsAutoLaternKey").GetValue<KeyBind>().Active || lastGameUpdateTime + new Random().Next(500, 1000) > Environment.TickCount)
                return;

            lastGameUpdateTime = Environment.TickCount;

            foreach (GameObject gObject in ObjectManager.Get<GameObject>())
            {
                if (gObject.Name.Contains("ThreshLantern") && gObject.IsAlly &&
                    gObject.Position.Distance(ObjectManager.Player.ServerPosition) < 400 &&
                    !ObjectManager.Player.ChampionName.Contains("Thresh"))

                {
                    //Game.SendPacket(
                    //new PKT_InteractReq
                    //{
                    //    NetworkId = ObjectManager.Player.NetworkId,
                    //    TargetNetworkId = gObject.NetworkId
                    //}.Encode(), PacketChannel.C2S, PacketProtocolFlags.NoFlags);
                }
            }
        }
    }
}
