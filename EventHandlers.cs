using CustomPlayerEffects;
using User = Exiled.API.Features.Player;
using Exiled.Events.EventArgs;
using SCP008X.Components;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.Loader;
using scp035.API;
using SerpentsHand.API;
using System;

namespace SCP008X
{
    public class EventHandlers
    {
        public SCP008X plugin;
        public EventHandlers(SCP008X plugin) => this.plugin = plugin;
        private Random Gen = new Random();
        private bool is035 { get; set; }
        private bool isSH { get; set; }
        private User TryGet035() => Scp035Data.GetScp035();

        public void OnRoundStart()
        {
            if (SCP008X.Instance.Config.CassieAnnounce && SCP008X.Instance.Config.Announcement != null)
            {
                Cassie.DelayedMessage(SCP008X.Instance.Config.Announcement, 5f, false, true);
            }
        }
        public void OnPlayerJoin(JoinedEventArgs ev)
        {
            ev.Player.SendConsoleMessage($"This server uses SCP-008-X, all zombies have been buffed!", "yellow");
        }
        public void OnPlayerLeave(LeftEventArgs ev)
        {

        }
        public void OnPlayerHurt(HurtingEventArgs ev)
        {
            if (ev.Target.UserId == "PET")
            {
                ev.IsAllowed = false;
                return;
            }
            try
            {
                if (ev.Target.UserId == TryGet035().UserId && ev.Attacker.Role == RoleType.Scp0492)
                {
                    ev.IsAllowed = false;
                    return;
                }
            }
            catch (Exception)
            {
                if (SCP008X.Instance.Config.DebugMode) Log.Debug($"SCP-035 is not installed, skipping method call.", Loader.ShouldDebugBeShown);
            }
            try
            {
                isSH = CheckForSH(ev.Target);
                if (isSH && ev.Attacker.Role == RoleType.Scp0492)
                {
                    ev.IsAllowed = false;
                    return;
                }
            }
            catch (Exception)
            {
                if (SCP008X.Instance.Config.DebugMode) Log.Debug($"SerpentsHand is not installed, skipping method call.", Loader.ShouldDebugBeShown);
            }
            SCP008BuffComponent comp = ev.Attacker.GameObject.GetComponent<SCP008BuffComponent>();
            if (comp == null) { ev.Attacker.GameObject.AddComponent<SCP008BuffComponent>(); }
            if (ev.Target != ev.Attacker && ev.Attacker.Role == RoleType.Scp0492)
            {
                if (SCP008X.Instance.Config.ZombieDamage >= 0)
                    ev.Amount = SCP008X.Instance.Config.ZombieDamage;
                if (SCP008X.Instance.Config.Scp008Buff >= 0)
                    ev.Attacker.AdrenalineHealth += SCP008X.Instance.Config.Scp008Buff;
                int chance = Gen.Next(1, 100);
                if (chance <= SCP008X.Instance.Config.InfectionChance && ev.Target.Team != Team.SCP)
                {
                    Infect(ev.Target);
                }
            }
        }
        public void OnHealing(UsedMedicalItemEventArgs ev)
        {
            if (ev.Player.ReferenceHub.playerEffectsController.GetEffect<Poisoned>().Enabled)
            {
                int cure = Gen.Next(1, 100);
                if (ev.Item == ItemType.SCP500)
                    ev.Player.ReferenceHub.playerEffectsController.DisableEffect<Poisoned>();
                if (ev.Item == ItemType.Medkit && cure <= SCP008X.Instance.Config.CureChance)
                {
                    ev.Player.ReferenceHub.playerEffectsController.DisableEffect<Poisoned>();
                    return;
                }
                ev.Player.ReferenceHub.playerEffectsController.DisableEffect<Poisoned>();
                ev.Player.ReferenceHub.playerEffectsController.EnableEffect<Poisoned>();
            }
        }
        public void OnPlayerDying(DyingEventArgs ev)
        {
            if (EnvironmentalCheck(ev))
            {
                ev.IsAllowed = true;
                return;
            }
            if (ev.HitInformation.GetDamageType() == DamageTypes.Poison || ev.Killer.Role == RoleType.Scp0492 && ev.Target != ev.Killer)
            {
                ev.Target.SetRole(RoleType.Scp0492, true, false);
                RoundSummary.changed_into_zombies++;
                return;
            }
            if (ev.Target.Role == RoleType.Scp0492) { ClearSCP008(ev.Target); }
        }
        public void OnRoleChange(ChangingRoleEventArgs ev)
        {
            if (ev.NewRole == RoleType.Scp0492)
            {
                Turn(ev.Player);
            }
            if (ev.NewRole != RoleType.Scp0492 || ev.NewRole != RoleType.Scp096) { ClearSCP008(ev.Player); ev.Player.AdrenalineHealth = 0; }
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
            if (SCP008X.Instance.Config.Scp008Buff >= 0) { ev.Target.AdrenalineHealth += SCP008X.Instance.Config.Scp008Buff; }
            ev.Target.Health = SCP008X.Instance.Config.ZombieHealth;
            ev.Target.ShowHint($"<color=yellow><b>SCP-008</b></color>\n{SCP008X.Instance.Config.SpawnHint}", 20f);
        }
        public void OnPlayerDied(DiedEventArgs ev)
        {
            if (ev.Target.Role == RoleType.Scp049 || ev.Target.Role == RoleType.Scp0492)
            {
                if (SCP008Check())
                {
                    Cassie.Message($"SCP 0 0 8 containedsuccessfully . noscpsleft", false, true);
                }
            }
            if (SCP008X.Instance.Config.AoeInfection && ev.Target.Role == RoleType.Scp0492)
            {
                IEnumerable<User> targets = User.List.Where(x => x.CurrentRoom == ev.Target.CurrentRoom);
                targets = targets.Where(x => x.UserId != ev.Target.UserId);
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
        }

        private void ClearSCP008(User player)
        {
            SCP008BuffComponent comp = player.GameObject.GetComponent<SCP008BuffComponent>();
            if (comp != null)
                UnityEngine.Object.Destroy(comp);
        }
        private void Infect(User target)
        {
            try
            {
                if (target.UserId == TryGet035().UserId) is035 = true;
                if (is035) return;
            }
            catch (Exception)
            {
                if (SCP008X.Instance.Config.DebugMode) Log.Debug($"SCP-035 is not installed, skipping method call.", Loader.ShouldDebugBeShown);
            }
            try
            {
                isSH = CheckForSH(target);
                if (isSH) return;
            }
            catch (Exception)
            {
                if (SCP008X.Instance.Config.DebugMode) Log.Debug($"SerpentsHand is not installed, skipping method call.", Loader.ShouldDebugBeShown);
            }
            target.ReferenceHub.playerEffectsController.EnableEffect<Poisoned>();
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
        private bool EnvironmentalCheck(DyingEventArgs e)
        {
            switch (DamageTypes.FromIndex(e.HitInformation.Tool).name)
            {
                case "FALLDOWN":
                    return true;
                case "DECONT":
                    return true;
                case "NUKE":
                    return true;
                case "POCKET":
                    return true;
                case "WALL":
                    return true;
                case "FLYING":
                    return true;
            }
            return false;
        }
        private bool SCP008Check()
        {
            IEnumerable<User> scps = User.List.Where(x => x.Team == Team.SCP);
            scps = scps.Where(x => x.Role == RoleType.Scp049 || x.Role == RoleType.Scp0492);
            List<User> scp008 = scps.ToList();
            if (scp008.Count == 0)
            {
                return true;
            }
            return false;
        }
    }
}