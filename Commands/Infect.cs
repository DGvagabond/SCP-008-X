using CommandSystem;
using System;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using scp035.API;
using SCP999X.API;

namespace SCP008X
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Infect : ICommand
    {
        private static Player Pull035() => Scp035Data.GetScp035();
        private static Player Pull999() => SCP999API.GetScp999();
        public string Command { get; } = "infect";

        public string[] Aliases { get; } = null;

        public string Description { get; }="Forcefully infect a player with SCP-008";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!((CommandSender)sender).CheckPermission("scp008.infect"))
            {
                response = "Missing permissions.";
                return false;
            }
            var ply = Player.Get(arguments.At(0));
            if(ply == null)
            {
                response = "Invalid player.";
                return false;
            }
            switch (ply.Team)
            {
                case Team.SCP:
                    response = "You can not infect SCP players.";
                    return false;
                case Team.TUT:
                    response = "You can not infect this class.";
                    return false;
                case Team.RIP:
                    response = "You can not infect the dead.";
                    return false;
            }
            try
            {
                if(ply.UserId == Pull035().UserId)
                {
                    response = "You can not infect SCP players.";
                    return false;
                }
            }
            catch (Exception)
            {
                Log.Debug($"SCP-035, by Cyanox, is not installed. Skipping check.", Scp008X.Instance.Config.DebugMode);
            }
            try
            {
                if (ply.UserId == Pull999().UserId)
                {
                    response = "You can not infect SCP-999 hosts.";
                    return false;
                }
            }
            catch (Exception)
            {
                Log.Debug($"SCP-999-X, by DGvagabond, is not installed. Skipping check.", Scp008X.Instance.Config.DebugMode);
            }
            if (ply.ReferenceHub.TryGetComponent(out Scp008 s008))
            {
                response = "This player is already infected.";
                return false;
            }
            ply.ReferenceHub.gameObject.AddComponent<Scp008>();
            EventHandlers.Victims.Add(ply);
            ply.ShowHint($"<color=yellow><b>SCP-008</b></color>\n{Scp008X.Instance.Config.InfectionAlert}", 10f);
            response = $"{ply.Nickname} has been infected.";
            return true;
        }
    }
}
