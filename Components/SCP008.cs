using UnityEngine;
using MEC;
using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.Events.EventArgs;

namespace SCP008X.Components
{
    public class SCP008 : MonoBehaviour
    {
        private Player ply;
        private float curAHP = 0;
        CoroutineHandle ahp;
        CoroutineHandle s008;
        public void Awake()
        {
            ply = Player.Get(gameObject);
            ahp = Timing.RunCoroutine(RetainAHP());
            s008 = Timing.RunCoroutine(Infection());
            Exiled.Events.Handlers.Player.Hurting += WhenHurt;
            Exiled.Events.Handlers.Player.ChangingRole += WhenRoleChange;
        }
        public void OnDestroy()
        {
            Exiled.Events.Handlers.Player.Hurting -= WhenHurt;
            Exiled.Events.Handlers.Player.ChangingRole -= WhenRoleChange;
            ply = null;
            Timing.KillCoroutines(ahp);
            Timing.KillCoroutines(s008);
        }
        public void WhenHurt(HurtingEventArgs ev)
        {
            if (ev.Target != ply || ev.Target.Role != RoleType.Scp0492)
                return;

            if (curAHP > 0)
                curAHP -= ev.Amount;
            else
                curAHP = 0;
        }
        public void WhenRoleChange(ChangingRoleEventArgs ev)
        {
            if (ev.Player != ply)
                return;

            switch (ev.Player.Team)
            {
                case Team.SCP:
                    switch (ev.NewRole)
                    {
                        case RoleType.Scp0492:
                            Timing.RunCoroutine(RetainAHP());
                            Log.Debug($"Started coroutine for {ply.Nickname}: RetainAHP.", SCP008X.Instance.Config.DebugMode);
                            break;
                        case RoleType.Scp096:
                            Timing.KillCoroutines(ahp);
                            Log.Debug($"Killed coroutine for {ply.Nickname}: RetainAHP.", SCP008X.Instance.Config.DebugMode);
                            ply.AdrenalineHealth = 500f;
                            break;
                    }
                    break;
                case Team.TUT:
                    break;
                case Team.RIP:
                    break;
                case Team.MTF:
                    Timing.RunCoroutine(Infection());
                    Timing.KillCoroutines(ahp);
                    Log.Debug($"Traded coroutines for {ply.Nickname}: RetainAHP -> Infection.", SCP008X.Instance.Config.DebugMode);
                    break;
                case Team.CDP:
                    Timing.RunCoroutine(Infection());
                    Timing.KillCoroutines(ahp);
                    Log.Debug($"Traded coroutines for {ply.Nickname}: RetainAHP -> Infection.", SCP008X.Instance.Config.DebugMode);
                    break;
                case Team.CHI:
                    Timing.RunCoroutine(Infection());
                    Timing.KillCoroutines(ahp);
                    Log.Debug($"Traded coroutines for {ply.Nickname}: RetainAHP -> Infection.", SCP008X.Instance.Config.DebugMode);
                    break;
                case Team.RSC:
                    Timing.RunCoroutine(Infection());
                    Timing.KillCoroutines(ahp);
                    Log.Debug($"Traded coroutines for {ply.Nickname}: RetainAHP -> Infection.", SCP008X.Instance.Config.DebugMode);
                    break;
            }
        }

        public IEnumerator<float> RetainAHP()
        {
            for(; ; )
            {
                if(ply.Role == RoleType.Scp0492)
                {
                    if (ply.AdrenalineHealth <= curAHP)
                    {
                        ply.AdrenalineHealth = curAHP;
                    }
                    else
                    {
                        if (ply.AdrenalineHealth >= SCP008X.Instance.Config.MaxAhp)
                        {
                            ply.AdrenalineHealth = SCP008X.Instance.Config.MaxAhp;
                        }
                        curAHP = ply.AdrenalineHealth;
                    }
                }

                yield return Timing.WaitForSeconds(0.05f);
            }
        }
        public IEnumerator<float> Infection()
        {
            for(; ; )
            {
                ply.Health -= 2;
                if(ply.Health <= 0)
                {
                    ply.Hurt(1,ply);
                    ply.Health++;
                    break;
                }

                yield return Timing.WaitForSeconds(2f);
            }
        }
    }
}
