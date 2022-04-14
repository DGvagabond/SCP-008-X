using RemoteAdmin;
using CommandSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using UnityEngine;
using Exiled.Permissions.Extensions;
using SCP008X.Components;

namespace SCP008X.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Infect : ICommand
    {
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
            Player ply = Player.Get(arguments.At(0));
            if(ply == null)
            {
                response = "Invalid player.";
                return false;
            }
            switch (ply.Role.Team)
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

            if(ply.ReferenceHub.TryGetComponent(out SCP008 s008))
            {
                response = "This player is already infected.";
                return false;
            }
            ply.ReferenceHub.gameObject.AddComponent<SCP008>();
            ply.ShowHint($"<color=yellow><b>SCP-008</b></color>\n{SCP008X.Instance.Config.InfectionAlert}", 10f);
            response = $"{ply.Nickname} has been infected.";
            return true;
        }
    }
}
