using System;
using System.IO;
using System.Text;

class Program {
    static void Main() {
        string line;
        while ((line = Console.ReadLine()) != null) {
            Console.WriteLine(line.ToUpperInvariant());
        }
    }
}