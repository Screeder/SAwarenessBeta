using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SAwareness.Trackers
{
    class Jungler
    {
        public static Menu.MenuItemSettings JunglerTracker = new Menu.MenuItemSettings(typeof(Jungler));

        public Jungler()
        {
            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsEnemy && hero.Spellbook.Spells.Find(inst => inst.Name.ToLower().Contains("smite")) != null)
                {
                    Render.Text text = new Render.Text(Drawing.Width / 2, Drawing.Height / 2 + 400, "", 20, Color.AliceBlue);
                    text.TextUpdate = delegate
                    {
                        return MapPositions.GetRegion(hero.ServerPosition.To2D()).ToString();
                    };
                    text.VisibleCondition = sender =>
                    {
                        return IsActive() && hero.IsVisible && !hero.IsDead;
                    };
                    text.OutLined = true;
                    text.Centered = true;
                    text.Add();
                }
            }
        }

        ~Jungler()
        {

        }

        public bool IsActive()
        {
            return Tracker.Trackers.GetActive() && JunglerTracker.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            JunglerTracker.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("TRACKERS_JUNGLER_MAIN"), "SAwarenessTrackersJungler"));
            JunglerTracker.MenuItems.Add(
                JunglerTracker.Menu.AddItem(new MenuItem("SAwarenessTrackersJunglerActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return JunglerTracker;
        }
    }
}
