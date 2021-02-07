using CustomPlayerEffects;
using User = Exiled.API.Features.Player;
using Exiled.Events.EventArgs;
using System.Linq;
using Exiled.API.Features;
using scp035.API;
using SCP999X.API;
using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Respawning;
using Respawning.NamingRules;
using UnityEngine;
using Random = System.Random;

namespace SCP008X
{
    public class EventHandlers
    {
        private Scp008X _plugin;
        public EventHandlers(Scp008X plugin) => _plugin = plugin;
        private readonly Random _gen = new Random();
        public static List<Player> Victims = new List<Player>();
        private static bool IsSH { get; set; }
        private static User TryGet035() => Scp035Data.GetScp035();
        private static User TryGet999() => SCP999API.GetScp999();

        public void OnRoundStart()
        {
            if (Scp008X.Instance.Config.CassieAnnounce && Scp008X.Instance.Config.Announcement != null)
            {
                Cassie.GlitchyMessage(Scp008X.Instance.Config.Announcement,15f,15f);
            }
        }
        public void OnRoundRestart()
        {
            
        }
        public void OnRoundEnd(RoundEndedEventArgs ev)
        {
            if (Scp008X.Instance.Config.SummaryStats)
            {
                Map.ShowHint($"\n\n\n\n\n\n\n\n\n\n\n\n\n\n<align=left><color=yellow><b>SCP-008 Victims:</b></color> {Victims.Count}/{RoundSummary.changed_into_zombies}", 30f);
            }
            Victims = null;
        }
        public void OnPlayerJoin(VerifiedEventArgs ev)
        {
            ev.Player.SendConsoleMessage($"This server uses SCP-008-X, all zombies have been buffed!", "yellow");
        }
        public void OnPlayerLeave(DestroyingEventArgs ev)
        {
            if (ev.Player.Role != RoleType.Scp0492 || !ev.Player.ReferenceHub.TryGetComponent(out Scp008 s008)) return;
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
        public void OnPlayerHurt(HurtingEventArgs ev)
        {
            if (ev.Target.UserId == "PET")
            {
                Log.Debug($"{ev.Target} is a pet object, skipping method call.", Scp008X.Instance.Config.DebugMode);
                ev.IsAllowed = false;
                return;
            }

            if (ev.Attacker.Role != RoleType.Scp0492) return;
            try
            {
                if (ev.Target.UserId == TryGet035().UserId)
                {
                    Log.Debug($"{ev.Target} is SCP-035, skipping method call.", Scp008X.Instance.Config.DebugMode);
                    ev.IsAllowed = false;
                    return;
                }
            }
            catch (Exception)
            {
                Log.Debug($"SCP-035, by Cyanox, is not installed. Skipping method call.", Scp008X.Instance.Config.DebugMode);
            }

            if (ev.Target == ev.Attacker) return;
            ev.IsAllowed = ev.Target.Role != RoleType.Tutorial;
            if (Scp008X.Instance.Config.ZombieDamage >= 0)
            {
                ev.Amount = Scp008X.Instance.Config.ZombieDamage;
                Log.Debug($"Damage overriden to be {ev.Amount}.", Scp008X.Instance.Config.DebugMode);
            }
            if (Scp008X.Instance.Config.Scp008Buff >= 0)
            {
                ev.Attacker.AdrenalineHealth += Scp008X.Instance.Config.Scp008Buff;
                Log.Debug($"Added {Scp008X.Instance.Config.Scp008Buff} AHP to {ev.Attacker}.", Scp008X.Instance.Config.DebugMode);
            }
            var chance = _gen.Next(1, 100);
            if (chance > Scp008X.Instance.Config.InfectionChance || ev.Target.Team == Team.SCP) return;
            try
            {
                Infect(ev.Target);
                Log.Debug($"Successfully infected {ev.Target} with {chance}% probability.", Scp008X.Instance.Config.DebugMode);
            }
            catch (Exception e)
            {
                Log.Error($"Failed to infect {ev.Target}! {e}");
                throw;
            }
        }
        public void OnHealed(UsedMedicalItemEventArgs ev)
        {
            var chance = _gen.Next(1, 100);
            if (!ev.Player.ReferenceHub.TryGetComponent(out Scp008 scp008)) return;
            switch (ev.Item)
            {
                case ItemType.SCP500:
                    UnityEngine.Object.Destroy(scp008);
                    try
                    {
                        Victims.Remove(ev.Player);
                    }
                    catch (Exception)
                    {
                        Log.Debug($"{ev.Player} was not in victim list.", Scp008X.Instance.Config.DebugMode);
                    }
                    Log.Debug($"{ev.Player} successfully cured themselves.", Scp008X.Instance.Config.DebugMode);
                    break;
                case ItemType.Medkit:
                    if (chance <= Scp008X.Instance.Config.CureChance)
                    {
                        UnityEngine.Object.Destroy(scp008);
                        try
                        {
                            Victims.Remove(ev.Player);
                        }
                        catch (Exception)
                        {
                            Log.Debug($"{ev.Player} was not in victim list.", Scp008X.Instance.Config.DebugMode);
                        }
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

            if (ev.NewRole == RoleType.Scp0492 && ev.NewRole == RoleType.Scp096) return;
            ClearScp008(ev.Player); ev.Player.AdrenalineHealth = 0; 
            Log.Debug($"Called ClearSCP008() method for {ev.Player}.", Scp008X.Instance.Config.DebugMode);
        }
        public void OnReviving(StartingRecallEventArgs ev)
        {
            if (!Scp008X.Instance.Config.BuffDoctor) return;
            ev.IsAllowed = false;
            ev.Target.SetRole(RoleType.Scp0492, true, false);
        }
        public void OnRevived(FinishingRecallEventArgs ev)
        {
            if (Scp008X.Instance.Config.SuicideBroadcast != null)
            {
                ev.Target.ClearBroadcasts();
                ev.Target.Broadcast(10, Scp008X.Instance.Config.SuicideBroadcast);
            }
            if (Scp008X.Instance.Config.Scp008Buff >= 0) { ev.Target.AdrenalineHealth += Scp008X.Instance.Config.Scp008Buff; Log.Debug($"Added {Scp008X.Instance.Config.Scp008Buff} AHP to {ev.Target}.", Scp008X.Instance.Config.DebugMode); }
            ev.Target.Health = Scp008X.Instance.Config.ZombieHealth;
            Log.Debug($"Set {ev.Target}'s HP to {Scp008X.Instance.Config.Scp008Buff}.", Scp008X.Instance.Config.DebugMode);
            ev.Target.ShowHint($"<color=yellow><b>SCP-008</b></color>\n{Scp008X.Instance.Config.SpawnHint}", 20f);
            Victims.Add(ev.Target);
        }
        public void OnPlayerDying(DyingEventArgs ev)
        {
            if (ev.Target.Role == RoleType.Scp0492) { ClearScp008(ev.Target); Log.Debug($"Called ClearSCP008() method for {ev.Target}.", Scp008X.Instance.Config.DebugMode); }
            if (ev.Target.ReferenceHub.TryGetComponent(out Scp008 scp008))
            {
                ev.Target.SetRole(RoleType.Scp0492, true, false);
            }
        }
        public void OnPlayerDied(DiedEventArgs ev)
        {
            if (Scp008X.Instance.Config.AoeInfection && ev.Target.Role == RoleType.Scp0492)
            {
                Log.Debug($"AOE infection enabled, running check...", Scp008X.Instance.Config.DebugMode);
                var infected = User.List.Where(x => x.CurrentRoom == ev.Target.CurrentRoom && x.UserId != ev.Target.UserId).ToList();
                Log.Debug($"Made a list of {infected.Count} players.", Scp008X.Instance.Config.DebugMode);
                if (infected.Count == 0) return;
                foreach (var ply in from ply in infected let chance = _gen.Next(1, 100) where chance <= Scp008X.Instance.Config.AoeChance && ply.Team != Team.SCP select ply)
                {
                    Infect(ply);
                    Log.Debug($"Called Infect() method for {ev.Target} due to AOE.", Scp008X.Instance.Config.DebugMode);
                }
            }

            if (!Scp008X.Instance.Config.AoeInfection)
            {
                if (ev.Target.Role == RoleType.Scp049 ||
                    ev.Target.Role == RoleType.Scp0492)
                {
                    Victims.Remove(ev.Target);
                    if (!Scp008Check()) return;
                    Contained(ev.Killer);
                }
            }
        }
        public void OnFail(FailingEscapePocketDimensionEventArgs ev)
        {
            if (ev.Player.Role != RoleType.Scp0492) return;
            ev.IsAllowed = false;
            var teammates = Player.List.Where(p => p.Team == Team.SCP && p.Role != RoleType.Scp079).ToList();
            if (teammates.Count > 0)
            {
                var spawn = teammates[_gen.Next(teammates.Count)].CurrentRoom;
                ev.Player.Position = new Vector3(spawn.Position.x, spawn.Position.y + 2, spawn.Position.z);
            }
            else
            {
                var facility = Map.Rooms.Where(r => r.Zone != ZoneType.LightContainment && r.Zone != ZoneType.Unspecified).ToList();
                var room = facility[_gen.Next(facility.Count)];
                ev.Player.Position = new Vector3(room.Position.x, room.Position.y + 2, room.Position.z);
            }
        }
        public void OnShoot(ShootingEventArgs ev)
        {
            switch (ev.Shooter.Team)
            {
                case Team.SCP:
                    ev.IsAllowed = false;
                    break;
            }
        }
        
        private static void ClearScp008(User player)
        {
            var s008 = player.ReferenceHub.TryGetComponent(out Scp008 scp008);
            var ahp = player.ReferenceHub.TryGetComponent(out RetainAhp rahp);
            if (s008)
            {
                UnityEngine.Object.Destroy(scp008);
            }
            if (ahp)
            {
                UnityEngine.Object.Destroy(rahp);
            }
            try
            {
                Victims.Remove(player);
            }
            catch (Exception)
            {
                Log.Debug($"{player} was not in victim list.", Scp008X.Instance.Config.DebugMode);
            }

        }
        public static void Infect(User target)
        {
            try
            {
                if (target.UserId == TryGet035().UserId) return;
            }
            catch (Exception)
            {
                Log.Debug($"SCP-035, by Cyanox, is not installed. Skipping method call.", Scp008X.Instance.Config.DebugMode);
            }
            try
            {
                IsSH = CheckForSH(target);
                if (IsSH) return;
            }
            catch (Exception)
            {
                Log.Debug($"SerpentsHand, by Cyanox, is not installed. Skipping method call.", Scp008X.Instance.Config.DebugMode);
            }
            try
            {
                if (target.UserId == TryGet999().UserId) return;
            }
            catch(Exception)
            {
                Log.Debug($"SCP-999-X, by DGvagabond, is not installed. Skipping method call.", Scp008X.Instance.Config.DebugMode);
            }
            if (target.ReferenceHub.gameObject.TryGetComponent(out Scp008 scp008)) { return; }
            target.ReferenceHub.gameObject.AddComponent<Scp008>();
            Victims.Add(target);
            
            target.ShowHint($"<color=yellow><b>SCP-008</b></color>\n{Scp008X.Instance.Config.InfectionAlert}", 10f);
        }

        private static bool CheckForSH(User player)
        {
            try
            {
                return SerpentsHand.API.SerpentsHand.GetSHPlayers().Contains(player);
            }
            catch (Exception)
            {
                return false;
            }
        }
        private void Turn(User target)
        {
            if (!target.ReferenceHub.TryGetComponent(out Scp008 scp008)) { target.GameObject.AddComponent<Scp008>(); }
            if (target.ReferenceHub.playerEffectsController.GetEffect<Scp207>().Enabled) { target.DisableEffect<Scp207>(); }
            if (target.CurrentItem.id.Gun()) { target.Inventory.ServerDropAll(); }
            if (Scp008X.Instance.Config.SuicideBroadcast != null)
            {
                target.ClearBroadcasts();
                target.Broadcast(10, Scp008X.Instance.Config.SuicideBroadcast);
            }
            if (!Scp008X.Instance.Config.RetainInventory) { target.ClearInventory(); }
            if (Scp008X.Instance.Config.Scp008Buff >= 0) { target.AdrenalineHealth += Scp008X.Instance.Config.Scp008Buff; }
            target.GameObject.AddComponent<RetainAhp>();
            target.Health = Scp008X.Instance.Config.ZombieHealth;
            target.ShowHint($"<color=yellow><b>SCP-008</b></color>\n{Scp008X.Instance.Config.SpawnHint}", 20f);
            if (!Scp008X.Instance.Config.AoeTurned) return;
            var infected = User.List.Where(x => x.CurrentRoom == target.CurrentRoom && x.UserId != target.UserId).ToList();
            if (infected.Count == 0) return;
            foreach (var ply in from ply in infected let chance = _gen.Next(1, 100) where chance <= Scp008X.Instance.Config.AoeChance && ply.Team != Team.SCP select ply)
            {
                Infect(ply);
            }
        }
        public static bool Scp008Check()
        {
            return Victims.Count <= 0;
        }
        public static void Contained(Player pl)
        {
            Log.Debug($"SCP008Check() passed. Announcing containment...", Scp008X.Instance.Config.DebugMode);
            string cause;
            switch (pl.Team)
            {
                case Team.MTF:
                    var team = "";
                    team = !UnitNamingRules.TryGetNamingRule(SpawnableTeamType.NineTailedFox, out var unit) 
                        ? "unknown" : unit.GetCassieUnitName(pl.ReferenceHub.characterClassManager.CurUnitName);
                    cause = $". containmentunit {team}";
                    break;
                case Team.CDP:
                    cause = "by classd personnel";
                    break;
                case Team.CHI:
                    cause = "by chaosinsurgency";
                    break;
                case Team.RSC:
                    cause = "by scientist personnel";
                    break;
                default:
                    cause = "containmentunit unknown";
                    break;
            }
            Cassie.GlitchyMessage($"SCP 0 0 8 successfully terminated {cause}",15,15);
        }
    }
}