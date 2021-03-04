using CommandSystem;
using System;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;

namespace SCP008X
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Cure : ICommand
    {
        public string Command { get; } = "cure";

        public string[] Aliases { get; } = { };

        public string Description { get; } = "Forcefully cure a player from SCP-008";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!((CommandSender)sender).CheckPermission("scp008.infect"))
            {
                response = "Missing permissions.";
                return false;
            }
            
            Player ply = Player.Get(arguments.At(0));
            
            if (ply == null)
            {
                response = "Invalid player.";
                return false;
            }
            
            if(!ply.ReferenceHub.TryGetComponent(out Scp008 scp008))
            {
                response = "This player is not infected.";
                return false;
            }
            
            try
            {
                UnityEngine.Object.Destroy(scp008);
                EventHandlers.Victims.Remove(ply);
                
                if (EventHandlers.Scp008Check()) 
                    EventHandlers.Contained(ply);

                response = $"{ply.Nickname} has been cured.";
                return true;
            }
            catch (Exception e)
            {
                Log.Debug($"Failed to destroy SCP008 component! {e}", Scp008X.Instance.Config.DebugMode);
                response = $"Failed to cure {ply.Nickname}. Please contact DGvagabond for support.";
                return false;
            }
        }
    }
}
