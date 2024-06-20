using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace titkosito;
public class Secret
{
    private List<char> charvals = new List<char>(){
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

    public string Decrypt(string smessage, string key)
    {
        return "blabla";
    }

    public string Help()
    {
        return 
        "Készítette Kiss Máté\n\nLeírás:\n\t\nMűveletek:\n\t-e [ÜZENET] [KULCS]\n\t-d [TITKOSÍTOTT ÜZENET] [KULCS]\n\t-h\n\nMagyarázat:\n\t-e \tTitkosítás, adja meg a titkosítani kívánt üznetet és hozzá a kulcsot.\n\t-d\tVisszafjetés, adja meg a titkosított üzenetet és utána a kulcsot.\n\t-h\tSegítség előhívása.";
    }
}