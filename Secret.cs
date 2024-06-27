using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace titkosito;
public class Secret
{
    private readonly List<char> charvals = new List<char>(){
        'a',
        'b',
        'c',
        'd',
        'e',
        'f',
        'g',
        'h',
        'i',
        'j',
        'k',
        'l',
        'm',
        'n',
        'o',
        'p',
        'q',
        'r',
        's',
        't',
        'u',
        'v',
        'w',
        'x',
        'y',
        'z',
        ' '
    };

    private List<string> words = new();

    public string Encrypt(string message, string key)
    { 
        if (message.Length > key.Length)
            throw new Exception("Kulcs nem lehet rövidebb mint az üzenet!"); 

        string output = "";

        for (int i = 0; i < message.Length; i++)
        {
            int add = charvals.IndexOf(message[i]) + charvals.IndexOf(key[i]);
            if (add > 26)
                add %= 27;

            output += charvals[add];
        }

        return output;
    }

    public string Decrypt(string message, string key)
    {
        if (message.Length > key.Length)
            throw new Exception("Kulcs nem lehet rövidebb mint az üzenet!"); 

        string output = "";

        for (int i = 0; i < message.Length; i++)
        {
            int add = charvals.IndexOf(message[i]) - charvals.IndexOf(key[i]);
            if (add < 0)
                add = 27 - Math.Abs(add);
            
            output += charvals[add];
        }

        return output;
    }

    public string[] Crack(string msg1, string msg2)
    {
        try
        {
            StreamReader sr = new("words.txt");
            while(!sr.EndOfStream)
            {
                words.Add(sr.ReadLine());
            }
            words.Sort();
            sr.Close();
        }
        catch (System.Exception)
        {
            throw new Exception("Nem talált szó gyűjteményt.");
        }

        List<string> possibleKeys = new();
        int keyLength = msg1.Length;

        if (msg1.Length > msg2.Length)
            keyLength = msg2.Length;

        
        int curWord = 0;
        while (curWord < words.Count) //ha túl lépi a kulcs maximum lehetséges méretét, vagy a szavak lista végére ért akkor kilép.
        {
            int curChar = 0;
            
            string keySegment = Decrypt(
                msg1.Substring(curChar,words[curWord].Length),
                words[curWord]
            ); //fogja az első mondatot és az épp ellenőrzött kezdő szót és vissza kér belőle egy kulcsot

            /*
            string[] pWords = Finder(
                Decrypt(
                    msg1.Substring(curChar,words[curWord].Length),
                    keySegment
                    )
                ); //pWords == Possible Words. Minden szó ami tartalmaz legalább egy részét a dekódolt szövegnek.
            
            Console.WriteLine("{0} : {1}",keySegment,curWord);
            if (pWords.Length < 0)
            {
                for (int i = 0; i < pWords.Length; i++)
                {
                    Console.WriteLine(pWords[i]);
                }
                return possibleKeys.ToArray();
            }
            else
                curWord++;

            */

            string[] pWords = Finder(
                Decrypt(
                    msg2.Substring(curChar,words[curWord].Length),
                    keySegment
                ).Split(' ')[0]
            );  //fogja a második mondatot és a korábban kapott kulcsunkat felhasználva vissza kérünk minden lehetséges kezdő szót.
                //Kell a split a "Finder" függvény működése miatt. 


            if (pWords.Length > 0) //ha talált kezdő szavakat akkor folytatja
            {
                possibleKeys.Add(keySegment); //kulcs szegmenst hozzá adjuk a listához

                Console.WriteLine("{0}\t:\t{1}",keySegment,curWord+1);

                curChar = Decrypt(
                    msg2.Substring(curChar,words[curWord].Length),
                    keySegment
                ).Split(' ')[0].Length+1;

                while (curChar <= keyLength) //fut amíg a lehetséges kulcs méret végére nem ér
                {
                    string[] messages = new[]{msg1,msg2};
                    string curKey = possibleKeys[^1];
                    string lastword = words[curWord];
                    int lastChar = curChar;


                    for(int i = 0; i < pWords.Length; i++) //minden lehetséges szón végig megy
                    {
                        if (pWords[i].Length < lastword.Length) //hogyha hosszabb a lehetséges 
                        {
                            for (int j = 0; j < pWords[i].Split(' ').Length; j++) //minden egyéb létező szót leelenőriz
                            {
                                
                            }
                        }
                        Console.WriteLine(pWords[i]);
                    }


                    if (lastChar == curChar)
                    {
                        possibleKeys.RemoveAt(possibleKeys.Count-1); //nem történt változás de nem érte el a mondat végét úgyhogy eldobjuk a lehetséges kulcsot és kilépünk
                        break;
                    }
                    else
                    {
                        possibleKeys[^1] = curKey; //a legutolsó kulcs
                    }
                    
                }
            }
            
            
            curWord++; //tovább lép a következő kezdő szóra

        }

        Console.WriteLine("Possible Keys - {0}",possibleKeys.Count);
        return possibleKeys.ToArray();
    }

    private string[] Finder(string inword) //Vissza ad egy tömböt az összes lehetséges szóval
    {
        List<string> output = new();
        foreach(string word in words)
        {
            if (word.StartsWith(inword))
                output.Add(word);
        }
        return output.ToArray();
    }

    public string Help()
    {
        return 
        "Készítette Kiss Máté\n\nLeírás:\n\tAz angol abc betűivel írt üzenetek titkosítására és visszafejtésére alkalmas program.\n\nMűveletek:\n\t-c [TITKOS ÜZENET] [TITKOS ÜZENET]\n\t-e [ÜZENET] [KULCS]\n\t-d [TITKOSÍTOTT ÜZENET] [KULCS]\n\t-h\n\nMagyarázat:\n\t-c \tFeltörés, adjon meg kettő titkosított üzenetet, és vissza adja a lehetséges közös kulcsukat.\n\t-e \tTitkosítás, adja meg a titkosítani kívánt üznetet és hozzá a kulcsot.\n\t-d\tVisszafejtés, adja meg a titkosított üzenetet és utána a kulcsot.\n\t-h\tSegítség előhívása.";
    }
}