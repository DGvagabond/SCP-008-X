using CustomPlayerEffects;
using User = Exiled.API.Features.Player;
using Exiled.Events.EventArgs;
using SCP008X.Components;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using scp035.API;
using SCP999X.API;
using System;
using MEC;

namespace SCP008X
{
    public class EventHandlers
    {
        public SCP008X plugin;
        public EventHandlers(SCP008X plugin) => this.plugin = plugin;
        private Random Gen = new Random();
        private int victims = 0;
        private bool is035 { get; set; }
        private bool isSH { get; set; }
        private User TryGet035() => Scp035Data.GetScp035();
        private User TryGet999() => SCP999API.GetScp999();

        public void OnRoundStart()
        {
            if (SCP008X.Instance.Config.CassieAnnounce && SCP008X.Instance.Config.Announcement != null)
            {
                Cassie.DelayedMessage(SCP008X.Instance.Config.Announcement, 5f, false, true);
            }
        }
        public void OnRoundRestart()
        {
            victims = 0;
        }
        public void OnRoundEnd(RoundEndedEventArgs ev)
        {
            if (SCP008X.Instance.Config.SummaryStats)
            {
                Map.ShowHint($"\n\n\n\n\n\n\n\n\n\n\n\n\n\n<align=left><color=yellow><b>SCP-008 Victims:</b></color> {victims}/{RoundSummary.changed_into_zombies}", 30f);
            }
        }
        public void OnPlayerJoin(JoinedEventArgs ev)
        {
            ev.Player.SendConsoleMessage($"This server uses SCP-008-X, all zombies have been buffed!", "yellow");
        }
        public void OnPlayerLeave(LeftEventArgs ev)
        {
            if(ev.Player.Role==RoleType.Scp0492 && ev.Player.ReferenceHub.TryGetComponent(out SCP008 s008))
            {
                ClearSCP008(ev.Player);
            }
        }
        public void OnPlayerHurt(HurtingEventArgs ev)
        {
            if (ev.Target.UserId == "PET")
            {
                Log.Debug($"{ev.Target} is a pet object, skipping method call.", SCP008X.Instance.Config.DebugMode);
                ev.IsAllowed = false;
                return;
            }
            if(ev.Attacker.Role == RoleType.Scp0492)
            {
                try
                {
                    if (ev.Target.UserId == TryGet035().UserId)
                    {
                        Log.Debug($"{ev.Target} is SCP-035, skipping method call.", SCP008X.Instance.Config.DebugMode);
                        ev.IsAllowed = false;
                        return;
                    }
                }
                catch (Exception)
                {
                    Log.Debug($"SCP-035, by Cyanox, is not installed. Skipping method call.", SCP008X.Instance.Config.DebugMode);
                }
                try
                {
                    isSH = CheckForSH(ev.Target);
                    if (isSH)
                    {
                        Log.Debug($"{ev.Target} is part of SerpentsHand, skipping method call.", SCP008X.Instance.Config.DebugMode);
                        ev.IsAllowed = false;
                        return;
                    }
                }
                catch (Exception)
                {
                    Log.Debug($"SerpentsHand, by Cyanox, is not installed. Skipping method call.", SCP008X.Instance.Config.DebugMode);
                }
                if(ev.Target != ev.Attacker)
                {
                    if (SCP008X.Instance.Config.ZombieDamage >= 0)
                    {
                        ev.Amount = SCP008X.Instance.Config.ZombieDamage;
                        Log.Debug($"Damage overriden to be {ev.Amount}.", SCP008X.Instance.Config.DebugMode);
                    }
                    if (SCP008X.Instance.Config.Scp008Buff >= 0)
                    {
                        ev.Attacker.AdrenalineHealth += SCP008X.Instance.Config.Scp008Buff;
                        Log.Debug($"Added {SCP008X.Instance.Config.Scp008Buff} AHP to {ev.Attacker}.", SCP008X.Instance.Config.DebugMode);
                    }
                    int chance = Gen.Next(1, 100);
                    if (chance <= SCP008X.Instance.Config.InfectionChance && ev.Target.Team != Team.SCP)
                    {
                        try
                        {
                            Infect(ev.Target);
                            Log.Debug($"Successfully infected {ev.Target} with {chance}% probability.", SCP008X.Instance.Config.DebugMode);
                        }
                        catch (Exception e)
                        {
                            Log.Error($"Failed to infect {ev.Target}! {e}");
                            throw;
                        }
                    }
                }
            }
        }
        public void OnHealed(UsedMedicalItemEventArgs ev)
        {
            int chance = Gen.Next(1, 100);
            if(ev.Player.ReferenceHub.TryGetComponent(out SCP008 scp008))
            {
                switch (ev.Item)
                {
                    case ItemType.SCP500:
                        UnityEngine.Object.Destroy(scp008);
                        Log.Debug($"{ev.Player} successfully cured themselves.", SCP008X.Instance.Config.DebugMode);
                        break;
                    case ItemType.Medkit:
                        if(chance <= SCP008X.Instance.Config.CureChance) { UnityEngine.Object.Destroy(scp008); Log.Debug($"{ev.Player} cured themselves with {chance}% probability.", SCP008X.Instance.Config.DebugMode); }
                        break;
                }
            }
        }
        public void OnRoleChange(ChangingRoleEventArgs ev)
        {
            if (ev.NewRole == RoleType.Scp0492)
            {
                Log.Debug($"Calling Turn() method for {ev.Player}.", SCP008X.Instance.Config.DebugMode);
                Turn(ev.Player);
            }
            if (ev.NewRole != RoleType.Scp0492 || ev.NewRole != RoleType.Scp096) { ClearSCP008(ev.Player); ev.Player.AdrenalineHealth = 0; Log.Debug($"Called ClearSCP008() method for {ev.Player}.", SCP008X.Instance.Config.DebugMode); }
        }
        public void OnReviving(StartingRecallEventArgs ev)
        {
            if (SCP008X.Instance.Config.BuffDoctor)
            {
                ev.IsAllowed = false;
                ev.Target.SetRole(RoleType.Scp0492, true, false);
            }
        }
        public void OnRevived(FinishingRecallEventArgs ev)
        {
            if (SCP008X.Instance.Config.SuicideBroadcast != null)
            {
                ev.Target.ClearBroadcasts();
                ev.Target.Broadcast(10, SCP008X.Instance.Config.SuicideBroadcast);
            }
            if (SCP008X.Instance.Config.Scp008Buff >= 0) { ev.Target.AdrenalineHealth += SCP008X.Instance.Config.Scp008Buff; Log.Debug($"Added {SCP008X.Instance.Config.Scp008Buff} AHP to {ev.Target}.", SCP008X.Instance.Config.DebugMode); }
            ev.Target.Health = SCP008X.Instance.Config.ZombieHealth;
            Log.Debug($"Set {ev.Target}'s HP to {SCP008X.Instance.Config.Scp008Buff}.", SCP008X.Instance.Config.DebugMode);
            ev.Target.ShowHint($"<color=yellow><b>SCP-008</b></color>\n{SCP008X.Instance.Config.SpawnHint}", 20f);
        }
        public void OnPlayerDying(DyingEventArgs ev)
        {
            if (ev.Target.Role == RoleType.Scp0492) { ClearSCP008(ev.Target); Log.Debug($"Called ClearSCP008() method for {ev.Target}.", SCP008X.Instance.Config.DebugMode); }
            if (ev.Target.ReferenceHub.TryGetComponent(out SCP008 scp008))
            {
                ev.Target.SetRole(RoleType.Scp0492, true, false);
            }
        }
        public void OnPlayerDied(DiedEventArgs ev)
        {
            if (ev.Target.Role == RoleType.Scp049 || ev.Target.Role == RoleType.Scp0492)
            {
                victims--;
                if (SCP008Check())
                {
                    Log.Debug($"SCP008Check() passed. Announcing recontainment...", SCP008X.Instance.Config.DebugMode);
                    Cassie.Message($"SCP 0 0 8 containedsuccessfully . noscpsleft", false, true);
                }
            }
            if (SCP008X.Instance.Config.AoeInfection && ev.Target.Role == RoleType.Scp0492)
            {
                Log.Debug($"AOE infection enabled, running check...", SCP008X.Instance.Config.DebugMode);
                IEnumerable<User> targets = User.List.Where(x => x.CurrentRoom == ev.Target.CurrentRoom);
                targets = targets.Where(x => x.UserId != ev.Target.UserId);
                List<User> infecteds = targets.ToList();
                Log.Debug($"Made a list of {infecteds.Count} players.", SCP008X.Instance.Config.DebugMode);
                if (infecteds.Count == 0) return;
                foreach (User ply in infecteds)
                {
                    int chance = Gen.Next(1, 100);
                    if (chance <= SCP008X.Instance.Config.AoeChance && ply.Team != Team.SCP)
                    {
                        Infect(ply);
                        Log.Debug($"Called Infect() method for {ev.Target} due to AOE.", SCP008X.Instance.Config.DebugMode);
                    }
                }
            }
        }

