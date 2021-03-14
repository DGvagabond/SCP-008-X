using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.CustomItems.API;
using Exiled.CustomItems.API.Features;
using Exiled.CustomItems.API.Spawn;
using Exiled.Events.EventArgs;

namespace SCP008X
{
    public class Scp0081 : CustomItem
    {
        public override uint Id { get; set; } = 0081;
        public override string Name { get; set; } = "SCP-008 Alpha";
        public override string Description { get; set; } = "Cures the consumer of SCP-008";
        public override ItemType Type { get; set; } = ItemType.Medkit;
        public override SpawnProperties SpawnProperties { get; set; } = new SpawnProperties
        {
            Limit = 3,
            DynamicSpawnPoints = new List<DynamicSpawnPoint>
            {
                new DynamicSpawnPoint{Chance=100,Location=SpawnLocation.Inside049Armory},
                new DynamicSpawnPoint{Chance=100,Location=SpawnLocation.InsideHczArmory},
                new DynamicSpawnPoint{Chance=100,Location=SpawnLocation.InsideNukeArmory},
                new DynamicSpawnPoint{Chance=100,Location=SpawnLocation.InsideHidLeft}
            }
        };
        
        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingMedicalItem += OnUsingMedicalItem;
            base.SubscribeEvents();
        }
        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingMedicalItem -= OnUsingMedicalItem;
            base.UnsubscribeEvents();
        }

        private void OnUsingMedicalItem(UsingMedicalItemEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem)) return;
            if (ev.Player.ReferenceHub.TryGetComponent(out Scp008 scp008))
            {
                UnityEngine.Object.Destroy(scp008);
                EventHandlers.Victims.Remove(ev.Player);
                Log.Debug($"{ev.Player} successfully cured themselves.", Scp008X.Instance.Config.DebugMode);
            }
        }
    }
}