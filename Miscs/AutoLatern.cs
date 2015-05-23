using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAssemblies.Miscs
{
    class AutoLatern
    {
        public static Menu.MenuItemSettings AutoLaternMisc = new Menu.MenuItemSettings(typeof(AutoLatern));
        private int lastGameUpdateTime = 0;

        public AutoLatern()
        {
            Game.OnUpdate += Game_OnGameUpdate;
        }

        ~AutoLatern()
        {
            Game.OnUpdate -= Game_OnGameUpdate;
        }

        public bool IsActive()
        {
#if MISCS
            return Misc.Miscs.GetActive() && AutoLaternMisc.GetActive();
#else
            return AutoLaternMisc.GetActive();
#endif
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            AutoLaternMisc.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("MISCS_AUTOLATERN_MAIN"), "SAssembliesMiscsAutoLatern"));
            AutoLaternMisc.MenuItems.Add(
                AutoLaternMisc.Menu.AddItem(new MenuItem("SAssembliesMiscsAutoLaternKey", Language.GetString("GLOBAL_KEY")).SetValue(new KeyBind(84, KeyBindType.Press))));
            AutoLaternMisc.MenuItems.Add(
                AutoLaternMisc.Menu.AddItem(new MenuItem("SAssembliesMiscsAutoLaternActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return AutoLaternMisc;
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive() || !AutoLaternMisc.GetMenuItem("SAssembliesMiscsAutoLaternKey").GetValue<KeyBind>().Active || lastGameUpdateTime + new Random().Next(500, 1000) > Environment.TickCount)
                return;

            lastGameUpdateTime = Environment.TickCount;

            foreach (GameObject gObject in ObjectManager.Get<GameObject>())
            {
                if (gObject.Name.Contains("ThreshLantern") && gObject.IsAlly &&
                    gObject.Position.Distance(ObjectManager.Player.ServerPosition) < 400 &&
                    !ObjectManager.Player.ChampionName.Contains("Thresh"))
                {
                    ((Obj_AI_Base) gObject).UseObject();
                    ObjectManager.Player.Spellbook.CastSpell((SpellSlot) 62, gObject);
                }
            }
        }
    }
}
