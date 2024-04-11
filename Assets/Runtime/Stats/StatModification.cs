using System;

namespace Circles3D.Runtime.Stats
{
    public class StatModification<T>
    {
        public Func<T, float, float> callback;
        public StatModification(Func<T, float, float> callback) { this.callback = callback; }
    }
}