using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAssemblies.Miscs
{
    internal class MoveToMouse
    {
        public static Menu.MenuItemSettings MoveToMouseMisc = new Menu.MenuItemSettings(typeof(MoveToMouse));

        private int lastGameUpdateTime = 0;

        public MoveToMouse()
        {
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        ~MoveToMouse()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
        }

        public bool IsActive()
        {
            return Misc.Miscs.GetActive() && MoveToMouseMisc.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            MoveToMouseMisc.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("MISCS_MOVETOMOUSE_MAIN"), "SAssembliesMiscsMoveToMouse"));
            MoveToMouseMisc.MenuItems.Add(
                MoveToMouseMisc.Menu.AddItem(new MenuItem("SAssembliesMiscsMoveToMouseKey", Language.GetString("GLOBAL_KEY")).SetValue(new KeyBind(90, KeyBindType.Press))));
            MoveToMouseMisc.MenuItems.Add(
                MoveToMouseMisc.Menu.AddItem(new MenuItem("SAssembliesMiscsMoveToMouseActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return MoveToMouseMisc;
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive() || !MoveToMouseMisc.GetMenuItem("SAssembliesMiscsMoveToMouseKey").GetValue<KeyBind>().Active || lastGameUpdateTime + new Random().Next(500, 1000) > Environment.TickCount)
                return;

            lastGameUpdateTime = Environment.TickCount;

            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
        }
    }
}