        private void ClearSCP008(User player)
        {
            if (player.ReferenceHub.TryGetComponent(out SCP008 scp008))
                UnityEngine.Object.Destroy(scp008);
        }
        public void Infect(User target)
        {
            try
            {
                if (target.UserId == TryGet035().UserId) return;
            }
            catch (Exception)
            {
                Log.Debug($"SCP-035, by Cyanox, is not installed. Skipping method call.", SCP008X.Instance.Config.DebugMode);
            }
            try
            {
                isSH = CheckForSH(target);
                if (isSH) return;
            }
            catch (Exception)
            {
                Log.Debug($"SerpentsHand, by Cyanox, is not installed. Skipping method call.", SCP008X.Instance.Config.DebugMode);
            }
            try
            {
                if (target.UserId == TryGet999().UserId) return;
            }
            catch(Exception)
            {
                Log.Debug($"SCP-999-X, by DGvagabond, is not installed. Skipping method call.", SCP008X.Instance.Config.DebugMode);
            }
            if (target.ReferenceHub.gameObject.TryGetComponent(out SCP008 scp008)) { return; }
            target.ReferenceHub.gameObject.AddComponent<SCP008>();
            target.ShowHint($"<color=yellow><b>SCP-008</b></color>\n{SCP008X.Instance.Config.InfectionAlert}", 10f);
        }

