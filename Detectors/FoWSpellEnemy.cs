using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SAwareness.Detectors
{
    class FoWSpellEnemy
    {
        public static Menu.MenuItemSettings FoWSpellEnemyDetector = new Menu.MenuItemSettings(typeof(FoWSpellEnemy));

        private SpellSlot spell = SpellSlot.Unknown;
        private Render.Text text;

        public FoWSpellEnemy()
        {
            switch (ObjectManager.Player.ChampionName)
            {
                case "Evelynn":
                    spell = SpellSlot.Q;
                    break;

                case "Katarina":
                    spell = SpellSlot.R;
                    break;

                case "Morgana":
                    spell = SpellSlot.R;
                    break;

                case "Tryndamere":
                    spell = SpellSlot.W;
                    break;
            }
            if (spell != SpellSlot.Unknown)
            {
                text = new Render.Text(0, 0, "", 24, SharpDX.Color.OrangeRed);
                text.TextUpdate = delegate
                {
                    return "";
                };
                text.PositionUpdate = delegate
                {
                    return new Vector2(Drawing.Width / 2, 100);
                };
                text.VisibleCondition = sender =>
                {
                    return Detector.Detectors.GetActive() && FoWSpellEnemyDetector.GetActive() && ObjectManager.Player.Spellbook.CanUseSpell(spell) == SpellState.Ready;
                };
                text.OutLined = true;
                text.Centered = true;
                text.Add(4);
            }
        }

        ~FoWSpellEnemy()
        {
            if (text != null)
            {
                text.Dispose();
            }
        }

        public bool IsActive()
        {
            return Detector.Detectors.GetActive() && FoWSpellEnemyDetector.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            FoWSpellEnemyDetector.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("DETECTORS_FOWSPELLENEMY_MAIN"), "SAwarenessDetectorsFoWSpellEnemy"));
            FoWSpellEnemyDetector.MenuItems.Add(
                FoWSpellEnemyDetector.Menu.AddItem(new MenuItem("SAwarenessDetectorsFoWSpellEnemyActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return FoWSpellEnemyDetector;
        }
    }
}
