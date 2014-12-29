using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAwareness.Wards
{
    class InvisibleRevealer
    {
        public static Menu.MenuItemSettings InvisibleRevealerWard = new Menu.MenuItemSettings(typeof(InvisibleRevealer));

        private List<String> _spellList = new List<string>();
        private int _lastTimeVayne;
        private int _lastTimeWarded;

        public InvisibleRevealer() //Passive Evelynn, Teemo Missing
        {
            _spellList.Add("AkaliSmokeBomb"); //Akali W
            _spellList.Add("RengarR"); //Rengar R
            _spellList.Add("KhazixR"); //Kha R
            _spellList.Add("khazixrlong"); //Kha R Evolved
            _spellList.Add("Deceive"); //Shaco Q
            _spellList.Add("TalonShadowAssault"); //Talon R
            _spellList.Add("HideInShadows"); //Twitch Q
            _spellList.Add("VayneTumble");
            //Vayne Q -> Check before if args.SData.Name == "vayneinquisition" then ability.ExtraTicks = (int)Game.Time + 6 + 2 * args.Level; if (Game.Time >= ability.ExtraTicks) return;
            _spellList.Add("MonkeyKingDecoy"); //Wukong W

            Obj_AI_Base.OnProcessSpellCast += ObjAiBase_OnProcessSpellCast;
        }

        ~InvisibleRevealer()
        {
            Obj_AI_Base.OnProcessSpellCast -= ObjAiBase_OnProcessSpellCast;
            _spellList = null;
        }

        public bool IsActive()
        {
            return Ward.Wards.GetActive() && InvisibleRevealerWard.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            InvisibleRevealerWard.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("WARDS_INVISIBLEREVEALER_MAIN"), "SAwarenessWardsInvisibleRevealer"));
            InvisibleRevealerWard.MenuItems.Add(
                InvisibleRevealerWard.Menu.AddItem(new MenuItem("SAwarenessWardsInvisibleRevealerMode", Language.GetString("GLOBAL_MODE")).SetValue(new StringList(new[]
                {
                    Language.GetString("WARDS_INVISIBLEREVEALER_MODE_MANUAL"), 
                    Language.GetString("WARDS_INVISIBLEREVEALER_MODE_AUTOMATIC")
                }))));
            InvisibleRevealerWard.MenuItems.Add(
                InvisibleRevealerWard.Menu.AddItem(new MenuItem("SAwarenessWardsInvisibleRevealerKey", Language.GetString("GLOBAL_KEY")).SetValue(new KeyBind(32, KeyBindType.Press))));
            InvisibleRevealerWard.MenuItems.Add(
                InvisibleRevealerWard.Menu.AddItem(new MenuItem("SAwarenessWardsInvisibleRevealerActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return InvisibleRevealerWard;
        }

        private void ObjAiBase_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!IsActive())
                return;

            var mode =
                InvisibleRevealerWard.GetMenuItem("SAwarenessWardsInvisibleRevealerMode")
                    .GetValue<StringList>();

            if (sender.IsEnemy && sender.IsValid && !sender.IsDead)
            {
                if (args.SData.Name.ToLower().Contains("vayneinquisition"))
                {
                    _lastTimeVayne = Environment.TickCount + 6000 + 2000 * args.Level;
                }
                if (mode.SelectedIndex == 0 &&
                    InvisibleRevealerWard.GetMenuItem("SAwarenessWardsInvisibleRevealerKey").GetValue<KeyBind>().Active ||
                    mode.SelectedIndex == 1)
                {
                    if (_spellList.Exists(x => x.ToLower().Contains(args.SData.Name.ToLower())))
                    {
                        if (_lastTimeWarded == 0 || Environment.TickCount - _lastTimeWarded > 500)
                        {
                            Ward.WardItem wardItem =
                                Ward.WardItems.First(
                                    x =>
                                        Items.HasItem(x.Id) && Items.CanUseItem(x.Id) && (x.Type == Ward.WardType.Vision || x.Type == Ward.WardType.TempVision));
                            if (wardItem == null)
                                return;
                            if (sender.ServerPosition.Distance(ObjectManager.Player.ServerPosition) > wardItem.Range)
                                return;

                            InventorySlot invSlot =
                                ObjectManager.Player.InventoryItems.FirstOrDefault(
                                    slot => slot.Id == (ItemId)wardItem.Id);
                            if (invSlot == null)
                                return;

                            if (args.SData.Name.ToLower().Contains("vaynetumble") &&
                                Environment.TickCount >= _lastTimeVayne)
                                return;

                            ObjectManager.Player.Spellbook.CastSpell(invSlot.SpellSlot, args.End);
                            _lastTimeWarded = Environment.TickCount;
                        }
                    }
                }
            }
        }
    }
}
