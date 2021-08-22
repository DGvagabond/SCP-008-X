using System;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;

namespace SCP008X.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Cure : ICommand
    {
        public string Command => "cure";
        public string[] Aliases { get; } = {"s8c"};
        public string Description => "Forcefully cure a player from SCP-008";

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
                case Team.TUT:
                case Team.RIP:
                    response = "This class can not have SCP-008 infection.";
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
            if(!ply.SessionVariables.ContainsKey("Scp008"))
            {
                response = "This player is not infected.";
                return false;
            }
            EventHandlers.ClearScp008(ply);
            response = $"{ply.Nickname} has been cured.";
            return true;
        }
    }
}