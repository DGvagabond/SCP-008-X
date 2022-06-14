// -----------------------------------------------------------------------
// <copyright file="Config.cs">
// Copyright (c) DGvagabond. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SCP008X
{
    using System.ComponentModel;
    using Exiled.API.Interfaces;
    
    public sealed class Config : IConfig
    {
        /// <inheritdoc />
        public bool IsEnabled { get; set; } = true;

        [Description("Only enable this if you're looking for bug sources!")]
        public bool DebugMode { get; set; } = false;

        [Description("Percent change to create infection.")]
        public int InfectionChance { get; set; } = 100;

        [Description("Percent change to successfully cure.")]
        public int CureChance { get; set; } = 50;

        [Description("Allow SCP-049 to instantly revive targets?")]
        public bool BuffDoctor { get; set; } = false;

        [Description("Base zombie health.")]
        public int ZombieHealth { get; set; } = 300;

        [Description("How much AHP should be given to Zombies?")]
        public ushort Scp008Buff { get; set; } = 10;

        [Description("008x zombie starting Ahp (Shield).")]
        public ushort StartingAhp { get; set; } = 100;

        [Description("How much AHP should zombies stop earning at?")]
        public ushort MaxAhp { get; set; } = 100;

        [Description("Whether to have a public cassie announcement.")]
        public bool CassieAnnounce { get; set; } = true;

        [Description("Announcement for server.")]
        public string Announcement { get; set; } = "SCP 0 0 8 containment breach detected . Allremaining";

        [Description("How much damage 008 zombie does per hit.")]
        public int ZombieDamage { get; set; } = 24;
        [Description("Text displayed to players after they've been infected")]
        public string InfectionAlert { get; set; } = "You've been infected! Use SCP-500 or a medkit to be cured!";
    }
}