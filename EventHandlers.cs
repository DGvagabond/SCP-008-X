using CustomPlayerEffects;
using User = Exiled.API.Features.Player;
using Exiled.Events.EventArgs;
using System.Linq;
using Exiled.API.Features;
using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Respawning;
using Respawning.NamingRules;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace SCP008X
{
    public class EventHandlers
    {
        private readonly Random _gen = new Random();
        
        public static List<Player> Victims = new List<Player>();
        
        public static bool Scp008Check() => Victims.Count <= 0;

        public void OnRoundStart()
        {
            Victims.Clear();
            if (Scp008X.Instance.Config.CassieAnnounce && Scp008X.Instance.Config.Announcement != null)
            {
                Cassie.GlitchyMessage(Scp008X.Instance.Config.Announcement,15f,15f);
            }
        }
        
        public void OnVerified(VerifiedEventArgs ev)
        {
            ev.Player.SendConsoleMessage($"This server uses SCP-008-X, all zombies have been buffed!", "yellow");
        }
        
        public void OnDestroying(DestroyingEventArgs ev)
        {
            if (ev.Player.Role != RoleType.Scp0492 || !ev.Player.ReferenceHub.GetComponent<Scp008>()) return;
            ClearScp008(ev.Player);
            try
            {
                Victims.Remove(ev.Player);
            }
            catch (Exception)
            {
                Log.Debug($"{ev.Player} was never added to victim list.", Scp008X.Instance.Config.DebugMode);
            }
        }
        
        public void OnHurt(HurtingEventArgs ev)
        {
            if (ev.Target == null || ev.Target == ev.Attacker || ev.Attacker.Role != RoleType.Scp049) 
                return;
            
            if (ev.Target.UserId == "PET")
            {
                Log.Debug($"{ev.Target} is a pet object, skipping method call.", Scp008X.Instance.Config.DebugMode);
                ev.IsAllowed = false;
                return;
            }

            try
            {
                if (ev.Target.IsScp035())
                {
                    Log.Debug($"{ev.Target} is SCP-035, skipping method call.", Scp008X.Instance.Config.DebugMode);
                    ev.IsAllowed = false;
                    return;
                }
            }
            catch (Exception)
            {
                Log.Debug($"SCP-035 by Exiled-Team is not installed. Skipping.", Scp008X.Instance.Config.DebugMode);
            }
            
            try
            {
                if (ev.Target.IsScp343())
                {
                    Log.Debug($"{ev.Target} is SCP-343, skipping.", Scp008X.Instance.Config.DebugMode);
                    ev.IsAllowed = false;
                }
            }
            catch (Exception)
            {
                Log.Debug($"SCP-343 by unknown is not installed.", Scp008X.Instance.Config.DebugMode);
            }

            try
            {
                if (ev.Target.IsScp999())
                {
                    Log.Debug($"{ev.Target} is SCP-999, skipping.", Scp008X.Instance.Config.DebugMode);
                    ev.IsAllowed = false;
                }
            }
            catch (Exception)
            {
                Log.Debug($"SCP-999 by british boi is not installed.", Scp008X.Instance.Config.DebugMode);
            }
            
            try
            {
                if (ev.Target.IsSerpentsHand())
                {
                    Log.Debug($"{ev.Target} is Serpent's Hand. Skipping.", Scp008X.Instance.Config.DebugMode);
                    ev.IsAllowed = false;
                    return;
                }
            }
            catch (Exception)
            {
                Log.Debug($"Serpent's Hand by Exiled-team is not installed. Skipping.");
            }
            
            ev.IsAllowed = ev.Target.Role != RoleType.Tutorial;
            
            if (Scp008X.Instance.Config.ZombieDamage >= 0)
            {
                ev.Amount = Scp008X.Instance.Config.ZombieDamage;
                Log.Debug($"Damage overriden to be {ev.Amount}.", Scp008X.Instance.Config.DebugMode);
            }
            
            if (Scp008X.Instance.Config.Scp008Buff >= 0)
            {
                ev.Attacker.ArtificialHealth += Scp008X.Instance.Config.Scp008Buff;
                Log.Debug($"Added {Scp008X.Instance.Config.Scp008Buff} AHP to {ev.Attacker}.", Scp008X.Instance.Config.DebugMode);
            }
            
            int chance = _gen.Next(1, 100);
            
            if (chance > Scp008X.Instance.Config.InfectionChance || ev.Target.Team == Team.SCP) 
                return;
            
            try
            {
                Infect(ev.Target);
                Log.Debug($"Successfully infected {ev.Target} with {chance}% probability.", Scp008X.Instance.Config.DebugMode);
            }
            catch (Exception e)
            {
                Log.Error($"Failed to infect {ev.Target}!\n{e}");
            }
        }
        
        public void OnHealed(UsedMedicalItemEventArgs ev)
        {
            int chance = _gen.Next(1, 100);
            
            if (!ev.Player.ReferenceHub.TryGetComponent(out Scp008 scp008)) 
                return;
            
            switch (ev.Item)
            {
                case ItemType.SCP500:
                    UnityEngine.Object.Destroy(scp008);
                    if (Victims.Contains(ev.Player))
                        Victims.Remove(ev.Player);
                    
                    Log.Debug($"{ev.Player} successfully cured themselves.", Scp008X.Instance.Config.DebugMode);
                    break;
                case ItemType.Medkit:
                    if (chance <= Scp008X.Instance.Config.CureChance)
                    {
                        UnityEngine.Object.Destroy(scp008);
                        if (Victims.Contains(ev.Player))
                            Victims.Remove(ev.Player);
                        
                        Log.Debug($"{ev.Player} cured themselves with {chance}% probability.", Scp008X.Instance.Config.DebugMode);
                    }
                    break;
            }
        }
        
        public void OnRoleChange(ChangingRoleEventArgs ev)
        {
            if (ev.NewRole == RoleType.Scp0492)
            {
                Log.Debug($"Calling Turn() method for {ev.Player}.", Scp008X.Instance.Config.DebugMode);
                Turn(ev.Player);
            }

            ClearScp008(ev.Player); ev.Player.ArtificialHealth = 0; 
            Log.Debug($"Called ClearSCP008() method for {ev.Player}.", Scp008X.Instance.Config.DebugMode);
        }
        
        public void OnReviving(StartingRecallEventArgs ev)
        {
            if (!Scp008X.Instance.Config.BuffDoctor) 
                return;
            
            ev.IsAllowed = false;
            ev.Target.SetRole(RoleType.Scp0492, true);
        }
        
        public void OnRevived(FinishingRecallEventArgs ev)
        {
            if(ev.Target == null) 
                return;
            
            if (Scp008X.Instance.Config.SuicideBroadcast != null)
            {
                ev.Target.ClearBroadcasts();
                ev.Target.Broadcast(10, Scp008X.Instance.Config.SuicideBroadcast);
            }

            if (Scp008X.Instance.Config.Scp008Buff >= 0)
            {
                ev.Target.ArtificialHealth += Scp008X.Instance.Config.Scp008Buff; 
                Log.Debug($"Added {Scp008X.Instance.Config.Scp008Buff} AHP to {ev.Target}.", Scp008X.Instance.Config.DebugMode);
            }
            
            ev.Target.Health = Scp008X.Instance.Config.ZombieHealth;
            
            Log.Debug($"Set {ev.Target}'s HP to {Scp008X.Instance.Config.Scp008Buff}.", Scp008X.Instance.Config.DebugMode);
            
            ev.Target.ShowHint($"<color=yellow><b>SCP-008</b></color>\n{Scp008X.Instance.Config.SpawnHint}", 20f);
            Victims.Add(ev.Target);
        }
        
        public void OnDying(DyingEventArgs ev)
        {
            if (ev.Target.Role == RoleType.Scp0492)
            {
                ClearScp008(ev.Target); 
                Log.Debug($"Called ClearSCP008() method for {ev.Target}.", Scp008X.Instance.Config.DebugMode);
            }
            
            if (ev.Target.ReferenceHub.TryGetComponent(out Scp008 _)) 
                ev.Target.SetRole(RoleType.Scp0492, true);
        }
        
        public void OnDied(DiedEventArgs ev)
        {
            if (ev.Target == null) 
                return;
            
            if (Scp008X.Instance.Config.AoeInfection && ev.Target.Role == RoleType.Scp0492)
            {
                Log.Debug($"AOE infection enabled, running check...", Scp008X.Instance.Config.DebugMode);
                
                List<Player> infected = User.List.Where(x => x.CurrentRoom == ev.Target.CurrentRoom && x.UserId != ev.Target.UserId).ToList();
                
                Log.Debug($"Made a list of {infected.Count} players.", Scp008X.Instance.Config.DebugMode);
                
                if (infected.Count == 0) 
                    return;
                
                foreach (Player ply in infected)
                {
                    int chance = _gen.Next(1, 100);
                    if (chance > Scp008X.Instance.Config.AoeChance || ply.Team == Team.SCP) 
                        continue;
                    
                    Infect(ply);
                    Log.Debug($"Called Infect() method for {ev.Target} due to AOE.", Scp008X.Instance.Config.DebugMode);
                }
            }

            if (!Scp008X.Instance.Config.AoeInfection)
            {
                if (ev.Target.Role == RoleType.Scp049 || ev.Target.Role == RoleType.Scp0492)
                {
                    if(Victims.Contains(ev.Target)) 
                        Victims.Remove(ev.Target);
                    
                    if (!Scp008Check() || !Scp008X.Instance.Config.ContainAnnounce) 
                        return;

                    Contained(ev.Killer);
                }
            }
        }
        
        public void OnFail(FailingEscapePocketDimensionEventArgs ev)
        {
            if (ev.Player.Role != RoleType.Scp0492)
                return;
            
            ev.IsAllowed = false;

            List<Player> teammates = Player.Get(Side.Scp).Where(p => p.Role != RoleType.Scp079).ToList();
            
            if (teammates.Count > 0)
            {
                Room spawn = teammates[_gen.Next(teammates.Count)].CurrentRoom;
                ev.Player.Position = new Vector3(spawn.Position.x, spawn.Position.y + 2, spawn.Position.z);
            }
            else
            {
                List<Room> facility = Map.Rooms.Where(r => r.Zone != ZoneType.LightContainment && r.Zone != ZoneType.Unspecified).ToList();
                
                Room room = facility[_gen.Next(facility.Count)];
                ev.Player.Position = new Vector3(room.Position.x, room.Position.y + 2, room.Position.z);
            }
        }
        
        public void OnShoot(ShootingEventArgs ev) => ev.IsAllowed = ev.Shooter.Team != Team.SCP;

        private static void ClearScp008(User player)
        {
            if (player.GameObject.TryGetComponent(out Scp008 scp008))
                Object.Destroy(scp008);
            if (player.GameObject.TryGetComponent(out RetainAhp rAhp))
                Object.Destroy(rAhp);

            if (Victims.Contains(player))
                Victims.Remove(player);
        }
        
        public static void Infect(User target)
        {
            if (target.Role == RoleType.Tutorial) 
                return;
            
            try
            {
                if (target.IsScp035())
                    return;
            }
            catch (Exception)
            {
                Log.Debug($"SCP-035, by Cyanox, is not installed. Skipping method call.", Scp008X.Instance.Config.DebugMode);
            }

            try
            {
                if (target.IsScp343())
                    return;
            }
            catch (Exception)
            {
                Log.Debug($"SCP-343 by unknown is not installed. Skipping.", Scp008X.Instance.Config.DebugMode);
            }
            
            try
            {
                if (target.IsScp999())
                    return;
            }
            catch(Exception)
            {
                Log.Debug($"SCP-999-X, by DGvagabond, is not installed. Skipping method call.", Scp008X.Instance.Config.DebugMode);
            }

            if (target.ReferenceHub.gameObject.TryGetComponent(out Scp008 _)) 
                return;
            
            target.ReferenceHub.gameObject.AddComponent<Scp008>();
            Victims.Add(target);
            
            target.ShowHint($"<color=yellow><b>SCP-008</b></color>\n{Scp008X.Instance.Config.InfectionAlert}", 10f);
        }
        
        private void Turn(User target)
        {
            if (!target.ReferenceHub.TryGetComponent(out Scp008 _)) 
                target.GameObject.AddComponent<Scp008>();
            
            if (target.ReferenceHub.playerEffectsController.GetEffect<Scp207>().Enabled) 
                target.DisableEffect<Scp207>();
            
            if (target.CurrentItem.id.Gun()) 
                target.Inventory.ServerDropAll();
            
            if (Scp008X.Instance.Config.SuicideBroadcast != null)
            {
                target.ClearBroadcasts();
                target.Broadcast(10, Scp008X.Instance.Config.SuicideBroadcast);
            }
            
            if (!Scp008X.Instance.Config.RetainInventory) 
                target.ClearInventory();
            
            if (Scp008X.Instance.Config.Scp008Buff >= 0) 
                target.ArtificialHealth += Scp008X.Instance.Config.Scp008Buff;
            
            target.GameObject.AddComponent<RetainAhp>();
            target.Health = Scp008X.Instance.Config.ZombieHealth;
            target.ShowHint($"<color=yellow><b>SCP-008</b></color>\n{Scp008X.Instance.Config.SpawnHint}", 20f);
            
            if (!Scp008X.Instance.Config.AoeTurned) 
                return;
            
            List<Player> infected = User.List.Where(x => x.CurrentRoom == target.CurrentRoom && x.UserId != target.UserId).ToList();
            if (infected.Count == 0) 
                return;

            foreach (Player player in infected)
                if (_gen.Next(100) <= Scp008X.Instance.Config.AoeChance && player.Team != Team.SCP)
                    Infect(player);
        }

        public static void Contained(Player pl)
        {
            Log.Debug($"SCP008Check() passed. Announcing containment...", Scp008X.Instance.Config.DebugMode);
            
            string cause;
            
            switch (pl.Team)
            {
                case Team.MTF:
                    string team = !UnitNamingRules.TryGetNamingRule(SpawnableTeamType.NineTailedFox, out var unit) 
                        ? "unknown" : unit.GetCassieUnitName(pl.ReferenceHub.characterClassManager.CurUnitName);
                    cause = $". containmentunit {team}";
                    break;
                case Team.CDP:
                    cause = "containmentunit classd";
                    break;
                case Team.CHI:
                    cause = "containmentunit chaosinsurgency";
                    break;
                case Team.RSC:
                    cause = "containmentunit scientist personnel";
                    break;
                default:
                    cause = "containmentunit unknown";
                    break;
            }
            
            Cassie.GlitchyMessage($"SCP 0 0 8 successfully terminated . {cause}",5,5);
        }
    }
}