        bool CheckForSH(User player)
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
            victims++;
            if (!target.ReferenceHub.TryGetComponent(out SCP008 scp008)) { target.GameObject.AddComponent<SCP008>(); }
            if (target.ReferenceHub.playerEffectsController.GetEffect<Scp207>().Enabled) { target.ReferenceHub.playerEffectsController.DisableEffect<Scp207>(); }
            if (target.CurrentItem.id.Gun()) { target.Inventory.ServerDropAll(); }
            if (SCP008X.Instance.Config.SuicideBroadcast != null)
            {
                target.ClearBroadcasts();
                target.Broadcast(10, SCP008X.Instance.Config.SuicideBroadcast);
            }
            if (!SCP008X.Instance.Config.RetainInventory) { target.ClearInventory(); }
            if (SCP008X.Instance.Config.Scp008Buff >= 0) { target.AdrenalineHealth += SCP008X.Instance.Config.Scp008Buff; }
            target.Health = SCP008X.Instance.Config.ZombieHealth;
            target.ShowHint($"<color=yellow><b>SCP-008</b></color>\n{SCP008X.Instance.Config.SpawnHint}", 20f);
            if (SCP008X.Instance.Config.AoeTurned)
            {
                IEnumerable<User> targets = User.List.Where(x => x.CurrentRoom == target.CurrentRoom);
                targets = targets.Where(x => x.UserId != target.UserId);
                List<User> infecteds = targets.ToList();
                if (infecteds.Count == 0) return;
                foreach (User ply in infecteds)
                {
                    int chance = Gen.Next(1, 100);
                    if (chance <= SCP008X.Instance.Config.AoeChance && ply.Team != Team.SCP)
                    {
                        Infect(ply);
                    }
                }
            }
            return;
        }
        private bool SCP008Check()
        {
            int check = 0;
            foreach(User ply in User.List)
            {
                if(ply.ReferenceHub.gameObject.TryGetComponent(out SCP008 scp008)) { check++; }
                if(ply.Role == RoleType.Scp049) { check++; }
            }
            if (check == 0)
            {
                return true;
            }
            return false;
        }
    }
}