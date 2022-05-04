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
    using Exiled.API.Extensions;
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
            if(ev.Attacker.Role==RoleType.Scp0492) ev.Amount=Scp008X.Instance.Config.ZombieDamage;
            if (ev.Target.ArtificialHealth >= 0)
            {
                ev.IsAllowed = false;
                if (ev.Target.ArtificialHealth <= ev.Amount)
                {
                    var leftover = ev.Amount - ev.Target.ArtificialHealth;
                    ev.Target.ArtificialHealth = 0;
                    ev.Target.Hurt(leftover,$"Hit by {ev.Attacker.DisplayNickname}");
                    return;
                }
                ev.Target.ArtificialHealth -= (ushort)ev.Amount;
            }
        }
        
        public void OnHealed(UsedItemEventArgs ev)
        {
            if (!ev.Player.GetEffect(EffectType.Poisoned).IsEnabled) return;

            var chance = Scp008X.Instance.Rng.Next(1, 100);
            switch (ev.Item.Type)
            {
                case ItemType.Medkit:
                    if (chance > Scp008X.Instance.Config.CureChance)
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
            CustomRole.Get(typeof(Scp008)).AddRole(ev.Target);
            ev.Scp049.ShowHint($"Revived <b><color=green>{ev.Target.Nickname}</color></b>");
        }
        
        public void OnDying(DyingEventArgs ev)
        {
            if (ev.Target.IsHuman && ev.Target.GetEffect(EffectType.Poisoned).IsEnabled)
            {
                ev.IsAllowed = false;
                ev.Target.DisableEffect(EffectType.Poisoned);
                ev.Target.ClearInventory();
                CustomRole.Get(typeof(Scp008)).AddRole(ev.Target);
                ev.Killer.ShowHint($"Infected <b><color=red>{ev.Target.Nickname}</color></b>");
                return;
            }
            ev.Killer.ShowHint($"Killed <b><color=red>{ev.Target.Nickname}</color></b>");
        }
        
        public void OnShoot(ShootingEventArgs ev) => ev.IsAllowed = ev.Shooter.Role.Team != Team.SCP;
    }
}