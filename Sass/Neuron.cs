using System.Security.Cryptography;

namespace sass
{
    public class Neuron {
        // public byte value {get {return Convert.FromBase64String(this.rValue + "==")[0];} set {this.rValue = valueFromByte(value).Substring(0, 2);}}
        // public string rValue;
        public byte value;
        public Weight[] weights;

        public Neuron(int slots) {
            this.value = RandomNumberGenerator.GetBytes(1)[0];
            this.weights = new Weight[slots];
        }

        public Neuron(byte value, Weight[] weights) {
            this.value = value;
            this.weights = weights;
        }

        public Neuron fillWeights() {
            for (int i = 0; i < weights.Length; i++) {
                this.weights[i] = new Weight();
            } return this;
        }

        public static string valueFromByte(byte value) =>
            Convert.ToBase64String(new byte[] {value});

        public Weight weightFromOther(int index) {
            if (this.weights.Length < index)
            return this.weights[index];
            return null;
        }

        private static T[] lmap<T>(object[] list, Func<object, T> func) {
            T[] nlist = new T[list.Length];
            for (int i = 0; i < list.Length; i++) nlist[i] = func(list[i]);
            return nlist;
        }

        private static object[] amap<T>(T[] list, Func<T, object> func) {
            object[] nlist = new object[list.Length];
            for (int i = 0; i < list.Length; i++) nlist[i] = func(list[i]);
            return nlist;
        }

        public class Weight {
            public float value {get; set;}
            public Weight(float value) => this.value = value;
            public Weight() {
                this.value = RandomNumberGenerator.GetInt32(1, 256) / 100f;
            }
        }
    }
}