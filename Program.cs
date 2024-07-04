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
        
        try
        {
            int exceptedInputCount = 3;
            if (args.Length != exceptedInputCount && args[0] != "-h" && args[0] != "-c")
            {
                throw new Exception("Túl "+((args.Length > exceptedInputCount)? "sok" : "kevés")+" érték volt megadva!");
            }

            Secret titkosito = new();

            switch (args[0]) // művelet megadása kötelező
            {
                case "-c" :
                    // Crack, avagy feltörés. Kettő titkosított üzenet megadásával lehetséges lehet vissza kapni az eredeti közös kulcsot.
                    
                    StreamReader sr =  (args.Length == 4 && args[3] != "teszt")? new(args[3]) : new("words.txt");
                    List<string> wordList = new();
                    while(!sr.EndOfStream)
                    {
                        string? line = sr.ReadLine();
                        if (line != null)
                        {
                            wordList.Add(line);
                        }
                    }
                    sr.Close();
                    Cracker cracker = new(args[1],args[2],wordList);
                    if(args.Length == 4 && args[3] == "teszt")
                    {
                        cracker.Test();
                    }
                    else
                    {
                        cracker.Start();
                    }
                    for (int i = 0; i < cracker.PossibleKeys.Length; i++)
                        Console.WriteLine(cracker.PossibleKeys[i]);
                    break;

                case "-e" : 
                    // Encryption, avagy titkosítás. Sikeres titkosítás esetén kettő értéket ad vissza üzenetet és kulcsot

                    Console.WriteLine(titkosito.Encrypt(args[1],args[2]));
                    break;

                case "-d" :
                    // Decryption, avagy visszafejtés. Sikeres visszafejtés esetén egy értéket ad vissza, ami az eredeti üzenet
                    
                    Console.WriteLine(titkosito.Decrypt(args[1],args[2]));
                    break;

                case "-h" :
                    // Segítés kiíratása. Először egy kis ascii art utána pedig a leírás
                    Console.WriteLine(" _   _ _   _             _ _     \n| |_(_) |_| | _____  ___(_) |_ ___  \n| __| | __| |/ / _ \\/ __| | __/ _ \\ \n| |_| | |_|   < (_) \\__ \\ | || (_) |\n \\__|_|\\__|_|\\_\\___/|___/_|\\__\\___/ ");
                    Console.WriteLine(Help());
                    return;

                default:
                    // Hogyha nem találja a megadott müveletet akkor dob Exception-t.
                    throw new Exception("Kért művelet nem létezik: "+args[0]);
            }
        }
        catch (System.Exception e)
        {
            Console.WriteLine("Hiba: {0}",e);
            return;
        }
        
    }
    public static string Help()
    {
        return
        "Készítette Kiss Máté\n\nLeírás:\n\tAz angol abc betűivel írt üzenetek titkosítására és visszafejtésére alkalmas program.\n\nMűveletek:\n\t-c [TITKOS ÜZENET] [TITKOS ÜZENET]\n\t-e [ÜZENET] [KULCS]\n\t-d [TITKOSÍTOTT ÜZENET] [KULCS]\n\t-h\n\nMagyarázat:\n\t-c \tFeltörés, adjon meg kettő titkosított üzenetet, és vissza adja a lehetséges közös kulcsukat.\n\t-e \tTitkosítás, adja meg a titkosítani kívánt üznetet és hozzá a kulcsot.\n\t-d\tVisszafejtés, adja meg a titkosított üzenetet és utána a kulcsot.\n\t-h\tSegítség előhívása.";
    }
}
