using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SAssemblies;
using SAssemblies.Miscs;
using Menu = SAssemblies.Menu;

namespace SAwareness.Miscs
{
    class AntiJump
    {
        public static Menu.MenuItemSettings AntiJumpMisc = new Menu.MenuItemSettings(typeof(AntiJump));
        public static Champ Champion = null;

        public AntiJump()
        {
            switch (ObjectManager.Player.ChampionName)
            {
                case "Azir":
                    Champion = new Champ("Azir", 250, SpellSlot.R, true);
                    break;

                case "Cassiopeia":
                    Champion = new Champ("Cassiopeia", 825, SpellSlot.R, true);
                    break;

                case "Draven":
                    Champion = new Champ("Draven", 1000, SpellSlot.E, true);
                    break;

                case "Syndra":
                    Champion = new Champ("Syndra", 700, SpellSlot.E, true);
                    break;

                case "Thresh":
                    Champion = new Champ("Thresh", 700, SpellSlot.E, true);
                    break;

                case "Tristana":
                    Champion = new Champ("Tristana", 500, SpellSlot.R, false);
                    break;

                case "Quinn":
                    Champion = new Champ("Quinn", 700, SpellSlot.E, false);
                    break;

                case "Vayne":
                    Champion = new Champ("Vayne", 500, SpellSlot.E, false);
                    break;

                default:
                    return;
            }
            Obj_AI_Hero.OnPlayAnimation += Obj_AI_Hero_OnPlayAnimation;
        }

        ~AntiJump()
        {
            Obj_AI_Hero.OnPlayAnimation -= Obj_AI_Hero_OnPlayAnimation;
        }

        public bool IsActive()
        {
#if MISCS
            return Misc.Miscs.GetActive() && AntiJumpMisc.GetActive();
#else
            return AntiJumpMisc.GetActive();
#endif
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            AntiJumpMisc.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("MISCS_ANTIJUMP_MAIN"), "SAssembliesMiscsAntiJump"));
            AntiJumpMisc.MenuItems.Add(
                AntiJumpMisc.Menu.AddItem(new MenuItem("SAssembliesMiscsAntiJumpActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return AntiJumpMisc;
        }

        void Obj_AI_Hero_OnPlayAnimation(Obj_AI_Base sender, GameObjectPlayAnimationEventArgs args)
        {
            if (sender is Obj_AI_Hero)
            {
                var hero = (Obj_AI_Hero)sender;
                if (hero.Team != ObjectManager.Player.Team)
                {
                    if (IsJumping(hero, args.Animation))
                    {
                        if (Champion.SpellSlot.IsReady())
                        {
                            if (Champion.PosSpell)
                            {
                                ObjectManager.Player.Spellbook.CastSpell(Champion.SpellSlot, hero.ServerPosition);
                            }
                            else
                            {
                                ObjectManager.Player.Spellbook.CastSpell(Champion.SpellSlot, hero);
                            }
                        }
                    }
                }
            }
        }

        bool IsJumping(Obj_AI_Hero champion, String animation)
        {
            if (ObjectManager.Player.Distance(champion) <= Champion.Range)
            {
                switch (champion.ChampionName)
                {
                    case "Rengar":
                        if (animation.Contains("Spell5"))
                            return true;
                        break;

                    case "Khazix":
                        if (animation.Contains("Spell3"))
                            return true;
                        break;
                }
            }
            return false;
        }

        internal class Champ
        {
            public String Name;
            public SpellSlot SpellSlot;
            public int Range;
            public bool PosSpell;
            public Champ(string name, int range, SpellSlot spellSlot, bool posSpell)
            {
                Name = name;
                Range = range;
                SpellSlot = spellSlot;
                PosSpell = posSpell;
            }
        }
    }
}
