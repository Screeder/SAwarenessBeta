using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace SAwareness.Detectors
{
    class Vision
    {
        public static Menu.MenuItemSettings VisionDetector = new Menu.MenuItemSettings(typeof(SAwareness.Detectors.Vision));

        public enum ObjectType
        {
            Vision,
            Sight,
            Trap,
            Unknown
        }

        private const int WardRange = 1200;
        private const int TrapRange = 300;
        public List<ObjectData> HidObjects = new List<ObjectData>();
        public List<Object> Objects = new List<Object>();
        private int lastGameUpdateTime = 0;

        public Vision()
        {
            Objects.Add(new Object(ObjectType.Vision, "Vision Ward", "VisionWard", "VisionWard", float.MaxValue, 8,
                6424612, Color.BlueViolet));
            Objects.Add(new Object(ObjectType.Sight, "Stealth Ward", "SightWard", "SightWard", 180.0f, 161, 234594676,
                Color.Green));
            Objects.Add(new Object(ObjectType.Sight, "Warding Totem (Trinket)", "YellowTrinket", "TrinketTotemLvl1", 60.0f,
                56, 263796881, Color.Green));
            Objects.Add(new Object(ObjectType.Sight, "Warding Totem (Trinket)", "YellowTrinketUpgrade", "TrinketTotemLvl2", 120.0f,
                56, 263796882, Color.Green));
            Objects.Add(new Object(ObjectType.Sight, "Greater Stealth Totem (Trinket)", "SightWard", "TrinketTotemLvl3",
                180.0f, 56, 263796882, Color.Green));
            Objects.Add(new Object(ObjectType.Sight, "Greater Vision Totem (Trinket)", "VisionWard", "TrinketTotemLvl3B",
                9999.9f, 137, 194218338, Color.BlueViolet));
            Objects.Add(new Object(ObjectType.Sight, "Wriggle's Lantern", "SightWard", "wrigglelantern", 180.0f, 73,
                177752558, Color.Green));
            Objects.Add(new Object(ObjectType.Sight, "Quill Coat", "SightWard", "", 180.0f, 73, 135609454, Color.Green));
            Objects.Add(new Object(ObjectType.Sight, "Ghost Ward", "SightWard", "ItemGhostWard", 180.0f, 229, 101180708,
                Color.Green));

            Objects.Add(new Object(ObjectType.Trap, "Yordle Snap Trap", "Cupcake Trap", "CaitlynYordleTrap", 240.0f, 62,
                176176816, Color.Red));
            Objects.Add(new Object(ObjectType.Trap, "Jack In The Box", "Jack In The Box", "JackInTheBox", 60.0f, 2,
                44637032, Color.Red));
            Objects.Add(new Object(ObjectType.Trap, "Bushwhack", "Noxious Trap", "Bushwhack", 240.0f, 9, 167611995,
                Color.Red));
            Objects.Add(new Object(ObjectType.Trap, "Noxious Trap", "Noxious Trap", "BantamTrap", 600.0f, 48, 176304336,
                Color.Red));

            //Game.OnGameProcessPacket += Game_OnGameProcessPacket;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            GameObject.OnDelete += Obj_AI_Base_OnDelete;
            Drawing.OnDraw += Drawing_OnDraw;
            GameObject.OnCreate += GameObject_OnCreate;
            //Game.OnGameUpdate += Game_OnGameUpdate;
            ThreadHelper.GetInstance().Called += Game_OnGameUpdate;
            foreach (var obj in ObjectManager.Get<GameObject>())
            {
                GameObject_OnCreate(obj, new EventArgs());
            }
        }

        ~Vision()
        {
            //Game.OnGameProcessPacket -= Game_OnGameProcessPacket;
            Obj_AI_Base.OnProcessSpellCast -= Obj_AI_Base_OnProcessSpellCast;
            GameObject.OnCreate -= GameObject_OnCreate;
            GameObject.OnDelete -= Obj_AI_Base_OnDelete;
            Drawing.OnDraw -= Drawing_OnDraw;
            ThreadHelper.GetInstance().Called -= Game_OnGameUpdate;
            //Game.OnGameUpdate += Game_OnGameUpdate;
            
            HidObjects = null;
            Objects = null;
        }

        public bool IsActive()
        {
            return Detector.Detectors.GetActive() && VisionDetector.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            VisionDetector.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("DETECTORS_VISION_MAIN"), "SAwarenessDetectorsVision"));
            VisionDetector.MenuItems.Add(
                VisionDetector.Menu.AddItem(new MenuItem("SAwarenessDetectorsVisionDrawRange", Language.GetString("DETECTORS_VISION_RANGE")).SetValue(false)));
            VisionDetector.MenuItems.Add(
                VisionDetector.Menu.AddItem(new MenuItem("SAwarenessDetectorsVisionDrawVisionRange", Language.GetString("DETECTORS_VISION_VISIONRANGE")).SetValue(false)));
            VisionDetector.MenuItems.Add(
                VisionDetector.Menu.AddItem(new MenuItem("SAwarenessDetectorsVisionDrawRealVisionRange", Language.GetString("DETECTORS_VISION_REALRANGE")).SetValue(false)));
            VisionDetector.MenuItems.Add(
                VisionDetector.Menu.AddItem(new MenuItem("SAwarenessDetectorsVisionActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return VisionDetector;
        }

        void Game_OnGameUpdate(object sender, EventArgs args)
        {
            if (!IsActive() || lastGameUpdateTime + new Random().Next(500, 1000) > Environment.TickCount)
                return;

            lastGameUpdateTime = Environment.TickCount;
            List<ObjectData> objects = HidObjects.FindAll(x => x.ObjectBase.Name == "Unknown");
            foreach (var obj1 in HidObjects.ToArray())
            {
                if (obj1.ObjectBase.Name.Contains("Unknown"))
                    continue;
                foreach (var obj2 in objects)
                {
                    if (Geometry.ProjectOn(obj1.EndPosition.To2D(), obj2.StartPosition.To2D(), obj2.EndPosition.To2D()).IsOnSegment)
                    {
                        HidObjects.Remove(obj2);
                    }
                }
            }
        }

        private void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (!IsActive())
                return;
            try
            {
                if(!sender.IsValid)
                    return;
                if (sender is Obj_AI_Minion && ObjectManager.Player.Team != sender.Team)
                {   
                    foreach (Object obj in Objects)
                    {
                        if (((Obj_AI_Minion)sender).BaseSkinName == obj.ObjectName && !ObjectExist(sender.Position))
                        {
                            HidObjects.Add(new ObjectData(obj, sender.Position, Game.Time + ((Obj_AI_Base)sender).Mana, sender.Name,
                                sender.NetworkId));
                            break;
                        }
                    }
                }
                
                if (sender is Obj_SpellLineMissile && ObjectManager.Player.Team != ((Obj_SpellMissile)sender).SpellCaster.Team)
                {
                    if (((Obj_SpellMissile)sender).SData.Name.Contains("itemplacementmissile"))
                    {
                        Utility.DelayAction.Add(10, () =>
                        {
                            if (!ObjectExist(((Obj_SpellMissile)sender).EndPosition))
                            {

                                HidObjects.Add(new ObjectData(new Object(ObjectType.Unknown, "Unknown", "Unknown", "Unknown", 180.0f, 0, 0, Color.Yellow), ((Obj_SpellMissile)sender).EndPosition, Game.Time + 180.0f, sender.Name,
                                    sender.NetworkId, ((Obj_SpellMissile)sender).StartPosition));
                            }
                        });
                        
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Create: " + ex);
            }
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!IsActive())
                return;
            try
            {
                for (int i = 0; i < HidObjects.Count; i++)
                {
                    ObjectData obj = HidObjects[i];
                    if (Game.Time > obj.EndTime)
                    {
                        HidObjects.RemoveAt(i);
                        break;
                    }
                    Vector2 objMPos = Drawing.WorldToMinimap(obj.EndPosition);
                    Vector2 objPos = Drawing.WorldToScreen(obj.EndPosition);
                    var posList = new List<Vector3>();
                    float range = 0;
                    String typeText = "";
                    switch (obj.ObjectBase.Type)
                    {
                        case ObjectType.Sight:
                            range = WardRange;
                            typeText = "S";
                            break;

                        case ObjectType.Trap:
                            range = TrapRange;
                            typeText = "T";
                            break;

                        case ObjectType.Vision:
                            range = WardRange;
                            typeText = "V";
                            break;

                        case ObjectType.Unknown:
                            if ((obj.StartPosition).IsOnScreen() &&
                                (obj.EndPosition).IsOnScreen())
                            {
                                Drawing.DrawLine(Drawing.WorldToScreen(obj.StartPosition), Drawing.WorldToScreen(obj.EndPosition), 1, obj.ObjectBase.Color);
                            }
                            break;
                    }
                    if (VisionDetector.GetMenuItem("SAwarenessDetectorsVisionDrawVisionRange").GetValue<bool>())
                    {
                        if (obj.EndPosition.IsOnScreen())
                        {
                            Utility.DrawCircle(obj.EndPosition, range, obj.ObjectBase.Color);
                        }
                    }
                    if (obj.Points == null)
                    {
                        posList = GetVision(obj.EndPosition, range);
                        obj.Points = posList;
                    }
                    else
                    {
                        posList = obj.Points;
                    }
                    if (VisionDetector.GetMenuItem("SAwarenessDetectorsVisionDrawRealVisionRange").GetValue<bool>())
                    {
                        for (int j = 0; j < posList.Count; j++)
                        {
                            Vector2 visionPos1 = Drawing.WorldToScreen(posList[j]);
                            Vector2 visionPos2;
                            try
                            {
                                visionPos2 = Drawing.WorldToScreen(posList[j + 1]);
                                if (posList[j].IsOnScreen() && posList[j + 1].IsOnScreen())
                                {
                                    Drawing.DrawLine(visionPos1.X, visionPos1.Y, visionPos2.X, visionPos2.Y, 2.0f, obj.ObjectBase.Color);
                                }
                            }
                            catch (Exception)
                            {
                                visionPos2 = Drawing.WorldToScreen(posList[0]);
                                if (posList[j].IsOnScreen() && posList[0].IsOnScreen())
                                {
                                    Drawing.DrawLine(visionPos1.X, visionPos1.Y, visionPos2.X, visionPos2.Y, 2.0f, obj.ObjectBase.Color);
                                }
                            }
                        }
                    }
                    //if (obj.EndPosition.IsOnScreen())
                    //{
                        Drawing.DrawText(objMPos[0], objMPos[1], obj.ObjectBase.Color, typeText);
                    //}

                    if (VisionDetector.GetMenuItem("SAwarenessDetectorsVisionDrawRange").GetValue<bool>())
                    {
                        if (obj.EndPosition.IsOnScreen())
                        {
                            Utility.DrawCircle(obj.EndPosition, 50, obj.ObjectBase.Color);
                        }
                    }
                    float endTime = obj.EndTime - Game.Time;
                    if (!float.IsInfinity(endTime) && !float.IsNaN(endTime) && endTime.CompareTo(float.MaxValue) != 0)
                    {
                        var m = (float) Math.Floor(endTime/60);
                        var s = (float) Math.Ceiling(endTime%60);
                        String ms = (s < 10 ? m + ":0" + s : m + ":" + s);
                        if (obj.EndPosition.IsOnScreen())
                        {
                            Drawing.DrawText(objPos[0], objPos[1], obj.ObjectBase.Color, ms);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Draw: " + ex);
            }
        }

        private List<Vector3> GetVision(Vector3 viewPos, float visionRange)
        {
            var list = new List<Vector3>();
            for (int i = 0; i <= 360; i += 15/*round*/)
            {
                var roundPos = new Vector3((float)(viewPos.X + visionRange * Math.Cos(i * Math.PI / 180)), (float)(viewPos.Y + visionRange * Math.Sin(i * Math.PI / 180)), 
                    NavMesh.GetHeightForPosition((float)(viewPos.X + visionRange * Math.Cos(i * Math.PI / 180)), (float)(viewPos.Y + visionRange * Math.Sin(i * Math.PI / 180))));
                for (int j = 100; j < visionRange; j += 100 /*accuracy*/)
                {
                    var visionPos = new Vector3((float)(viewPos.X + j * Math.Cos(i * Math.PI / 180)), (float)(viewPos.Y + j * Math.Sin(i * Math.PI / 180)),
                        NavMesh.GetHeightForPosition((float)(viewPos.X + j * Math.Cos(i * Math.PI / 180)), (float)(viewPos.Y + j * Math.Sin(i * Math.PI / 180))));
                    if (!NavMesh.GetCollisionFlags(visionPos).HasFlag(CollisionFlags.Wall))
                    {
                        continue;
                    }
                    roundPos = visionPos;
                    break;
                }
                list.Add(roundPos);
            }
            return list;
        }

        private Object HiddenObjectById(int id)
        {
            return Objects.FirstOrDefault(vision => id == vision.Id2);
        }

        private bool ObjectExist(Vector3 pos)
        {
            return HidObjects.Any(obj => pos.Distance(obj.EndPosition) < 30);
        }

        private void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            if (!IsActive())
                return;
            try
            {
                var reader = new BinaryReader(new MemoryStream(args.PacketData));
                byte packetId = reader.ReadByte(); //PacketId
                if (packetId == 181) //OLD 180
                {
                    int networkId = BitConverter.ToInt32(reader.ReadBytes(4), 0);
                    var creator = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(networkId);
                    if (creator != null && creator.Team != ObjectManager.Player.Team)
                    {
                        reader.ReadBytes(7);
                        int id = reader.ReadInt32();
                        reader.ReadBytes(21);
                        networkId = BitConverter.ToInt32(reader.ReadBytes(4), 0);
                        reader.ReadBytes(12);
                        float x = BitConverter.ToSingle(reader.ReadBytes(4), 0);
                        float y = BitConverter.ToSingle(reader.ReadBytes(4), 0);
                        float z = BitConverter.ToSingle(reader.ReadBytes(4), 0);
                        var pos = new Vector3(x, y, z);
                        Object obj = HiddenObjectById(id);
                        if (obj != null && !ObjectExist(pos))
                        {
                            if (obj.Type == ObjectType.Trap)
                                pos = new Vector3(x, z, y);
                            networkId = networkId + 2;
                            Utility.DelayAction.Add(1, () =>
                            {
                                for (int i = 0; i < HidObjects.Count; i++)
                                {
                                    ObjectData objectData = HidObjects[i];
                                    if (objectData != null && objectData.NetworkId == networkId)
                                    {
                                        var objNew = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(networkId);
                                        if (objNew != null && objNew.IsValid)
                                            objectData.EndPosition = objNew.Position;
                                    }
                                }
                            });
                            HidObjects.Add(new ObjectData(obj, pos, Game.Time + obj.Duration, creator.Name, networkId));
                        }
                    }
                }
                else if (packetId == 178)
                {
                    int networkId = BitConverter.ToInt32(reader.ReadBytes(4), 0);
                    var gObject = ObjectManager.GetUnitByNetworkId<GameObject>(networkId);
                    if (gObject != null)
                    {
                        for (int i = 0; i < HidObjects.Count; i++)
                        {
                            ObjectData objectData = HidObjects[i];
                            if (objectData != null && objectData.NetworkId == networkId)
                            {
                                objectData.EndPosition = gObject.Position;
                            }
                        }
                    }
                }
                else if (packetId == 50) //OLD 49
                {
                    int networkId = BitConverter.ToInt32(reader.ReadBytes(4), 0);
                    for (int i = 0; i < HidObjects.Count; i++)
                    {
                        ObjectData objectData = HidObjects[i];
                        var gObject = ObjectManager.GetUnitByNetworkId<GameObject>(networkId);
                        if (objectData != null && objectData.NetworkId == networkId)
                        {
                            HidObjects.RemoveAt(i);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("HiddenObjectProcess: " + ex);
            }
        }

        private void Obj_AI_Base_OnDelete(GameObject sender, EventArgs args)
        {
            if (!IsActive())
                return;
            try
            {
                if (!sender.IsValid)
                    return;
                for (int i = 0; i < HidObjects.Count; i++)
                {
                    ObjectData obj = HidObjects[i];
                    if ((obj.ObjectBase != null && sender.Name == obj.ObjectBase.ObjectName) ||
                        sender.Name.Contains("Ward") && sender.Name.Contains("Death"))
                        if (sender.Position.Distance(obj.EndPosition) < 30 || sender.Position.Distance(obj.StartPosition) < 30)
                        {
                            HidObjects.RemoveAt(i);
                        }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("HiddenObjectDelete: " + ex);
            }
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!IsActive())
                return;
            try
            {
                if (!sender.IsValid)
                    return;
                if (ObjectManager.Player.Team != sender.Team)
                {
                    foreach (Object obj in Objects)
                    {
                        if (args.SData.Name == obj.SpellName && !ObjectExist(args.End))
                        {
                            HidObjects.Add(new ObjectData(obj, args.End, Game.Time + obj.Duration, sender.Name,
                                sender.NetworkId));
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("HiddenObjectSpell: " + ex);
            }
        }

        public class Object
        {
            public Color Color;
            public float Duration;
            public int Id;
            public int Id2;
            public String Name;
            public String ObjectName;
            public String SpellName;
            public ObjectType Type;

            public Object(ObjectType type, String name, String objectName, String spellName, float duration, int id,
                int id2, Color color)
            {
                Type = type;
                Name = name;
                ObjectName = objectName;
                SpellName = spellName;
                Duration = duration;
                Id = id;
                Id2 = id2;
                Color = color;
            }
        }

        public class ObjectData
        {
            public String Creator;
            public float EndTime;
            public int NetworkId;
            public Object ObjectBase;
            public List<Vector3> Points;
            public Vector3 EndPosition;
            public Vector3 StartPosition;

            public ObjectData(Object objectBase, Vector3 endPosition, float endTime, String creator, int networkId, Vector3 startPosition = new Vector3())
            {
                ObjectBase = objectBase;
                EndPosition = endPosition;
                EndTime = endTime;
                Creator = creator;
                NetworkId = networkId;
                StartPosition = startPosition;
            }
        }
    }
}
