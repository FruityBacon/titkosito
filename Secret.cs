using System;
using System.Collections.Generic;

namespace titkosito;
public class Secret
{
    private Dictionary<char,int> charvals = new Dictionary<char, int>() {
        {'a',0},
        {'b',1},
        {'c',2},
        {'d',3},
        {'e',4},
        {'f',5},
        {'g',6},
        {'h',7},
        {'i',8},
        {'j',9},
        {'k',10},
        {'l',11},
        {'m',12},
        {'n',13},
        {'o',14},
        {'p',15},
        {'q',16},
        {'r',17},
        {'s',18},
        {'t',19},
        {'u',20},
        {'v',21},
        {'w',22},
        {'x',23},
        {'y',24},
        {'z',25},
        {' ',26},
    };
    public string Encrypt(string message, string key)
    {
        if (message.Length > key.Length)
            throw new Exception("Kulcs nem lehet rövidebb mint az üzenet!"); 

        

        return "blah";
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