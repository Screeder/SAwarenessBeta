using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Resources;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using LeagueSharp;
using LeagueSharp.Common;
using SAwareness.Properties;
using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;
using Font = SharpDX.Direct3D9.Font;
using Rectangle = SharpDX.Rectangle;

namespace SAwareness
{
    internal static class Log
    {
        public static String File = "C:\\SAwareness.log";
        public static String Prefix = "Packet";

        public static void LogString(String text, String file = null, String prefix = null)
        {
            switch (text)
            {
                case "missile":
                case "DrawFX":
                case "Mfx_pcm_mis.troy":
                case "Mfx_bcm_tar.troy":
                case "Mfx_bcm_mis.troy":
                case "Mfx_pcm_tar.troy":
                    return;
            }
            LogWrite(text, file, prefix);
        }

        private static void LogGamePacket(GamePacket result, String file = null, String prefix = null)
        {
            byte[] b = new byte[result.Size()];
            long size = result.Size();
            int cur = 0;
            while (cur < size - 1)
            {
                b[cur] = result.ReadByte(cur);
                cur++;
            }
            LogPacket(b, file, prefix);
        }

        public static void LogPacket(byte[] data, String file = null, String prefix = null)
        {
            LogWrite(BitConverter.ToString(data), file, prefix);
        }

        private static void LogWrite(String text, String file = null, String prefix = null)
        {
            if (text == null)
                return;
            if (file == null)
                file = File;
            if (prefix == null)
                prefix = Prefix;
            using (var stream = new StreamWriter(file, true))
            {
                stream.WriteLine(prefix + "@" + Game.ClockTime + ": " + text);
            }
        }
    }

    internal static class Common
    {
        public static bool IsOnScreen(Vector3 vector)
        {
            Vector2 screen = Drawing.WorldToScreen(vector);
            if (screen[0] < 0 || screen[0] > Drawing.Width || screen[1] < 0 || screen[1] > Drawing.Height)
                return false;
            return true;
        }

        public static bool IsOnScreen(Vector2 vector)
        {
            Vector2 screen = vector;
            if (screen[0] < 0 || screen[0] > Drawing.Width || screen[1] < 0 || screen[1] > Drawing.Height)
                return false;
            return true;
        }

        public static Size ScaleSize(this Size size, float scale, Vector2 mainPos = default(Vector2))
        {
            size.Height = (int) (((size.Height - mainPos.Y)*scale) + mainPos.Y);
            size.Width = (int) (((size.Width - mainPos.X)*scale) + mainPos.X);
            return size;
        }

        public static bool IsInside(Vector2 mousePos, Size windowPos, int width, int height)
        {
            return Utils.IsUnderRectangle(mousePos, windowPos.Width, windowPos.Height, width, height);
        }
    }

    internal class Downloader
    {
        public delegate void DownloadFinished(object sender, DlEventArgs args);

        public static String Host = "https://github.com/Screeder/SAwareness/raw/master/Sprites/SAwareness/";
        public static String Path = "CHAMP/";

        private readonly List<Files> _downloadQueue = new List<Files>();
        public event DownloadFinished DownloadFileFinished;

        public void AddDownload(String hostFile, String localFile)
        {
            _downloadQueue.Add(new Files(hostFile, localFile));
        }

        public void StartDownload()
        {
            StartDownloadInternal();
        }

        private async Task StartDownloadInternal()
        {
            var webClient = new WebClient();
            var tasks = new List<DlTask>();
            foreach (Files files in _downloadQueue)
            {
                Task t = webClient.DownloadFileTaskAsync(new Uri(Host + Path + files.OnlineFile), files.OfflineFile);
                tasks.Add(new DlTask(files, t));
            }
            foreach (DlTask task in tasks)
            {
                await task.Task;
                tasks.Remove(task);
                OnFinished(new DlEventArgs(task.Files));
            }
        }

        protected virtual void OnFinished(DlEventArgs args)
        {
            if (DownloadFileFinished != null)
                DownloadFileFinished(this, args);
        }

