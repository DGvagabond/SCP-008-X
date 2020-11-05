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

        public override PluginPriority Priority { get; } = PluginPriority.Medium;

        public override string Author { get; } = "DGvagabond";
        public override string Name { get; } = "Scp008X";
        public override Version Version { get; } = new Version(1, 0, 1, 2);
        public override Version RequiredExiledVersion { get; } = new Version(2, 1, 13);

        private EventHandlers events;
        public static SCP008X Singleton;

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
            base.OnDisabled();
            UnregisterEvents();
        }

        public void RegisterEvents()
        {
            Singleton = this;
            events = new EventHandlers(this);

            Player.Left += events.OnPlayerLeave;
            Player.Hurting += events.OnPlayerHurt;
            Player.Dying += events.OnPlayerDying;
            Player.Died += events.OnPlayerDied;
            Player.ChangingRole += events.OnRoleChange;
            Player.MedicalItemUsed += events.OnHealing;
            Scp049.StartingRecall += events.OnReviving;
            Scp049.FinishingRecall += events.OnRevived;
            Server.RoundStarted += events.OnRoundStart;
        }
        public void UnregisterEvents()
        {
            Player.Left -= events.OnPlayerLeave;
            Player.Hurting -= events.OnPlayerHurt;
            Player.Dying -= events.OnPlayerDying;
            Player.Died -= events.OnPlayerDied;
            Player.ChangingRole -= events.OnRoleChange;
            Player.MedicalItemUsed -= events.OnHealing;
            Scp049.StartingRecall -= events.OnReviving;
            Scp049.FinishingRecall -= events.OnRevived;
            Server.RoundStarted -= events.OnRoundStart;

            events = null;
        }
    }
}