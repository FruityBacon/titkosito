using System;
using titkosito;

namespace titkosito;
class Program
{
    static void Main(string[] args)
    {
        
        if (args.Length == 0)
        {
            Console.WriteLine("Nem volt megadva művelet.");
            Console.WriteLine("Segítségért használja a -h müveletet!");
            return;
        }
        Secret titkosito = new();
        try
        {
            if (args.Length != 3 && args[0] != "-h")
            {
                throw new Exception("Túl "+((args.Length > 3)? "sok" : "kevés")+" érték volt megadva!");
            }

            switch (args[0])
            {
                case "-e" : 
                    // Encryption, avagy titkosítás. Sikeres titkosítás esetén kettő értéket ad vissza üzenetet és kulcsot
                    (string message,string key) eredmeny = titkosito.Encrypt(args[1],args[2]);
                    Console.WriteLine("{0}\n{1}",eredmeny.message,eredmeny.key);
                    break;

                case "-d" :
                    // Decryption, avagy visszafejtés. Sikeres visszafejtés esetén egy értéket ad vissza, ami az eredeti üzenet
                    
                    Console.WriteLine("{0}",titkosito.Decrypt(args[1],args[2]));
                    break;
                case "-h" :
                    Console.WriteLine(" _   _ _   _             _ _     \n| |_(_) |_| | _____  ___(_) |_ ___  \n| __| | __| |/ / _ \\/ __| | __/ _ \\ \n| |_| | |_|   < (_) \\__ \\ | || (_) |\n \\__|_|\\__|_|\\_\\___/|___/_|\\__\\___/ ");
                    Console.WriteLine(titkosito.Help());
                    return;
                default:
                    throw new Exception("Kért művelet nem létezik: "+args[0]);
            }
        }
        catch (System.Exception e)
        {
            Console.WriteLine("Hiba: {0}",e.Message);
            return;
        }
        
    }
}
