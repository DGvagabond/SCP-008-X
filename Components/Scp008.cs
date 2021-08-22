using UnityEngine;
using MEC;
using System.Collections.Generic;
using Exiled.API.Features;

namespace SCP008X
{
    public class Scp008 : MonoBehaviour
    {
        private Player _ply;
        private CoroutineHandle _s008;
        public void Awake()
        {
            _ply = Player.Get(gameObject);
            _s008 = Timing.RunCoroutine(Infection());
        }
        public void OnDestroy()
        {
            _ply = null;
            Timing.KillCoroutines(_s008);
        }
        private IEnumerator<float> Infection()
        {
            for(; ; )
            {
                _ply.Health -= 1;
                if(_ply.Health <= 0)
                {
                    _ply.Hurt(1,_ply);
                    _ply.Health++;
                    break;
                }
                yield return Timing.WaitForSeconds(0.85f);
            }
        }
    }
}
