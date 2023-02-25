namespace sass
{
    public class Store {
        private string filename;
        private Parser parser;
        public Compiler compiler;
        private long pos {get => this.parser.stream.Position; set => this.advance();}
        private long length {get => this.parser.stream.Length;}
        private Neuron trigger;
        public Neuron[]? before = null;
        public Neuron[]? current = null;

        public Neuron Trigger {get => this.trigger; set => this.compiler.layer(new Neuron[] {value});}

        public Store(string filename) {
            this.filename = filename;
            File.Move(filename, filename + ".tmp");
            this.parser = new Parser(new FileStream(filename + ".tmp", FileMode.Open));
            this.compiler = new Compiler(new FileStream(filename, FileMode.Create));
            this.compiler.file.Write((byte) this.parser.slots);
            this.trigger = this.parser.trigger();
            this.advance();
        }

        public static Parser load(string filename) {
            return new Parser(new FileStream(filename, FileMode.Open));
        }

        public static void init(Neuron[][] network, string filename) =>
            new Compiler(new FileStream(filename, FileMode.Create)).compile(network);

        public void advance() {
            // if (!this.parser.stream.CanRead) return;
            if (this.before is not null) this.compiler.layer(this.before);
            this.before = this.current;
            try {
                this.current = this.parser.layer();
            } catch {
                this.close();
            }
        }

        public void close() {
            if (this.before is not null) this.compiler.layer(this.before);
            this.before = null;
            this.current = null;
            this.compiler.file.Write((byte) 61);
            this.parser.file.Close();
            this.parser.stream.Close();
            this.compiler.file.Close();
            this.compiler.stream.Close();
            File.Delete(this.filename + ".tmp");
        }

        public class Compiler {
            public BinaryWriter file;
            public FileStream stream;

            public Compiler(FileStream stream) {
                this.stream = stream;
                this.file = new BinaryWriter(stream);
            }

            public void compile(Neuron[][] network) {
                file.Write(Convert.ToByte(network[1].Length));
                this.layer(network[0]);
                for (long i = 1; i < network.Length; i++) {
                    this.layer(network[i]);
                } this.file.Close();
                this.stream.Close();
            }

            public void layer(Neuron[] layer) {
                this.neuron(layer[0], 0);
                for (int i = 1; i < layer.Length; i++) {
                    this.neuron(layer[i], i);
                }
            }

            public void neuron(Neuron neuron, long layer) {
                this.file.Write(neuron.value);
                for (int i = 0; i < neuron.weights.Length; i++) {
                    this.file.Write(Convert.ToByte((float) neuron.weights[i].value * 100f));
                }
            }
        }

        public class Parser {
            public BinaryReader file;
            public FileStream stream;
            public byte current;
            public byte slots;

            public Parser(FileStream stream) {
                this.stream = stream;
                this.file = new BinaryReader(stream);
                this.slots = file.ReadByte();
            }

            public Neuron[] layer() {
                List<Neuron> neurons = new List<Neuron>();
                for (int i = 0; i < slots; i++) {neurons.Add(this.neuron()); this.current = file.ReadByte();}
                return neurons.ToArray();
            }

            public Neuron trigger() {
                this.current = file.ReadByte();
                Neuron res = neuron();
                this.current = file.ReadByte();
                return res;
            }

            public Neuron neuron() {
                byte data = this.current;
                List<Neuron.Weight> weights = new List<Neuron.Weight>();
                for (int i = 0; i < slots; i++) {
                    this.current = this.file.ReadByte();
                    weights.Add(new Neuron.Weight(current / 100f));
                } return new Neuron(data, weights.ToArray());
            }
        }
    }
}