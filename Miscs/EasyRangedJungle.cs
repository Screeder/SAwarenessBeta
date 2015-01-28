using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SAwareness.Miscs
{
    class EasyRangedJungle
    {
        public static Menu.MenuItemSettings EasyRangedJungleMisc = new Menu.MenuItemSettings(typeof(EasyRangedJungle));
        private List<Vector3> spots = new List<Vector3>();

        public EasyRangedJungle()
        {
            spots.Add(new Vector3(7600f, 3140f, 60f));
            spots.Add(new Vector3(7160, 4600f, 60f));
            spots.Add(new Vector3(4570f, 6170f, 60f));
            spots.Add(new Vector3(3370f, 8610f, 60f));
            spots.Add(new Vector3(7650f, 2120f, 60f));
            spots.Add(new Vector3(7320f, 11610f, 60f));
            spots.Add(new Vector3(7290f, 10090f, 60f));
            spots.Add(new Vector3(10220f, 9000f, 60f));
            spots.Add(new Vector3(11550f, 6230f, 60f));
            spots.Add(new Vector3(7120f, 12800f, 60f));
            spots.Add(new Vector3(10930f, 5400f, 60f));
            Drawing.OnEndScene += Drawing_OnEndScene;
        }

        ~EasyRangedJungle()
        {
            Drawing.OnEndScene -= Drawing_OnEndScene;
        }

        public bool IsActive()
        {
            return Misc.Miscs.GetActive() && EasyRangedJungleMisc.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            EasyRangedJungleMisc.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("MISCS_EASYRANGEDJUNGLE_MAIN"), "SAwarenessMiscsEasyRangedJungle"));
            EasyRangedJungleMisc.MenuItems.Add(
                EasyRangedJungleMisc.Menu.AddItem(new MenuItem("SAwarenessMiscsEasyRangedJungleActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return EasyRangedJungleMisc;
        }

        private void Drawing_OnEndScene(EventArgs args)
        {
            if (!IsActive())
                return;

            foreach (var spot in spots)
            {
                if (spot.IsOnScreen())
                {
                    Utility.DrawCircle(spot, 50, System.Drawing.Color.Fuchsia);
                }
            }
        }
    }
}
