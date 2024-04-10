namespace Runtime.Util
{
    public class Shuffler
    {
        private int position;

        public Shuffler(int start)
        {
            position = Shuffle(start + 1);
        }

        public float Next01() => Shuffle01(position = Shuffle(position));
        public float Next(float min, float max) => Next01() * (max - min) + min;

        public static int Shuffle(int position) => (int)(Shuffle01(position) * int.MaxValue);

        public static float Shuffle01(int position)
        {
            var p = (float)position;
            p = p * 0.1031f % 1f;
            p *= p + 33.33f;
            p *= p + p;
            return (p % 1f + 1f) % 1f;
        }
    }
}