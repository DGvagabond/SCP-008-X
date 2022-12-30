using Exiled.API.Features;
using Footprinting;

namespace SCP008X.DamageUtilities
{
    internal class Scp008GenericDamage : PlayerStatsSystem.AttackerDamageHandler
    {
        private Player Player;

        public Scp008GenericDamage(Player target, Player attacker, float damage, string customReason)
        {
            Player = target;
            Attacker = attacker.Footprint;
            Damage = damage;
            ServerLogsText = customReason;
            Log.Debug($"Scp008x log: Target {Player}, Attacker {Attacker} Amount {Damage}");
        }

        public Scp008GenericDamage(Player player, Player attacker, float damageAmount)
        {
            Player = player;
            Attacker = attacker.Footprint;
            Damage = damageAmount;
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
