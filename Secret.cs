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

    private string message1 = "";
    private string message2 = "";
    private bool curmsg;
    private string CurrentMessage {
        get => curmsg? message1 : message2;
        set {
            if (curmsg)
            {
                message1 = value;
            }
            else
            {
                message2 = value;
            }
        }
    }
    private string OppositeMessage {
        get => !curmsg? message1 : message2;
        set {
            if (!curmsg)
            {
                message1 = value;
            }
            else
            {
                message2 = value;
            }
        }
    }
    private int message1CurrentCharacter = 0;
    private int message2CurrentCharacter = 0;
    private int messageGiveCharacter(bool select = true) // Ha igaz akkor vissza adja az aktív charactert, ha hamis akkor az ellenkezőjét
    {
       if (select)
       {
            return curmsg? message1CurrentCharacter : message2CurrentCharacter;
       }
       else
       {
            return curmsg? message2CurrentCharacter : message1CurrentCharacter;
       }
    }
    private int maxKeyLength {
        get => (message1.Length < message2.Length)? message1.Length : message2.Length;
    }
    private List<string> PossibleKeys = new();
    private string LastKey
    {
        get => PossibleKeys[^1];
        set => PossibleKeys[^1] = value;
    }

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
            while (!sr.EndOfStream)
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
        int maxKeyLength = (msg1.Length > msg2.Length)? msg1.Length : msg2.Length;

        this.message1 = msg1;
        this.message2 = msg2;
        
        #region ElsőPróba
        /*
        while (curWord < words.Count) //ha túl lépi a kulcs maximum lehetséges méretét, vagy a szavak lista végére ért akkor kilép.
        {
            int curChar = 0;

            string keySegment = Decrypt(
                msg1.Substring(curChar, words[curWord].Length),
                words[curWord]
            ); //fogja az első mondatot és az épp ellenőrzött kezdő szót és vissza kér belőle egy kulcsot

            messageCharPos[0] = keySegment.Length + 1;
            /*
            string[] possibleWords = Finder(
                Decrypt(
                    msg1.Substring(curChar,words[curWord].Length),
                    keySegment
                    )
                ); //possibleWords == Possible Words. Minden szó ami tartalmaz legalább egy részét a dekódolt szövegnek.
            
            Console.WriteLine("{0} : {1}",keySegment,curWord);
            if (possibleWords.Length < 0)
            {
                for (int i = 0; i < possibleWords.Length; i++)
                {
                    Console.WriteLine(possibleWords[i]);
                }
                return possibleKeys.ToArray();
            }
            else
                curWord++;


            string msg2Segment = Decrypt(
                    msg2.Substring(curChar, words[curWord].Length),
                    keySegment
                );

            messageCharPos[1] = msg2Segment.Length;

            string[] possibleWords = Finder(msg2Segment.Split(' ')[0]);  //fogja a második mondatot és a korábban kapott kulcsunkat felhasználva vissza kérünk minden lehetséges kezdő szót.
                                                                               //Kell a split a "Finder" függvény működése miatt. 


            System.Console.WriteLine("bah");
            if (possibleWords.Length == 0) //ha talált kezdő szavakat akkor folytatja
            {
                curWord++;
                continue;
            }
            System.Console.WriteLine("beh");
            possibleKeys.Add(keySegment); //kulcs szegmenst hozzá adjuk a listához

            Console.WriteLine("{0}\t:\t{1}", keySegment, curWord + 1);
            string lastWord = words[curWord];

            while (curChar <= maxKeyLength) //fut amíg a lehetséges kulcs méret végére nem ér
            {
                string curKey = possibleKeys[^1];
                int lastChar = curChar;


                for (int i = 0; i < possibleWords.Length; i++) //minden lehetséges szón végig megy
                {
                    string curPossibleWord = possibleWords[i];

                    if (curPossibleWord.Length > lastWord.Length) //ha hoszabb az új szó mint az előző, akkor az új szó lesz a kulcs
                    {
                        string[] newPossibleWords = Finder(Decrypt(
                            messages[curMsg].Substring(messageCharPos[curMsg],curPossibleWord.Length),
                            curKey.Substring(messageCharPos[curMsg])
                        ));
                        foreach (var item in newPossibleWords)
                        {
                            Console.WriteLine("--{0}",item);
                        }
                    }
                    else
                    {

                    }
                }


                if (lastChar == curChar)
                {
                    possibleKeys.RemoveAt(possibleKeys.Count - 1); //nem történt változás de nem érte el a mondat végét úgyhogy eldobjuk a lehetséges kulcsot és kilépünk
                    break;
                }
                else
                {
                    possibleKeys[^1] = curKey; //a legutolsó kulcs
                    curMsg = (curMsg == 0) ? 1 : 0;
                }

            }



            curWord++; //tovább lép a következő kezdő szóra

        }

        */
        #endregion
        //Több ideje gondolkoztam azon hogy valószínüleg rekurzívan kellene megoldani, de még sok tapasztalatom nem volt vele
        //de megpróbálom mégiscsak.

        for (int i = 0; i < words.Count; i++)
        {
            curmsg = true;
            message1CurrentCharacter = 0;
            message2CurrentCharacter = 0;

            PossibleKeys.Add(
                Decrypt(
                    CurrentMessage.Substring(0,words[i].Length),
                    words[i]
                )
            );

            
            if(!Checker(words[i]))
            {
                PossibleKeys.RemoveAt(PossibleKeys.Count-1);
            }
            else if (LastKey.Length < maxKeyLength)
            {
                PossibleKeys.RemoveAt(PossibleKeys.Count-1);
            }
        }

        Console.WriteLine("Possible Keys - {0}", possibleKeys.Count);
        return possibleKeys.ToArray();
    }

    private bool Checker(string lastWord)
    {
        string[] pWords = Finder(
            Decrypt(
                OppositeMessage.Substring(messageGiveCharacter(false),lastWord.Length),
                LastKey.Substring(messageGiveCharacter(),lastWord.Length)
            ).Split(' ')[0]
        );
        System.Console.WriteLine("{0} - {1}",LastKey,PossibleKeys.Count);
        foreach (string word in pWords)
        {
            Console.WriteLine(word);
        }
        return true;
    }

    private string[] Finder(string inword) //Vissza ad egy tömböt az összes lehetséges szóval
    {
        List<string> output = new();
        foreach (string word in words)
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