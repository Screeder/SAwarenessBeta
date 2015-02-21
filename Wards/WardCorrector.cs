using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SAwareness.Wards
{
    class WardCorrector
    {
        public static Menu.MenuItemSettings WardCorrector_Wards = new Menu.MenuItemSettings(typeof(WardCorrector));

        private static List<WardSpot> WardSpots = new List<WardSpot>();

        private bool _drawSpots;
        private SpellSlot _latestSpellSlot = SpellSlot.Unknown;
        private WardSpot _latestWardSpot;
        private bool _wardAlreadyCorrected;
        private int lastGameUpdateTime = 0;

        public WardCorrector() //Coords by DrunkenNinja
        {
            //WardSpots.Add(new WardSpot("BlueGolem", new Vector3(3274f, 7772f, 52f)));
            //WardSpots.Add(new WardSpot("BlueLizard", new Vector3(7422f, 3282f, 46.53f)));
            //WardSpots.Add(new WardSpot("BlueTriBush", new Vector3(10148f, 2839f, 44.41f)));
            //WardSpots.Add(new WardSpot("BluePassBush", new Vector3(6269f, 4445f, 42.51f)));
            //WardSpots.Add(new WardSpot("BlueRiver", new Vector3(7151.64f, 4719.66f, 51.67f)));
            //WardSpots.Add(new WardSpot("BlueRiverRoundBush", new Vector3(4728f, 8336f, -51.29f)));
            //WardSpots.Add(new WardSpot("BlueRiverSplitBush", new Vector3(6762.52f, 2918.75f, 55.68f)));
            //WardSpots.Add(new WardSpot("PurpleGolem", new Vector3(11217.39f, 6841.89f, 54.87f)));
            //WardSpots.Add(new WardSpot("PurpleLizard", new Vector3(6610.35f, 11064.61f, 54.45f)));
            //WardSpots.Add(new WardSpot("PurpleTriBush", new Vector3(3883f, 11577f, 39.87f)));
            //WardSpots.Add(new WardSpot("PurplePassBush", new Vector3(7775f, 10046.49f, 43.14f)));
            //WardSpots.Add(new WardSpot("PurpleRiver", new Vector3(6867.68f, 9567.63f, 57.01f)));
            //WardSpots.Add(new WardSpot("PurpleRoundBush", new Vector3(9720.86f, 7501.50f, 54.85f)));
            //WardSpots.Add(new WardSpot("PurpleRiverRoundBush", new Vector3(9233.13f, 6094.48f, -44.63f)));
            //WardSpots.Add(new WardSpot("PurpleRiverSplitPush", new Vector3(7282.69f, 11482.53f, 52.59f)));
            //WardSpots.Add(new WardSpot("Dragon", new Vector3(10180.18f, 4969.32f, -62.32f)));
            //WardSpots.Add(new WardSpot("DragonBush", new Vector3(8875.13f, 5390.57f, -64.07f)));
            //WardSpots.Add(new WardSpot("Baron", new Vector3(3920.88f, 9477.78f, -60.42f)));
            //WardSpots.Add(new WardSpot("BaronBush", new Vector3(5017.27f, 8954.09f, -62.70f)));
            //WardSpots.Add(new WardSpot("PurpleBotTower2", new Vector3(12731.25f, 9132.66f, 50.32f)));
            //WardSpots.Add(new WardSpot("PurpleTopTower2", new Vector3(8036.52f, 12882.94f, 45.19f)));
            //WardSpots.Add(new WardSpot("PurpleMidTower1", new Vector3(9260.02f, 8582.67f, 54.62f)));
            //WardSpots.Add(new WardSpot("BlueMidTower1", new Vector3(4749.79f, 5890.76f, 53.59f)));
            //WardSpots.Add(new WardSpot("BlueBotTower2", new Vector3(5983.58f, 1547.98f, 52.99f)));
            //WardSpots.Add(new WardSpot("BlueTopTower2", new Vector3(1213.70f, 5324.73f, 58.77f)));

            //WardSpots.Add(new WardSpot("NoName", new Vector3(9641.65f, 6368.74f, 53.01f)));
            //WardSpots.Add(new WardSpot("NoName", new Vector3(8081.43f, 4683.44f, 55.94f)));
            //WardSpots.Add(new WardSpot("NoName", new Vector3(5943.51f, 9792.40f, 53.18f)));
            //WardSpots.Add(new WardSpot("NoName", new Vector3(4379.51f, 8093.74f, 42.73f)));
            //WardSpots.Add(new WardSpot("NoName", new Vector3(4222.72f, 7038.58f, 53.61f)));
            //WardSpots.Add(new WardSpot("NoName", new Vector3(9068.02f, 11186.68f, 53.22f)));
            //WardSpots.Add(new WardSpot("NoName", new Vector3(7970.82f, 10005.07f, 53.52f)));
            //WardSpots.Add(new WardSpot("NoName", new Vector3(4978.19f, 3042.69f, 54.34f)));
            //WardSpots.Add(new WardSpot("NoName", new Vector3(7907.63f, 11629.32f, 49.94f)));
            //WardSpots.Add(new WardSpot("NoName", new Vector3(7556.06f, 11739.62f, 50.61f)));
            //WardSpots.Add(new WardSpot("NoName", new Vector3(5973.48f, 11115.68f, 54.34f)));
            //WardSpots.Add(new WardSpot("NoName", new Vector3(5732.81f, 10289.76f, 53.39f)));
            //WardSpots.Add(new WardSpot("NoName", new Vector3(7969.15f, 3307.56f, 56.94f)));
            //WardSpots.Add(new WardSpot("NoName", new Vector3(12073.18f, 4795.50f, 52.32f)));
            //WardSpots.Add(new WardSpot("NoName", new Vector3(4044.13f, 11600.50f, 48.59f)));
            //WardSpots.Add(new WardSpot("NoName", new Vector3(5597.66f, 12491.04f, 39.73f)));
            //WardSpots.Add(new WardSpot("NoName", new Vector3(10070.20f, 4132.45f, -60.33f)));
            //WardSpots.Add(new WardSpot("NoName", new Vector3(8320.28f, 4292.80f, 56.47f)));
            //WardSpots.Add(new WardSpot("NoName", new Vector3(9603.52f, 7872.23f, 54.71f)));

            //WardSpots.Add(new WardSpot("Dragon->TriBush", new Vector3(9695f, 3465f, 43.02f),
            //    new Vector3(9843.38f, 3125.16f, 43.02f), new Vector3(9946.10f, 3064.81f, 43.02f),
            //    new Vector3(9595f, 3665f, 43.02f)));
            //WardSpots.Add(new WardSpot("Nashor->TriBush", new Vector3(4346.10f, 10964.81f, 36.62f),
            //    new Vector3(4214.93f, 11202.01f, 36.62f), new Vector3(4146.10f, 11314.81f, 36.62f),
            //    new Vector3(4384.36f, 10680.41f, 36.62f)));
            //WardSpots.Add(new WardSpot("BlueTop->SoloBush", new Vector3(2349f, 10387f, 44.20f),
            //    new Vector3(2267.97f, 10783.37f, 44.20f), new Vector3(2446.10f, 10914.81f, 44.20f),
            //    new Vector3(2311f, 10185f, 44.20f)));
            //WardSpots.Add(new WardSpot("BlueMid->RoundBush", new Vector3(4946.52f, 6474.56f, 54.71f),
            //    new Vector3(4891.98f, 6639.05f, 53.62f), new Vector3(4546.10f, 6864.81f, 53.78f),
            //    new Vector3(5217f, 6263f, 54.95f)));
            //WardSpots.Add(new WardSpot("BlueMid->RiverLaneBush", new Vector3(5528.96f, 7615.20f, 45.64f),
            //    new Vector3(5688.96f, 7825.20f, 45.64f), new Vector3(5796.10f, 7914.81f, 45.64f),
            //    new Vector3(5460.13f, 7469.77f, 45.64f)));
            //WardSpots.Add(new WardSpot("BlueLizard->DragonPassBush", new Vector3(7745f, 4065f, 47.71f),
            //    new Vector3(7927.65f, 4239.77f, 47.71f), new Vector3(8146.10f, 4414.81f, 47.71f),
            //    new Vector3(7645f, 4015f, 47.71f)));
            //WardSpots.Add(new WardSpot("PurpleMid->RoundBush", new Vector3(9057f, 8245f, 45.73f),
            //    new Vector3(9230.77f, 7897.22f, 66.39f), new Vector3(9446.10f, 7814.81f, 54.66f),
            //    new Vector3(8895f, 8313f, 54.89f)));
            //WardSpots.Add(new WardSpot("PurpleMid->RiverRoundBush", new Vector3(9025.78f, 6591.64f, 46.27f),
            //    new Vector3(9200.08f, 6425.05f, 43.21f), new Vector3(9396.10f, 6264.81f, 23.72f),
            //    new Vector3(8795f, 6815f, 56.11f)));
            //WardSpots.Add(new WardSpot("PurpleMid->RiverLaneBush", new Vector3(8530.27f, 6637.38f, 46.98f),
            //    new Vector3(8539.27f, 6637.38f, 46.98f), new Vector3(8396.10f, 6464.81f, 46.98f),
            //    new Vector3(8779.17f, 6804.70f, 46.98f)));
            //WardSpots.Add(new WardSpot("PurpleBot->SoloBush", new Vector3(11889f, 4205f, 42.84f),
            //    new Vector3(11974.23f, 3807.21f, 42.84f), new Vector3(11646.10f, 3464.81f, 42.84f),
            //    new Vector3(11939f, 4255f, 42.84f)));
            //WardSpots.Add(new WardSpot("PurpleLizard->NashorPassBush", new Vector3(6299f, 10377.75f, 45.47f),
            //    new Vector3(6030.24f, 10292.37f, 54.29f), new Vector3(5846.10f, 10164.81f, 53.94f),
            //    new Vector3(6447f, 10463f, 54.63f)));

            //WardSpots.Add(new WardSpot("BlueTop->RiverLaneBush", new Vector3(2277.609f, 10033.42f, 50.784f), new Vector3(2277.609f, 10033.42f, 50), new Vector3(2277.609f, 10033.42f, 50.784f), new Vector3(1780f, 10690f, 52.8381f)));
            //WardSpots.Add(new WardSpot("Dragon", new Vector3(10152.86f, 2884.957f, 1009.02f), new Vector3(10152.86f, 2884.957f, 1009.02f), new Vector3(10152.86f, 2884.957f, 1009.02f), new Vector3(10122f, 3958f, -71.2406f)));
            WardSpots.Add(new WardSpot("", new Vector3(2729f, 10879f, -71f), new Vector3(2729f, 10879f, -71f), new Vector3(2729, 10879, -71f), new Vector3(2524f, 10406f, 54f)));
            WardSpots.Add(new WardSpot("", new Vector3(2303f, 10868f, 53f), new Vector3(2303f, 10868f, 53f), new Vector3(2303f, 10868f, 53f), new Vector3(1774f, 10756f, 52f)));
            WardSpots.Add(new WardSpot("", new Vector3(5223f, 6789f, 50f), new Vector3(5223f, 6789f, 50f), new Vector3(5223f, 6789f, 50f), new Vector3(5520f, 6342f, 51f)));
            WardSpots.Add(new WardSpot("", new Vector3(5191f, 7137f, 50f), new Vector3(5191f, 7137f, 50f), new Vector3(5191f, 7137f, 50f), new Vector3(5674f, 7358f, 51f)));
            WardSpots.Add(new WardSpot("", new Vector3(8368f, 4594f, 51f), new Vector3(8368f, 4594f, 51f), new Vector3(8368f, 4594f, 51f), new Vector3(7990f, 4282f, 53f)));
            WardSpots.Add(new WardSpot("", new Vector3(8100f, 3429f, 51f), new Vector3(8100f, 3429f, 51f), new Vector3(8100f, 3429f, 51f), new Vector3(8256f, 2920f, 51f)));
            WardSpots.Add(new WardSpot("", new Vector3(4634f, 11283f, 49f), new Vector3(4634f, 11283f, 49f), new Vector3(4634f, 11283f, 49f), new Vector3(4818f, 10866f, -71f)));
            WardSpots.Add(new WardSpot("", new Vector3(6672f, 11466f, 53f), new Vector3(6672f, 11466f, 53f), new Vector3(6672f, 11466f, 53f), new Vector3(6824f, 10656f, 55f)));
            WardSpots.Add(new WardSpot("", new Vector3(6518f, 10367f, 53f), new Vector3(6518f, 10367f, 53f), new Vector3(6518f, 10367f, 53f), new Vector3(6574f, 12006f, 56f)));
            WardSpots.Add(new WardSpot("", new Vector3(9572f, 8038f, 57f), new Vector3(9572f, 8038f, 57f), new Vector3(9572f, 8038f, 57f), new Vector3(9130f, 8346f, 53f)));
            WardSpots.Add(new WardSpot("", new Vector3(9697f, 7854f, 51f), new Vector3(9697f, 7854f, 51f), new Vector3(9697f, 7854f, 51f), new Vector3(9422f, 7408f, 52f)));
            WardSpots.Add(new WardSpot("", new Vector3(12235f, 4068f, -68f), new Vector3(12235f, 4068f, -68f), new Vector3(12235f, 4068f, -68f), new Vector3(12372f, 4508f, 51f)));
            WardSpots.Add(new WardSpot("", new Vector3(12443f, 4021f, -7f), new Vector3(12443f, 4021f, -7f), new Vector3(12443f, 4021f, -7f), new Vector3(13003f, 3818f, 51f)));

            WardSpots.Add(new WardSpot("Blue Golem", new Vector3(3261.93f, 7773.65f, 60.0f)));
            WardSpots.Add(new WardSpot("Blue Lizard", new Vector3(7831.46f, 3501.13f, 60.0f)));
            WardSpots.Add(new WardSpot("Blue Tri Bush", new Vector3(10586.62f, 3067.93f, 60.0f)));
            WardSpots.Add(new WardSpot("Blue Pass Bush", new Vector3(6483.73f, 4606.57f, 60.0f)));
            WardSpots.Add(new WardSpot("Blue River Entrance", new Vector3(7610.46f, 5000.0f, 60.0f)));
            WardSpots.Add(new WardSpot("Blue Round Bush", new Vector3(4717.09f, 7142.35f, 50.83f)));
            WardSpots.Add(new WardSpot("Blue River Round Bush", new Vector3(4882.86f, 8393.77f, 27.83f)));
            WardSpots.Add(new WardSpot("Blue Split Push Bush", new Vector3(6951.01f, 3040.55f, 52.26f)));
            WardSpots.Add(new WardSpot("Blue Riveer Center Close", new Vector3(5583.74f, 3573.83f, 51.43f)));
            WardSpots.Add(new WardSpot("Purple Golem", new Vector3(11600.35f, 7090.37f, 51.73f)));
            WardSpots.Add(new WardSpot("Purple Golem2", new Vector3(11573.9f, 6457.76f, 51.71f)));
            WardSpots.Add(new WardSpot("Purple Tri Bush2", new Vector3(12629.72f, 4908.16f, 48.62f)));
            WardSpots.Add(new WardSpot("Purple Lizard", new Vector3(7018.75f, 11362.12f, 54.76f)));
            WardSpots.Add(new WardSpot("Purple Tri Bush", new Vector3(4232.69f, 11869.25f, 47.56f)));
            WardSpots.Add(new WardSpot("Purple Pass Bush", new Vector3(8198.22f, 10267.89f, 49.38f)));
            WardSpots.Add(new WardSpot("Purple River Entrance", new Vector3(7202.43f, 9881.83f, 53.18f)));
            WardSpots.Add(new WardSpot("Purple Round Bush", new Vector3(10074.63f, 7761.62f, 51.74f)));
            WardSpots.Add(new WardSpot("Purple River Round Bush", new Vector3(9795.85f, 6355.15f, -12.21f)));
            WardSpots.Add(new WardSpot("Purple Split Push Bush", new Vector3(7836.85f, 11906.34f, 56.48f)));
            WardSpots.Add(new WardSpot("Dragon", new Vector3(10546.35f, 5019.06f, -60.0f)));
            WardSpots.Add(new WardSpot("Dragon Bush", new Vector3(9344.95f, 5703.43f, -64.07f)));
            WardSpots.Add(new WardSpot("Baron", new Vector3(4334.98f, 9714.54f, -60.42f)));
            WardSpots.Add(new WardSpot("Baron Bush", new Vector3(5363.31f, 9157.05f, -62.70f)));
            WardSpots.Add(new WardSpot("Purple Bot T2", new Vector3(12731.25f, 9132.66f, 50.32f)));
            WardSpots.Add(new WardSpot("Purple Bot T2", new Vector3(8036.52f, 12882.94f, 45.19f)));
            WardSpots.Add(new WardSpot("Purple Mid T1", new Vector3(9757.9f, 8768.25f, 50.73f)));
            WardSpots.Add(new WardSpot("Blue Mid T1", new Vector3(4749.79f, 5890.76f, 53.59f)));
            WardSpots.Add(new WardSpot("Blue Bot T2", new Vector3(5983.58f, 1547.98f, 52.99f)));
            WardSpots.Add(new WardSpot("Blue Top T2", new Vector3(1213.70f, 5324.73f, 58.77f)));
            WardSpots.Add(new WardSpot("Blue MidLane", new Vector3(6523.58f, 6743.31f, 60.0f)));
            WardSpots.Add(new WardSpot("Purple Nidlane", new Vector3(8223.67f, 8110.15f, 60.0f)));
            WardSpots.Add(new WardSpot("Purple Mid Path", new Vector3(9736.8f, 6916.26f, 51.98f)));
            WardSpots.Add(new WardSpot("Blue Tri Top", new Vector3(2222.31f, 9964.1f, 53.2f)));

            WardSpots.Add(new WardSpot("Dragon -> Tri Bush", new Vector3(10072.0f, 3908.0f, -71.24f), new Vector3(10297.93f, 3358.59f, 49.03f), new Vector3(10273.9f, 3257.76f, 49.03f), new Vector3(10072.0f, 3908.0f, -71.24f)));
            WardSpots.Add(new WardSpot("Nashor -> Tri Bush", new Vector3(4724.0f, 10856.0f, -71.24f), new Vector3(4627.26f, 11311.69f, -71.24f), new Vector3(4473.9f, 11457.76f, 51.4f), new Vector3(4724.0f, 10856.0f, -71.24f)));
            WardSpots.Add(new WardSpot("Blue Top -> Solo Bush", new Vector3(2824.0f, 10356.0f, 54.33f), new Vector3(3078.62f, 10868.39f, 54.33f), new Vector3(3078.62f, 10868.39f, -67.95f), new Vector3(2824.0f, 10356.0f, 54.33f)));
            WardSpots.Add(new WardSpot("Blue Mid -> round Bush", new Vector3(5474.0f, 7906.0f, 51.67f), new Vector3(5132.65f, 8373.2f, 51.67f), new Vector3(5123.9f, 8457.76f, -21.23f), new Vector3(5474.0f, 7906.0f, 51.67f)));
            WardSpots.Add(new WardSpot("Blue Mid -> River Lane Bush", new Vector3(5874.0f, 7656.0f, 51.65f), new Vector3(6202.24f, 8132.12f, 51.65f), new Vector3(6202.24f, 8132.12f, -67.39f), new Vector3(5874.0f, 7656.0f, 51.65f)));
            WardSpots.Add(new WardSpot("Blue Lizard -> Dragon Pass Bush", new Vector3(8022.0f, 4258.0f, 53.72f), new Vector3(8400.68f, 4657.41f, 53.72f), new Vector3(8523.9f, 4707.76f, 51.24f), new Vector3(8022.0f, 4258.0f, 53.72f)));
            WardSpots.Add(new WardSpot("Purple Mid -> Round Bush", new Vector3(9372.0f, 7008.0f, 52.63f), new Vector3(9703.5f, 6589.9f, 52.63f), new Vector3(9823.9f, 6507.76f, 23.47f), new Vector3(9372.0f, 7008.0f, 52.63f)));
            WardSpots.Add(new WardSpot("Purple Mid -> River Round Bush", new Vector3(9072.0f, 7158.0f, 53.04f), new Vector3(8705.95f, 6819.1f, 53.04f), new Vector3(8718.88f, 6764.86f, 95.75f), new Vector3(9072.0f, 7158.0f, 53.04f)));
            WardSpots.Add(new WardSpot("Purple Mid -> River Lane Bush", new Vector3(8530.27f, 6637.38f, 46.98f), new Vector3(8539.27f, 6637.38f, 46.98f), new Vector3(8396.10f, 6464.81f, 46.98f), new Vector3(8779.17f, 6804.70f, 46.98f)));
            WardSpots.Add(new WardSpot("Purple Bottom -> Solo Bush", new Vector3(12422.0f, 4508.0f, 51.73f), new Vector3(12353.94f, 4031.58f, 51.73f), new Vector3(12023.9f, 3757.76f, -66.25f), new Vector3(12422.0f, 4508.0f, 51.73f)));
            WardSpots.Add(new WardSpot("Purple Lizard -> Nashor Pass Bush", new Vector3(6824.0f, 10656.0f, 56.0f), new Vector3(6370.69f, 10359.92f, 56.0f), new Vector3(6273.9f, 10307.76f, 53.67f), new Vector3(6824.0f, 10656.0f, 56.0f)));
            WardSpots.Add(new WardSpot("Blue Golem -> Blue Lizard", new Vector3(8272.0f, 2908.0f, 51.13f), new Vector3(8163.7056f, 3436.0476f, 51.13f), new Vector3(8163.71f, 3436.05f, 51.6628f), new Vector3(8272.0f, 2908.0f, 51.13f)));
            WardSpots.Add(new WardSpot("Red Golem -> Red Lizard", new Vector3(6574.0f, 12006.0f, 56.48f), new Vector3(6678.08f, 11477.83f, 56.48f), new Vector3(6678.08f, 11477.83f, 53.85f), new Vector3(6574.0f, 12006.0f, 56.48f)));
            WardSpots.Add(new WardSpot("Blue Top Side Brush", new Vector3(1774.0f, 10756.0f, 52.84f), new Vector3(2302.36f, 10874.22f, 52.84f), new Vector3(2773.9f, 11307.76f, -71.24f), new Vector3(1774.0f, 10756.0f, 52.84f)));
            WardSpots.Add(new WardSpot("Mid Lane Death Brush", new Vector3(5874.0f, 8306.0f, -70.12f), new Vector3(5332.9f, 8275.21f, -70.12f), new Vector3(5123.9f, 8457.76f, -21.23f), new Vector3(5874.0f, 8306.0f, -70.12f)));
            WardSpots.Add(new WardSpot("Mid Lane Death Brush Right Side", new Vector3(9022.0f, 6558.0f, 71.24f), new Vector3(9540.43f, 6657.68f, 71.24f), new Vector3(9773.9f, 6457.76f, 9.56f), new Vector3(9022.0f, 6558.0f, 71.24f)));
            WardSpots.Add(new WardSpot("Blue Inner Turret Jungle", new Vector3(6874.0f, 1708.0f, 50.52f), new Vector3(6849.11f, 2252.01f, 50.52f), new Vector3(6723.9f, 2507.76f, 52.17f), new Vector3(6874.0f, 1708.0f, 50.52f)));
            WardSpots.Add(new WardSpot("Purple Inner Turret Jungle", new Vector3(8122.0f, 13206.0f, 52.84f), new Vector3(8128.53f, 12658.41f, 52.84f), new Vector3(8323.9f, 12457.76f, 56.48f), new Vector3(8122.0f, 13206.0f, 52.84f)));

            Drawing.OnDraw += Drawing_OnDraw;
            //Game.OnGameUpdate += Game_OnGameUpdate;
            ThreadHelper.GetInstance().Called += Game_OnGameUpdate;
            Game.OnWndProc += Game_OnWndProc;
            Game.OnGameSendPacket += Game_OnGameSendPacket;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
        }

        ~WardCorrector()
        {
            Game.OnWndProc -= Game_OnWndProc;
            //Game.OnGameUpdate -= Game_OnGameUpdate;
            ThreadHelper.GetInstance().Called -= Game_OnGameUpdate;
            Game.OnGameSendPacket -= Game_OnGameSendPacket;
            Drawing.OnDraw -= Drawing_OnDraw;
            Spellbook.OnCastSpell -= Spellbook_OnCastSpell;

            WardSpots = null;
            _latestSpellSlot = SpellSlot.Unknown;
            _latestWardSpot = null;
        }

        public bool IsActive()
        {
            return Ward.Wards.GetActive() && WardCorrector_Wards.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            WardCorrector_Wards.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("WARDS_WARDCORRECTOR_MAIN"), "SAwarenessWardsWardCorrector"));
            WardCorrector_Wards.MenuItems.Add(
                WardCorrector_Wards.Menu.AddItem(new MenuItem("SAwarenessWardsWardCorrectorKey", Language.GetString("WARDS_WARDCORRECTOR_TRINKET")).SetValue(new KeyBind(52, KeyBindType.Press))));
            WardCorrector_Wards.MenuItems.Add(
                WardCorrector_Wards.Menu.AddItem(new MenuItem("SAwarenessWardsWardCorrectorActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return WardCorrector_Wards;
        }

        void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            //throw new NotImplementedException();
            if (!IsActive())
                return;


            if (_latestSpellSlot != SpellSlot.Unknown && sender.Owner.IsMe && IsWard(sender, args))
            {
                _drawSpots = false;
                foreach (WardSpot wardSpot in WardSpots)
                {
                    if (!wardSpot.SafeWard &&
                        Vector3.Distance(wardSpot.Pos,
                            new Vector3(args.StartPosition.X, args.StartPosition.Y, args.StartPosition.Z)) <= 250 &&
                        !_wardAlreadyCorrected)
                    {
                        args.Process = false;
                        _wardAlreadyCorrected = true;
                        //SendPacket
                        //var sCastPacket = new byte[28];
                        //var writer = new BinaryWriter(new MemoryStream(sCastPacket));
                        //writer.Write((byte)0x9A);
                        //writer.Write(mNetworkId);
                        //writer.Write(spellId);
                        //writer.Write(unknown);
                        //writer.Write(wardSpot.Pos.X);
                        //writer.Write(wardSpot.Pos.Y);
                        //writer.Write(wardSpot.Pos.X);
                        //writer.Write(wardSpot.Pos.Y);
                        //writer.Write(tNetworkId);
                        //Game.SendPacket(sCastPacket, PacketChannel.C2S, PacketProtocolFlags.Reliable);
                        sender.CastSpell(
                            args.Slot,
                            new Vector3(
                                wardSpot.Pos.X, wardSpot.Pos.Y,
                                NavMesh.GetHeightForPosition(wardSpot.Pos.X, wardSpot.Pos.Y)));
                        //Packet.C2S.Cast.Encoded(new Packet.C2S.Cast.Struct(decoded.TargetNetworkId, decoded.Slot, decoded.SourceNetworkId,
                        //    wardSpot.Pos.X, wardSpot.Pos.Y, wardSpot.Pos.X, wardSpot.Pos.Y, decoded.SpellFlag)).Send();
                        //TODO: Check if its correct
                        _wardAlreadyCorrected = false;
                        return;
                    }
                    if (wardSpot.SafeWard &&
                        Vector3.Distance(wardSpot.MagneticPos,
                            new Vector3(args.StartPosition.X, args.StartPosition.Y, args.StartPosition.Z)) <=
                        250 &&
                        !_wardAlreadyCorrected)
                    {
                        args.Process = false;
                        ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo,
                            new Vector3(wardSpot.MovePos.X, wardSpot.MovePos.Y, wardSpot.MovePos.Z));
                        _latestWardSpot = wardSpot;
                        return;
                    }
                }
            }
        }

        private bool IsWard(Spellbook spellBook, SpellbookCastSpellEventArgs args)
        {
            return
                Ward.WardItems.Exists(
                    y =>
                        y.Id ==
                        (int)
                            ObjectManager.Player.InventoryItems.Find(
                                x => x.SpellSlot == spellBook.GetSpell(args.Slot).Slot).Id);
        }

        private void Game_OnGameSendPacket(GamePacketEventArgs args) //TODO: Need to find a way to block item usage
        {
            if (!IsActive())
                return;

            var gPacket = new GamePacket(args.PacketData);
            var reader = new BinaryReader(new MemoryStream(args.PacketData));

            byte packetId = reader.ReadByte(); //PacketId
            if (packetId == 0x9A) //OLD 0x9A
            {
                //int mNetworkId = BitConverter.ToInt32(reader.ReadBytes(4), 0);
                //byte spellId = reader.ReadByte();
                //byte unknown = reader.ReadByte();
                //float fromX = BitConverter.ToSingle(reader.ReadBytes(4), 0);
                //float fromY = BitConverter.ToSingle(reader.ReadBytes(4), 0);
                //float toX = BitConverter.ToSingle(reader.ReadBytes(4), 0);
                //float toY = BitConverter.ToSingle(reader.ReadBytes(4), 0);
                //int tNetworkId = BitConverter.ToInt32(reader.ReadBytes(4), 0);
                Packet.C2S.Cast.Struct decoded = Packet.C2S.Cast.Decoded(args.PacketData);
                //PacketSpellId nSpellId = PacketSpellId.ConvertPacketCastToId(spellId);
                if (/*_latestSpellSlot == nSpellId.SSpellSlot && */_latestSpellSlot != SpellSlot.Unknown)
                {
                    _drawSpots = false;
                    foreach (WardSpot wardSpot in WardSpots)
                    {
                        if (!wardSpot.SafeWard &&
                            Vector3.Distance(wardSpot.Pos,
                                new Vector3(decoded.FromX, decoded.FromY, ObjectManager.Player.ServerPosition.Z)) <= 250 &&
                            !_wardAlreadyCorrected)
                        {
                            args.Process = false;
                            _wardAlreadyCorrected = true;
                            //SendPacket
                            //var sCastPacket = new byte[28];
                            //var writer = new BinaryWriter(new MemoryStream(sCastPacket));
                            //writer.Write((byte)0x9A);
                            //writer.Write(mNetworkId);
                            //writer.Write(spellId);
                            //writer.Write(unknown);
                            //writer.Write(wardSpot.Pos.X);
                            //writer.Write(wardSpot.Pos.Y);
                            //writer.Write(wardSpot.Pos.X);
                            //writer.Write(wardSpot.Pos.Y);
                            //writer.Write(tNetworkId);
                            //Game.SendPacket(sCastPacket, PacketChannel.C2S, PacketProtocolFlags.Reliable);
                            Packet.C2S.Cast.Encoded(new Packet.C2S.Cast.Struct(decoded.TargetNetworkId, decoded.Slot, decoded.SourceNetworkId, 
                                wardSpot.Pos.X, wardSpot.Pos.Y, wardSpot.Pos.X, wardSpot.Pos.Y, decoded.SpellFlag)).Send();
                            //TODO: Check if its correct
                            _wardAlreadyCorrected = false;
                            return;
                        }
                        if (wardSpot.SafeWard &&
                            Vector3.Distance(wardSpot.MagneticPos,
                                new Vector3(decoded.FromX, decoded.FromY, ObjectManager.Player.ServerPosition.Z)) <=
                            250 &&
                            !_wardAlreadyCorrected)
                        {
                            args.Process = false;
                            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo,
                                new Vector3(wardSpot.MovePos.X, wardSpot.MovePos.Y, wardSpot.MovePos.Z));
                            _latestWardSpot = wardSpot;
                            return;
                        }
                    }
                }
            }
        }

        private void Game_OnWndProc(WndEventArgs args)
        {
            //http://msdn.microsoft.com/en-us/library/windows/desktop/ms632585(v=vs.85).aspx
            const int WM_KEYDOWN = 0x100;
            const int WM_LBUTTONUP = 0x202;
            const int WM_LBUTTONDOWN = 0x201;
            const int WM_RBUTTONUP = 0x205;
            const int WM_RBUTTONDOWN = 0x204;

            if (!IsActive())
                return;
            if (MenuGUI.IsChatOpen)
                return;
            uint trinketKey =
                WardCorrector_Wards.GetMenuItem("SAwarenessWardsWardCorrectorKey").GetValue<KeyBind>().Key;
            if (args.Msg == WM_KEYDOWN)
            {
                //Console.WriteLine("Hero: " + ObjectManager.Player.ServerPosition);
                //Console.WriteLine("Cursor: " + Drawing.ScreenToWorld(Utils.GetCursorPos()));
                InventorySlot inventoryItem = null;
                int inventoryItemId = -1;
                if (trinketKey == args.WParam)
                {
                    _latestSpellSlot = SpellSlot.Trinket;
                    inventoryItemId = 6;
                }
                else
                {
                    _latestSpellSlot = SpellSlot.Unknown;
                }
                if (_latestSpellSlot == SpellSlot.Unknown)
                {
                    switch (args.WParam)
                    {
                        case '1':
                            _latestSpellSlot = SpellSlot.Item1;
                            inventoryItemId = 0;
                            break;

                        case '2':
                            _latestSpellSlot = SpellSlot.Item2;
                            inventoryItemId = 1;
                            break;

                        case '3':
                            _latestSpellSlot = SpellSlot.Item3;
                            inventoryItemId = 2;
                            break;

                        //case trinketKey:
                        //    _latestSpellSlot = SpellSlot.Trinket;
                        //    inventoryItemId = 6;
                        //    break;

                        case '5':
                            _latestSpellSlot = SpellSlot.Item5;
                            inventoryItemId = 3;
                            break;

                        case '6':
                            _latestSpellSlot = SpellSlot.Item6;
                            inventoryItemId = 4;
                            break;

                        case '7':
                            _latestSpellSlot = SpellSlot.Item4;
                            inventoryItemId = 5;
                            break;

                        default:
                            _drawSpots = false;
                            _latestSpellSlot = SpellSlot.Unknown;
                            break;
                    }
                }

                foreach (InventorySlot inventorySlot in ObjectManager.Player.InventoryItems)
                {
                    if (inventorySlot.Slot == inventoryItemId)
                    {
                        inventoryItem = inventorySlot;
                        break;
                    }
                }

                if (_latestSpellSlot != SpellSlot.Unknown)
                {
                    if (inventoryItem != null)
                    {
                        foreach (Ward.WardItem wardItem in Ward.WardItems)
                        {
                            if ((int)inventoryItem.Id == wardItem.Id &&
                                wardItem.Type != Ward.WardType.Temp &&
                                wardItem.Type != Ward.WardType.TempVision &&
                                ObjectManager.Player.Spellbook.CanUseSpell(_latestSpellSlot) == SpellState.Ready ||
                                ObjectManager.Player.Spellbook.CanUseSpell(_latestSpellSlot) == (SpellState)1)
                            {
                                _drawSpots = true;
                            }
                        }
                        if (_drawSpots == false)
                        {
                            _latestSpellSlot = SpellSlot.Unknown;
                        }
                    }
                }
            }
            else if (args.Msg == WM_LBUTTONUP && _drawSpots)
            {
                _drawSpots = false;
            }
            else if (args.Msg == WM_RBUTTONDOWN && _drawSpots)
            {
                _drawSpots = false;
            }
            else if (args.Msg == WM_RBUTTONDOWN)
            {
                _latestWardSpot = null;
            }
        }

        private void Game_OnGameUpdate(object sender, EventArgs args)
        {
            if (!IsActive() || lastGameUpdateTime + new Random().Next(500, 1000) > Environment.TickCount)
                return;

            lastGameUpdateTime = Environment.TickCount;
            if (_latestWardSpot != null && _latestSpellSlot != SpellSlot.Unknown)
            {
                if (Vector3.Distance(ObjectManager.Player.ServerPosition, _latestWardSpot.ClickPos) <= 650)
                {
                    _wardAlreadyCorrected = true;
                    ObjectManager.Player.Spellbook.CastSpell(_latestSpellSlot, _latestWardSpot.ClickPos);
                    _wardAlreadyCorrected = false;
                    _latestWardSpot = null;
                }
            }
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!IsActive())
                return;
            if (!_drawSpots)
                return;
            foreach (WardSpot ward in WardSpots)
            {
                if (Common.IsOnScreen(ward.Pos))
                    Utility.DrawCircle(ward.Pos, 50, System.Drawing.Color.GreenYellow);
                if (ward.SafeWard)
                {
                    if (Common.IsOnScreen(ward.MagneticPos))
                    {
                        Utility.DrawCircle(ward.MagneticPos, 30, System.Drawing.Color.Red);
                        DrawArrow(ward.MagneticPos, ward.Pos, System.Drawing.Color.RoyalBlue);
                    }
                }
            }
            //Utility.DrawCircle();
        }

        private void DrawArrow(Vector3 start, Vector3 end, System.Drawing.Color color) //TODO. Check if its correct calculated
        {
            Vector2 mPos1 = Drawing.WorldToScreen(start);
            Vector2 mPos2 = Drawing.WorldToScreen(end);
            Drawing.DrawLine(mPos1[0], mPos1[1], mPos2[0], mPos2[1], 1.0f, color);
            //Vector2 mmPos2 = new Vector2(mPos2[0], mPos2[1]);
            //Vector2 end1 = Geometry.Rotated(mmPos2, Geometry.DegreeToRadian(45));
            //Vector2 end2 = Geometry.Rotated(mmPos2, Geometry.DegreeToRadian(315));
            //end1.Normalize();
            //end2.Normalize();
            //end1 = Vector2.Multiply(mmPos2, end1);
            //Drawing.DrawLine(mPos2[0], mPos2[1], mmPos2.X * end1.X, mmPos2.Y * end1.Y, 1.0f, color);
            //Drawing.DrawLine(mPos2[0], mPos2[1], mmPos2.X * end2.X, mmPos2.Y * end2.Y, 1.0f, color);
            //float rad1 = Geometry.DegreeToRadian(45);
            //float cos1 = (float)Math.Cos(rad1);
            //float sin1 = (float)Math.Sin(rad1);
            //float rad2 = Geometry.DegreeToRadian(315);
            //float cos2 = (float)Math.Cos(rad2);
            //float sin2 = (float)Math.Sin(rad2);
            //Drawing.DrawLine(mPos2[0], mPos2[1], mPos1[0] * cos1 - mPos1[1] * sin1, mPos1[0] * sin1 + mPos1[1] * cos1, 1.0f, color);
            //Drawing.DrawLine(mPos2[0], mPos2[1], mPos2[0] * cos2 - mPos2[1] * sin2, mPos2[0] * sin2 + mPos2[1] * cos2, 1.0f, color);
            //double r1 = Math.Sqrt(mPos1[0] * mPos1[0] + mPos1[1] + mPos1[1]);
            //double r2 = Math.Sqrt(mPos2[0] * mPos2[0] + mPos2[1] + mPos2[1]);
            //Vector2 mPos2P = new Vector2((float)(r2 * Math.Cos(45)), (float)(r2 * Math.Sin(45)));
            //Vector2 mPos2N = new Vector2((float)(r2 * Math.Cos(-45)), (float)(r2 * Math.Sin(-45)));
            //Drawing.DrawLine(mPos2[0], mPos2[1], mPos2P.X, mPos2P.Y, 1.0f, color);
            //Drawing.DrawLine(mPos2[0], mPos2[1], mPos2N.X, mPos2N.Y, 1.0f, color);
        }

        private class PacketSpellId
        {
            public readonly SpellSlot SSpellSlot;
            public bool IsSummoner;

            public PacketSpellId(SpellSlot spellSlot, bool isSummoner)
            {
                SSpellSlot = spellSlot;
                IsSummoner = isSummoner;
            }

            public static PacketSpellId ConvertPacketCastToId(int id)
            {
                switch (id)
                {
                    case 128:
                        return new PacketSpellId(SpellSlot.Q, false);

                    case 129:
                        return new PacketSpellId(SpellSlot.W, false);

                    case 130:
                        return new PacketSpellId(SpellSlot.E, false);

                    case 131:
                        return new PacketSpellId(SpellSlot.R, false);

                    case 132:
                        return new PacketSpellId(SpellSlot.Item1, false);

                    case 133:
                        return new PacketSpellId(SpellSlot.Item2, false);

                    case 134:
                        return new PacketSpellId(SpellSlot.Item3, false);

                    case 145:
                        return new PacketSpellId(SpellSlot.Item4, false);

                    case 136:
                        return new PacketSpellId(SpellSlot.Item5, false);

                    case 137:
                        return new PacketSpellId(SpellSlot.Item6, false);

                    case 138:
                        return new PacketSpellId(SpellSlot.Trinket, false);

                    case 64:
                        return new PacketSpellId(SpellSlot.Q, true);

                    case 65:
                        return new PacketSpellId(SpellSlot.W, true);

                    case 192:
                        return new PacketSpellId(SpellSlot.Q, true);

                    case 193:
                        return new PacketSpellId(SpellSlot.W, true);

                    case 10:
                        return new PacketSpellId(SpellSlot.Recall, false);
                }
                return new PacketSpellId(SpellSlot.Unknown, false);
            }
        }

        private class WardSpot
        {
            public readonly Vector3 ClickPos;
            public readonly Vector3 MagneticPos;
            public readonly bool SafeWard;
            public Vector3 MovePos;
            public String Name;
            public Vector3 Pos;

            public WardSpot(string name, Vector3 pos)
            {
                Name = name;
                Pos = pos;
                MagneticPos = new Vector3();
                ClickPos = new Vector3();
                MovePos = new Vector3();
                SafeWard = false;
            }

            public WardSpot(string name, Vector3 magneticPos, Vector3 clickPos, Vector3 pos, Vector3 movePos)
            {
                Name = name;
                Pos = pos;
                MagneticPos = magneticPos;
                ClickPos = clickPos;
                MovePos = movePos;
                SafeWard = true;
            }
        }
    }
}