        public static void DownloadFile(String hostfile, String localfile)
        {
            var webClient = new WebClient();
            webClient.DownloadFile(Host + Path + hostfile, localfile);
        }

        public class DlEventArgs : EventArgs
        {
            public Files DlFiles;

            public DlEventArgs(Files files)
            {
                DlFiles = files;
            }
        }

        private struct DlTask
        {
            public readonly Files Files;
            public readonly Task Task;

            public DlTask(Files files, Task task)
            {
                Files = files;
                Task = task;
            }
        }

        public struct Files
        {
            public String OfflineFile;
            public String OnlineFile;

            public Files(String onlineFile, String offlineFile)
            {
                OnlineFile = onlineFile;
                OfflineFile = offlineFile;
            }
        }
    }

    public static class SpriteHelper
    {
        public enum TextureType
        {
            Default,
            Summoner,
            Item
        }

        public enum DownloadType
        {
            Champion,
            Spell,
            Summoner,
            Item
        }

        private static Downloader _downloader = new Downloader();
        public static readonly Dictionary<String, byte[]> MyResources = new Dictionary<String, byte[]>();

        //private static List<SpriteRef> Sprites = new List<SpriteRef>();

        static SpriteHelper()
        {
            ResourceSet resourceSet = Resources.ResourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
            foreach (DictionaryEntry entry in resourceSet)
            {
                var conv = entry.Value as Bitmap;
                if (conv != null)
                {
                    MyResources.Add(entry.Key.ToString().ToLower(), (byte[])new ImageConverter().ConvertTo((Bitmap)entry.Value, typeof(byte[])));
                }
                else
                {
                    MyResources.Add(entry.Key.ToString().ToLower(), (byte[])entry.Value);
                }               
            }
        }

        private static Dictionary<String, Bitmap> cachedMaps = new Dictionary<string, Bitmap>(); 

        //struct SpriteRef
        //{
        //    public String OnlineFile;
        //    public Texture Texture;

        //    public SpriteRef(String onlineFile, ref Texture texture)
        //    {
        //        OnlineFile = onlineFile;
        //        Texture = texture;
        //    }
        //}

        //public static void LoadTexture(String onlineFile, String subOnlinePath, String localPathFile,
        //    ref Texture texture, bool bForce = false)
        //{
        //    if (!File.Exists(localPathFile))
        //    {
        //        try
        //        {
        //            Downloader.Path = subOnlinePath;
        //            Sprites.Add(new SpriteRef(onlineFile, ref texture));
        //            Downloader.DownloadFileFinished += Downloader_DownloadFileFinished;
        //            Downloader.AddDownload(onlineFile, localPathFile);
        //            Downloader.StartDownload();
        //            //Downloader.DownloadFile(onlineFile, localPathFile);
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine("SAwareness: Path: " + onlineFile + " \nException: " + ex);
        //        }
        //    }
        //    LoadTexture(localPathFile, ref texture, bForce);
        //}

        //private static void LoadTexture(String localPathFile, ref Texture texture, bool bForce = false)
        //{
        //    if (File.Exists(localPathFile) && (bForce || texture == null))
        //    {
        //        try
        //        {
        //            texture = Texture.FromFile(Drawing.Direct3DDevice, localPathFile);
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine("SAwarness: Couldn't load texture: " + localPathFile + "\n Ex: " + ex);
        //        }
        //        if (texture == null)
        //        {
        //            return;
        //        }
        //    }
        //}
        
