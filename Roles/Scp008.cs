// -----------------------------------------------------------------------
// <copyright file="Scp008.cs">
// Copyright (c) DGvagabond. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SCP008X
{
    using Exiled.API.Enums;
    using Exiled.API.Extensions;
    using Exiled.API.Features;
    using Exiled.API.Features.Attributes;
    using Exiled.CustomRoles.API.Features;
    using Exiled.Events.EventArgs;
    
    [CustomRole(RoleType.Scp0492)]
    public class Scp008 : CustomRole
    {
        public override uint Id { get; set; } = 008;
        public override RoleType Role { get; set; } = RoleType.Scp0492;
        public override int MaxHealth { get; set; } = Scp008X.Instance.Config.ZombieHealth;
        public override string Name { get; set; } = "SCP-008";
        public override string Description { get; set; } =
            "An instance of SCP-008 that spreads the infection with each hit.";
        public override string CustomInfo { get; set; } = "SCP-008";
        protected override void SubscribeEvents()
        {
            Log.Debug($"{nameof(SubscribeEvents)}: Loading 008 custom role events..", Scp008X.Instance.Config.DebugMode);
            Exiled.Events.Handlers.Player.Hurting += OnHurting;
            Exiled.Events.Handlers.Player.ChangingRole += OnSpawning;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Log.Debug($"{nameof(UnsubscribeEvents)}: Unloading 008 custom role events..", Scp008X.Instance.Config.DebugMode);
            Exiled.Events.Handlers.Player.Hurting -= OnHurting;
            Exiled.Events.Handlers.Player.ChangingRole -= OnSpawning;
            base.UnsubscribeEvents();
        }

        private void OnSpawning(ChangingRoleEventArgs ev)
        {
            if(ev.NewRole == RoleType.Scp0492)
            {
                if(ev.Player.GetEffect(EffectType.Scp207).IsEnabled) ev.Player.DisableEffect(EffectType.Scp207);
                ev.Player.AddAhp(Scp008X.Instance.Config.StartingAhp, Scp008X.Instance.Config.MaxAhp, 0);
            }
            else if (ev.NewRole.GetTeam() != Team.SCP)
            {
                ev.Player.ArtificialHealth = 0;
            }
        }

        private void OnHurting(HurtingEventArgs ev)
        {
            if(ev.Attacker == null)return;
            if (ev.Attacker.Role == RoleType.Scp0492)
            {
                var buff = Scp008X.Instance.Config.Scp008Buff;
                var max = Scp008X.Instance.Config.MaxAhp;
                ev.Attacker.AddAhp(buff > 0 && ev.Attacker.ArtificialHealth + buff < max ? buff : (ushort)0,Scp008X.Instance.Config.MaxAhp,0);

                if (Scp008X.Instance.Rng.Next(100) > Scp008X.Instance.Config.InfectionChance)
                {
                    ev.Target.ShowHint($"<color=yellow><b>SCP-008</b></color>\n{Scp008X.Instance.Config.InfectionAlert}");
                    ev.Target.EnableEffect(EffectType.Poisoned);
                }    
            }
        }
    }
}