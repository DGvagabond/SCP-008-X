using System;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;

namespace SCP008X.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Infect : ICommand
    {
        public string Command => "infect";
        public string[] Aliases { get; } = {"s8i"};
        public string Description => "Forcefully infect a player with SCP-008";

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
                if(ply.SessionVariables.ContainsKey("Scp035"))
                {
                    response = "You can not infect SCP players.";
                    return false;
                }
            }
            catch (Exception)
            {
                Log.Debug($"SCP-035, by Exiled-Team, is not installed. Skipping check.", Scp008X.Instance.Config.DebugMode);
            }
            if(ply.SessionVariables.ContainsKey("Scp008"))
            {
                response = "This player is already infected.";
                return false;
            }
            EventHandlers.Infect(ply);
            response = $"{ply.Nickname} has been infected.";
            return true;
        }
    }
}