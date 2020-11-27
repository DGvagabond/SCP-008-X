using RemoteAdmin;
using CommandSystem;
using System;
using Exiled.Permissions.Extensions;
using Exiled.API.Features;
using scp035.API;
using SCP999X.API;
using SCP008X.Components;

namespace SCP008X.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Outbreak : ICommand
    {
        private Player Pull035() => Scp035Data.GetScp035();
        private Player Pull999() => SCP999API.GetScp999();
        private Random Gen = new Random();
        public string Command { get; } = "outbreak";

        public string[] Aliases { get; } = null;

        public string Description { get; } = "Force an outbreak of SCP-008";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!((CommandSender)sender).CheckPermission("scp008.infect"))
            {
                response = "Missing permissions.";
                return false;
            }
            if (SCP008X.Instance.outbreak)
            {
                response = "An outbreak has already happened.";
                return false;
            }
            SCP008X.Instance.outbreak = true;
            Cassie.Message("JAM_" + Gen.Next(0, 70).ToString("000") + "_" + Gen.Next(1, 4) + " SCP 0 0 0 8 containment breach detected . Lockdown of heavy containment zone has begun", Cassie.IsSpeaking, false);
            Generator079.Generators[0].ServerOvercharge(10f, true);
            foreach(Player ply in Player.List)
            {
                switch (ply.CurrentRoom.Zone)
                {
                    case Exiled.API.Enums.ZoneType.HeavyContainment:
                        int chance = Gen.Next(1, 100);
                        if(chance <= SCP008X.Instance.Config.InfectionChance)
                        {
                            Infect(ply);
                        }
                        break;
                }
            }
            response = "SCP-008 outbreak has begun.";
            return true;
        }
        private void Infect(Player target)
        {
            try
            {
                if (target.UserId == Pull035().UserId) return;
            }
            catch (Exception)
            {
                Log.Debug($"SCP-035, by Cyanox, is not installed. Skipping method call.", SCP008X.Instance.Config.DebugMode);
            }
            try
            {
                if (target.UserId == Pull999().UserId) return;
            }
            catch (Exception)
            {
                Log.Debug($"SCP-999-X, by DGvagabond, is not installed. Skipping method call.", SCP008X.Instance.Config.DebugMode);
            }
            if(target.Role == RoleType.Tutorial) { return; }
            if (target.ReferenceHub.gameObject.TryGetComponent(out SCP008 scp008)) { return; }
            target.ReferenceHub.gameObject.AddComponent<SCP008>();
            target.ShowHint($"<color=yellow><b>SCP-008</b></color>\n{SCP008X.Instance.Config.InfectionAlert}", 10f);
        }
    }
}