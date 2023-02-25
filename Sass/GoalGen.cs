namespace sass
{
    public class GoalGen {
        private const int Multiplier = 1664525;
        private const int Increment = 1013904223;
        private const int Modulus = int.MaxValue;

        public static float getGoal(float input, long idx) {
            int seed = getSeed(input, idx);
            seed = (Multiplier * seed + Increment) % Modulus;
            System.Console.WriteLine((float) seed / (float) Modulus);
            return MathF.Abs((float)seed / (float)Modulus);
        }

        public static int getSeed(float input, long idx) {
            int hash1 = input.GetHashCode();
            int hash2 = idx.GetHashCode();
            unchecked {
                int hash = 17;
                hash = hash * 31 + hash1;
                hash = hash * 31 + hash2;
                return hash;
            }
        }
    }
}