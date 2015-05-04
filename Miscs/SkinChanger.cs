using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Speech.Synthesis.TtsEngine;
using System.Threading;
using System.Web.Script.Serialization;
using LeagueSharp;
using LeagueSharp.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharpDX;

namespace SAssemblies.Miscs
{
    internal class SkinChanger
    {
        public static Menu.MenuItemSettings SkinChangerMisc = new Menu.MenuItemSettings(typeof(SkinChanger));

        public static Dictionary<String, String[]> Skins = new Dictionary<string, string[]>();
        private static List<String> _skins = new List<string>(); 
        private int _lastSkinId = -1;
        private bool _isDead = false;
        private int lastGameUpdateTime = 0;

        static SkinChanger()
        {
            Skins.Add("Aatrox", new[]
            {
                "Classic",
                "Justicar",
                "Mecha"
            });

            Skins.Add("Ahri", new[]
            {
                "Classic",
                "Dynasty",
                "Midnight",
                "Foxfire",
                "Popstar"
            });

            Skins.Add("Akali", new[]
            {
                "Classic",
                "Stinger",
                "Crimson",
                "All-Star",
                "Nurse",
                "Blood Moon",
                "Silverfang"
            });

            Skins.Add("Alistar", new[]
            {
                "Classic",
                "Black",
                "Golden",
                "Matador",
                "Longhorn",
                "Unchained",
                "Infernal",
                "Sweeper"
            });

            Skins.Add("Amumu", new[]
            {
                "Classic",
                "Pharaoh",
                "Vancouver",
                "Emumu",
                "Re-Gifted",
                "Almost-Prom King",
                "Little Knight",
                "Sad Robot"
            });

            Skins.Add("Anivia", new[]
            {
                "Classic",
                "Team Spirit",
                "Bird of Prey",
                "Noxus Hunter",
                "Hextech",
                "Blackfrost"
            });

            Skins.Add("Annie", new[]
            {
                "Classic",
                "Goth",
                "Red Riding",
                "Annie in Wonderland",
                "Prom Queen",
                "Frostfire",
                "Reverse",
                "FrankenTibbers",
                "Panda"
            });

            Skins.Add("Ashe", new[]
            {
                "Classic",
                "Freljord",
                "Sherwood Forest",
                "Woad",
                "Queen",
                "Amethyst",
                "Heartseeker"
            });

            Skins.Add("Azir", new[]
            {
                "Classic",
                "Galactic"
            });

            Skins.Add("Blitzcrank", new[]
            {
                "Classic",
                "Rusty",
                "Goalkeeper",
                "Boom Boom",
                "Piltover Customs",
                "Definitely Not Blitzcrank",
                "iBlitzcrank",
                "Riot"
            });

            Skins.Add("Brand", new[]
            {
                "Classic",
                "Apocalyptic",
                "Vandal",
                "Cryocore",
                "Zombie"
            });

            Skins.Add("Braum", new[]
            {
                "Classic",
                "Dragonslayer"
            });

            Skins.Add("Caitlyn", new[]
            {
                "Classic",
                "Resistance",
                "Sheriff",
                "Safari",
                "Arctic Warfare",
                "Officer",
                "Headhunter"
            });

            Skins.Add("Cassiopeia", new[]
            {
                "Classic",
                "Desperada",
                "Siren",
                "Mythic",
                "Jade Fang"
            });

            Skins.Add("Chogath", new[]
            {
                "Classic",
                "Nightmare",
                "Gentleman",
                "Loch Ness",
                "Jurassic",
                "Battlecast"
            });

            Skins.Add("Corki", new[]
            {
                "Classic",
                "UFO",
                "Ice Toboggan",
                "Red Baron",
                "Hot Rod",
                "Urfrider",
                "Dragonwing"
            });

            Skins.Add("Darius", new[]
            {
                "Classic",
                "Lord",
                "Direforge",
                "Woad King",
                "Dunkmaster"
            });

            Skins.Add("Diana", new[]
            {
                "Classic",
                "Dark Valkyrie",
                "Lunar Goddess"
            });

            Skins.Add("Drmundo", new[]
            {
                "Classic",
                "Toxic",
                "Mr. Mundoverse",
                "Corporate",
                "Mundo Mundo",
                "Executioner",
                "Rageborn",
                "TPA"
            });

            Skins.Add("Draven", new[]
            {
                "Classic",
                "Soul Reaver",
                "Gladiator",
                "Primetime"
            });

            Skins.Add("Elise", new[]
            {
                "Classic",
                "Death Blossom",
                "Victorious"
            });

            Skins.Add("Evelynn", new[]
            {
                "Classic",
                "Shadow",
                "Masquerade",
                "Tango",
                "Safecracker"
            });

            Skins.Add("Ezreal", new[]
            {
                "Classic",
                "Nottingham",
                "Striker",
                "Frosted",
                "Explorer",
                "Pulsefire",
                "TPA"
            });

            Skins.Add("FiddleSticks", new[]
            {
                "Classic",
                "Spectral",
                "Union Jack",
                "Bandito",
                "Pumpkinhead",
                "Fiddle Me Timbers",
                "Surprise Party",
                "Dark Candy"
            });

            Skins.Add("Fiora", new[]
            {
                "Classic",
                "Royal Guard",
                "Nightraven",
                "Headmistress"
            });

            Skins.Add("Fizz", new[]
            {
                "Classic",
                "Atlantean",
                "Tundra",
                "Fisherman",
                "Void"
            });

            Skins.Add("Galio", new[]
            {
                "Classic",
                "Enchanted",
                "Hextech",
                "Commando",
                "Gatekeeper"
            });

            Skins.Add("Gangplank", new[]
            {
                "Classic",
                "Spooky",
                "Minuteman",
                "Sailor",
                "Toy Soldier",
                "Special Forces",
                "Sultan"
            });

            Skins.Add("Garen", new[]
            {
                "Classic",
                "Sanguine",
                "Desert Trooper",
                "Commando",
                "Dreadknight",
                "Rugged",
                "Steel Legion"
            });

            Skins.Add("Gnar", new[]
            {
                "Classic",
                "Dino"
            });

            Skins.Add("Gragas", new[]
            {
                "Classic",
                "Scuba",
                "Hillbilly",
                "Santa",
                "Gragas, Esq",
                "Vandal",
                "Oktoberfest",
                "Superfan"
            });

            Skins.Add("Graves", new[]
            {
                "Classic",
                "Hired Gun",
                "Jailbreak",
                "Mafia",
                "Riot",
                "Pool Party"
            });

            Skins.Add("Hecarim", new[]
            {
                "Classic",
                "Blood Knight",
                "Reaper",
                "Headless",
                "Arcade"
            });

            Skins.Add("Heimerdinger", new[]
            {
                "Classic",
                "Alien Invader",
                "Blast Zone",
                "Piltover Customs",
                "Snowmerdinger",
                "Hazmat"
            });

            Skins.Add("Irelia", new[]
            {
                "Classic",
                "Nightblade",
                "Aviator",
                "Infiltrator",
                "Frostblade"
            });

            Skins.Add("Janna", new[]
            {
                "Classic",
                "Tempest",
                "Hextech",
                "Frost Queen",
                "Victorious",
                "Forecast"
            });

            Skins.Add("JarvanIV", new[]
            {
                "Classic",
                "Commando",
                "Dragonslayer",
                "Darkforge",
                "Victorious",
                "Warring Kingdoms"
            });

            Skins.Add("Jax", new[]
            {
                "Classic",
                "The Mighty",
                "Vandal",
                "Angler",
                "PAX",
                "Jaximus",
                "Temple",
                "Nemesis",
                "SKT T1"
            });

            Skins.Add("Jayce", new[]
            {
                "Classic",
                "Full Metal",
                "Debonair"
            });

            Skins.Add("Jinx", new[]
            {
                "Classic",
                "Mafia"
            });

            Skins.Add("Kalista", new[]
            {
                "Classic",
                "Bloodmoon"
            });

            Skins.Add("Karma", new[]
            {
                "Classic",
                "Sun Goddess",
                "Sakura",
                "Traditional"
            });

            Skins.Add("Karthus", new[]
            {
                "Classic",
                "Phantom",
                "Statue of Karthus",
                "Grim Reaper",
                "Pentakill"
            });

            Skins.Add("Kassadin", new[]
            {
                "Classic",
                "Festival",
                "Deep One",
                "Pre-Void",
                "Harbinger"
            });

            Skins.Add("Katarina", new[]
            {
                "Classic",
                "Mercenary",
                "Red Card",
                "Bilgewater",
                "Kitty Cat",
                "High Command",
                "Sandstorm",
                "Slay Belle"
            });

            Skins.Add("Kayle", new[]
            {
                "Classic",
                "Silver",
                "Viridian",
                "Unmasked",
                "Battleborn",
                "Judgment",
                "Aether Wing",
                "Riot"
            });

            Skins.Add("Kennen", new[]
            {
                "Classic",
                "Deadly",
                "Swamp Master",
                "Karate",
                "Kennen M.D.",
                "Arctic Ops"
            });

            Skins.Add("Khazix", new[]
            {
                "Classic",
                "Mecha",
                "Guardian of the Sands"
            });

            Skins.Add("Kogmaw", new[]
            {
                "Classic",
                "Caterpillar",
                "Sonoran",
                "Monarch",
                "Reindeer",
                "Lion Dance",
                "Deep Sea",
                "Jurassic",
                "Battlecast"
            });

            Skins.Add("Leblanc", new[]
            {
                "Classic",
                "Wicked",
                "Prestigious",
                "Mistletoe",
                "Ravenborn"
            });

            Skins.Add("LeeSin", new[]
            {
                "Classic",
                "Traditional",
                "Acolyte",
                "Dragon Fist",
                "Muay Thai",
                "Pool Party",
                "SKT T1"
            });

            Skins.Add("Leona", new[]
            {
                "Classic",
                "Valkyrie",
                "Defer",
                "Iron Solari",
                "Pool Party"
            });

            Skins.Add("Lissandra", new[]
            {
                "Classic",
                "Bloodstone",
                "Blade Queen"
            });

            Skins.Add("Lucian", new[]
            {
                "Classic",
                "Hired Gun",
                "Striker"
            });

            Skins.Add("Lulu", new[]
            {
                "Classic",
                "Bittersweet",
                "Wicked",
                "Dragon Trainer",
                "Winter Wonder"
            });

            Skins.Add("Lux", new[]
            {
                "Classic",
                "Sorceress",
                "Spellthief",
                "Commando",
                "Imperial",
                "Steel Legion"
            });

            Skins.Add("Malphite", new[]
            {
                "Classic",
                "Shamrock",
                "Coral Reef",
                "Marble",
                "Obsidian",
                "Glacial",
                "Mecha"
            });

            Skins.Add("Malzahar", new[]
            {
                "Classic",
                "Vizier",
                "Shadow Prince",
                "Djinn",
                "Overlord"
            });

            Skins.Add("Maokai", new[]
            {
                "Classic",
                "Charred",
                "Totemic",
                "Festive",
                "Haunted",
                "Goalkeeper"
            });

            Skins.Add("MasterYi", new[]
            {
                "Classic",
                "Assassin",
                "Chosen",
                "Ionia",
                "Samurai",
                "Headhunter"
            });

            Skins.Add("MissFortune", new[]
            {
                "Classic",
                "Cowgirl",
                "Waterloo",
                "Secret Agent",
                "Candy Cane",
                "Road Warrior",
                "Mafia",
                "Arcade"
            });

            Skins.Add("Mordekaiser", new[]
            {
                "Classic",
                "Dragon Knight",
                "Infernal",
                "Pentakill",
                "Lord"
            });

            Skins.Add("Morgana", new[]
            {
                "Classic",
                "Exiled",
                "Sinful Succulence",
                "Blade Mistress",
                "Blackthorn",
                "Ghost Bride",
                "Victorious"
            });

            Skins.Add("Nami", new[]
            {
                "Classic",
                "Koi",
                "River Spirit"
            });

            Skins.Add("Nasus", new[]
            {
                "Classic",
                "Galactic",
                "Pharaoh",
                "Dreadknight",
                "Riot K-9",
                "Infernal"
            });

            Skins.Add("Nautilus", new[]
            {
                "Classic",
                "Abyssal",
                "Subterranean",
                "AstroNautilus"
            });

            Skins.Add("Nidalee", new[]
            {
                "Classic",
                "Snow Bunny",
                "Leopard",
                "French Maid",
                "Pharaoh",
                "Bewitching",
                "Headhunter"
            });

            Skins.Add("Nocturne", new[]
            {
                "Classic",
                "Frozen Terror",
                "Void",
                "Ravager",
                "Haunting",
                "Eternum"
            });

            Skins.Add("Nunu", new[]
            {
                "Classic",
                "Sasquatch",
                "Workshop",
                "Grungy",
                "Nunu Bot",
                "Demolisher",
                "TPA"
            });

            Skins.Add("Olaf", new[]
            {
                "Classic",
                "Forsaken",
                "Glacial",
                "Brolaf",
                "Pentakill"
            });

            Skins.Add("Orianna", new[]
            {
                "Classic",
                "Gothic",
                "Sewn Chaos",
                "Bladecraft",
                "TPA"
            });

            Skins.Add("Pantheon", new[]
            {
                "Classic",
                "Myrmidon",
                "Ruthless",
                "Perseus",
                "Full Metal",
                "Glaive Warrior",
                "Dragonslayer"
            });

            Skins.Add("Poppy", new[]
            {
                "Classic",
                "Noxus",
                "Lollipoppy",
                "Blacksmith",
                "Ragdoll",
                "Battle Regalia",
                "Scarlet Hammer"
            });

            Skins.Add("Quinn", new[]
            {
                "Classic",
                "Phoenix",
                "Woad Scout"
            });

            Skins.Add("Rammus", new[]
            {
                "Classic",
                "King",
                "Chrome",
                "Molten",
                "Freljord",
                "Ninja",
                "Full Metal"
            });

            Skins.Add("Renekton", new[]
            {
                "Classic",
                "Galactic",
                "Outback",
                "Bloodfury",
                "Rune Wars",
                "Pool Party",
                "Scorched Earth"
            });

            Skins.Add("Rengar", new[]
            {
                "Classic",
                "Headhunter",
                "Night Hunter"
            });

            Skins.Add("Riven", new[]
            {
                "Classic",
                "Redeemed",
                "Crimson Elite",
                "Battle Bunny",
                "Championship",
                "Dragonblade"
            });

            Skins.Add("Rumble", new[]
            {
                "Classic",
                "Rumble in the Jungle",
                "Bilgerat",
                "Supergalactic"
            });

            Skins.Add("Ryze", new[]
            {
                "Classic",
                "Human",
                "Tribal",
                "Uncle",
                "Triumphant",
                "Professor",
                "Zombie",
                "Dark Crystal",
                "Pirate"
            });

            Skins.Add("Sejuani", new[]
            {
                "Classic",
                "Sabretusk",
                "Darkrider",
                "Traditional",
                "Bear Cavalry"
            });

            Skins.Add("Shaco", new[]
            {
                "Classic",
                "Mad Hatter",
                "Royal",
                "Nutcracko",
                "Workshop",
                "Asylum",
                "Masked"
            });

            Skins.Add("Shen", new[]
            {
                "Classic",
                "Frozen",
                "Yellow Jacket",
                "Surgeon",
                "Blood Moon",
                "Warlord",
                "TPA"
            });

            Skins.Add("Shyvana", new[]
            {
                "Classic",
                "Ironscale",
                "Boneclaw",
                "Darkflame",
                "Ice Drake",
                "Championship"
            });

            Skins.Add("Singed", new[]
            {
                "Classic",
                "Riot Squad",
                "Hextech",
                "Surfer",
                "Mad Scientist",
                "Augmented",
                "Snow Day"
            });

            Skins.Add("Sion", new[]
            {
                "Classic",
                "Hextech",
                "Barbarian",
                "Lumberjack",
                "Warmonger"
            });

            Skins.Add("Sivir", new[]
            {
                "Classic",
                "Warrior Princess",
                "Spectacular",
                "Huntress",
                "Bandit",
                "PAX",
                "Snowstorm"
            });

            Skins.Add("Skarner", new[]
            {
                "Classic",
                "Sandscourge",
                "Earthrune",
                "Battlecast"
            });

            Skins.Add("Sona", new[]
            {
                "Classic",
                "Muse",
                "Pentakill",
                "Silent Night",
                "Guqin",
                "Arcade"
            });

            Skins.Add("Soraka", new[]
            {
                "Classic",
                "Dryad",
                "Divine",
                "Celestine",
                "Reaper"
            });

            Skins.Add("Swain", new[]
            {
                "Classic",
                "Northern Front",
                "Bilgewater",
                "Tyrant"
            });

            Skins.Add("Syndra", new[]
            {
                "Classic",
                "Justicar",
                "Atlantean"
            });

            Skins.Add("Talon", new[]
            {
                "Classic",
                "Renegade",
                "Crimson Elite",
                "Dragonblade"
            });

            Skins.Add("Taric", new[]
            {
                "Classic",
                "Emerald",
                "Armor of the Fifth Age",
                "Bloodstone"
            });

            Skins.Add("Teemo", new[]
            {
                "Classic",
                "Happy Elf",
                "Recon",
                "Badger",
                "Astronaut",
                "Cottontail",
                "Super",
                "Panda"
            });

            Skins.Add("Thresh", new[]
            {
                "Classic",
                "Deep Terror",
                "Championship"
            });

            Skins.Add("Tristana", new[]
            {
                "Classic",
                "Riot Girl",
                "Earnest Elf",
                "Firefighter",
                "Guerrilla",
                "Buccaneer",
                "Rocketeer"
            });

            Skins.Add("Trundle", new[]
            {
                "Classic",
                "Lil' Slugger",
                "Junkyard",
                "Traditional",
                "Constable"
            });

            Skins.Add("Tryndamere", new[]
            {
                "Classic",
                "Highland",
                "King",
                "Viking",
                "Demonblade",
                "Sultan",
                "Warring Kingdoms"
            });

            Skins.Add("TwistedFate", new[]
            {
                "Classic",
                "PAX",
                "Jack of Hearts",
                "The Magnificent",
                "Tango",
                "High Noon",
                "Musketeer",
                "Underworld",
                "Red Card"
            });

            Skins.Add("Twitch", new[]
            {
                "Classic",
                "Kingpin",
                "Whistler Village",
                "Medieval",
                "Gangster",
                "Vandal",
                "Pickpocket"
            });

            Skins.Add("Udyr", new[]
            {
                "Classic",
                "Black Belt",
                "Primal",
                "Spirit Guard"
            });

            Skins.Add("Urgot", new[]
            {
                "Classic",
                "Giant Enemy Crabgot",
                "Butcher",
                "Battlecast"
            });

            Skins.Add("Varus", new[]
            {
                "Classic",
                "Blight Crystal",
                "Arclight",
                "Arctic Ops"
            });

            Skins.Add("Vayne", new[]
            {
                "Classic",
                "Vindicator",
                "Aristocrat",
                "Dragonslayer",
                "Heartseeker",
                "SKT T1"
            });

            Skins.Add("Veigar", new[]
            {
                "Classic",
                "White Mage",
                "Curling",
                "Veigar Greybeard",
                "Leprechaun",
                "Baron Von Veigar",
                "Superb Villain",
                "Bad Santa",
                "Final Boss"
            });

            Skins.Add("Velkoz", new[]
            {
                "Classic",
                "Battlecast"
            });

            Skins.Add("Vi", new[]
            {
                "Classic",
                "Neon Strike",
                "Officer",
                "Debonair"
            });

            Skins.Add("Viktor", new[]
            {
                "Classic",
                "Full Machine",
                "Prototype",
                "Creator"
            });

            Skins.Add("Vladimir", new[]
            {
                "Classic",
                "Count",
                "Marquis",
                "Nosferatu",
                "Vandal",
                "Blood Lord",
                "Soulstealer"
            });

            Skins.Add("Volibear", new[]
            {
                "Classic",
                "Thunder Lord",
                "Northern Storm",
                "Runeguard",
                "Captain"
            });

            Skins.Add("Warwick", new[]
            {
                "Classic",
                "Grey",
                "Urf the Manatee",
                "Big Bad",
                "Tundra Hunter",
                "Feral",
                "Firefang",
                "Hyena"
            });

            Skins.Add("Monkeyking", new[]
            {
                "Classic",
                "Volcanic",
                "General",
                "Jade Dragon",
                "Underworld"
            });

            Skins.Add("Xerath", new[]
            {
                "Classic",
                "Runeborn",
                "Battlecast",
                "Scorched Earth"
            });

            Skins.Add("Xinzhao", new[]
            {
                "Classic",
                "Commando",
                "Imperial",
                "Viscero",
                "Winged Hussar",
                "Warring Kingdoms"
            });

            Skins.Add("Yasuo", new[]
            {
                "Classic",
                "High Noon",
                "PROJECT: Yasuo"
            });

            Skins.Add("Yorick", new[]
            {
                "Classic",
                "Undertaker",
                "Pentakill"
            });

            Skins.Add("Zac", new[]
            {
                "Classic",
                "Special Weapon"
            });

            Skins.Add("Zed", new[]
            {
                "Classic",
                "Bladestorm",
                "SKT T1"
            });

            Skins.Add("Ziggs", new[]
            {
                "Classic",
                "Mad Scientist",
                "Major",
                "Pool Party",
                "Snow Day"
            });

            Skins.Add("Zilean", new[]
            {
                "Classic",
                "Old Saint",
                "Groovy",
                "Shurima Desert",
                "Time Machine"
            });

            Skins.Add("Zyra", new[]
            {
                "Classic",
                "Wildfire",
                "Haunted",
                "SKT T1"
            });
        }

