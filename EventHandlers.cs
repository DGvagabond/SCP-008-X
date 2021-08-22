using Exiled.Events.EventArgs;
using System.Linq;
using Exiled.API.Features;
using System;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Respawning;
using Respawning.NamingRules;
using UnityEngine;
using Scp207 = CustomPlayerEffects.Scp207;

namespace SCP008X
{
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
            ev.Player.SendConsoleMessage($"This server uses SCP-008-X, all zombies have been buffed!", "yellow");
        }
        
        public void OnDestroying(DestroyingEventArgs ev)
        {
            if (ev.Player.Role != RoleType.Scp0492 || !ev.Player.ReferenceHub.GetComponent<Scp008>()) return;
            ClearScp008(ev.Player);
            try
            {
                ev.Player.SessionVariables.Remove("Scp008");
            }
            catch (Exception)
            {
                Log.Debug($"{ev.Player} was never added to victim list.", Scp008X.Instance.Config.DebugMode);
            }
        }
        
        public void OnHurt(HurtingEventArgs ev)
        {
            if (ev.Target == null || ev.Target == ev.Attacker || ev.Attacker.Role != RoleType.Scp0492) 
                return;
            
            if (ev.Target.UserId == "PET")
            {
                Log.Debug($"{ev.Target} is a pet object, skipping.", Scp008X.Instance.Config.DebugMode);
                ev.IsAllowed = false;
                return;
            }

            try
            {
                if (ev.Target.SessionVariables.ContainsKey("Scp035Tag")) //IsScp035())
                {
                    Log.Debug($"{ev.Target} is SCP-035, skipping.", Scp008X.Instance.Config.DebugMode);
                    ev.IsAllowed = false;
                    return;
                }
            }
            catch
            {
                Log.Debug($"SCP-035 by Exiled-Team is not installed. Skipping.", Scp008X.Instance.Config.DebugMode);
            }
            
            try
            {
                if (ev.Target.SessionVariables.ContainsKey("Scp343Tag")) //IsScp343())
                {
                    Log.Debug($"{ev.Target} is SCP-343, skipping.", Scp008X.Instance.Config.DebugMode);
                    ev.IsAllowed = false;
                }
            }
            catch
            {
                Log.Debug($"SCP-343 by [UNKNOWN AUTHOR] is not installed. Skipping.", Scp008X.Instance.Config.DebugMode);
            }

            try
            {
                if (ev.Target.SessionVariables.ContainsKey("Scp999Tag")) //IsScp999())
                {
                    Log.Debug($"{ev.Target} is SCP-999, skipping.", Scp008X.Instance.Config.DebugMode);
                    ev.IsAllowed = false;
                }
            }
            catch
            {
                Log.Debug($"SCP-999 by DGvagabond is not installed. Skipping.", Scp008X.Instance.Config.DebugMode);
            }
            
            try
            {
                if (ev.Target.SessionVariables.ContainsKey("SerpentsHand")) //IsSerpentsHand())
                {
                    Log.Debug($"{ev.Target} is Serpent's Hand. Skipping.", Scp008X.Instance.Config.DebugMode);
                    ev.IsAllowed = false;
                    return;
                }
            }
            catch
            {
                Log.Debug($"Serpent's Hand by Exiled-Team is not installed. Skipping.");
            }
            
            ev.IsAllowed = ev.Target.Role != RoleType.Tutorial;
            
            if (Scp008X.Instance.Config.ZombieDamage >= 0)
            {
                ev.Amount = Scp008X.Instance.Config.ZombieDamage;
                Log.Debug($"Damage overriden to be {ev.Amount}.", Scp008X.Instance.Config.DebugMode);
            }
            
            if (Scp008X.Instance.Config.Scp008Buff >= 0 && ev.Target.Team != Team.SCP)
            {
                ev.Attacker.ArtificialHealth += Scp008X.Instance.Config.Scp008Buff;
                Log.Debug($"Added {Scp008X.Instance.Config.Scp008Buff} AHP to {ev.Attacker}.", Scp008X.Instance.Config.DebugMode);
            }
            
            var chance = Scp008X.Instance.Rng.Next(1, 100);
            
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
        
        public void OnHealed(UsedItemEventArgs ev)
        {
            var chance = Scp008X.Instance.Rng.Next(1, 100);
            
            if (!ev.Player.ReferenceHub.TryGetComponent(out Scp008 scp008)) 
                return;
            
            switch (ev.Item.Type)
            {
                 case Exiled.API.Enums.ItemType.Scp500:
                    ClearScp008(ev.Player);
                    
                    Log.Debug($"{ev.Player} successfully cured themselves.", Scp008X.Instance.Config.DebugMode);
                    break;
                case Exiled.API.Enums.ItemType.Medkit:
                    if (chance <= Scp008X.Instance.Config.CureChance)
                    {
                        ClearScp008(ev.Player);
                        
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

            ClearScp008(ev.Player);
            ev.Player.ArtificialHealth = 0; 
            Log.Debug($"Called ClearSCP008() method for {ev.Player}.", Scp008X.Instance.Config.DebugMode);
        }
        
        public void OnReviving(StartingRecallEventArgs ev)
        {
            if (!Scp008X.Instance.Config.BuffDoctor) 
                return;
            
            ev.IsAllowed = false;
            ev.Target.SetRole(RoleType.Scp0492, SpawnReason.Revived, true);
        }
        
        public void OnRevived(FinishingRecallEventArgs ev)
        {
            if(ev.Target == null) 
                return;
            
            if (Scp008X.Instance.Config.SuicideBroadcast != null)
            {
                ev.Target.ClearBroadcasts();
                ev.Target.Broadcast(10, Scp008X.Instance.Config.SuicideBroadcast, Broadcast.BroadcastFlags.Normal, true);
            }

            if (Scp008X.Instance.Config.Scp008Buff >= 0)
            {
                ev.Target.ArtificialHealth += Scp008X.Instance.Config.Scp008Buff; 
                Log.Debug($"Added {Scp008X.Instance.Config.Scp008Buff} AHP to {ev.Target}.", Scp008X.Instance.Config.DebugMode);
            }
            
            ev.Target.Health = Scp008X.Instance.Config.ZombieHealth;
            
            Log.Debug($"Set {ev.Target}'s HP to {Scp008X.Instance.Config.Scp008Buff}.", Scp008X.Instance.Config.DebugMode);
            
            ev.Target.ShowHint($"<color=yellow><b>SCP-008</b></color>\n{Scp008X.Instance.Config.SpawnHint}", 20f);
            ev.Target.SessionVariables.Add("Scp008", 1);
        }
        
        public void OnDying(DyingEventArgs ev)
        {
            if (ev.Target.Role != RoleType.Scp0492)
            {
                ClearScp008(ev.Target); 
                Log.Debug($"Called ClearSCP008() method for {ev.Target}.", Scp008X.Instance.Config.DebugMode);
            }
            
            if (ev.Target.ReferenceHub.TryGetComponent(out Scp008 _)) 
                ev.Target.SetRole(RoleType.Scp0492, SpawnReason.Revived, true);
        }
        
        public void OnDied(DiedEventArgs ev)
        {
            if (ev.Target == null) 
                return;
            
            if (Scp008X.Instance.Config.AoeInfection && ev.Target.Role == RoleType.Scp0492)
            {
                Log.Debug($"AOE infection enabled, running check...", Scp008X.Instance.Config.DebugMode);
                
                var infected = Player.List.Where(x => x.CurrentRoom == ev.Target.CurrentRoom && x.UserId != ev.Target.UserId).ToList();
                
                Log.Debug($"Made a list of {infected.Count} players.", Scp008X.Instance.Config.DebugMode);
                
                if (infected.Count == 0) 
                    return;
                
                foreach (var ply in from ply in infected let chance = Scp008X.Instance.Rng.Next(1, 100) where chance <= Scp008X.Instance.Config.AoeChance && ply.Team != Team.SCP select ply)
                {
                    Infect(ply);
                    Log.Debug($"Called Infect() method for {ev.Target} due to AOE.", Scp008X.Instance.Config.DebugMode);
                }
            }

            if (!Scp008X.Instance.Config.AoeInfection)
            {
                if (ev.Target.Role == RoleType.Scp049 || ev.Target.Role == RoleType.Scp0492)
                {
                    ClearScp008(ev.Target);
                    
                    if (!Scp008X.Instance.Config.ContainAnnounce) 
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

            var teammates = Player.Get(Side.Scp).Where(p => p.Role != RoleType.Scp079).ToList();
            
            if (teammates.Count > 0)
            {
                var spawn = teammates[Scp008X.Instance.Rng.Next(teammates.Count)].CurrentRoom;
                ev.Player.Position = new Vector3(spawn.Position.x, spawn.Position.y + 2, spawn.Position.z);
            }
            else
            {
                var facility = Map.Rooms.Where(r => r.Zone != ZoneType.LightContainment && r.Zone != ZoneType.Unspecified).ToList();
                
                var room = facility[Scp008X.Instance.Rng.Next(facility.Count)];
                ev.Player.Position = new Vector3(room.Position.x, room.Position.y + 2, room.Position.z);
            }
        }
        
        public void OnShoot(ShootingEventArgs ev) => ev.IsAllowed = ev.Shooter.Team != Team.SCP;

        public static void ClearScp008(Player player)
        {
            if (player.GameObject.TryGetComponent(out Scp008 scp008))
                UnityEngine.Object.Destroy(scp008);

            if (player.SessionVariables.ContainsKey("Scp008"))
                player.SessionVariables.Remove("Scp008");
        }
        
        public static void Infect(Player target)
        {
            if (target.Role == RoleType.Tutorial) 
                return;
            
            try
            {
                if (target.SessionVariables.ContainsKey("Scp035")) //IsScp035())
                    return;
            }
            catch
            {
                Log.Debug($"SCP-035, by Exiled-Team, is not installed. Skipping.", Scp008X.Instance.Config.DebugMode);
            }

            try
            {
                if (target.SessionVariables.ContainsKey("Scp343")) //IsScp343())
                    return;
            }
            catch
            {
                Log.Debug($"SCP-343 by [UNKNOWN AUTHOR] is not installed. Skipping.", Scp008X.Instance.Config.DebugMode);
            }
            
            try
            {
                if (target.SessionVariables.ContainsKey("Scp999")) //IsScp999())
                    return;
            }
            catch
            {
                Log.Debug($"SCP-999-X, by DGvagabond, is not installed. Skipping.", Scp008X.Instance.Config.DebugMode);
            }

            if (target.ReferenceHub.gameObject.TryGetComponent(out Scp008 _)) 
                return;
            
            target.ReferenceHub.gameObject.AddComponent<Scp008>();
            target.SessionVariables.Add("Scp008", 1);
            
            target.ShowHint($"<color=yellow><b>SCP-008</b></color>\n{Scp008X.Instance.Config.InfectionAlert}", 10f);
        }
        
        private void Turn(Player target)
        {
            if (!target.ReferenceHub.TryGetComponent(out Scp008 _)) 
                target.GameObject.AddComponent<Scp008>();
            
            if (target.ReferenceHub.playerEffectsController.GetEffect<Scp207>().enabled) 
                target.DisableEffect<Scp207>();
            
            if (target.CurrentItem.Type.IsWeapon())
                target.Inventory.CmdDropItem(target.CurrentItem.Serial, true);
            
            if (Scp008X.Instance.Config.SuicideBroadcast != null)
            {
                target.ClearBroadcasts();
                target.Broadcast(10,Scp008X.Instance.Config.SuicideBroadcast,Broadcast.BroadcastFlags.Normal,true);
            }
            
            if (!Scp008X.Instance.Config.RetainInventory) 
                target.ClearInventory();
            
            if (Scp008X.Instance.Config.Scp008Buff >= 0) 
                target.ArtificialHealth += Scp008X.Instance.Config.Scp008Buff;

            target.ArtificialHealthDecay = 0f;
            target.Health = Scp008X.Instance.Config.ZombieHealth;
            target.ShowHint($"<color=yellow><b>SCP-008</b></color>\n{Scp008X.Instance.Config.SpawnHint}", 20f);
            
            if (!Scp008X.Instance.Config.AoeTurned) 
                return;
            
            var infected = Player.List.Where(x => x.CurrentRoom == target.CurrentRoom && x.UserId != target.UserId).ToList();
            if (infected.Count == 0) 
                return;

            foreach (var player in infected.Where(player => Scp008X.Instance.Rng.Next(100) <= Scp008X.Instance.Config.AoeChance && player.Team != Team.SCP))
                Infect(player);
        }

        public static void Contained(Player pl)
        {
            Log.Debug($"SCP008Check() passed. Announcing containment...", Scp008X.Instance.Config.DebugMode);
            
            string cause;
            
            switch (pl.Team)
            {
                case Team.MTF:
                    var team = !UnitNamingRules.TryGetNamingRule(SpawnableTeamType.NineTailedFox, out var unit) 
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
            
            Cassie.Message($"SCP 0 0 8 successfully terminated . {cause}");
        }
    }
}