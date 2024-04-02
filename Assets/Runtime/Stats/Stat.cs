using UnityEngine;

namespace Runtime.Stats
{
    [System.Serializable]
    public class Stat
    {
        public float baseValue;
        public float value;
        
        public Stat(float baseValue)
        {
            this.baseValue = baseValue;
        }

        public void Reset()
        {
            value = baseValue;
        }
        
        public int AsInt() => Mathf.RoundToInt(value);
        public int AsIntMax(int max) => Mathf.Max(max, Mathf.RoundToInt(value));

        public override string ToString() => value.ToString();
        
        public static implicit operator float(Stat stat) => stat.value;
        public static implicit operator Stat(float value) => new(value);
    }
}