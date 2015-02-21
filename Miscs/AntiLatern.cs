using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAwareness.Miscs
{
    class AntiLatern //Works only if you move on it. Items and Spells are placed besides the latern
    {
        public static Menu.MenuItemSettings AntiLaternMisc = new Menu.MenuItemSettings(typeof(AntiLatern));
        private int lastGameUpdateTime = 0;
        private int lastTimeUsed = 0;

        Dictionary<String, SpellSlot> _spells = new Dictionary<string, SpellSlot>()
        {
            { "Anivia", SpellSlot.W },
            { "Annie", SpellSlot.R },
            { "Shaco", SpellSlot.W },
            { "Teemo", SpellSlot.R },
            { "Trundle", SpellSlot.E },
            { "Wukong", SpellSlot.W },
            { "Zyra", SpellSlot.W },
        };

        Dictionary<int, String> _wards = new Dictionary<int, String>()
        {
            { 3360, "Feral Flare" },
            { 2043, "Vision Ward" },
            { 2044, "Stealth Ward" },
            { 3154, "Wriggle's Lantern" },
            { 2045, "Ruby Sightstone" },
            { 2049, "Sightstone" },
            { 2050, "Explorer's Ward" },
            { 3340, "Greater Stealth Totem" },
            { 3361, "Greater Stealth Totem" },
            { 3362, "Greater Vision Totem" },
            { 3366, "Bonetooth Necklace" },
            { 3367, "Bonetooth Necklace" },
            { 3368, "Bonetooth Necklace" },
            { 3369, "Bonetooth Necklace" },
            { 3371, "Bonetooth Necklace" },
            { 3375, "Head of Kha'Zix" },
            { 3205, "Quill Coat" },
            { 3207, "Spirit of the Ancient Golem" },
        };

        public AntiLatern()
        {
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        ~AntiLatern()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
        }

        public bool IsActive()
        {
            return Misc.Miscs.GetActive() && AntiLaternMisc.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            AntiLaternMisc.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("MISCS_ANTILATERN_MAIN"), "SAwarenessMiscsAntiLatern"));
            AntiLaternMisc.MenuItems.Add(
                AntiLaternMisc.Menu.AddItem(new MenuItem("SAwarenessMiscsAntiLaternKey", Language.GetString("GLOBAL_KEY")).SetValue(new KeyBind(84, KeyBindType.Press))));
            AntiLaternMisc.MenuItems.Add(
                AntiLaternMisc.Menu.AddItem(new MenuItem("SAwarenessMiscsAntiLaternActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return AntiLaternMisc;
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive() || !AntiLaternMisc.GetMenuItem("SAwarenessMiscsAntiLaternKey").GetValue<KeyBind>().Active || lastGameUpdateTime + new Random().Next(500, 1000) > Environment.TickCount
                || lastTimeUsed + 6000 > Environment.TickCount)
                return;

            lastGameUpdateTime = Environment.TickCount;

            foreach (GameObject gObject in ObjectManager.Get<GameObject>())
            {
                if (gObject.Name.Contains("ThreshLantern") && gObject.IsEnemy)
                {
                    //var spell = _spells.Find(x => x.Key.Equals(ObjectManager.Player.ChampionName));
                    //var spellSlot = spell.Key != null ? spell.Value : SpellSlot.Unknown;
                    //if (spellSlot != SpellSlot.Unknown)
                    //{
                    //    if (ObjectManager.Player.Spellbook.CanUseSpell(spellSlot) == SpellState.Ready && 
                    //        ObjectManager.Player.Spellbook.GetSpell(spellSlot).SData.CastRange[0] > ObjectManager.Player.ServerPosition.Distance(gObject.Position))
                    //    {
                    //        ObjectManager.Player.Spellbook.CastSpell(spellSlot, gObject.Position);
                    //        lastTimeUsed = Environment.TickCount;
                    //        break;
                    //    }
                    //}

                    //InventorySlot invSlot = ObjectManager.Player.InventoryItems.FirstOrDefault(x => _wards.ContainsKey((int) x.Id));
                    //if (invSlot != null)
                    //{
                    //    if (ObjectManager.Player.Spellbook.CanUseSpell(invSlot.SpellSlot) == SpellState.Ready &&
                    //        ObjectManager.Player.Spellbook.GetSpell(invSlot.SpellSlot).SData.CastRange[0] > ObjectManager.Player.ServerPosition.Distance(gObject.Position))
                    //    {
                    //        ObjectManager.Player.Spellbook.CastSpell(invSlot.SpellSlot, gObject.Position);
                    //        lastTimeUsed = Environment.TickCount;
                    //        break;
                    //    }
                    //}

                    if (gObject.Position.Distance(ObjectManager.Player.ServerPosition) < 400)
                    {
                        ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, gObject);
                        break;
                    }
                }
            }
        }
    }
}