        public SkinChanger()
        {
            Game.OnUpdate += Game_OnGameUpdate;
            Obj_AI_Base.OnCreate += Obj_AI_Base_OnCreate;
            new Thread(LoadSpritesAsync).Start();
            SkinChangerMisc.GetMenuItem("SAssembliesMiscsSkinChangerSkinName" + ObjectManager.Player.ChampionName).ValueChanged += SkinChanger_ValueChanged;
            Game.OnWndProc += Game_OnWndProc;
        }

        ~SkinChanger()
        {
            Game.OnUpdate -= Game_OnGameUpdate;
            Skins = null;
        }

        public bool IsActive()
        {
            return Misc.Miscs.GetActive() && SkinChangerMisc.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            SkinChangerMisc.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("MISCS_SKINCHANGER_MAIN"), "SAssembliesMiscsSkinChanger"));
            SkinChangerMisc.MenuItems.Add(
                SkinChangerMisc.Menu.AddItem(new MenuItem("SAssembliesMiscsSkinChangerSkinName" + ObjectManager.Player.ChampionName, Language.GetString("MISCS_SKINCHANGER_SKIN")).SetValue(
                        new StringList(GetSkins().ToArray()))));
            SkinChangerMisc.MenuItems.Add(
                SkinChangerMisc.Menu.AddItem(new MenuItem("SAssembliesMiscsSkinChangerSkinNameLoading", Language.GetString("MISCS_SKINCHANGER_SKIN_LOADING")).SetValue(false).DontSave()));
            SkinChangerMisc.MenuItems.Add(
                SkinChangerMisc.Menu.AddItem(new MenuItem("SAssembliesMiscsSkinChangerSkinNameSplash", Language.GetString("MISCS_SKINCHANGER_SKIN_SPLASH")).SetValue(false).DontSave()));
            SkinChangerMisc.MenuItems.Add(
                SkinChangerMisc.Menu.AddItem(new MenuItem("SAssembliesMiscsSkinChangerActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return SkinChangerMisc;
        }

        void Game_OnWndProc(WndEventArgs args)
        {
            if (!IsActive())
                return;

            HandleInput((WindowsMessages)args.Msg, Utils.GetCursorPos(), args.WParam);
        }

        private void HandleInput(WindowsMessages message, Vector2 cursorPos, uint key)
        {
            if (message != WindowsMessages.WM_LBUTTONDOWN || !SkinChangerMisc.GetMenuItem("SAssembliesMiscsSkinChangerSkinNameLoading").GetValue<bool>())
            {
                return;
            }
            int active = GetActiveIndex();
            if (ChampSkinGUI.ChampSkins[active] != null && Common.IsInside(cursorPos, ChampSkinGUI.ChampSkins[active].Pos, ChampSkinGUI.ChampSkins[active].SpriteInfoSmall.Sprite.Width,
                    ChampSkinGUI.ChampSkins[active].SpriteInfoSmall.Sprite.Height))
            {
                return;
            }
            for (int i = active - 1; i >= 0; i--)
            {
                if (ChampSkinGUI.ChampSkins[i] != null && Common.IsInside(cursorPos, ChampSkinGUI.ChampSkins[i].Pos, ChampSkinGUI.ChampSkins[i].SpriteInfoSmall.Sprite.Width,
                    ChampSkinGUI.ChampSkins[i].SpriteInfoSmall.Sprite.Height))
                {
                    StringList list =
                        SkinChangerMisc.GetMenuItem(
                            "SAssembliesMiscsSkinChangerSkinName" + ObjectManager.Player.ChampionName)
                            .GetValue<StringList>();
                    list.SelectedIndex = i;
                    SkinChangerMisc.GetMenuItem(
                        "SAssembliesMiscsSkinChangerSkinName" + ObjectManager.Player.ChampionName).SetValue(list);
                    return;
                }
            }
            for (int i = active + 1; i < ChampSkinGUI.ChampSkins.Length; i++)
            {
                if (ChampSkinGUI.ChampSkins[i] != null && Common.IsInside(cursorPos, ChampSkinGUI.ChampSkins[i].Pos, ChampSkinGUI.ChampSkins[i].SpriteInfoSmall.Sprite.Width,
                    ChampSkinGUI.ChampSkins[i].SpriteInfoSmall.Sprite.Height))
                {
                    StringList list =
                        SkinChangerMisc.GetMenuItem(
                            "SAssembliesMiscsSkinChangerSkinName" + ObjectManager.Player.ChampionName)
                            .GetValue<StringList>();
                    list.SelectedIndex = i;
                    SkinChangerMisc.GetMenuItem(
                        "SAssembliesMiscsSkinChangerSkinName" + ObjectManager.Player.ChampionName).SetValue(list);
                    return;
                }
            }
        }

        private void Obj_AI_Base_OnCreate(GameObject sender, EventArgs args)
        {
            var unit = sender as Obj_AI_Base;

            if (unit != null && unit.IsValid && unit.Name.Equals(ObjectManager.Player.Name))
            {
                SetSkin(unit, SpriteHelper.ConvertNames(ObjectManager.Player.BaseSkinName), GetActiveIndex());
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive() || lastGameUpdateTime + new Random().Next(500, 1000) > Environment.TickCount)
                return;

            lastGameUpdateTime = Environment.TickCount;
            var mode =
                SkinChangerMisc.GetMenuItem("SAssembliesMiscsSkinChangerSkinName" + ObjectManager.Player.ChampionName)
                    .GetValue<StringList>();
            if (!ObjectManager.Player.IsDead && _isDead)
            {
                SetSkin(ObjectManager.Player, SpriteHelper.ConvertNames(ObjectManager.Player.BaseSkinName), GetActiveIndex());
                _isDead = false;
            }
            else if (ObjectManager.Player.IsDead && !_isDead)
            {
                _isDead = true;
            }
            if (mode.SelectedIndex != _lastSkinId)
            {
                _lastSkinId = mode.SelectedIndex;
                SetSkin(ObjectManager.Player, SpriteHelper.ConvertNames(ObjectManager.Player.BaseSkinName), GetActiveIndex());
            }
            LoadSprites();
        }

        private void SetSkin(Obj_AI_Base unit, String name, int id)
        {
            unit.SetSkin(SpriteHelper.ConvertNames(name), id);

            var hero = unit as Obj_AI_Hero;

            if (hero != null && hero.ChampionName.Equals("Lulu") && !hero.IsDead)
            {
                var pix = ObjectManager.Get<Obj_AI_Base>().FirstOrDefault(obj => obj.IsValid && obj.Name.Equals("RobotBuddy"));
                if (pix != null && pix.IsValid)
                {
                    pix.SetSkin(pix.BaseSkinName, id);
                }
            }
        }

        void SkinChanger_ValueChanged(object sender, OnValueChangeEventArgs e)
        {
            ChampSkinGUI.ChangeOrder(e.GetNewValue<StringList>().SelectedIndex);
        }

        private void LoadSprites()
        {
            foreach (var champSkin in ChampSkinGUI.ChampSkins)
            {
                if (champSkin.SpriteInfoSmall == null || champSkin.SpriteInfoSmall.Sprite == null)
                {
                    SpriteHelper.LoadTexture(champSkin.PicName, ref champSkin.SpriteInfoSmall, "SkinChanger\\Loading");
                }
                if (champSkin.SpriteInfoSmall != null && champSkin.SpriteInfoSmall.DownloadFinished && !champSkin.SpriteInfoSmall.LoadingFinished)
                {
                    champSkin.Pos = new Vector2(Drawing.Width / 2 - champSkin.SpriteInfoSmall.Sprite.Width / 2, Drawing.Height / 1.75f);
                    champSkin.SpriteInfoSmall.Sprite.Scale = new Vector2(0.4f, 0.4f);
                    champSkin.SpriteInfoSmall.Sprite.PositionUpdate = delegate
                    {
                        return champSkin.Pos;
                    };
                    champSkin.SpriteInfoSmall.Sprite.VisibleCondition = delegate
                    {
                        return Misc.Miscs.GetActive() && SkinChangerMisc.GetActive() && SkinChangerMisc.GetMenuItem("SAssembliesMiscsSkinChangerSkinNameLoading").GetValue<bool>();
                    };
                    champSkin.SpriteInfoSmall.Sprite.Add(0);
                    champSkin.SpriteInfoSmall.LoadingFinished = true;
                    ChampSkinGUI.ChangeOrder(GetActiveIndex());
                }

                if (champSkin.SpriteInfoBig == null || champSkin.SpriteInfoBig.Sprite == null)
                {
                    SpriteHelper.LoadTexture(champSkin.PicName, ref champSkin.SpriteInfoBig, "SkinChanger\\Splash");
                }
                if (champSkin.SpriteInfoBig != null && champSkin.SpriteInfoBig.DownloadFinished && !champSkin.SpriteInfoBig.LoadingFinished)
                {
                    champSkin.SpriteInfoBig.Sprite.Scale = new Vector2(0.7f, 0.7f);
                    champSkin.SpriteInfoBig.Sprite.PositionUpdate = delegate
                    {
                        return new Vector2(champSkin.Pos.X + champSkin.SpriteInfoSmall.Sprite.Width / 2 - champSkin.SpriteInfoBig.Sprite.Width / 2, champSkin.Pos.Y - champSkin.SpriteInfoBig.Sprite.Height / 2);
                    };
                    champSkin.SpriteInfoBig.Sprite.VisibleCondition = delegate
                    {
                        return Misc.Miscs.GetActive() && SkinChangerMisc.GetActive() && champSkin.Active
                            && SkinChangerMisc.GetMenuItem("SAssembliesMiscsSkinChangerSkinNameLoading").GetValue<bool>()
                            && SkinChangerMisc.GetMenuItem("SAssembliesMiscsSkinChangerSkinNameSplash").GetValue<bool>();
                    };
                    champSkin.SpriteInfoBig.Sprite.Add(-1);
                    champSkin.SpriteInfoBig.LoadingFinished = true;
                }
            }
        }

        public static String[] GetSkinList(String championName)
        {
            if (Skins.ContainsKey(championName))
            {
                return Skins[championName];
            }
            return new []{""};
        }

        private static void LoadSpritesAsync()
        {
            for (int i = 0; i < ChampSkinGUI.ChampSkins.Length; i++)
            {
                ChampSkinGUI.ChampSkins[i].PicName = SpriteHelper.DownloadImageRiot(ObjectManager.Player.ChampionName, SpriteHelper.ChampionType.ChampionSkin, SpriteHelper.DownloadType.ChampionSkinSmall, "SkinChanger\\Loading", i);
                ChampSkinGUI.ChampSkins[i].PicName = SpriteHelper.DownloadImageRiot(ObjectManager.Player.ChampionName, SpriteHelper.ChampionType.ChampionSkin, SpriteHelper.DownloadType.ChampionSkinBig, "SkinChanger\\Splash", i);
            }
        }

        private static List<String> GetSkins() //http://ddragon.leagueoflegends.com/cdn/img/champion/loading/Aatrox_0.jpg Big Skin pic
        {
            if (_skins.Count != 0)
                return _skins;
            String version = "";
            try
            {
                String jsonV = new WebClient().DownloadString("http://ddragon.leagueoflegends.com/realms/euw.json");
                version = (string)new JavaScriptSerializer().Deserialize<Dictionary<String, Object>>(jsonV)["v"];
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot load DDragon Version: Exception: {1}", ex);
            }
            String json = new WebClient().DownloadString("http://ddragon.leagueoflegends.com/cdn/" + version + "/data/en_US/champion/" + SpriteHelper.ConvertNames(ObjectManager.Player.ChampionName) + ".json");
            JObject data = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject<Object>(json);
            List<String> skinList = new List<string>();
            for (int i = 0; i < 15; i++)
            {
                try
                {
                    skinList.Add(data.SelectToken(data["data"].First.First["skins"].Path + "[" + i + "]")["name"].ToString());
                }
                catch (Exception)
                {
                    break;
                }
            }
            if (skinList.Count == 0)
            {
                skinList.Add("NOT WORKING!");
            }
            else
            {
                ChampSkinGUI.ChampSkins = new ChampSkin[skinList.Count];
                for (int i = 0; i < ChampSkinGUI.ChampSkins.Length; i++)
                {
                    ChampSkinGUI.ChampSkins[i] = new ChampSkin(i, skinList[i]);
                }
            }
            _skins = skinList;
            return skinList;
        }

        private int GetActiveIndex()
        {
            return SkinChangerMisc.GetMenuItem("SAssembliesMiscsSkinChangerSkinName" + ObjectManager.Player.ChampionName) .GetValue<StringList>().SelectedIndex;
        }

        class ChampSkinGUI
        {
            public static ChampSkin[] ChampSkins;

            public ChampSkinGUI()
            {
                
            }

            public static void ChangeOrder(int activeId)
            {
                if(ChampSkins.All(x => x.SpriteInfoSmall != null && x.SpriteInfoSmall.LoadingFinished))
                { 
                    for (int i = 0; i < ChampSkins.Length; i++)
                    {
                        ChampSkins[i].Active = false;
                        ChampSkins[i].SpriteInfoSmall.Sprite.Remove();
                    }
                    ChampSkins[activeId].Active = true;
                    ChampSkins[activeId].Pos = new Vector2(Drawing.Width / 2 - ChampSkins[activeId].SpriteInfoSmall.Sprite.Width / 2, Drawing.Height / 1.75f);
                    ChampSkins[activeId].SpriteInfoSmall.Sprite.Add(0);

                    float count = 0;
                    float renderId = 1;
                    for (int i = activeId - 1; i >= 0; i--)
                    {
                        float offset = ChampSkins[activeId].SpriteInfoSmall.Sprite.Width + 20 + (30 * count);
                        ChampSkins[i].Pos = new Vector2(ChampSkins[activeId].Pos.X - offset, ChampSkins[activeId].Pos.Y);
                        count++;
                        renderId = renderId - 0.01f;
                        ChampSkins[i].SpriteInfoSmall.Sprite.Add(renderId);
                    }

                    renderId = 0;
                    count = ChampSkins.Length - activeId - 2;
                    for (int i = ChampSkins.Length - 1; i > activeId; i--)
                    {
                        float offset = ChampSkins[activeId].SpriteInfoSmall.Sprite.Width + 20 + (30 * count);
                        ChampSkins[i].Pos = new Vector2(ChampSkins[activeId].Pos.X + offset, ChampSkins[activeId].Pos.Y);
                        count--;
                        ChampSkins[i].SpriteInfoSmall.Sprite.Add(renderId);
                    }
                }
            }
        }

        class ChampSkin
        {
            public int Id;
            public SpriteHelper.SpriteInfo SpriteInfoSmall;
            public SpriteHelper.SpriteInfo SpriteInfoBig;
            public Render.Text Name;
            public String PicName;
            public bool Active = true;
            public Vector2 Pos = new Vector2();

            public ChampSkin(int id, String name)
            {
                Id = id;
                Name = new Render.Text(0, 0, name, 18, SharpDX.Color.Orange);
                Name.PositionUpdate = delegate
                {
                    if (SpriteInfoSmall == null || SpriteInfoSmall.Sprite == null || SpriteInfoBig.Sprite == null)
                    {
                        return new Vector2(-50,-50);
                    }
                    if (SkinChangerMisc.GetMenuItem("SAssembliesMiscsSkinChangerSkinNameSplash").GetValue<bool>())
                    {
                        return new Vector2(SpriteInfoBig.Sprite.Position.X + SpriteInfoBig.Sprite.Width / 2, SpriteInfoBig.Sprite.Position.Y - 18);
                    }
                    else
                    {
                        return new Vector2(SpriteInfoSmall.Sprite.Position.X + SpriteInfoSmall.Sprite.Width / 2, SpriteInfoSmall.Sprite.Position.Y - 18);
                    }
                };
                Name.VisibleCondition = sender =>
                {
                    return Misc.Miscs.GetActive() && SkinChangerMisc.GetActive() && SkinChangerMisc.GetMenuItem("SAssembliesMiscsSkinChangerSkinNameLoading").GetValue<bool>() && Active;
                };
                Name.OutLined = true;
                Name.Centered = true;
                Name.Add(4);
            }
        }

        public static void GenAndSendModelPacket(String champName, int skinId)
        {
            GamePacket gPacket = Skin.Encoded(new Skin.Struct(champName, 0, skinId));
            gPacket.Process();
        }

        public class Skin
        {
            public static byte Header;

            static Skin()
            {
                Header = 0x97;
            }

            public static GamePacket Encoded(Struct packetStruct)
            {
                var gamePacket = new GamePacket(Header);
                gamePacket.WriteInteger(packetStruct.SourceNetworkId);
                gamePacket.WriteByte(packetStruct.SourceNetworkIdP1);
                gamePacket.WriteByte(packetStruct.SourceNetworkIdP2);
                gamePacket.WriteByte(packetStruct.SourceNetworkIdP3);
                gamePacket.WriteByte(packetStruct.SourceNetworkIdP4);
                gamePacket.WriteByte(packetStruct.Unknown);
                gamePacket.WriteInteger(packetStruct.SkinId); //SKIN ID
                foreach (byte b in packetStruct.Unknown2)
                {
                    gamePacket.WriteByte(b);
                }
                foreach (byte b in packetStruct.Unknown3)
                {
                    gamePacket.WriteByte(b);
                }
                return gamePacket;
            }

            public struct Struct
            {
                public int SkinId;
                public int SourceNetworkId;
                public byte SourceNetworkIdP1;
                public byte SourceNetworkIdP2;
                public byte SourceNetworkIdP3;
                public byte SourceNetworkIdP4;
                public byte Unknown;
                public byte[] Unknown2;
                public byte[] Unknown3;

                public Struct(String charName, int sourceNetworkId = 0, int skinId = 0)
                {
                    if (sourceNetworkId == 0)
                        SourceNetworkId = ObjectManager.Player.NetworkId;
                    else
                        SourceNetworkId = sourceNetworkId;
                    SkinId = skinId;
                    byte[] tBytes = BitConverter.GetBytes(SourceNetworkId);
                    SourceNetworkIdP1 = tBytes[0];
                    SourceNetworkIdP2 = tBytes[1];
                    SourceNetworkIdP3 = tBytes[2];
                    SourceNetworkIdP4 = tBytes[3];
                    Unknown = 1;
                    Unknown2 = new byte[charName.Length];
                    for (int i = 0; i < charName.Length; i++)
                    {
                        Unknown2[i] = (Convert.ToByte(charName.ToCharArray()[i]));
                    }
                    Unknown3 = new byte[64 - charName.Length];
                    for (int i = 0; i < 64 - charName.Length; i++)
                    {
                        try
                        {
                            Unknown3[i] = (0);
                        }
                        catch (Exception)
                        {
                            break;
                        }
                    }
                }
            }
        }
    }
}