        public static Bitmap DownloadImage(string name, DownloadType type)
        {
            String json = new WebClient().DownloadString("http://ddragon.leagueoflegends.com/realms/euw.json");
            String version = (string)new JavaScriptSerializer().Deserialize<Dictionary<String, Object>>(json)["v"];
            WebRequest request = null;
            if (type == DownloadType.Champion)
            {
                request =
                WebRequest.Create("http://ddragon.leagueoflegends.com/cdn/" + version + "/img/champion/" + name + ".png");
            }
            else if (type == DownloadType.Spell)
            {
                //http://ddragon.leagueoflegends.com/cdn/4.20.1/img/spell/AhriFoxFire.png
                request =
                WebRequest.Create("http://ddragon.leagueoflegends.com/cdn/" + version + "/img/spell/" + name + ".png");
            }
            else if (type == DownloadType.Summoner)
            {
                //summonerexhaust
                name = name[0].ToString().ToUpper() + name.Substring(1, 7) + name[8].ToString().ToUpper() + name.Substring(9, name.Length - 9);
                request =
                WebRequest.Create("http://ddragon.leagueoflegends.com/cdn/" + version + "/img/spell/" + name + ".png");
            }
            else if (type == DownloadType.Item)
            {
                //http://ddragon.leagueoflegends.com/cdn/4.20.1/img/spell/AhriFoxFire.png
                request =
                WebRequest.Create("http://ddragon.leagueoflegends.com/cdn/" + version + "/img/spell/" + name + ".png");
            }
            if (request == null)
                return null;
            try
            {
                Stream responseStream;
                using (WebResponse response = request.GetResponse())
                using (responseStream = response.GetResponseStream())
                    return responseStream != null ? new Bitmap(responseStream) : null;
            }
            catch (Exception)
            {
                
                throw;
            }            
        }

