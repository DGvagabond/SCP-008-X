using CustomPlayerEffects;
using User = Exiled.API.Features.Player;
using Exiled.Events.EventArgs;
using SCP008X.Components;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;

namespace SCP008X.Handlers
{
    public class Player
    {
        public Plugin plugin;
        public Player(Plugin plugin) => this.plugin = plugin;
        private System.Random Gen = new System.Random();

        public void OnPlayerJoin(JoinedEventArgs ev)
        {
            ev.Player.SendConsoleMessage($"This server uses SCP-008-X, all zombies have been buffed!", "yellow");
        }
        public void OnPlayerLeave(LeftEventArgs ev)
        {
            
        }
        public void OnPlayerHurt(HurtingEventArgs ev)
        {
            SCP008BuffComponent comp = ev.Attacker.GameObject.GetComponent<SCP008BuffComponent>();
            if(comp == null) { ev.Attacker.GameObject.AddComponent<SCP008BuffComponent>(); }
            if (ev.Target != ev.Attacker && ev.Attacker.Role == RoleType.Scp0492)
            {
                if (Plugin.Instance.Config.ZombieDamage >= 0)
                    ev.Amount = Plugin.Instance.Config.ZombieDamage;
                if (Plugin.Instance.Config.Scp008Buff >= 0)
                    ev.Attacker.AdrenalineHealth += Plugin.Instance.Config.Scp008Buff;
                int chance = Gen.Next(1, 100);
                if (chance <= Plugin.Instance.Config.InfectionChance && ev.Target.Team != Team.SCP)
                {
                    Infect(ev.Target);
                }
            }
        }
        public void OnHealing(UsedMedicalItemEventArgs ev)
        {
            if(ev.Player.ReferenceHub.playerEffectsController.GetEffect<Poisoned>().Enabled)
            {
                int cure = Gen.Next(1, 100);
                if (ev.Item == ItemType.SCP500)
                    ev.Player.ReferenceHub.playerEffectsController.DisableEffect<Poisoned>();
                if (ev.Item == ItemType.Medkit && cure <= Plugin.Instance.Config.CureChance)
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
            if(EnvironmentalCheck(ev))
            {
                ev.IsAllowed = true;
                return;
            }
            if (ev.HitInformation.GetDamageType() == DamageTypes.Poison)
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
                if (ev.Player.CurrentItem.id.Gun()) { ev.Player.Inventory.ServerDropAll(); }
                if(Plugin.Instance.Config.SuicideBroadcast != null)
                {
                    ev.Player.ClearBroadcasts();
                    ev.Player.Broadcast(10, Plugin.Instance.Config.SuicideBroadcast);
                }
                if (!Plugin.Instance.Config.RetainInventory) { ev.Player.ClearInventory(); }
                if (Plugin.Instance.Config.Scp008Buff >= 0) { ev.Player.AdrenalineHealth += Plugin.Instance.Config.Scp008Buff; }
                ev.Player.Health = Plugin.Instance.Config.ZombieHealth;
                ev.Player.ShowHint($"<color=yellow><b>SCP-008</b></color>\n{Plugin.Instance.Config.SpawnHint}", 20f);
                if (Plugin.Instance.Config.AoeTurned)
                {
                    IEnumerable<User> targets = User.List.Where(x => x.CurrentRoom == ev.Player.CurrentRoom);
                    targets = targets.Where(x => x.UserId != ev.Player.UserId);
                    List<User> infecteds = targets.ToList();
                    if (infecteds.Count == 0) return;
                    foreach (User ply in infecteds)
                    {
                        int chance = Gen.Next(1, 100);
                        if (chance <= Plugin.Instance.Config.AoeChance && ply.Team != Team.SCP)
                        {
                            Infect(ply);
                        }
                    }
                }
                return;
            }
            if (ev.NewRole != RoleType.Scp0492 || ev.NewRole != RoleType.Scp096) { ClearSCP008(ev.Player); ev.Player.AdrenalineHealth = 0; }
        }
        public void OnReviving(StartingRecallEventArgs ev)
        {
            if(Plugin.Instance.Config.BuffDoctor)
            {
                ev.IsAllowed = false;
                ev.Target.SetRole(RoleType.Scp0492, true, false);
            }
        }
        public void OnRevived(FinishingRecallEventArgs ev)
        {
            if (Plugin.Instance.Config.SuicideBroadcast != null)
            {
                ev.Target.ClearBroadcasts();
                ev.Target.Broadcast(10, Plugin.Instance.Config.SuicideBroadcast);
            }
            if (Plugin.Instance.Config.Scp008Buff >= 0) { ev.Target.AdrenalineHealth += Plugin.Instance.Config.Scp008Buff; }
            ev.Target.Health = Plugin.Instance.Config.ZombieHealth;
            ev.Target.ShowHint($"<color=yellow><b>SCP-008</b></color>\n{Plugin.Instance.Config.SpawnHint}", 20f);
        }
        public void OnPlayerDied(DiedEventArgs ev)
        {
            if(ev.Target.Role == RoleType.Scp049 || ev.Target.Role == RoleType.Scp0492)
            {
                if(SCP008Check())
                {
                    Cassie.Message($"SCP 0 0 8 containedsuccessfully . noscpsleft", false, true);
                }
            }
            if(Plugin.Instance.Config.AoeInfection && ev.Target.Role == RoleType.Scp0492)
            {
                IEnumerable<User> targets = User.List.Where(x => x.CurrentRoom == ev.Target.CurrentRoom);
                targets = targets.Where(x => x.UserId != ev.Target.UserId);
                List<User> infecteds = targets.ToList();
                if (infecteds.Count == 0) return;
                foreach (User ply in infecteds)
                {
                    int chance = Gen.Next(1, 100);
                    if (chance <= Plugin.Instance.Config.AoeChance && ply.Team != Team.SCP)
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
            target.ReferenceHub.playerEffectsController.EnableEffect<Poisoned>();
            target.ShowHint($"<color=yellow><b>SCP-008</b></color>\n{Plugin.Instance.Config.InfectionAlert}", 10f);
        }
        private bool EnvironmentalCheck(DyingEventArgs e)
        {
            switch(DamageTypes.FromIndex(e.HitInformation.Tool).name)
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
            if(scp008.Count == 0)
            {
                return true;
            }
            return false;
        }
    }
}