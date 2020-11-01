using User = Exiled.API.Features.Player;
using Exiled.Events.EventArgs;
using Exiled.API.Features;
using System.Linq;

namespace SCP008X.Handlers
{
    public class Server
    {
        public System.Random Gen = new System.Random();
        public SCP008X plugin;
        public Server(SCP008X plugin) => this.plugin = plugin;

        public void OnRoundStart()
        {
            if (SCP008X.Instance.Config.CassieAnnounce && SCP008X.Instance.Config.Announcement != null)
            {
                Cassie.DelayedMessage(SCP008X.Instance.Config.Announcement, 5f, false, true);
            }
        }
    }
}