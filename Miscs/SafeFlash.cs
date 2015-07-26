using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SAssemblies.Miscs
{
    class SafeFlash
    {
        public static Menu.MenuItemSettings SafeFlashMisc = new Menu.MenuItemSettings(typeof(SafeFlash));

        public SafeFlash()
        {
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
        }

        ~SafeFlash()
        {
            Spellbook.OnCastSpell -= Spellbook_OnCastSpell;
        }

        public bool IsActive()
        {
#if MISCS
            return Misc.Miscs.GetActive() && SafeFlashMisc.GetActive();
#else
            return TurnAroundMisc.GetActive();
#endif
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            SafeFlashMisc.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("MISCS_SAFEFLASH_MAIN"), "SAssembliesMiscsSafeFlash"));
            SafeFlashMisc.MenuItems.Add(SafeFlashMisc.CreateActiveMenuItem("SAssembliesMiscsSafeFlashActive", () => new SafeFlash()));
            return SafeFlashMisc;
        }

        void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (!IsActive() || SummonerSpells.GetFlashSlot() == args.Slot || !sender.Owner.IsMe)
                return;

            var startWall = GetStartWall();
            if (!startWall.IsZero && Game.CursorPos.IsWall() && ObjectManager.Player.Distance(startWall) < 1000)
            {
                args.Process = false;
                ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, startWall);

                return;
            }

            for (var dist = ObjectManager.Player.Distance(Game.CursorPos); dist < 800; dist += 25)
            {
                var curPos = ObjectManager.Player.Position.Extend(Game.CursorPos, dist);
                if (!curPos.IsWall())
                {
                    ObjectManager.Player.Spellbook.CastSpell(args.Slot, curPos, false);
                    return;
                }
            }

            if (ObjectManager.Player.ServerPosition.To2D().Distance(args.StartPosition) < 390f)
            {
                args.Process = false;
                ObjectManager.Player.Spellbook.CastSpell(args.Slot, ObjectManager.Player.ServerPosition.Extend(args.StartPosition, 400f));
            }
        }

        public static Vector3 GetStartWall()
        {
            Vector3 from = ObjectManager.Player.Position;
            Vector3 to = Game.CursorPos;
            Vector3 dir = (to - from).Normalized();
            int inc = 10;

            for (float d = 0; d < from.Distance(to); d = d + inc)
            {
                var wallCheck = (d * dir) + from;
                if (wallCheck.IsWall())
                {
                    return ((d - inc) * dir) + from;
                }
            }
            return new Vector3();
        }
    }
}
