using System.Diagnostics;

namespace sass
{
    public class Program {
        public static void Main(string[] args) {
            System.Console.WriteLine("Program Started...");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            File.Delete("Empty2.sas");
            Disk.init(121, 10, "Empty.sas");
            Disk disk = new Disk("Empty.sas");
            disk.sprinkle("OG.txt");
            // disk.sprinkle("OG2.txt");
            File.Copy("Empty.sas", "Empty2.sas");
            Disk._train("Empty.sas", "OG.txt", 123);
            // Disk._train("Empty.sas", "OG2.txt", 302);
            // System.Console.WriteLine(Disk.verify("Empty.sas", "OG.txt", 123));
            Disk.fire(123, new FileStream("Empty.txt", FileMode.Create), "Empty.sas");
            Disk.fire(303, new FileStream("Empty2.txt", FileMode.Create), "Empty.sas");
            // Disk.fire(150, new FileStream("Empty2.txt", FileMode.Create), "Empty.sas");

            stopwatch.Stop();
            System.Console.WriteLine($"Program Ended at {stopwatch.Elapsed}");
        }
    }
}