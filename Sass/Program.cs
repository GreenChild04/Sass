using System.Text.Json;
using Newtonsoft.Json;

namespace sass
{
    public class Program {
        public static void Main(string[] args) {
            Disk disk = new Disk(33, 2);
            disk.sprinkle(new FileStream("OG.txt", FileMode.Open));
            disk.sprinkle(new FileStream("OG2.txt", FileMode.Open));
            new Store(disk.network, "Empty.sas");
            File.WriteAllBytes("Empty.txt", disk.fire(123));
        }
    }
}