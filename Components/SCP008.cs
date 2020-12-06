using UnityEngine;
using MEC;
using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.Events.EventArgs;

namespace SCP008X
{
    public class Scp008 : MonoBehaviour
    {
        private Player _ply;
        private float _curAhp = 0;
        private CoroutineHandle _ahp;
        private CoroutineHandle _s008;
        public void Awake()
        {
            _ply = Player.Get(gameObject);
            _ahp = Timing.RunCoroutine(RetainAhp());
            _s008 = Timing.RunCoroutine(Infection());
            Exiled.Events.Handlers.Player.Hurting += WhenHurt;
            Exiled.Events.Handlers.Player.ChangingRole += WhenRoleChange;
        }
        public void OnDestroy()
        {
            Exiled.Events.Handlers.Player.Hurting -= WhenHurt;
            Exiled.Events.Handlers.Player.ChangingRole -= WhenRoleChange;
            _ply = null;
            Timing.KillCoroutines(_ahp);
            Timing.KillCoroutines(_s008);
        }
        public void WhenHurt(HurtingEventArgs ev)
        {
            if (ev.Target != _ply || ev.Target.Role != RoleType.Scp0492)
                return;

            if (_curAhp > 0)
                _curAhp -= ev.Amount;
            else
                _curAhp = 0;
        }
        public void WhenRoleChange(ChangingRoleEventArgs ev)
        {
            if (ev.Player != _ply)
                return;

            switch (ev.Player.Team)
            {
                case Team.SCP:
                    switch (ev.NewRole)
                    {
                        case RoleType.Scp0492:
                            Timing.RunCoroutine(RetainAhp());
                            Log.Debug($"Started coroutine for {_ply.Nickname}: RetainAhp.", SCP008X.Instance.Config.DebugMode);
                            break;
                        case RoleType.Scp096:
                            Timing.KillCoroutines(_ahp);
                            Log.Debug($"Killed coroutine for {_ply.Nickname}: RetainAhp.", SCP008X.Instance.Config.DebugMode);
                            _ply.AdrenalineHealth = 500f;
                            break;
                    }
                    break;
                case Team.TUT:
                    break;
                case Team.RIP:
                    break;
                case Team.MTF:
                    Timing.RunCoroutine(Infection());
                    Timing.KillCoroutines(_ahp);
                    Log.Debug($"Traded coroutines for {_ply.Nickname}: RetainAhp -> Infection.", SCP008X.Instance.Config.DebugMode);
                    break;
                case Team.CDP:
                    Timing.RunCoroutine(Infection());
                    Timing.KillCoroutines(_ahp);
                    Log.Debug($"Traded coroutines for {_ply.Nickname}: RetainAhp -> Infection.", SCP008X.Instance.Config.DebugMode);
                    break;
                case Team.CHI:
                    Timing.RunCoroutine(Infection());
                    Timing.KillCoroutines(_ahp);
                    Log.Debug($"Traded coroutines for {_ply.Nickname}: RetainAhp -> Infection.", SCP008X.Instance.Config.DebugMode);
                    break;
                case Team.RSC:
                    Timing.RunCoroutine(Infection());
                    Timing.KillCoroutines(_ahp);
                    Log.Debug($"Traded coroutines for {_ply.Nickname}: RetainAhp -> Infection.", SCP008X.Instance.Config.DebugMode);
                    break;
            }
        }

        private IEnumerator<float> RetainAhp()
        {
            while(_ply.Role == RoleType.Scp0492)
            {
                if (_ply.AdrenalineHealth <= _curAhp)
                {
                    _ply.AdrenalineHealth = _curAhp;
                }
                else
                {
                    if (_ply.AdrenalineHealth >= SCP008X.Instance.Config.MaxAhp)
                    {
                        _ply.AdrenalineHealth = SCP008X.Instance.Config.MaxAhp;
                    }
                    _curAhp = _ply.AdrenalineHealth;
                }
                yield return Timing.WaitForSeconds(0.05f);
            }
        }
        private IEnumerator<float> Infection()
        {
            for(; ; )
            {
                _ply.Health -= 2;
                if(_ply.Health <= 0)
                {
                    _ply.Hurt(1,_ply);
                    _ply.Health++;
                    break;
                }
                yield return Timing.WaitForSeconds(2f);
            }
        }
    }
}
