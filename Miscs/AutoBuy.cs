using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Sandbox;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SAssemblies;
using SAssemblies.Miscs;
using SAssemblies.Trackers;
using Menu = SAssemblies.Menu;

//Extend Picture with place for item panel
//left click on item panel item -> choose catergory -> choose item -> add item
//right lick on item panel item -> delete item

namespace SAssemblies.Miscs
{
    class AutoBuy
    {

        public static Menu.MenuItemSettings AutoBuyMisc = new Menu.MenuItemSettings(typeof(AutoBuy));

        public AutoBuy()
        {
            GetItemConfig();
        }

        ~AutoBuy()
        {

        }

        public bool IsActive()
        {
#if MISCS
            return Misc.Miscs.GetActive() && AutoBuyMisc.GetActive();
#else
            return AutoBuyMisc.GetActive();
#endif
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            AutoBuyMisc.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("MISCS_AUTOBUY_MAIN"), "SAssembliesMiscsAutoBuy"));
            AutoBuyMisc.MenuItems.Add(
                AutoBuyMisc.Menu.AddItem(new MenuItem("SAssembliesMiscsAutoBuyActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return AutoBuyMisc;
        }

        static ItemInfo GetItemConfig() //Does not download the item.json correctly, it is missing the tags attribute somehow!!!
        {
            String name = "";
            String version = "";
            try
            {
                String json = new WebClient().DownloadString("http://ddragon.leagueoflegends.com/realms/euw.json");
                version = (string)new JavaScriptSerializer().Deserialize<Dictionary<String, Object>>(json)["v"];
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot download file: {0}, Exception: {1}", name, ex);
                return null;
            }
            List<ItemInfo> itemInfoList = new List<ItemInfo>();
            try
            {
                String url = "http://ddragon.leagueoflegends.com/cdn/5.8.1/data/en_US/item.json"; //temp fix for riot fault otherwise + version +
                String json = new WebClient().DownloadString(url);
                JObject data = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject<Object>(json);
                JToken token = data["data"].First;
                while ((token = token.Next) != null)
                {
                    ItemInfo itemInfo = new ItemInfo();
                    itemInfo.ItemId = (ItemId)Convert.ToInt32(((JProperty)token).Name);
                    itemInfo.Name = token.First["name"].ToString();
                    itemInfo.Description = token.First["description"].ToString();
                    if (token.First["from"] != null)
                    {
                        itemInfo.ToItemIds = token.First["from"].Values<int>().Select(x => (ItemId)x).ToList();
                    }
                    if (token.First["into"] != null)
                    {
                        itemInfo.ToItemIds = token.First["into"].Values<int>().Select(x => (ItemId)x).ToList();
                    }
                    itemInfo.Price = token.First["gold"]["base"].Value<int>();
                    itemInfo.TotalGold = token.First["gold"]["total"].Value<int>();
                    itemInfo.ItemTypeList = token.First["tags"].Values<String>().Select(ItemInfo.StringToItemType).ToList(); ;//tags -> String
                    itemInfo.MapId = new List<GameMapId>();//null = everything, maps -> MapId: false
                    if (token.First["maps"] != null)
                    {
                        JToken mapToken = token.First["maps"];
                        while ((mapToken = mapToken.Next) != null)
                        {
                            itemInfo.MapId.Add((GameMapId)Convert.ToInt32(((JProperty)mapToken).Name));
                        }
                    }
                    itemInfoList.Add(itemInfo);
                }
                //name = data["data"].First.First["image"]["full"].ToString().Replace(".png", "_" + skinId + ".jpg");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot download file: {0}, Exception: {1}", name, ex);
                return null;
            }
            return null;
        }

        [Serializable]
        class Build
        {
            public String Name;
            private List<ItemId> _itemId = new List<ItemId>(36);

            public Build(string name)
            {
                Name = name;
                _itemId = new List<ItemId>();
            }

            public Build(string name, List<ItemId> itemId)
            {
                Name = name;
                _itemId = itemId;
            }

        }

        class ItemInfo
        {
            public enum ItemType
            {
                Nothing,
                Jungle,
                Lane,
                Consumable,
                Vision,
                GoldPer,
                Armor,
                SpellBlock,
                Health,
                HealthRegen,
                AttackSpeed,
                CriticalStrike,
                Damage,
                Lifesteal,
                CooldownReduction,
                Mana,
                ManaRegen,
                SpellDamage,
                Boots,
                NonBootsMovement,
                SpellVamp,
                ArmorPenetration,
                MagicPenetration,
                Active,
                Aura,
                Slow,
                Stealth,
                Trinket,
                Tenacity,
                OnHit
            }

            public ItemId ItemId;
            public String Name;
            public String Description;
            public int Price;
            public List<ItemType> ItemTypeList;
            public List<GameMapId> MapId;
            public List<ItemId> FromItemIds = new List<ItemId>();
            public List<ItemId> ToItemIds = new List<ItemId>();
            public int TotalGold;

            private static Dictionary<String, ItemType> dicItemType; 

            public static ItemType StringToItemType(String sItemType)
            {
                if (dicItemType == null)
                {
                    dicItemType = new Dictionary<string, ItemType>();
                    for (int i = 0; i < Enum.GetValues(typeof(ItemType)).Length; i++)
                    {
                        String name = Enum.GetName(typeof(ItemType), i);
                        if (name != null)
                        {
                            dicItemType.Add(name.ToLower(), (ItemType)i);
                        }
                    }
                }
                ItemType returnValue;
                if (dicItemType.TryGetValue(sItemType.ToLower(), out returnValue))
                {
                    return returnValue;
                }
                else
                {
                    return ItemType.Nothing;
                }
            }
        }

        class AutoBuyGUI
        {
            
        }

    }
}
