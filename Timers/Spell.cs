using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SAwareness.Timers
{
    class Spell
    {
        public static Menu.MenuItemSettings SpellTimer = new Menu.MenuItemSettings(typeof(Spell));

        private static Dictionary<Ability, List<AbilityDetails>> Abilities = new Dictionary<Ability, List<AbilityDetails>>();
        private int lastGameUpdateTime = 0;

        public Spell()
        {
            Abilities.Add(new Ability("LifeAura", 4f, Position.Sender, "Guardian Angel / Zilean Revive"), new List<AbilityDetails>());
            Abilities.Add(new Ability("global_ss_teleport_", 3.5f, Position.Default, "Teleport"), new List<AbilityDetails>());
            Abilities.Add(new Ability("zhonyas_ring_activate", 2.5f, Position.Hero, "Zhonya Hourglass"), new List<AbilityDetails>());

            Abilities.Add(new Ability("Passive_Death_Activate", 3f, Position.Hero, "Aatrox Passive"), new List<AbilityDetails>());
            Abilities.Add(new Ability("akali_smoke_bomb_tar_team_", 8f, Position.Default, "Akali W"), new List<AbilityDetails>());
            Abilities.Add(new Ability("EggTimer", 6f, Position.Hero, "Anivia Passive"), new List<AbilityDetails>());
            Abilities.Add(new Ability("Azir_Base_R_SoldierCape_", 5f, Position.Sender, "Azir R"), new List<AbilityDetails>());
            Abilities.Add(new Ability("Crowstorm_", 5f, Position.Hero, "FiddleSticks R"), new List<AbilityDetails>());
            Abilities.Add(new Ability("galio_beguilingStatue_taunt_indicator_team_", 2f, Position.Default, "Galio R"), new List<AbilityDetails>());
            Abilities.Add(new Ability("pirate_cannonBarrage_aoe_indicator_", 7f, Position.Default, "Gangplank R"), new List<AbilityDetails>());
            Abilities.Add(new Ability("ReapTheWhirlwind_", 3f, Position.Default, "Janna R"), new List<AbilityDetails>());
            Abilities.Add(new Ability("Jinx_Base_E_Mine_Ready_", 4.5f, Position.Default, "Jinx E"), new List<AbilityDetails>());
            Abilities.Add(new Ability("Karthus_Base_W_Post", 5f, Position.Default, "Karthus W"), new List<AbilityDetails>());
            Abilities.Add(new Ability("Karthus_Base_R_Cas", 3f, Position.Default, "Karthus R"), new List<AbilityDetails>());
            Abilities.Add(new Ability("Karthus_Base_R_Target", 3f, Position.Hero, "Karthus R(Target)"), new List<AbilityDetails>());
            Abilities.Add(new Ability("eyeforaneye", 2f, Position.Hero, "Kayle R"), new List<AbilityDetails>());
            Abilities.Add(new Ability("kennen_ss_aoe_", 3f, Position.Hero, "Kennen R"), new List<AbilityDetails>());
            Abilities.Add(new Ability("LeBlanc_Base_W_return_indicator", 4f, Position.Default, "LeBlanc W"), new List<AbilityDetails>());
            Abilities.Add(new Ability("LeBlanc_Base_RW_return_indicator", 4f, Position.Default, "LeBlanc R"), new List<AbilityDetails>());
            Abilities.Add(new Ability("Lissandra_Base_R_ring_", 1.5f, Position.Hero, "Lissandra R"), new List<AbilityDetails>());
            Abilities.Add(new Ability("Lissandra_Base_R_iceblock", 2.5f, Position.Default, "Lissandra Self-R"), new List<AbilityDetails>());
            Abilities.Add(new Ability("Malzahar_Base_R_tar", 2.5f, Position.Default, "Malzahar R"), new List<AbilityDetails>());
            Abilities.Add(new Ability("MasterYi_Base_W_Buf", 4f, Position.Default, "MasterYi W"), new List<AbilityDetails>());
            Abilities.Add(new Ability("Morgana_base_R_Indicator_Ring", 3f, Position.Hero, "Morgana R"), new List<AbilityDetails>());
            Abilities.Add(new Ability("dr_mundo_heal", 12f, Position.Hero, "Mundo R"), new List<AbilityDetails>());
            Abilities.Add(new Ability("AbsoluteZero2_", 3f, Position.Default, "Nunu R"), new List<AbilityDetails>());
            Abilities.Add(new Ability("olaf_ragnorok_", 6f, Position.Hero, "Olaf R"), new List<AbilityDetails>());
            Abilities.Add(new Ability("Pantheon_Base_R_indicator_", 1.5f, Position.Default, "Pantheon R"), new List<AbilityDetails>()); // Visible = 2.5f / Invisible = 1.5f
            Abilities.Add(new Ability("DiplomaticImmunity_buf", 7f, Position.Hero, "Poppy R"), new List<AbilityDetails>());
            Abilities.Add(new Ability("ShenTeleport_v2", 3f, Position.Default, "Shen R"), new List<AbilityDetails>());
            Abilities.Add(new Ability("Shen_StandUnited_shield_v2", 3f, Position.Hero, "Shen R (Target)"), new List<AbilityDetails>());
            Abilities.Add(new Ability("Sion_Base_R_Cas", 8f, Position.Hero, "Sion R"), new List<AbilityDetails>());
            Abilities.Add(new Ability("talon_ult_sound", 2.5f, Position.Default, "Talon R"), new List<AbilityDetails>());
            Abilities.Add(new Ability("Thresh_Base_Lantern_cas_", 6f, Position.Default, "Thresh W"), new List<AbilityDetails>());
            Abilities.Add(new Ability("UndyingRage_glow", 5f, Position.Hero, "Tryndamere R"), new List<AbilityDetails>());
            Abilities.Add(new Ability("Veigar_Base_W_cas_", 1.2f, Position.Default, "Veigar W"), new List<AbilityDetails>());
            Abilities.Add(new Ability("Veigar_Base_E_cage_", 3f, Position.Default, "Veigar E"), new List<AbilityDetails>());
            Abilities.Add(new Ability("Velkoz_Base_R_Beam_Eye", 2.5f, Position.Hero, "Velkoz R"), new List<AbilityDetails>());
            Abilities.Add(new Ability("Viktor_Catalyst_", 4f, Position.Default, "Viktor W"), new List<AbilityDetails>());
            Abilities.Add(new Ability("InfiniteDuress_tar", 1.8f, Position.Hero, "Warwick R"), new List<AbilityDetails>());
            Abilities.Add(new Ability("MonkeyKing_Base_R_Cas", 4f, Position.Hero, "Wukong R"), new List<AbilityDetails>());
            Abilities.Add(new Ability("w_windwall_enemy", 4f, Position.Sender, "Yasuo W"), new List<AbilityDetails>());
            Abilities.Add(new Ability("Zac_R_tar", 5f, Position.Hero, "Zac R"), new List<AbilityDetails>());
            Abilities.Add(new Ability("Zed_Base_W_cloneswap_buf", 4.5f, Position.Default, "Zed W"), new List<AbilityDetails>());
            Abilities.Add(new Ability("Zed_Base_R_cloneswap_buf", 7.5f, Position.Default, "Zed R"), new List<AbilityDetails>());
            Abilities.Add(new Ability("nickoftime_tar", 7f, Position.Hero, "Zilean R"), new List<AbilityDetails>());
            Abilities.Add(new Ability("Zyra_R_cast_", 2f, Position.Default, "Zyra R"), new List<AbilityDetails>());

            GameObject.OnCreate += Obj_AI_Base_OnCreate;
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        ~Spell()
        {
            GameObject.OnCreate -= Obj_AI_Base_OnCreate;
            Game.OnGameUpdate -= Game_OnGameUpdate;
            Abilities = null;
        }

        public bool IsActive()
        {
            return Timer.Timers.GetActive() && SpellTimer.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            SpellTimer.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("TIMERS_SPELL_MAIN"), "SAwarenessTimersSpell"));
            SpellTimer.MenuItems.Add(
                SpellTimer.Menu.AddItem(new MenuItem("SAwarenessTimersSpellSpeech", Language.GetString("GLOBAL_VOICE")).SetValue(false)));
            SpellTimer.MenuItems.Add(
                SpellTimer.Menu.AddItem(new MenuItem("SAwarenessTimersSpellActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return SpellTimer;
        }

        private void CreateText(Ability ability, Obj_AI_Hero owner, GameObject sender)
        {
            Render.Text text = new Render.Text(new Vector2(0, 0), "", 28, SharpDX.Color.Goldenrod);
            text.OutLined = true;
            text.Centered = true;
            text.TextUpdate = delegate
            {
                float endTime = ability.TimeCasted - (int)Game.ClockTime + ability.Delay;
                var m = (float)Math.Floor(endTime / 60);
                var s = (float)Math.Ceiling(endTime % 60);
                return (s < 10 ? m + ":0" + s : m + ":" + s);
            };
            text.PositionUpdate = delegate
            {
                Vector2 pos = new Vector2();
                switch (ability.Position)
                {
                    case Position.Hero:
                        if (owner.IsValid)
                        {
                            pos = Drawing.WorldToScreen(owner.Position);
                        }
                        break;

                    case Position.Sender:
                        if (sender.IsValid)
                        {
                            pos = Drawing.WorldToScreen(sender.Position);
                        }
                        break;

                    case Position.Default:
                        if (sender.IsValid)
                        {
                            pos = Drawing.WorldToScreen(sender.Position);
                        }
                        break;
                }
                return pos;
            };
            text.VisibleCondition = delegate
            {
                return Timer.Timers.GetActive() && SpellTimer.GetActive() &&
                        ability.Casted && ability.TimeCasted > -1;
            };
            text.Add();
            Abilities[ability].Add(new AbilityDetails(owner, sender, text));
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive() || lastGameUpdateTime + new Random().Next(500, 1000) > Environment.TickCount)
                return;

            lastGameUpdateTime = Environment.TickCount;
            foreach (var ability in Abilities)
            {
                if ((ability.Key.TimeCasted + ability.Key.Delay) < Game.ClockTime)
                {
                    ability.Key.Casted = false;
                    ability.Key.TimeCasted = 0;
                }
                if (ability.Key.TimeCasted == 0)
                {
                    foreach (var abilityDetails in ability.Value)
                    {
                        abilityDetails.Text.Dispose();
                        abilityDetails.Text.Remove();
                        abilityDetails.Text = null;
                    }
                    ability.Value.Clear();
                }
            }
        }

        private void Obj_AI_Base_OnCreate(GameObject sender, EventArgs args)
        {
            if (!IsActive())
                return;

            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsEnemy)
                {
                    foreach (var ability in Abilities)
                    {
                        if (sender.Name.Contains(ability.Key.SpellName))
                        {
                            ability.Key.Casted = true;
                            ability.Key.TimeCasted = (int) Game.ClockTime;
                            if (SpellTimer.GetMenuItem("SAwarenessTimersSpellSpeech").GetValue<bool>())
                            {
                                Speech.Speak(ability.Key.Name + " casted on " + hero.ChampionName);
                            }
                            CreateText(ability.Key, hero, sender);
                        }
                    }
                }
            }
        }

        public enum Position
        {
            Default,
            Hero,
            Sender
        }

        public class Ability
        {
            public String Name;
            public bool Casted;
            public float Delay;
            public int Range;
            public int TimeCasted;
            public String SpellName;
            public Position Position;

            public Ability(string spellName, float delay, Position position, String name)
            {
                SpellName = spellName;
                Delay = delay;
                Name = name;
                Position = position;
            }
        }

        public class AbilityDetails
        {
            public Obj_AI_Hero Owner;
            public GameObject Sender;
            public Render.Text Text;

            public AbilityDetails(Obj_AI_Hero owner, GameObject sender, Render.Text text)
            {
                Owner = owner;
                Sender = sender;
                Text = text;
            }
        }
    }
}
