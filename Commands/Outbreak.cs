using CommandSystem;
using System;
using System.Linq;
using Exiled.API.Enums;
using Exiled.Permissions.Extensions;
using Exiled.API.Features;
using MEC;

namespace SCP008X
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Leak : ICommand
    {
        private readonly Random _gen = new Random();
        public string Command { get; } = "leak";

        public string[] Aliases { get; } = new string[0];

        public string Description { get; } = "Force an outbreak of SCP-008";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!((CommandSender)sender).CheckPermission("scp008.infect"))
            {
                response = "Missing permissions.";
                return false;
            }
            if (Scp008X.Instance.Outbreak)
            {
                response = "An outbreak has already happened.";
                return false;
            }
            Scp008X.Instance.Outbreak = true;
            Cassie.GlitchyMessage($"SCP 0 0 8 containment breach detected . Lockdown of heavy containment zone has begun",
                15,
                15);
            Generator079.Generators[0].ServerOvercharge(20f,
                true);
            Timing.CallDelayed(_gen.Next(1, 15), () =>
            {
                foreach(var ply in Player.List.Where(p => p.CurrentRoom.Zone == ZoneType.HeavyContainment))
                {
                    var chance = _gen.Next(1, 100);
                    if(chance <= Scp008X.Instance.Config.InfectionChance)
                    {
                        EventHandlers.Infect(ply);
                    }
                }
            });

            response = "SCP-008 outbreak has begun.";
            return true;
        }
    }
}