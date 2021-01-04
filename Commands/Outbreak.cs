using CommandSystem;
using System;
using Exiled.Permissions.Extensions;
using Exiled.API.Features;
using scp035.API;
using SCP999X.API;

namespace SCP008X
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Outbreak : ICommand
    {
        private static Player Pull035() => Scp035Data.GetScp035();
        private static Player Pull999() => SCP999API.GetScp999();
        private readonly Random _gen = new Random();
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
            if (Scp008X.Instance.Outbreak)
            {
                response = "An outbreak has already happened.";
                return false;
            }
            Scp008X.Instance.Outbreak = true;
            Cassie.Message("JAM_" + _gen.Next(0, 70).ToString("000") + "_" + _gen.Next(1, 4) + " SCP 0 0 8 containment breach detected . Lockdown of heavy containment zone has begun", Cassie.IsSpeaking, false);
            Generator079.Generators[0].ServerOvercharge(20f, true);
            foreach(var ply in Player.List)
            {
                switch (ply.CurrentRoom.Zone)
                {
                    case Exiled.API.Enums.ZoneType.HeavyContainment:
                        var chance = _gen.Next(1, 100);
                        if(chance <= Scp008X.Instance.Config.InfectionChance)
                        {
                            EventHandlers.Infect(ply);
                        }
                        break;
                }
            }
            foreach(var door in Map.Doors)
            {
                switch (door.doorType)
                {
                    case Door.DoorTypes.Checkpoint:
                        door.NetworkisOpen = false;
                        door.Networklocked = true;
                        break;
                }
            }
            response = "SCP-008 outbreak has begun.";
            return true;
        }
    }
}