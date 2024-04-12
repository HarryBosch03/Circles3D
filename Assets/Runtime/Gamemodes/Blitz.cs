using Circles3D.Runtime.Damage;
using Circles3D.Runtime.Player;
using Fusion;
using UnityEngine;

namespace Circles3D.Runtime.Gamemodes
{
    public class Blitz : Gamemode
    {
        [Networked]
        public NetworkDictionary<PlayerRef, int> scoreboard => default;

        public override bool CanRespawn(PlayerInstance player) => true;

        protected override void OnEnable()
        {
            base.OnEnable();
            PlayerAvatar.DeathEvent += OnPlayerDead;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            PlayerAvatar.DeathEvent -= OnPlayerDead;
        }

        private void OnPlayerDead(PlayerAvatar avatar, GameObject invoker, DamageArgs args, Vector3 point, Vector3 velocity)
        {
            if (!HasStateAuthority) return;

            var player = GetPlayerFromAvatar(invoker.GetComponentInParent<PlayerAvatar>());
            if (!player.HasValue) return;

            var value = scoreboard.ContainsKey(player.Value) ? scoreboard.Get(player.Value) : 0;
            scoreboard.Set(player.Value, value + 1);
            ScoreChanged();
        }

        private void ScoreChanged()
        {
            var str = "Score Changed:";
            foreach (var pair in scoreboard)
            {
                str += $"\n- {players[pair.Key].name}: {pair.Value}";
            }
            Debug.Log(str);
        }
    }
}