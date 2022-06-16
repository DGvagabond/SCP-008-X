using Exiled.API.Features;
using Exiled.API.Features.DamageHandlers;
using Footprinting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCP008X.DamageUtilities
{
    internal class Scp008GenericDamage : PlayerStatsSystem.AttackerDamageHandler
    {
        private Player Player;

        public Scp008GenericDamage(Player target, Player attacker, float damage, string customReason)
        {
            this.Player = target;
            this.Attacker = attacker.Footprint;
            this.Damage = damage;
            this.ServerLogsText = customReason;
            Log.Debug($"Scp008x log: Target {Player}, Attacker {Attacker} Amount {Damage}", Scp008X.Instance.Config.DebugMode);
        }

        public Scp008GenericDamage(Player player, Player attacker, float damageAmount)
        {
            this.Player = player;
            this.Attacker = attacker.Footprint;
            this.Damage = damageAmount;
        }

        public override bool AllowSelfDamage => true;

        public override string ServerLogsText { get; } = "008X damage";
        public override Footprint Attacker { get; set; }
        public override float Damage { get; set; }

        public override void ProcessDamage(ReferenceHub ply)
        {

        }
    }
}
