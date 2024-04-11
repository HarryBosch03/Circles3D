using Runtime.Damage;
using Runtime.Stats;
using UnityEngine;

namespace Runtime.Mods
{
    public class Stoneskin : Mod
    {
        public override void Apply(ref StatBoard.Stats stats)
        {
            stats.maxBuffer += 4;
            stats.maxHealth -= 100;
        }
    }
}