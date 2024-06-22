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

        for (int i = 0; i < keyLength; i++)
        {
            
        }

        return possibleKeys.ToArray();
    }

    private string[] Finder(string inword) //Vissza ad egy tömböt az összes lehetséges szóval
    {
        List<string> output = new();
        foreach(string word in words)
        {
            if (word.Contains(inword))
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