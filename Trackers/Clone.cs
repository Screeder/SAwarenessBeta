using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAssemblies.Trackers
{
    class Clone
    {
        public static Menu.MenuItemSettings CloneTracker = new Menu.MenuItemSettings(typeof(Clone));

        public Clone()
        {
            Drawing.OnDraw += Drawing_OnDraw;
        }

        ~Clone()
        {
            Drawing.OnDraw -= Drawing_OnDraw;
        }

        public bool IsActive()
        {
#if TRACKERS
            return Tracker.Trackers.GetActive() && CloneTracker.GetActive();
#else
            return CloneTracker.GetActive();
#endif
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            CloneTracker.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("TRACKERS_CLONE_MAIN"), "SAssembliesTrackersClone"));
            CloneTracker.MenuItems.Add(
                CloneTracker.Menu.AddItem(new MenuItem("SAssembliesTrackersCloneActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return CloneTracker;
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!IsActive())
                return;

            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsEnemy && !hero.IsDead && hero.IsVisible)
                {
                    if (hero.ChampionName.Contains("Shaco") ||
                        hero.ChampionName.Contains("Leblanc") ||
                        hero.ChampionName.Contains("MonkeyKing") ||
                        hero.ChampionName.Contains("Yorick"))
                    {
                        if (hero.ServerPosition.IsOnScreen())
                        {
                            Utility.DrawCircle(hero.ServerPosition, 100, Color.Red);
                            Utility.DrawCircle(hero.ServerPosition, 110, Color.Red);
                        }
                    }
                    
                }
            }
        }
    }
}