        public async static Task<Bitmap> DownloadImageAsync(string name, DownloadType type)
        {
            String json = new WebClient().DownloadString("http://ddragon.leagueoflegends.com/realms/euw.json");
            String version = (string)new JavaScriptSerializer().Deserialize<Dictionary<String, Object>>(json)["v"];
            WebRequest request = null;
            if (type == DownloadType.Champion)
            {
                request =
                WebRequest.Create("http://ddragon.leagueoflegends.com/cdn/" + version + "/img/champion/" + name + ".png");
            }
            else if (type == DownloadType.Spell)
            {
                //http://ddragon.leagueoflegends.com/cdn/4.20.1/img/spell/AhriFoxFire.png
                request =
                WebRequest.Create("http://ddragon.leagueoflegends.com/cdn/" + version + "/img/spell/" + name + ".png");
            }
            else if (type == DownloadType.Summoner)
            {
                //summonerexhaust
                if (name.Contains("summonerodingarrison"))
                    name = "SummonerOdinGarrison";
                else
                    name = name[0].ToString().ToUpper() + name.Substring(1, 7) + name[8].ToString().ToUpper() + name.Substring(9, name.Length - 9);
                request =
                WebRequest.Create("http://ddragon.leagueoflegends.com/cdn/" + version + "/img/spell/" + name + ".png");
            }
            else if (type == DownloadType.Item)
            {
                //http://ddragon.leagueoflegends.com/cdn/4.20.1/img/spell/AhriFoxFire.png
                request =
                WebRequest.Create("http://ddragon.leagueoflegends.com/cdn/" + version + "/img/spell/" + name + ".png");
            }
            if (request == null)
                return null;
            try
            {
                Stream responseStream;
                Task<WebResponse> reqA = request.GetResponseAsync();
                using (WebResponse response = await reqA) //Crash with AsyncRequest
                using (responseStream = response.GetResponseStream())
                {
                    return responseStream != null ? new Bitmap(responseStream) : null;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("SAwarness: Couldn't load texture: " + name + "\n Ex: " + ex);
                return null;
            }
        }

        public static void LoadTexture(String name, ref Texture texture, TextureType type)
        {
            if ((type == TextureType.Default || type == TextureType.Summoner) && MyResources.ContainsKey(name.ToLower()))
            {
                try
                {
                    texture = Texture.FromMemory(Drawing.Direct3DDevice, MyResources[name.ToLower()]);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("SAwarness: Couldn't load texture: " + name + "\n Ex: " + ex);
                }
            }
            else if (type == TextureType.Summoner && MyResources.ContainsKey(name.ToLower().Remove(name.Length - 1)))
            {
                try
                {
                    texture = Texture.FromMemory(Drawing.Direct3DDevice,
                        MyResources[name.ToLower().Remove(name.Length - 1)]);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("SAwarness: Couldn't load texture: " + name + "\n Ex: " + ex);
                }
            }
            else if (type == TextureType.Item && MyResources.ContainsKey(name.ToLower().Insert(0, "_")))
            {
                try
                {
                    texture = Texture.FromMemory(Drawing.Direct3DDevice, MyResources[name.ToLower().Insert(0, "_")]);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("SAwarness: Couldn't load texture: " + name + "\n Ex: " + ex);
                }
            }
            else
            {
                Console.WriteLine("SAwarness: " + name + " is missing. Please inform Screeder!");
            }
        }

        public static void LoadTexture(String name, ref SpriteInfo texture, TextureType type)
        {
            Bitmap bmp;
            if ((type == TextureType.Default || type == TextureType.Summoner) && MyResources.ContainsKey(name.ToLower()))
            {
                try
                {
                    using (var ms = new MemoryStream(MyResources[name.ToLower()]))
                    {
                        bmp = new Bitmap(ms);
                    }
                    texture.Bitmap = (Bitmap)bmp.Clone();
                    texture.Sprite = new Render.Sprite(bmp, new Vector2(0, 0));
                    //texture.Sprite.UpdateTextureBitmap(bmp);
                    //texture = new Render.Sprite(bmp, new Vector2(0, 0));
                }
                catch (Exception ex)
                {
                    if (texture == null)
                    {
                        texture = new SpriteInfo();
                        texture.Sprite = new Render.Sprite(MyResources["questionmark"], new Vector2(0, 0));
                    }
                    Console.WriteLine("SAwarness: Couldn't load texture: " + name + "\n Ex: " + ex);
                }
            }
            else if (type == TextureType.Summoner && MyResources.ContainsKey(name.ToLower().Remove(name.Length - 1)))
            {
                try
                {
                    //texture = new Render.Sprite((Bitmap)Resources.ResourceManager.GetObject(name.ToLower().Remove(name.Length - 1)), new Vector2(0, 0));
                }
                catch (Exception ex)
                {
                    if (texture == null)
                    {
                        texture = new SpriteInfo();
                        texture.Sprite = new Render.Sprite(MyResources["questionmark"], new Vector2(0, 0));
                    }
                    Console.WriteLine("SAwarness: Couldn't load texture: " + name + "\n Ex: " + ex);
                }
            }
            else if (type == TextureType.Item && MyResources.ContainsKey(name.ToLower().Insert(0, "_")))
            {
                try
                {
                    //texture = new Render.Sprite((Bitmap)Resources.ResourceManager.GetObject(name.ToLower().Insert(0, "_")), new Vector2(0, 0));
                }
                catch (Exception ex)
                {
                    if (texture == null)
                    {
                        texture = new SpriteInfo();
                        texture.Sprite = new Render.Sprite(MyResources["questionmark"], new Vector2(0, 0));
                    }
                    Console.WriteLine("SAwarness: Couldn't load texture: " + name + "\n Ex: " + ex);
                }
            }
            else
            {
                if (texture == null)
                {
                    texture = new SpriteInfo();
                    texture.Sprite = new Render.Sprite(MyResources["questionmark"], new Vector2(0, 0));
                }
                Console.WriteLine("SAwarness: " + name + " is missing. Please inform Screeder!");
            }
        }

        public static void LoadTexture(Bitmap map, ref Render.Sprite texture)
        {
            if (texture == null)
                texture = new Render.Sprite(MyResources["questionmark"], new Vector2(0, 0));
            texture.UpdateTextureBitmap(map);
        }

        public static bool LoadTexture(String name, ref Render.Sprite texture, DownloadType type)
        {
            try
            {
                Bitmap map;
                if (!cachedMaps.ContainsKey(name))
                {
                    map = DownloadImage(name, type);
                    cachedMaps.Add(name, (Bitmap)map.Clone());
                }
                else
                {
                    map = new Bitmap((Bitmap)cachedMaps[name].Clone());
                }
                if (map == null)
                {
                    texture = new Render.Sprite(MyResources["questionmark"], new Vector2(0, 0));
                    Console.WriteLine("SAwarness: " + name + " is missing. Please inform Screeder!");
                    return false;
                }
                texture = new Render.Sprite(map, new Vector2(0, 0));
                //texture.UpdateTextureBitmap(map);
                return true;
                //texture = new Render.Sprite(map, new Vector2(0, 0));
            }
            catch (Exception ex)
            {
                Console.WriteLine("SAwarness: Couldn't load texture: " + name + "\n Ex: " + ex);
                return false;
            }
        }

        public async static Task<SpriteInfo> LoadTextureAsync(String name, SpriteInfo texture, DownloadType type)
        {
            try
            {
                if (texture == null)
                    texture = new SpriteInfo();
                Render.Sprite tex = texture.Sprite;
                LoadTextureAsyncInternal(name, () => texture, x => texture = x, type);
                //texture.Sprite = tex;
                texture.LoadingFinished = true;
                return texture;
                //texture = new Render.Sprite(map, new Vector2(0, 0));
            }
            catch (Exception ex)
            {
                Console.WriteLine("SAwarness: Couldn't load texture: " + name + "\n Ex: " + ex);
                return new SpriteInfo();
            }
        }

        private static void LoadTextureAsyncInternal(String name, Func<SpriteInfo> getTexture, Action<SpriteInfo> setTexture, DownloadType type)
        {
            try
            {
                SpriteInfo spriteInfo = getTexture();
                Render.Sprite texture;
                Bitmap map;
                if (!cachedMaps.ContainsKey(name))
                {
                    Task<Bitmap> bitmap = DownloadImageAsync(name, type);
                    if (bitmap == null || bitmap.Result == null || bitmap.Status == TaskStatus.Faulted)
                    {
                        texture = new Render.Sprite(MyResources["questionmark"], new Vector2(0, 0));
                        Console.WriteLine("SAwarness: " + name + " is missing. Please inform Screeder!");
                        spriteInfo.Sprite = texture;
                        setTexture(spriteInfo);
                        throw new Exception();
                    }
                    map = bitmap.Result; //Change to Async to make it Async, currently crashing through loading is not thread safe.
                    //Bitmap map = await bitmap;
                    cachedMaps.Add(name, (Bitmap)map.Clone());
                }
                else
                {
                    map = new Bitmap((Bitmap)cachedMaps[name].Clone());
                }
                if (map == null)
                {
                    texture = new Render.Sprite(MyResources["questionmark"], new Vector2(0, 0));
                    spriteInfo.Sprite = texture;
                    setTexture(spriteInfo);
                    Console.WriteLine("SAwarness: " + name + " is missing. Please inform Screeder!");
                    throw new Exception();
                }
                spriteInfo.Bitmap = (Bitmap)map.Clone();
                texture = new Render.Sprite(map, new Vector2(0, 0));
                spriteInfo.DownloadFinished = true;
                spriteInfo.Sprite = texture;
                
                setTexture(spriteInfo);
                //texture = new Render.Sprite(map, new Vector2(0, 0));
            }
            catch (Exception ex)
            {
                Console.WriteLine("SAwarness: Could not load async " + name + ".");
            }
        }

        //static void Downloader_DownloadFileFinished(object sender, Downloader.DlEventArgs args)
        //{
        //    for (int i = 0; i < Sprites.Count - 1; i++)
        //    {
        //        var spriteRef = Sprites[i];
        //        if (spriteRef.OnlineFile.Contains(args.DlFiles.OnlineFile))
        //        {
        //            LoadTexture(args.DlFiles.OfflineFile, ref spriteRef.Texture);
        //        }
        //    }
        //    foreach (var spriteRef in Sprites.ToArray())
        //    {
        //        if (spriteRef.OnlineFile.Contains(args.DlFiles.OnlineFile))
        //        {
        //            Sprites.Remove(spriteRef);
        //        }
        //    }
        //}

        public class SpriteInfo : IDisposable
        {
            public enum OVD
            {
                Small,
                Big
            }

            public Render.Sprite Sprite;
            public Bitmap Bitmap;
            public bool DownloadFinished = false;
            public bool LoadingFinished = false;
            public OVD Mode = OVD.Small;

            public void Dispose()
            {
                if (Sprite != null)
                    Sprite.Dispose();

                if (Bitmap != null)
                    Bitmap.Dispose();
            }

            ~SpriteInfo()
            {
                Dispose();
            }
        }
    }

