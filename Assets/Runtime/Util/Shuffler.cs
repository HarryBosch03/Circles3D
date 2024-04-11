namespace Runtime.Util
{
    public class Shuffler
    {
        private int position;

        public Shuffler(int start) { position = Shuffle(start + 1); }

        public float Next01() => Shuffle01(position = Shuffle(position));
        public float Next(float min, float max) => Next01() * (max - min) + min;

        public static int Shuffle(int n)
        {
            n = (n << 13) ^ n;
            return n * (n * n * 15731 + 789221) + 1376312589;
        }

        public static float Shuffle01(int position)
        {
            var r = Shuffle(position);
            return (float)(r & 0x0fffffff) / 0x0fffffff;
        }
    }
}