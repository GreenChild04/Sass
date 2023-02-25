using System.Security.Cryptography;

namespace sass
{
    public class Disk {
        public Dictionary<long, Nullable<int>> sprinkleTaken;
        string network;

        public Disk(string network) {
            this.network = network;
            this.sprinkleTaken = new Dictionary<long, Nullable<int>>();
        }

        public static void init(ulong max, int slots, string filename) {
            Store.Compiler compiler = new Store.Compiler(new FileStream(filename, FileMode.Create));

            compiler.file.Write(Convert.ToByte(slots));
            compiler.layer(new Neuron[] {new Neuron(slots).fillWeights()});
            for (ulong a = 0; a < max; a++) {
                Neuron[] layer = new Neuron[slots];
                for (int i = 0; i < layer.Length; i++) {
                    layer[i] = new Neuron(slots);
                    layer[i].fillWeights();
                } compiler.layer(layer);
            } compiler.file.Write((byte) 61);
            compiler.file.Close();
            compiler.stream.Close();
        }

        public static void fire(float input, FileStream stream, string filename) {
            BinaryWriter file = new BinaryWriter(stream);
            Store.Parser parser = Store.load(filename);
            Neuron.Weight[] weights = parser.trigger().weights;
            File.WriteAllText("fire.input", null);
            for (long i = 1;; i++) {
                byte data;
                try {
                    File.AppendAllText("fire.input", input.ToString());
                    (input, data, int tmp) = layer(parser.layer(), i, input, ref weights);
                } catch {break;}
                file.Write(data);
            } file.Close();
            stream.Close();
        }

        public static void train(string network, string filename, int input) {
            if (verify(network, filename, input)) return;
        }

        public static bool verify(string network, string filename, int input) {
            string outfile = filename + ".verify.tmp";
            fire(input, new FileStream(outfile, FileMode.Create), network);
            bool output = areFilesEqual(filename, outfile);
            File.Delete(outfile);
            return output;
        }

        static bool areFilesEqual(string file1, string file2) {
            const int blockSize = 4096; // 4K Block Size
            byte[] buffer1 = new byte[blockSize];
            byte[] buffer2 = new byte[blockSize];

            FileStream first = new FileStream(file1, FileMode.Open);
            FileStream second = new FileStream(file2, FileMode.Open);

            if (first.Length != second.Length) return false;

            int read1, read2;

            do {
                read1 = first.Read(buffer1, 0, blockSize);
                read2 = first.Read(buffer2, 0, blockSize);

                if (read1 != read2) return false;

                for (int i = 0; i < read1; i++)
                if (buffer1[i] != buffer2[i]) return false;
            } while (read1 > 0);

            first.Close();
            second.Close();

            return true;
        }

        public void sprinkle(string filename) {
            FileStream file = new FileStream(filename, FileMode.Open);
            Store store = new Store(this.network);
            store.Trigger = store.Trigger;
            byte data;

            bool last = false;
            for (long l = 0; store.current is not null && l < file.Length; l++) {
                if (last) break;
                if (store.current is null) last = true;
                data = (byte) file.ReadByte();
                bool found = false;
                int slots = store.current.Length;

                if (!this.sprinkleTaken.ContainsKey(l)) this.sprinkleTaken[l] = null;
                for (int i = 0; i < store.current.Length; i++) 
                    if (store.current[i].value.Equals(data)) found = true;
                if (!found) {
                    int random = RandomNumberGenerator.GetInt32(slots);
                    while (this.sprinkleTaken[l] == random) random = RandomNumberGenerator.GetInt32(slots);
                    store.current[random].value = data;
                    this.sprinkleTaken[l] = random;
                } store.advance();
            } while (store.current is not null) store.advance();
            file.Close();
        }

        private static float newWeight(float goal, float input, float weight, float change, float minIncl, float maxIncl, bool max=false) {
            float output = input * weight;
            float res;
            if (max) res = goal;
            else res = output > goal ? output - MathF.Min(output - goal, change): output + MathF.Min(goal - output, change);
            return MathF.Round(Math.Clamp(res / input, minIncl, maxIncl), 2);
        }

        public static void _train(string network, string filename, float input) {
            FileStream file = new FileStream(filename, FileMode.Open);
            Store store = new Store(network);
            store.advance();

            Neuron tran(byte data, long idx, Neuron before, Neuron[] current, ref int desired) {
                int getSlot() {
                    float[] results = new float[before.weights.Length];
                    for (int i = 0; i < before.weights.Length; i++) results[i] = input * before.weights[i].value;
                    float max = results.MinBy(x => MathF.Abs((long) x - (input + idx)));
                    return Array.IndexOf(results, max);
                } int slot = getSlot();
                
                float goal = input + idx;
                int rslot = Array.IndexOf(current, current.First(x => x.value == data));
                // for (; current[slot].value != data; slot = RandomNumberGenerator.GetInt32(store.before.Length)) {
                //     before.weights[slot].value = newWeight(goal, input, before.weights[slot].value, 0.1f, 0.1f, 2f);
                // } desired = slot;
                float output = input * before.weights[slot].value;
                for (; slot != rslot; slot = getSlot()) {
                    before.weights[slot].value = newWeight(goal, input, before.weights[slot].value, output / -10f, 0.01f, 2.55f);
                    before.weights[rslot].value = newWeight(goal, input, before.weights[rslot].value, 0f, 0.01f, 2.55f, true);
                } desired = slot;
                return before;
            }

            int desired = 0;
            long idx = 1;
            store.Trigger = tran((byte) file.ReadByte(), idx, store.Trigger, store.before, ref desired);
            File.WriteAllText("train.input", $"{input.ToString()} {store.Trigger.weights[desired].value}\n");
            input *= store.Trigger.weights[desired].value;
            idx++;
            for (long l = 1; store.before is not null && l < file.Length; l++) {
                byte data = (byte) file.ReadByte();
                int oldDesire = desired;
                // System.Console.WriteLine(oldDesire.ToString() + ' ' + store.before.Length.ToString());
                store.before[oldDesire] = tran(data, idx, store.before[oldDesire], store.current, ref desired);
                File.AppendAllText("train.input", $"{input.ToString()} {store.before[oldDesire].weights[desired].value}\n");
                input *= store.before[oldDesire].weights[desired].value;
                store.advance();
                idx++;
            }
        }

        public static (float, byte, int) layer(Neuron[] layer, long idx, float input, ref Neuron.Weight[] weights) {
            float[] results = new float[weights.Length];
            for (int i = 0; i < weights.Length; i++) results[i] = input * weights[i].value;
            // float max = results.MinBy(x => Math.Abs((long) x - idx));
            float max = results.MinBy(x => MathF.Abs((long) x - (input + idx)));
            // float max = results.MinBy(x => Math. Abs((long) x - (Math.Sqrt(input) + idx)));
            Neuron neuron = layer[Array.IndexOf(results, max)];
            File.AppendAllText("fire.input", ' ' + (max / input).ToString() + '\n');
            weights = neuron.weights;
            return (max, neuron.value, Array.IndexOf(results, max));
        }
    }
}