    internal static class Serialize
    {
        public static List<T> Load<T>(string file)
        {
            List<T> listofa = new List<T>();
            XmlSerializer formatter = new XmlSerializer(typeof(T));
            FileStream aFile = new FileStream(file, FileMode.Open);
            byte[] buffer = new byte[aFile.Length];
            aFile.Read(buffer, 0, (int)aFile.Length);
            MemoryStream stream = new MemoryStream(buffer);
            return (List<T>)formatter.Deserialize(stream);
        }


        public static void Save<T>(string path, List<T> listofa)
        {
            FileStream outFile = File.Create(path);
            XmlSerializer formatter = new XmlSerializer(typeof(T));
            formatter.Serialize(outFile, listofa);
        }
    }

    internal static class DirectXDrawer
    {
        private static void InternalRender(Vector3 target)
        {
            //Drawing.Direct3DDevice.SetTransform(TransformState.World, Matrix.Translation(target));
            //Drawing.Direct3DDevice.SetTransform(TransformState.View, Drawing.View);
            //Drawing.Direct3DDevice.SetTransform(TransformState.Projection, Drawing.Projection);

            Drawing.Direct3DDevice.VertexShader = null;
            Drawing.Direct3DDevice.PixelShader = null;
            Drawing.Direct3DDevice.SetRenderState(RenderState.AlphaBlendEnable, true);
            Drawing.Direct3DDevice.SetRenderState(RenderState.BlendOperation, BlendOperation.Add);
            Drawing.Direct3DDevice.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
            Drawing.Direct3DDevice.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
            Drawing.Direct3DDevice.SetRenderState(RenderState.Lighting, 0);
            Drawing.Direct3DDevice.SetRenderState(RenderState.ZEnable, true);
            Drawing.Direct3DDevice.SetRenderState(RenderState.AntialiasedLineEnable, true);
            Drawing.Direct3DDevice.SetRenderState(RenderState.Clipping, true);
            Drawing.Direct3DDevice.SetRenderState(RenderState.EnableAdaptiveTessellation, true);
            Drawing.Direct3DDevice.SetRenderState(RenderState.MultisampleAntialias, true);
            Drawing.Direct3DDevice.SetRenderState(RenderState.ShadeMode, ShadeMode.Gouraud);
            Drawing.Direct3DDevice.SetTexture(0, null);
            Drawing.Direct3DDevice.SetRenderState(RenderState.CullMode, Cull.None);
        }

