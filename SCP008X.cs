using System;
using Exiled.API.Features;
using Exiled.API.Enums;
using Player = Exiled.Events.Handlers.Player;
using Server = Exiled.Events.Handlers.Server;
using Exiled.Events.Handlers;

namespace SCP008X
{
    public class SCP008X : Plugin<Config>
    {
        internal static SCP008X Instance { get; } = new SCP008X();
        private SCP008X() { }
        public bool Outbreak {get; set; }

        public override PluginPriority Priority { get; } = PluginPriority.Medium;

        public override string Author { get; } = "DGvagabond";
        public override string Name { get; } = "Scp008X";
        public override Version Version { get; } = new Version(2, 0, 1, 0);
        public override Version RequiredExiledVersion { get; } = new Version(2, 1, 18);

        private EventHandlers _events;
        private static SCP008X _singleton;

        public override void OnEnabled()
        {
            try
            {
                base.OnEnabled();
                RegisterEvents();
            }

            catch (Exception e)
            {
                Log.Error($"There was an error loading the plugin: {e}");
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
                Log.Error($"There was an error unloading the plugin: {e}");
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
                Log.Error($"There was an error reloading the plugin: {e}");
            }
        }

        private void RegisterEvents()
        {
            _singleton = this;
            _events = new EventHandlers(this);

            Player.Died += _events.OnPlayerDied;
            Player.Left += _events.OnPlayerLeave;
            Player.Dying += _events.OnPlayerDying;
            Player.Hurting += _events.OnPlayerHurt;
            Server.RoundEnded += _events.OnRoundEnd;
            Player.MedicalItemUsed += _events.OnHealed;
            Player.ChangingRole += _events.OnRoleChange;
            Scp049.StartingRecall += _events.OnReviving;
            Scp049.FinishingRecall += _events.OnRevived;
            Server.RoundStarted += _events.OnRoundStart;
            Server.RestartingRound += _events.OnRoundRestart;
        }
        private void UnregisterEvents()
        {
            Server.RestartingRound -= _events.OnRoundRestart;
            Player.ChangingRole -= _events.OnRoleChange;
            Scp049.StartingRecall -= _events.OnReviving;
            Scp049.FinishingRecall -= _events.OnRevived;
            Server.RoundStarted -= _events.OnRoundStart;
            Player.MedicalItemUsed -= _events.OnHealed;
            Server.RoundEnded -= _events.OnRoundEnd;
            Player.Hurting -= _events.OnPlayerHurt;
            Player.Dying -= _events.OnPlayerDying;
            Player.Left -= _events.OnPlayerLeave;
            Player.Died -= _events.OnPlayerDied;

            _events = null;
        }
    }
}