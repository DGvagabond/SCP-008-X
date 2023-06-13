// -----------------------------------------------------------------------
// <copyright file="EventHandlers.cs">
// Copyright (c) DGvagabond. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp049;
using PlayerRoles;

namespace SCP008X
{
    using Exiled.CustomRoles.API.Features;
    using Exiled.API.Features;
    using Exiled.API.Enums;
    using MEC;

    public class EventHandlers
    {
        public void OnRoundStart()
        {
            if(Scp008X.Instance.Config.CassieAnnounce && Scp008X.Instance.Config.Announcement != null) Cassie.Message(Scp008X.Instance.Config.Announcement);
        }
        
        public void OnVerified(VerifiedEventArgs ev) => ev.Player.SendConsoleMessage("This server uses SCP-008-X, all zombies have been reworked.", "yellow");

        public void OnHurt(HurtingEventArgs ev)
        {
            if (ev.Player == null) return;
            
            if(ev.Attacker.Role == RoleTypeId.Scp049 && Scp008X.Instance.Config.BuffDoctor) ev.Player.Kill(ev.DamageHandler.Type);

            if (ev.Player.Role == RoleTypeId.Scp0492) ev.Amount = Scp008X.Instance.Config.ZombieDamage;
            
            if (ev.Player.ArtificialHealth >= 0)
            {
                ev.IsAllowed = false;
                if (ev.Player.ArtificialHealth <= ev.Amount) {
                    var leftover = ev.Amount - ev.Player.ArtificialHealth;
                    ev.Player.ArtificialHealth = 0;
                    ev.Player.Health -= leftover;
                }
                
                if(ev.Player.Health <= 0) ev.Player.Kill(ev.DamageHandler.Type);
            }
        }
        
        public void OnHealed(UsedItemEventArgs ev)
        {
            if (!ev.Player.GetEffect(EffectType.Poisoned).IsEnabled) return;

            var chance = UnityEngine.Random.Range(1, 100);
            switch (ev.Item.Type)
            {
                case ItemType.Medkit:
                    if (chance <= Scp008X.Instance.Config.CureChance)
                    {
                        ev.Player.DisableEffect(EffectType.Poisoned);
                        Log.Debug($"{ev.Player.Nickname} cured themselves with {chance}% probability.");
                        return;
                    }

                    Log.Debug($"{ev.Player.Nickname} failed to cure themselves with {chance}% probability.");
                    break;
                case ItemType.SCP500:
                    ev.Player.DisableEffect(EffectType.Poisoned);
                    Log.Debug($"{ev.Player.Nickname} cured themselves with SCP-500.");
                    break;
            }
        }
        
        public void OnReviving(StartingRecallEventArgs ev)
        {
            if (!Scp008X.Instance.Config.BuffDoctor) return;
            
            ev.IsAllowed = false;
            CustomRole.Get(typeof(Scp008))?.AddRole(ev.Target);
            ev.Player.ShowHint($"Revived <b><color=green>{ev.Target.Nickname}</color></b>");
        }
        
        public void OnDying(DyingEventArgs ev)
        {
            if (ev.Player == null && ev.DamageHandler?.Type is DamageType.Poison)
            {
                ev.IsAllowed = false;
                CustomRole.Get(typeof(Scp008))?.AddRole(ev.Player);
                return;
            }

            if(ev.Player?.Role == RoleTypeId.Scp0492){
                ev.IsAllowed = false;
                CustomRole.Get(typeof(Scp008))?.AddRole(ev.Player);
                Timing.CallDelayed(1f, delegate
                {
                    ev.Player.ShowHint($"Infected <b><color=red>{ev.Player.Nickname}</color></b>", 5);
                });
            }
        }

        public void OnShoot(ShootingEventArgs ev)
        {
            var targetPlayer = Player.Get(ev.TargetNetId);
            if(targetPlayer != null){
                if(ev.Player.Role.Side is Side.Scp && targetPlayer.Role.Side is Side.Scp){
                    ev.IsAllowed = false;
                }
            }
        }
    }
}