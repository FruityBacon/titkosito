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
    private int CurrentMessageCharacter {
        get => curmsg? message1CurrentCharacter : message2CurrentCharacter;
        set {
            if (curmsg)
            {
                message1CurrentCharacter = value;
            }
            else
            {
                message2CurrentCharacter = value;
            }
        }
    }
    private int OppositeMessageCharacter {
        get => !curmsg? message1CurrentCharacter : message2CurrentCharacter;
        set {
            if (!curmsg)
            {
                message1CurrentCharacter = value;
            }
            else
            {
                message2CurrentCharacter = value;
            }
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
            
            if(Checker(words[i]) == null)
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

    private string[]? Checker(string lastWord)
    {

        string[] pWords = Finder(
            Decrypt(
                OppositeMessage.Substring(messageGiveCharacter(false),lastWord.Length),
                LastKey.Substring(messageGiveCharacter(),lastWord.Length)
            ).Split(' ')[0]
        );
        if (pWords.Length == 0)
        {
            return null;
        }

        System.Console.WriteLine("{0} - {1}",LastKey,PossibleKeys.Count);

        List<string[]> pSegments = new();

        foreach (string word in pWords)
        {
            int backup1 = messageGiveCharacter();
            int backup2 = messageGiveCharacter(false);
            string backupKey = LastKey;
            bool messageState = curmsg;
            string[] sWords;

            if (word.Length > lastWord.Length)
            {
                //ha az új szó hoszabb mint az utolsó, akkor az régi szó hosszát + 1 (space) adjuk meg
                //a kulcshoz is ennyit adunk hozzá
                if (word.Length == lastWord.Length +1)
                {
                    continue;
                }

                //System.Console.WriteLine("{0}\t{1}\t{2}\t{3}",);
                sWords = Finder(
                    Decrypt(
                        CurrentMessage.Substring(messageGiveCharacter()+lastWord.Length+1,messageGiveCharacter()+word.Length),
                        LastKey.Substring(messageGiveCharacter(false)+lastWord.Length+1)
                    )
                );
                CurrentMessageCharacter += word.Length;
                OppositeMessageCharacter += lastWord.Length+1;
            }
            else
            {
                //ha az új szó rövidebb akkor az új szó hosszát + 1 (space) adjuk meg a character helyének
                //a kulcshoz is ezt adjuk meg
                if (word.Length+1 == lastWord.Length)
                {
                    continue;
                }

                sWords = Finder(
                    Decrypt(
                        CurrentMessage.Substring(messageGiveCharacter()+word.Length+1,messageGiveCharacter()+lastWord.Length),
                        LastKey.Substring(messageGiveCharacter(false)+word.Length+1)
                    )
                );
                CurrentMessageCharacter += lastWord.Length;
                OppositeMessageCharacter += word.Length+1;
            }

            curmsg = !messageState;
            foreach (string sWord in sWords)
            {
                string[]? retval = Checker(sWord);
                if (retval != null)
                {
                    pSegments.Add(retval);
                }
                else
                {
                    curmsg = messageState;
                    LastKey = backupKey;
                    CurrentMessageCharacter = backup1;
                    OppositeMessageCharacter = backup2;
                    
                }
            }
            
        }
        if (pSegments.Count > 0)
        {
            foreach (var item in pSegments)
            {
                Console.Write("{0};",item);
            }
            System.Console.WriteLine();
        }
        return null;
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
}

public class Cracker
{
    private Secret secret = new();
    private Message message1;
    public Message FirstMessage
    {
        get => message1;
    }
    private Message message2;
    public Message SecondMessage
    {
        get => message2;
    }
    
    private List<List<string>> possible_keys = new(); // [i] = kezdőszó, [j] = minden lehetőség ezzel a kezdőszóval
    public string[] PossibleKeys {
        get {
            List<string> output = new();
            for (int i = 0; i < possible_keys.Count; i++)
            {
                for (int j = 0; j < possible_keys[i].Count; j++)
                {
                    output.Add(possible_keys[i][j]);
                }
            }
            return output.ToArray();
        }
    }
    private List<string> word_list = new();
    public string[] WordList {
        get => word_list.ToArray();
    }

    public Cracker(string message1, string message2, List<string> word_list)
    {
        this.message1 = new(message1);
        this.message2 = new(message2);
        this.word_list = word_list;
    }

    public void Start()
    {
        foreach (string word in WordList)
        {
            List<string>? perword = Checker(word);
            if (perword == null)
            {
                continue;
            }
            possible_keys.Add(perword);
        }
    }

    private List<string>? Checker(string inputWord)
    {

        return null;
    }

    private string? FindSegment(string inputWord, Message message) //vissza ad egy kulcs részleget amit fel lehet használni keresésre
    {
        string? segment = message.GetSegment(inputWord.Length);

        if (segment == null)
        {
            return null;
        }

        return this.secret.Decrypt(
            segment,
            inputWord
        );
        
    }

    private string[]? FindWords(string inputSegment) //vissza ad minden lehetséges szót ami a szó részlettel kezdődik
    {
        List<string> words = new();
        string[] seperates = inputSegment.Split(' ');
        for (int i = 0; i < seperates.Length; i++)
        {
            if (seperates[i].Length == 1)
            {
                switch (seperates[i].ToLower()) //Csak kettő 1 betűs szó létezik
                {
                    case "a":
                        words.Add(seperates[i]);
                        continue;
                    case "i":
                        words.Add(seperates[i]);
                        continue;
                    default:
                        continue;
                }
            }
            foreach (string word in word_list)
            {
                if (word.StartsWith(seperates[i]))
                {
                    words.Add(word);
                }
            }
        }
        
        return (words.Count > 0)? words.ToArray() : null;
    }
}

public class Message
{
    private string msg;
    public string FullMessage {
        get => msg;
    }
    public int MaxLength {
        get => msg.Length;
    }

    private int charPos = 0;
    public int CharacterPosition {
        get => charPos;
        set => charPos = value;
    }

    public string GetAfterCharPos(int pos = -1)
    {
        if (pos < 0)
        {
            pos = charPos;
        }
        if (pos >= this.MaxLength)
        {
            pos = this.MaxLength - 1;
        }
        return msg.Substring(pos);
    }
    public string GetBeforeCharPos(int pos = -1)
    {
        if (pos < 0)
        {
            pos = charPos;
        }
        if (pos >= this.MaxLength)
        {
            pos = this.MaxLength - 1;
        }
        return msg.Substring(0,pos);
    }

    public string? GetSegment(int? endpos = null) //karakter pozíciótól függően vissza adja az előtte vagy utána jövő n karaktert string ként
    {
        if (endpos == null)
        {
            return null;
        }
        else if(endpos < 0)
        {
            if (this.charPos + (int)endpos >= 0)
            {
                endpos += charPos; 
            }
            else
            {
                endpos = 0;
            }
            return msg.Substring((int)endpos,charPos);
        }
        else
        {
            endpos = (endpos >= MaxLength)? MaxLength-1 : endpos;
            return msg.Substring(charPos,(int)endpos);
        }
    }

    public override string ToString()
    {
        return msg;
    }

    public Message(string msg)
    {
        this.msg = msg;
    }
}