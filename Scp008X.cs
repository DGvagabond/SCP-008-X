// -----------------------------------------------------------------------
// <copyright file="Scp008X.cs">
// Copyright (c) DGvagabond. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SCP008X
{
    using System;
    using Exiled.API.Features;
    using PlayerEvents = Exiled.Events.Handlers.Player;
    using ServerEvents = Exiled.Events.Handlers.Server;
    using Exiled.Events.Handlers;
    using Exiled.CustomRoles.API;
    
    public class Scp008X : Plugin<Config>
    {
        internal static Scp008X Instance { get; } = new Scp008X();

        private Scp008X() { }
        public Random Rng = new Random();

        public override string Author => "DGvagabond";
        public override string Name => "Scp008X";
        public override Version Version { get; } = new Version(3, 1, 1, 1);
        public override Version RequiredExiledVersion { get; } = new Version(5, 2, 1);

        private EventHandlers _events;

        public override void OnEnabled()
        {
            RegisterRoles();
            RegisterEvents();
            base.OnEnabled();
        }
        
        public override void OnDisabled()
        {
            UnregisterRoles();
            UnregisterEvents();
            base.OnDisabled();
        }

        private void RegisterEvents()
        {
            Log.Debug("Loading events...");
            _events = new EventHandlers();
            
            PlayerEvents.Dying += _events.OnDying;
            PlayerEvents.Hurting += _events.OnHurt;
            PlayerEvents.Shooting += _events.OnShoot;
            PlayerEvents.Verified += _events.OnVerified;
            PlayerEvents.UsedItem += _events.OnHealed;
            
            Scp049.StartingRecall += _events.OnReviving;
            
            ServerEvents.RoundStarted += _events.OnRoundStart;
        }
        private void UnregisterEvents()
        {
            Log.Debug("Unloading events...");
            
            PlayerEvents.Dying -= _events.OnDying;
            PlayerEvents.Hurting -= _events.OnHurt;
            PlayerEvents.Shooting -= _events.OnShoot;
            PlayerEvents.Verified -= _events.OnVerified;
            PlayerEvents.UsedItem -= _events.OnHealed;
            
            Scp049.StartingRecall -= _events.OnReviving;
            
            ServerEvents.RoundStarted += _events.OnRoundStart;

            _events = null;
        }

        private void RegisterRoles()
        {
            Log.Debug("Loading custom roles...");
            
            new Scp008().Register();
        }
        private void UnregisterRoles()
        {
            Log.Debug("Unloading custom roles...");
            
            new Scp008().Unregister();
        }
    }
}