        public static void DrawLine(Vector3 from, Vector3 to, Color color)
        {
            var vertices = new PositionColored[2];
            vertices[0] = new PositionColored(Vector3.Zero, color.ToArgb());
            from = from.SwitchYZ();
            to = to.SwitchYZ();
            vertices[1] = new PositionColored(to - from, color.ToArgb());

            InternalRender(from);

            Drawing.Direct3DDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices.Length/2, vertices);
        }

        public static void DrawLine(Line line, Vector3 from, Vector3 to, ColorBGRA color, Size size = default(Size),
            float[] scale = null, float rotation = 0.0f)
        {
            if (line != null)
            {
                from = from.SwitchYZ();
                to = to.SwitchYZ();
                Matrix nMatrix = (scale != null ? Matrix.Scaling(scale[0], scale[1], 0) : Matrix.Scaling(1))*
                                 Matrix.RotationZ(rotation)*Matrix.Translation(from);
                Vector3[] vec = {from, to};
                line.DrawTransform(vec, nMatrix, color);
            }
        }

        public static void DrawText(Font font, String text, Size size, SharpDX.Color color)
        {
            DrawText(font, text, size.Width, size.Height, color);
        }


        //TODO: Too many drawtext for shadowtext, need another method fps issues
        public static void DrawText(Font font, String text, int posX, int posY, SharpDX.Color color)
        {
            if (font == null || font.IsDisposed)
            {
                throw new SharpDXException("");
            }
            Rectangle rec = font.MeasureText(null, text, FontDrawFlags.Center);
            //font.DrawText(null, text, posX + 1 + rec.X, posY, Color.Black);
            font.DrawText(null, text, posX + 1 + rec.X, posY + 1, SharpDX.Color.Black);
            font.DrawText(null, text, posX + rec.X, posY + 1, SharpDX.Color.Black);
            //font.DrawText(null, text, posX - 1 + rec.X, posY, Color.Black);
            font.DrawText(null, text, posX - 1 + rec.X, posY - 1, SharpDX.Color.Black);
            font.DrawText(null, text, posX + rec.X, posY - 1, SharpDX.Color.Black);
            font.DrawText(null, text, posX + rec.X, posY, color);
        }

