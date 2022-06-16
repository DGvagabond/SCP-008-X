// -----------------------------------------------------------------------
// <copyright file="EventHandlers.cs">
// Copyright (c) DGvagabond. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SCP008X
{
    using Exiled.CustomRoles.API.Features;
    using Exiled.Events.EventArgs;
    using Exiled.API.Features;
    using Exiled.API.Enums;
    using SCP008X.DamageUtilities;
    using MEC;

    public class EventHandlers
    {
        public void OnRoundStart()
        {
            if (Scp008X.Instance.Config.CassieAnnounce && Scp008X.Instance.Config.Announcement != null)
            {
                Cassie.Message(Scp008X.Instance.Config.Announcement);
            }
        }
        
        public void OnVerified(VerifiedEventArgs ev)
        {
            ev.Player.SendConsoleMessage("This server uses SCP-008-X, all zombies have been reworked!", "yellow");
        }
        
        public void OnHurt(HurtingEventArgs ev)
        {
            if (ev.Attacker == null) {
                return;
            }

            if (ev.Attacker.Role == RoleType.Scp0492)
            {
                ev.Amount = Scp008X.Instance.Config.ZombieDamage;
                return;
            }
        }
        
        public void OnHealed(UsedItemEventArgs ev)
        {
            if (!ev.Player.GetEffect(EffectType.Poisoned).IsEnabled) 
            {
                return;
            }

            var chance = UnityEngine.Random.Range(1, 100);
            switch (ev.Item.Type)
            {
                case ItemType.Medkit:
                    if (chance <= Scp008X.Instance.Config.CureChance)
                    {
                        ev.Player.DisableEffect(EffectType.Poisoned);
                        Log.Debug($"{ev.Player.Nickname} cured themselves with {chance}% probability.", Scp008X.Instance.Config.DebugMode);
                        return;
                    }

                    Log.Debug($"{ev.Player.Nickname} failed to cure themselves with {chance}% probability.", Scp008X.Instance.Config.DebugMode);
                    break;
                case ItemType.SCP500:
                    ev.Player.DisableEffect(EffectType.Poisoned);
                    Log.Debug($"{ev.Player.Nickname} cured themselves with SCP-500.", Scp008X.Instance.Config.DebugMode);
                    break;
            }
        }
        
        public void OnReviving(StartingRecallEventArgs ev)
        {
            if (!Scp008X.Instance.Config.BuffDoctor) return;
            
            ev.IsAllowed = false;
            CustomRole.Get(typeof(Scp008))?.AddRole(ev.Target);
            ev.Scp049.ShowHint($"Revived <b><color=green>{ev.Target.Nickname}</color></b>");
        }
        
        public void OnDying(DyingEventArgs ev)
        {
            if (ev.Killer == null && ev?.Handler?.Type is DamageType.Poison)
            {
                ev.IsAllowed = false;
                CustomRole.Get(typeof(Scp008))?.AddRole(ev.Target);
                return;
            }

            if(ev?.Killer?.Role == RoleType.Scp0492){
                ev.IsAllowed = false;
                CustomRole.Get(typeof(Scp008))?.AddRole(ev.Target);
                Timing.CallDelayed(1f, delegate
                {
                    ev.Killer.ShowHint($"Infected <b><color=red>{ev.Target.Nickname}</color></b>", 5);
                });
            }
        }

        public void OnShoot(ShootingEventArgs ev)
        {
            Player targetPlayer = Player.Get(ev.TargetNetId);
            if(targetPlayer != null){
                if(ev.Shooter.Role.Side is Side.Scp && targetPlayer.Role.Side is Side.Scp){
                    ev.IsAllowed = false;
                    return;
                }
            }
        }
    }
}