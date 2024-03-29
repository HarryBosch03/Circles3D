using Runtime.Stats;
using UnityEngine;

namespace Runtime.Mods.GunMods
{
    public abstract class Mod : MonoBehaviour
    {
        public abstract void Apply(StatBoard statBoard);
    }

    public abstract class Mod<T> : Mod where T : StatBoard
    {
        private T stats;

        protected virtual void Awake() { stats = GetComponentInParent<T>(); }

        protected virtual void OnEnable() { stats.mods.Add(this); }

        protected virtual void OnDisable() { stats.mods.Remove(this); }

        public sealed override void Apply(StatBoard statBoard) { OnApply((T)statBoard); }

        public abstract void OnApply(T statBoard);
    }
}