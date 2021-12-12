// -----------------------------------------------------------------------
// <copyright file="Scp008.cs">
// Copyright (c) DGvagabond. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SCP008X
{
    using Exiled.CustomRoles.API.Features;
    
    public class Scp008 : CustomRole
    {
        public override uint Id { get; set; } = 008;
        public override RoleType Role { get; set; } = RoleType.Scp0492;
        public override int MaxHealth { get; set; } = Scp008X.Instance.Config.ZombieHealth;
        public override string Name { get; set; } = "SCP-008";
        public override string Description { get; set; } =
            "An instance of SCP-008 that spreads the 008 infection with each hit.";
    }
}