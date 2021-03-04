using System.Collections.Generic;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using MEC;
using UnityEngine;

namespace SCP008X
{
    public class RetainAhp : MonoBehaviour
    {
        private Player _ply;
        private float _currentAhp;
        private CoroutineHandle _keepAhp;
        
        public void Awake()
        {
            _ply = Player.Get(gameObject);
            _keepAhp = Timing.RunCoroutine(KeepAhp());
            Exiled.Events.Handlers.Player.Hurting += WhenHurt;
            Exiled.Events.Handlers.Player.ChangingRole += WhenChange;
        }
        public void OnDestroy()
        {
            _ply = null;
            Timing.KillCoroutines(_keepAhp);
            Exiled.Events.Handlers.Player.Hurting -= WhenHurt;
            Exiled.Events.Handlers.Player.ChangingRole -= WhenChange;
        }
        public void WhenHurt(HurtingEventArgs e)
        {
            if (e.Target != _ply)
                return;

            if (_currentAhp > 0)
                _currentAhp -= e.Amount;
            else
                _currentAhp = 0;
        }

        public void WhenChange(ChangingRoleEventArgs e)
        {
            if (e.Player != _ply)
                return;
    
            if (e.NewRole.GetTeam() == Team.SCP)
            {
                Timing.KillCoroutines(_keepAhp);
                _ply.ArtificialHealth = e.NewRole == RoleType.Scp096 ? 500f : 0f;
            }
            else
                Timing.RunCoroutine(KeepAhp());
        }
        private IEnumerator<float> KeepAhp()
        {
            for(; ; )
            {
                if (_ply.ArtificialHealth <= _currentAhp)
                    _ply.ArtificialHealth = _currentAhp;
                else
                {
                    if (_ply.ArtificialHealth >= Scp008X.Instance.Config.MaxAhp)
                    {
                        _ply.ArtificialHealth = Scp008X.Instance.Config.MaxAhp;
                    }
                    _currentAhp = _ply.ArtificialHealth;
                }

                yield return Timing.WaitForSeconds(0.05f);
            }
        }
    }
}