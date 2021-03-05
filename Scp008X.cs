using System;
using Exiled.API.Features;
using Exiled.API.Enums;
using PlayerEvents = Exiled.Events.Handlers.Player;
using ServerEvents = Exiled.Events.Handlers.Server;
using Exiled.Events.Handlers;

namespace SCP008X
{
    public class Scp008X : Plugin<Config>
    {
        internal static Scp008X Instance { get; } = new Scp008X();

        private Scp008X() { }

        public bool Outbreak {get; set; }

        public override PluginPriority Priority { get; } = PluginPriority.Medium;

        public override string Author { get; } = "DGvagabond";
        public override string Name { get; } = "Scp008X";
        public override Version Version { get; } = new Version(2, 3, 0, 0);
        public override Version RequiredExiledVersion { get; } = new Version(2, 7, 0);

        private EventHandlers _events;

        public override void OnEnabled()
        {
            RegisterEvents();
            base.OnEnabled();
        }
        
        public override void OnDisabled()
        {
            UnregisterEvents();
            base.OnDisabled();
        }

        private void RegisterEvents()
        {
            _events = new EventHandlers();
            
            PlayerEvents.Died += _events.OnDied;
            PlayerEvents.Dying += _events.OnDying;
            PlayerEvents.Hurting += _events.OnHurt;
            PlayerEvents.Shooting += _events.OnShoot;
            PlayerEvents.Verified += _events.OnVerified;
            PlayerEvents.Destroying += _events.OnDestroying;
            PlayerEvents.MedicalItemUsed += _events.OnHealed;
            PlayerEvents.ChangingRole += _events.OnRoleChange;
            PlayerEvents.FailingEscapePocketDimension += _events.OnFail;
            
            Scp049.StartingRecall += _events.OnReviving;
            Scp049.FinishingRecall += _events.OnRevived;
            
            ServerEvents.RoundStarted += _events.OnRoundStart;
        }
        private void UnregisterEvents()
        {
            PlayerEvents.Died -= _events.OnDied;
            PlayerEvents.Dying -= _events.OnDying;
            PlayerEvents.Hurting -= _events.OnHurt;
            PlayerEvents.Shooting -= _events.OnShoot;
            PlayerEvents.Verified -= _events.OnVerified;
            PlayerEvents.Destroying -= _events.OnDestroying;
            PlayerEvents.MedicalItemUsed -= _events.OnHealed;
            PlayerEvents.ChangingRole -= _events.OnRoleChange;
            PlayerEvents.FailingEscapePocketDimension -= _events.OnFail;
            
            Scp049.StartingRecall -= _events.OnReviving;
            Scp049.FinishingRecall -= _events.OnRevived;
            
            ServerEvents.RoundStarted += _events.OnRoundStart;

            _events = null;
        }
    }
}