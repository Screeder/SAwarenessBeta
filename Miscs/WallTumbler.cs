using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using SharpDX;

namespace SAwareness.Miscs
{
    class WallTumbler
    {

        InternalWallTumbler _positions = null;

        public WallTumbler()
        {
            switch (ObjectManager.Player.ChampionName)
            {
                case "Vayne":
                    _positions = new InternalWallTumbler(ObjectManager.Player.ChampionName, new List<InternalWallTumbler.Positions>(new[]
                    {
                        new InternalWallTumbler.Positions(new Vector3(12050f, 50f, 4830f), new Vector3(11510f, 50f, 4460f)), 
                        new InternalWallTumbler.Positions(new Vector3(6960f, 50f, 8940f), new Vector3(6700f, 50f, 8800f)), 
                    }));
                    break;
            }
        }

        class InternalWallTumbler
        {
            public String ChampionName;
            public List<Positions> Position;

            public InternalWallTumbler(String championName, List<Positions> position)
            {
                ChampionName = championName;
                Position = position;
            }

           public  class Positions
            {
                public Vector3 PositionStart;
                public Vector3 PositionEnd;

                public Positions(Vector3 positionStart, Vector3 positionEnd)
                {
                    PositionStart = positionStart;
                    PositionEnd = positionEnd;
                }
            }
        }
    }
}
