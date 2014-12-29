using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAwareness.Miscs
{
    internal class MoveToMouse
    {
        public static Menu.MenuItemSettings MoveToMouseMisc = new Menu.MenuItemSettings(typeof(MoveToMouse));

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

        private void FreeReferences()
        {
            if (MoveToMouseMisc.Item == null)
                Game.OnGameUpdate -= Game_OnGameUpdate;
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            MoveToMouseMisc.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("MISCS_MOVETOMOUSE_MAIN"), "SAwarenessMiscsMoveToMouse"));
            MoveToMouseMisc.MenuItems.Add(
                MoveToMouseMisc.Menu.AddItem(new MenuItem("SAwarenessMiscsMoveToMouseKey", Language.GetString("GLOBAL_KEY")).SetValue(new KeyBind(90, KeyBindType.Press))));
            MoveToMouseMisc.MenuItems.Add(
                MoveToMouseMisc.Menu.AddItem(new MenuItem("SAwarenessMiscsMoveToMouseActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return MoveToMouseMisc;
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            FreeReferences();
            if (!IsActive() || !MoveToMouseMisc.GetMenuItem("SAwarenessMiscsMoveToMouseKey").GetValue<KeyBind>().Active)
                return;

            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
        }
    }
}