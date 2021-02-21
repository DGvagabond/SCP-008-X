using System;
using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.API.Enums;
using Player = Exiled.Events.Handlers.Player;
using Server = Exiled.Events.Handlers.Server;
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
        public override Version Version { get; } = new Version(2, 2, 0, 0);
        public override Version RequiredExiledVersion { get; } = new Version(2, 3, 4);

        private EventHandlers _events;
        private static Scp008X _singleton;

        public override void OnEnabled()
        {
            try
            {
                base.OnEnabled();
                RegisterEvents();
            }

            catch (Exception e)
            {
                Log.Error($"There was an error loading {Version}: {e}");
            }
        }
        public override void OnDisabled()
        {
            try
            {
                base.OnDisabled();
                UnregisterEvents();
            }
            catch(Exception e)
            {
                Log.Error($"There was an error unloading {Version}: {e}");
            }
        }
        public override void OnReloaded()
        {
            try
            {
                base.OnReloaded();
            }
            catch(Exception e)
            {
                Log.Error($"There was an error reloading {Version}: {e}");
            }
        }

        private void RegisterEvents()
        {
            _singleton = this;
            _events = new EventHandlers(this);
            
            Player.Shooting += _events.OnShoot;
            Player.Died += _events.OnDied;
            Player.Destroying += _events.OnDestroying;
            Player.Dying += _events.OnDying;
            Player.Verified += _events.OnVerified;
            Player.Hurting += _events.OnHurt;
            Server.RoundEnded += _events.OnRoundEnd;
            Player.MedicalItemUsed += _events.OnHealed;
            Player.ChangingRole += _events.OnRoleChange;
            Scp049.StartingRecall += _events.OnReviving;
            Scp049.FinishingRecall += _events.OnRevived;
            Server.RoundStarted += _events.OnRoundStart;
            Player.FailingEscapePocketDimension += _events.OnFail;
        }
        private void UnregisterEvents()
        {
            Player.FailingEscapePocketDimension -= _events.OnFail;
            Player.ChangingRole -= _events.OnRoleChange;
            Scp049.StartingRecall -= _events.OnReviving;
            Scp049.FinishingRecall -= _events.OnRevived;
            Server.RoundStarted -= _events.OnRoundStart;
            Player.MedicalItemUsed -= _events.OnHealed;
            Server.RoundEnded -= _events.OnRoundEnd;
            Player.Hurting -= _events.OnHurt;
            Player.Dying -= _events.OnDying;
            Player.Verified -= _events.OnVerified;
            Player.Destroying -= _events.OnDestroying;
            Player.Died -= _events.OnDied;
            Player.Shooting -= _events.OnShoot;

            _events = null;
        }
    }
}