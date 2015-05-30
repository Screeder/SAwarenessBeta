using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SAssemblies.Wards
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
            GameObject.OnCreate += GameObject_OnCreate;
        }

        ~InvisibleRevealer()
        {
            Obj_AI_Base.OnProcessSpellCast -= ObjAiBase_OnProcessSpellCast;
            GameObject.OnCreate -= GameObject_OnCreate;
            _spellList = null;
        }

        public bool IsActive()
        {
#if WARDS
            return Ward.Wards.GetActive() && InvisibleRevealerWard.GetActive();
#else
            return InvisibleRevealerWard.GetActive();
#endif
        }

        private static void SetupMainMenu()
        {
            var menu = new LeagueSharp.Common.Menu("SAssembliesInvisibleRevealer", "SAssembliesWardsInvisibleRevealer", true);
            SetupMenu(menu);
            menu.AddToMainMenu();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            InvisibleRevealerWard.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("WARDS_INVISIBLEREVEALER_MAIN"), "SAssembliesWardsInvisibleRevealer"));
            InvisibleRevealerWard.MenuItems.Add(
                InvisibleRevealerWard.Menu.AddItem(new MenuItem("SAssembliesWardsInvisibleRevealerMode", Language.GetString("GLOBAL_MODE")).SetValue(new StringList(new[]
                {
                    Language.GetString("GLOBAL_MODE_MANUAL"), 
                    Language.GetString("GLOBAL_MODE_AUTOMATIC")
                }))));
            InvisibleRevealerWard.MenuItems.Add(
                InvisibleRevealerWard.Menu.AddItem(new MenuItem("SAssembliesWardsInvisibleRevealerKey", Language.GetString("GLOBAL_KEY")).SetValue(new KeyBind(32, KeyBindType.Press))));
            InvisibleRevealerWard.MenuItems.Add(
                InvisibleRevealerWard.Menu.AddItem(new MenuItem("SAssembliesWardsInvisibleRevealerActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return InvisibleRevealerWard;
        }

        private void ObjAiBase_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!IsActive())
                return;

            var mode =
                InvisibleRevealerWard.GetMenuItem("SAssembliesWardsInvisibleRevealerMode")
                    .GetValue<StringList>();

            if (sender.IsEnemy && sender.IsValid && !sender.IsDead)
            {
                if (args.SData.Name.ToLower().Contains("vayneinquisition"))
                {
                    _lastTimeVayne = Environment.TickCount + 6000 + 2000 * args.Level;
                }
                if (mode.SelectedIndex == 0 &&
                    InvisibleRevealerWard.GetMenuItem("SAssembliesWardsInvisibleRevealerKey").GetValue<KeyBind>().Active ||
                    mode.SelectedIndex == 1)
                {
                    if (_spellList.Exists(x => x.ToLower().Contains(args.SData.Name.ToLower())))
                    {
                        if (_lastTimeWarded == 0 || Environment.TickCount - _lastTimeWarded > 500)
                        {
                            if (args.SData.Name.ToLower().Contains("vaynetumble") &&
                                Environment.TickCount >= _lastTimeVayne)
                                return;

                            InventorySlot invSlot = GetWardItemSlot(sender);
                            if (invSlot != null)
                            {
                                ObjectManager.Player.Spellbook.CastSpell(invSlot.SpellSlot, args.End);
                                _lastTimeWarded = Environment.TickCount;
                            }
                            else if (ObjectManager.Player.ChampionName.Equals("LeeSin") && ObjectManager.Player.Spellbook.CanUseSpell(SpellSlot.E) == SpellState.Ready &&
                                sender.ServerPosition.Distance(ObjectManager.Player.ServerPosition) < 350)
                            {
                                ObjectManager.Player.Spellbook.CastSpell(SpellSlot.E);
                                _lastTimeWarded = Environment.TickCount;
                            }
                        }
                    }
                }
            }
        }

        private void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (!IsActive())
                return;

            var mode =
                InvisibleRevealerWard.GetMenuItem("SAssembliesWardsInvisibleRevealerMode")
                    .GetValue<StringList>();

            if (sender.IsEnemy && sender.IsValid && !sender.IsDead)
            {
                if (mode.SelectedIndex == 0 &&
                    InvisibleRevealerWard.GetMenuItem("SAssembliesWardsInvisibleRevealerKey").GetValue<KeyBind>().Active ||
                    mode.SelectedIndex == 1)
                {
                    if (_lastTimeWarded == 0 || Environment.TickCount - _lastTimeWarded > 500)
                    {
                        Vector3? endPos = null;
                        if (sender.IsEnemy && sender.Name.Contains("Rengar_Base_R_Alert")) //Rengar
                        {
                            Obj_AI_Hero rengar = HeroManager.Enemies.Find(champ => champ.ChampionName.ToLower() == "rengar");
                            if (ObjectManager.Player.HasBuff("rengarralertsound") && rengar.IsValid && !rengar.IsVisible && !rengar.IsDead)
                            {
                                endPos = ObjectManager.Player.Position;
                            }
                        }
                        if (sender.IsEnemy && sender.Name == "LeBlanc_Base_P_poof.troy") //Leblanc
                        {
                            Obj_AI_Hero leblanc = HeroManager.Enemies.Find(champ => champ.ChampionName.ToLower() == "leblanc");
                            if (leblanc.IsValid && !leblanc.IsVisible && !leblanc.IsDead)
                            {
                                endPos = ObjectManager.Player.Position;
                            }
                        }
                        InventorySlot invSlot = GetWardItemSlot(sender);
                        if (invSlot != null)
                        {
                            if (endPos != null)
                            {
                                ObjectManager.Player.Spellbook.CastSpell(invSlot.SpellSlot, endPos.Value);
                                _lastTimeWarded = Environment.TickCount;
                            }
                        }
                        else if (endPos != null && (ObjectManager.Player.ChampionName.Equals("LeeSin") && ObjectManager.Player.Spellbook.CanUseSpell(SpellSlot.E) == SpellState.Ready &&
                                                    endPos.Value.Distance(ObjectManager.Player.ServerPosition) < 350))
                        {
                            ObjectManager.Player.Spellbook.CastSpell(SpellSlot.E);
                            _lastTimeWarded = Environment.TickCount;
                        }
                    }
                }
            }
        }

        private InventorySlot GetWardItemSlot(GameObject sender)
        {
            SAssemblies.Ward.WardItem wardItem =
                                SAssemblies.Ward.WardItems.FirstOrDefault(
                                    x =>
                                        Items.HasItem(x.Id) && Items.CanUseItem(x.Id) && (x.Type == SAssemblies.Ward.WardType.Vision || x.Type == SAssemblies.Ward.WardType.TempVision));
            if (wardItem == null)
                return null;
            if (sender.Position.Distance(ObjectManager.Player.ServerPosition) > wardItem.Range)
                return null;

            InventorySlot invSlot =
                ObjectManager.Player.InventoryItems.FirstOrDefault(
                    slot => slot.Id == (ItemId)wardItem.Id);
            return invSlot;
        }
    }
}
