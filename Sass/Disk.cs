using System.Security.Cryptography;

namespace sass
{
    public class Disk {
        public Neuron[][] network;
        public Nullable<int>[] taken;

        public Disk(ulong max, int slots) {
            this.taken = new Nullable<int>[max + 1];
            this.network = new Neuron[max + 1][];
            this.fill(slots);
            this.network[0] = new Neuron[1];
            this.network[0][0] = new Neuron(slots);
            this.network[0][0].fillWeights();
        }

        public byte[] fire(float input, ref int[] path) {
            Neuron.Weight[] weights = this.network[0][0].weights;
            byte[] data = new byte[this.network.Length - 1];
            for (long i = 0; i < this.network.Length -1; i++) {
                (input, data[i], path[i]) = layer(this.network[i + 1], i, input, ref weights);
            } return data;
        }

        public byte[] fire(float input) {
            Neuron.Weight[] weights = this.network[0][0].weights;
            byte[] data = new byte[this.network.Length - 1];
            for (long i = 0; i < this.network.Length -1; i++) {
                int tmp;
                (input, data[i], tmp) = layer(this.network[i + 1], i, input, ref weights);
            } return data;
        }

        public void sprinkle(FileStream file) {
            long length = file.Length;
            for (long l = 1; l - 1 < length && l < this.network.Length; l++) {
                byte current = (byte) file.ReadByte();
                bool found = false;
                for (int i = 0; i < this.network[l].Length; i++)
                    if (this.network[l][i].value.Equals(current)) found = true;
                if (!found) {
                    int random = RandomNumberGenerator.GetInt32(this.network[l].Length);
                    while (this.taken[l] == random) random = RandomNumberGenerator.GetInt32(this.network[l].Length);
                    this.network[l][random].value = current;
                    this.taken[l] = random;
                }
            }
        }

        public static (float, byte, int) layer(Neuron[] layer, long idx, float input, ref Neuron.Weight[] weights) {
            float[] results = new float[weights.Length];
            for (int i = 0; i < weights.Length; i++) results[i] = input * weights[i].value;
            float max = results.MinBy(x => Math. Abs((long) x - (Math.Sqrt(input) + idx)));
            Neuron neuron = layer[Array.IndexOf(results, max)];
            weights = neuron.weights;
            return (max, neuron.value, Array.IndexOf(results, max));
        }

        public static byte test(object stmt) => 0;

        public void fill(int slots) {
            for (int i = this.network.Length - 1; i > 0; i--) {
                this.network[i] = new Neuron[slots];
                for (int a = 0; a < slots; a++) {
                    this.network[i][a] = new Neuron(slots);
                    this.network[i][a].fillWeights();
                }
            }
        }
    }
}