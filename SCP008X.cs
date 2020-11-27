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
        public bool outbreak {get; set; }

        public override PluginPriority Priority { get; } = PluginPriority.Medium;

        public override string Author { get; } = "DGvagabond";
        public override string Name { get; } = "Scp008X";
        public override Version Version { get; } = new Version(2, 0, 0, 1);
        public override Version RequiredExiledVersion { get; } = new Version(2, 1, 16);

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

        public void RegisterEvents()
        {
            Singleton = this;
            events = new EventHandlers(this);

            Player.Died += events.OnPlayerDied;
            Player.Left += events.OnPlayerLeave;
            Player.Dying += events.OnPlayerDying;
            Player.Hurting += events.OnPlayerHurt;
            Server.RoundEnded += events.OnRoundEnd;
            Player.MedicalItemUsed += events.OnHealed;
            Player.ChangingRole += events.OnRoleChange;
            Scp049.StartingRecall += events.OnReviving;
            Scp049.FinishingRecall += events.OnRevived;
            Server.RoundStarted += events.OnRoundStart;
            Server.RestartingRound += events.OnRoundRestart;
        }
        public void UnregisterEvents()
        {
            Server.RestartingRound -= events.OnRoundRestart;
            Player.ChangingRole -= events.OnRoleChange;
            Scp049.StartingRecall -= events.OnReviving;
            Scp049.FinishingRecall -= events.OnRevived;
            Server.RoundStarted -= events.OnRoundStart;
            Player.MedicalItemUsed -= events.OnHealed;
            Server.RoundEnded -= events.OnRoundEnd;
            Player.Hurting -= events.OnPlayerHurt;
            Player.Dying -= events.OnPlayerDying;
            Player.Left -= events.OnPlayerLeave;
            Player.Died -= events.OnPlayerDied;

            events = null;
        }
    }
}