        public static void DrawSprite(Sprite sprite, Texture texture, Size size, float[] scale = null,
            float rotation = 0.0f)
        {
            DrawSprite(sprite, texture, size, SharpDX.Color.White, scale, rotation);
        }

        public static void DrawSprite(Sprite sprite, Texture texture, Size size, SharpDX.Color color,
            float[] scale = null,
            float rotation = 0.0f)
        {
            if (sprite != null && !sprite.IsDisposed && texture != null && !texture.IsDisposed)
            {
                Matrix matrix = sprite.Transform;
                Matrix nMatrix = (scale != null ? Matrix.Scaling(scale[0], scale[1], 0) : Matrix.Scaling(1))*
                                 Matrix.RotationZ(rotation)*Matrix.Translation(size.Width, size.Height, 0);
                sprite.Transform = nMatrix;
                Matrix mT = Drawing.Direct3DDevice.GetTransform(TransformState.World);

                //InternalRender(mT.TranslationVector);
                if (Common.IsOnScreen(new Vector2(size.Width, size.Height)))
                    sprite.Draw(texture, color);
                sprite.Transform = matrix;
            }
        }

        public static void DrawTransformSprite(Sprite sprite, Texture texture, SharpDX.Color color, Size size,
            float[] scale,
            float rotation, Rectangle? spriteResize)
        {
            if (sprite != null && texture != null)
            {
                Matrix matrix = sprite.Transform;
                Matrix nMatrix = Matrix.Scaling(scale[0], scale[1], 0)*Matrix.RotationZ(rotation)*
                                 Matrix.Translation(size.Width, size.Height, 0);
                sprite.Transform = nMatrix;
                sprite.Draw(texture, color);
                sprite.Transform = matrix;
            }
        }

        public static void DrawTransformedSprite(Sprite sprite, Texture texture, Rectangle spriteResize,
            SharpDX.Color color)
        {
            if (sprite != null && texture != null)
            {
                sprite.Draw(texture, color);
            }
        }

        public static void DrawSprite(Sprite sprite, Texture texture, Size size, SharpDX.Color color,
            Rectangle? spriteResize)
        {
            if (sprite != null && texture != null)
            {
                sprite.Draw(texture, color, spriteResize, new Vector3(size.Width, size.Height, 0));
            }
        }

        public static void DrawSprite(Sprite sprite, Texture texture, Size size, SharpDX.Color color)
        {
            if (sprite != null && texture != null)
            {
                DrawSprite(sprite, texture, size, color, null);
            }
        }

        public struct PositionColored
        {
            public static readonly int Stride = Vector3.SizeInBytes + sizeof (int);

            public int Color;
            public Vector3 Position;

            public PositionColored(Vector3 pos, int col)
            {
                Position = pos;
                Color = col;
            }
        }
    }
}