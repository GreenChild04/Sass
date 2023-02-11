namespace sass
{
    public class Store {
        private Compiler compiler;
        private string filename;

        public Store(Neuron[][] network, string filename) {
            this.filename = filename;
            this.compiler = new Compiler(new StreamWriter(filename));
            this.compiler.compile(network);
        }


        static class Symbols {
            public static char com = ',';
            public static char space = ' ';
            public static char line = '\n';
        }

        class Compiler {
            public StreamWriter file;
            public Compiler(StreamWriter file) {
                this.file = file;
            }

            public void compile(Neuron[][] network) {
                this.layer(network, 0);
                for (long i = 1; i < network.Length; i++) {
                    this.file.Write(Symbols.line);
                    this.layer(network, i);
                } this.file.Close();
            }

            public void layer(Neuron[][] network, long idx) {
                this.neuron(network[idx][0], 0);
                for (int i = 1; i < network[idx].Length; i++) {
                    this.file.Write(Symbols.space);
                    this.neuron(network[idx][i], i);
                }
            }

            public void neuron(Neuron neuron, long layer) {
                this.file.Write(System.Text.Encoding.UTF8.GetChars(new byte[] {neuron.value})[0]);
                for (int i = 0; i < neuron.weights.Length; i++) {
                    this.file.Write(Symbols.com);
                    // this.file.Write(neuron.weights[i].value); // Readable weights
                    this.file.Write(System.Text.Encoding.UTF8.GetChars(new byte[] {(byte) (neuron.weights[i].value * 10)})[0]); // Byte weights
                }
            }
        }
    }
}