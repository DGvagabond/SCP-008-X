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
    using Random = System.Random;
    
    public class EventHandlers
    {
        public Random Gen = new Random();
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
            if (ev.Attacker == null || ev.Target == null) return;
            if (ev.Attacker.Role == RoleType.Scp0492 && ev.Attacker != ev.Target)
            {
                ev.Amount = Scp008X.Instance.Config.ZombieDamage;
                
                var buff = Scp008X.Instance.Config.Scp008Buff;
                var max = Scp008X.Instance.Config.MaxAhp;
                ev.Attacker.ArtificialHealth += buff > 0 && ev.Attacker.ArtificialHealth + buff < max ? buff : (ushort)0;
                
                var chance = Gen.Next(1, 100);
                if (chance >= Scp008X.Instance.Config.InfectionChance ||
                    ev.Target.GetEffect(EffectType.Poisoned).IsEnabled) return;

                ev.Target.EnableEffect(EffectType.Poisoned);
                ev.Target.ShowHint($"<color=yellow><b>SCP-008</b></color>\n{Scp008X.Instance.Config.InfectionAlert}");

                Log.Debug($"{ev.Attacker.Nickname} infected {ev.Target.Nickname} with {chance}% probability.",
                    Scp008X.Instance.Config.DebugMode);
            }

            if (ev.Target.Role == RoleType.Scp0492 && ev.Target != ev.Attacker && ev.Target.ArtificialHealth >= 0)
            {
                ev.IsAllowed = false;
                if (ev.Target.ArtificialHealth <= ev.Amount)
                {
                    var leftover = ev.Amount - ev.Target.ArtificialHealth;
                    ev.Target.ArtificialHealth = 0;
                    ev.Target.Hurt(leftover,$"Hit by {ev.Attacker.DisplayNickname}");
                }
                else
                {
                    ev.Target.ArtificialHealth -= (ushort)ev.Amount;
                }
            }
        }
        
        public void OnHealed(UsedItemEventArgs ev)
        {
            if (!ev.Player.GetEffect(EffectType.Poisoned).IsEnabled) return;

            var chance = Gen.Next(1, 100);
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
        
        public void OnRoleChange(ChangingRoleEventArgs ev)
        {
            Timing.CallDelayed(1f, () =>
            {
                // TODO Create a workaround for ArtificialHealthDecay
                // ev.Player.ArtificialHealthDecay = ev.NewRole.GetTeam() != Team.SCP ? 1 : 0;
                if(ev.NewRole == RoleType.Scp0492)
                {
                    if(ev.Player.GetEffect(EffectType.Scp207).IsEnabled) ev.Player.DisableEffect(EffectType.Scp207);
                    ev.Player.Health = Scp008X.Instance.Config.ZombieHealth;
                    ev.Player.ArtificialHealth = Scp008X.Instance.Config.StartingAhp;
                }
                else if (ev.NewRole.GetTeam() != Team.SCP)
                {
                    ev.Player.ArtificialHealth = 0;
                }
            });
        }
        
        public void OnReviving(StartingRecallEventArgs ev)
        {
            if (!Scp008X.Instance.Config.BuffDoctor) return;
            
            ev.IsAllowed = false;
            CustomRole.Get(typeof(Scp008))?.AddRole(ev.Target);
        }
        
        public void OnDying(DyingEventArgs ev)
        {
            if (ev.Target.IsHuman && ev.Target.GetEffect(EffectType.Poisoned).IsEnabled)
            {
                ev.IsAllowed = false;
                ev.Target.DisableEffect(EffectType.Poisoned);
                ev.Target.ClearInventory();
                CustomRole.Get(typeof(Scp008))?.AddRole(ev.Target);
            }
        }
        
        public void OnShoot(ShootingEventArgs ev) => ev.IsAllowed = ev.Shooter.Role.Team != Team.SCP;
    }
}