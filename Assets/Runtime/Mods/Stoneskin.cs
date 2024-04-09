using Runtime.Stats;
using UnityEngine;

namespace Runtime.Mods
{
    public class Stoneskin : Mod
    {
        public override void Apply(ref StatBoard.Stats stats)
        {
            stats.maxBuffer += Mathf.CeilToInt(stats.maxHealth / 30f);
            stats.maxHealth = 0;
        }
    }
}