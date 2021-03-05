using CommandSystem;
using System;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;

namespace SCP008X
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Infect : ICommand
    {
        public string Command { get; } = "infect";

        public string[] Aliases { get; } = { };

        public string Description { get; }="Forcefully infect a player with SCP-008";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!((CommandSender)sender).CheckPermission("scp008.infect"))
            {
                response = "Missing permissions.";
                return false;
            }
            
            Player ply = Player.Get(arguments.At(0));
            
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
                if(ply.IsScp035())
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
                if (ply.IsScp999())
                {
                    response = "You can not infect SCP-999 hosts.";
                    return false;
                }
            }
            catch (Exception)
            {
                Log.Debug($"SCP-999-X, by DGvagabond, is not installed. Skipping check.", Scp008X.Instance.Config.DebugMode);
            }

            try
            {
                if (ply.IsScp999())
                {
                    response = "You cannot infect SCP-343.";
                    return false;
                }
            }
            catch (Exception)
            {
                Log.Debug($"SCP-999 by british boi is not installed.", Scp008X.Instance.Config.DebugMode);
            }
            
            if (ply.ReferenceHub.TryGetComponent(out Scp008 _))
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
