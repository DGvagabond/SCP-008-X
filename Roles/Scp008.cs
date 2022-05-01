// -----------------------------------------------------------------------
// <copyright file="Scp008.cs">
// Copyright (c) DGvagabond. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SCP008X
{
    using CustomPlayerEffects;
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.CustomRoles.API.Features;
    using Exiled.Events.EventArgs;
    using PlayerStatsSystem;
    
    public class Scp008 : CustomRole
    {
        public override uint Id { get; set; } = 008;
        public override RoleType Role { get; set; } = RoleType.Scp0492;
        public override int MaxHealth { get; set; } = Scp008X.Instance.Config.ZombieHealth;
        public override string Name { get; set; } = "SCP-008";
        public override string Description { get; set; } =
            "An instance of SCP-008 that spreads the 008 infection with each hit.";
        public override string CustomInfo { get; set; } = "SCP-008";
        protected override void SubscribeEvents()
        {
            Log.Debug($"{nameof(SubscribeEvents)}: Loading 008 events..", Scp008X.Instance.Config.DebugMode);
            Exiled.Events.Handlers.Player.Hurting += OnHurting;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Log.Debug($"{nameof(UnsubscribeEvents)}: Unloading 008 events..", Scp008X.Instance.Config.DebugMode);
            Exiled.Events.Handlers.Player.Hurting -= OnHurting;
            base.UnsubscribeEvents();
        }

        private void OnHurting(HurtingEventArgs ev)
        {
            ev.Amount = Scp008X.Instance.Config.ZombieDamage;
                
            var buff = Scp008X.Instance.Config.Scp008Buff;
            var max = Scp008X.Instance.Config.MaxAhp;
            ev.Attacker.AddAhp(buff > 0 && ev.Attacker.ArtificialHealth + buff < max ? buff : (ushort)0,Scp008X.Instance.Config.MaxAhp,0);

            if (ev.Target.IsHuman && ev.Target.Health - ev.Amount <= 0 &&
                ev.Target.TryGetEffect(EffectType.Poisoned, out PlayerEffect poisoned) && poisoned.Intensity > 0)
            {
                ev.IsAllowed = false;
                ev.Amount = 0;
                ev.Target.DropItems();
                ev.Target.SetRole(RoleType.Scp0492, SpawnReason.ForceClass, true);
            }

            if (!Check(ev.Attacker))
                return;

            if (ev.Target.Role.Team == Team.SCP)
            {
                ev.Amount = 0f;
                return;
            }

            if (Scp008X.Instance.Rng.Next(100) < Scp008X.Instance.Config.InfectionChance)
            {
                ev.Target.ShowHint($"<color=yellow><b>SCP-008</b></color>\n{Scp008X.Instance.Config.InfectionAlert}");
                ev.Target.EnableEffect(EffectType.Poisoned);
            }
        }
    }
}