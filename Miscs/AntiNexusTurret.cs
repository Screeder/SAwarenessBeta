using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SAwareness.Miscs
{
    class AntiNexusTurret
    {
        public static Menu.MenuItemSettings AntiNexusTurretMisc = new Menu.MenuItemSettings(typeof(AntiNexusTurret));

        public AntiNexusTurret()
        {
            Obj_AI_Base.OnNewPath += Obj_AI_Hero_OnNewPath;
            ThreadHelper.GetInstance().Called += Game_OnGameUpdate;
        }

        ~AntiNexusTurret()
        {
            Obj_AI_Base.OnNewPath -= Obj_AI_Hero_OnNewPath;
            ThreadHelper.GetInstance().Called -= Game_OnGameUpdate;
        }

        public bool IsActive()
        {
            return Misc.Miscs.GetActive() && AntiNexusTurretMisc.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            AntiNexusTurretMisc.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("MISCS_ANTINEXUSTURRET_MAIN"), "SAwarenessMiscsAntiNexusTurret"));
            AntiNexusTurretMisc.MenuItems.Add(
                AntiNexusTurretMisc.Menu.AddItem(new MenuItem("SAwarenessMiscsAntiNexusTurretActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return AntiNexusTurretMisc;
        }

        void Game_OnGameUpdate(object sender, EventArgs args)
        {
            if (!IsActive())
                return;

            Obj_AI_Turret baseTurret = ObjectManager.Get<Obj_AI_Turret>().Find(turret => IsBaseTurret(turret, 1410, true, ObjectManager.Player.ServerPosition));
            if (baseTurret != null)
            {
                Obj_AI_Turret baseAllyTurret = ObjectManager.Get<Obj_AI_Turret>().Find(turret => IsBaseTurret(turret, 999999999, false, ObjectManager.Player.ServerPosition));
                Vector3 newPos = baseTurret.ServerPosition.Extend(baseAllyTurret.ServerPosition, 1425);
                ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, newPos);
            }
        }

        void Obj_AI_Hero_OnNewPath(Obj_AI_Base sender, GameObjectNewPathEventArgs args)
        {
            if(!IsActive() || !sender.IsMe)
                return;

            for (int i = 0; i < args.Path.Length; i++)
            {
                var point = args.Path[i];
                Obj_AI_Turret baseTurret = ObjectManager.Get<Obj_AI_Turret>().Find(turret => IsBaseTurret(turret, 1425, true, point));
                if (baseTurret != null)
                {
                    float dist =
                        args.Path[i - 1].Distance(
                            baseTurret.ServerPosition) - 1425f - 20f;
                    Vector3 newPos = args.Path[i - 1].Extend(point, dist);
                    ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, newPos);
                }
            }
        }

        public static bool IsBaseTurret(Obj_AI_Turret unit,
            float range,
            bool enemyTeam,
            Vector3 from)
        {
            if (unit == null || !unit.IsValid)
            {
                return false;
            }

            if (enemyTeam && unit.Team == ObjectManager.Player.Team)
            {
                return false;
            }
            else if (!enemyTeam && unit.Team != ObjectManager.Player.Team)
            {
                return false;
            }

            if (!unit.Name.Contains("TurretShrine_A"))
            {
                return false;
            }

            var @base = unit as Obj_AI_Base;
            var unitPosition = @base != null ? @base.ServerPosition : unit.Position;

            return !(range < float.MaxValue) ||
                   !(Vector2.DistanceSquared(
                       (@from.To2D().IsValid() ? @from : ObjectManager.Player.ServerPosition).To2D(),
                       unitPosition.To2D()) > range * range);
        }
    }
}
