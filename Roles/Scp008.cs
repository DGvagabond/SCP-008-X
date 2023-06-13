// -----------------------------------------------------------------------
// <copyright file="Scp008.cs">
// Copyright (c) DGvagabond. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SCP008X
{
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.API.Features.Attributes;
    using Exiled.CustomRoles.API.Features;
    using Exiled.Events.EventArgs.Player;
    using MEC;
    using PlayerRoles;

    [CustomRole(RoleTypeId.Scp0492)]
    public class Scp008 : CustomRole
    {
        public override uint Id { get; set; } = 008;
        public override RoleTypeId Role { get; set; } = RoleTypeId.Scp0492;
        public override int MaxHealth { get; set; } = Scp008X.Instance.Config.MaxZombieHealth;
        public override string Name { get; set; } = "SCP-008";
        public override string Description { get; set; } =
            "An instance of SCP-008 that spreads its infection with each hit.";
        public override string CustomInfo { get; set; } = "SCP-008";
        protected override void SubscribeEvents()
        {
            Log.Debug($"{nameof(SubscribeEvents)}: Loading 008 custom role events..");
            Exiled.Events.Handlers.Player.Hurting += OnHurting;
            Exiled.Events.Handlers.Player.Spawning += OnSpawning;
            Exiled.Events.Handlers.Player.ChangingRole += OnRoleChange;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Log.Debug($"{nameof(UnsubscribeEvents)}: Unloading 008 custom role events..");
            Exiled.Events.Handlers.Player.Hurting -= OnHurting;
            Exiled.Events.Handlers.Player.Spawning -= OnSpawning;
            Exiled.Events.Handlers.Player.ChangingRole += OnRoleChange;
            base.UnsubscribeEvents();
        }

        private void OnSpawning(SpawningEventArgs ev)
        {
            if (ev.Player.Role == RoleTypeId.Scp0492) {
                Timing.CallDelayed(.5f, delegate
                {
                    if (ev.Player.GetEffect(EffectType.Scp207).IsEnabled) ev.Player.DisableEffect(EffectType.Scp207);
                    ev.Player.AddAhp(Scp008X.Instance.Config.StartingAhp, Scp008X.Instance.Config.MaxAhp, 0);
                    ev.Player.Health = Scp008X.Instance.Config.ZombieHealth;
                    ev.Player.MaxHealth = MaxHealth;
                });
            }
        }

        private void OnRoleChange(ChangingRoleEventArgs ev)
        {
            if (ev.NewRole == RoleTypeId.Scp0492) {
                Timing.CallDelayed(.5f, delegate
                {
                    if (ev.Player.GetEffect(EffectType.Scp207).IsEnabled) ev.Player.DisableEffect(EffectType.Scp207);
                    ev.Player.AddAhp(Scp008X.Instance.Config.StartingAhp, Scp008X.Instance.Config.MaxAhp, 0);
                    ev.Player.Health = Scp008X.Instance.Config.ZombieHealth;
                    ev.Player.MaxHealth = MaxHealth;
                });
            }
        }

        private void OnHurting(HurtingEventArgs ev)
        {
            if (ev.Player == null) return;
            if (ev.Player.Role != RoleTypeId.Scp0492) return;
            var buff = Scp008X.Instance.Config.Scp008Buff;
            var max = Scp008X.Instance.Config.MaxAhp;
            ev.Player.AddAhp(buff > 0 && ev.Player.ArtificialHealth + buff < max ? buff : (ushort)0,Scp008X.Instance.Config.MaxAhp,0);

            if (UnityEngine.Random.Range(0, 100) > Scp008X.Instance.Config.InfectionChance) return;
            ev.Player.EnableEffect(EffectType.Poisoned);
            ev.Player.ShowHint($"<color=yellow><b>SCP-008</b></color>\n{Scp008X.Instance.Config.InfectionAlert}", 5);
        }
    }
}