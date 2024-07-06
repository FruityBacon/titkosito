using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;
using System.Security.AccessControl;
using System.Security.Cryptography;

namespace titkosito;
public class Secret
{
    protected readonly List<char> charvals = new List<char>(){
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

}

public class Cracker : Secret
{
    #region variables

    private Message message1; //Tartalmazza az első kulcsot
    public Message FirstMessage
    {
        get => message1;
        set => message1 = value;
    }

    private Message message2; //Tartalmazza a második kulcsot
    public Message SecondMessage
    {
        get => message2;
        set => message2 = value;
    }

    private bool cmsg = true;
    private Message CurrentMessage
    {
        get => cmsg ? message1 : message2; //vissza adja vagy az első vagy a második üzenetet attól függően hogy igaz vagy hamis
    }
    private Message OppositeMessage //vissza adja a CurrentMessage(cmsg) ellenkezőjét képviselő üzenetet
    {
        get => !cmsg ? message1 : message2;
    }

    private int MaxKeyLength // A kapott kulcs csak akkora lehet mint a rövidebbik mondat, ezért annak a hosszát adja vissza
    {
        get
        {
            return (message1.MaxLength > message2.MaxLength) ? message2.MaxLength : message1.MaxLength;
        }
    }

    #endregion

    #region arrays

    private List<List<string>> possible_keys = new(); // Kezdőszavanként listákat tartalmaz lehetséges kulcsok részére
    public string[] PossibleKeys
    {
        get
        {
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
    private List<string> word_list = new(); // Ez tartalmazza a szótárat
    private Dictionary<char, int> letter_bookmarks = new(); // Ez tartalmaz könyvjelzőket ahhoz hogy a szótáron belül hol kezdődnek betűk
    public string[] WordList
    {
        get => word_list.ToArray();
    }

    #endregion

    public Cracker(string message1, string message2, List<string> word_list) // A feltöréshez mindenféle képpen kell a kettő üzenet és egy szótár, a szótárat kívülről kapja meg valamilyen módszer szerint
    {
        this.message1 = new(message1);
        this.message2 = new(message2);
        
        word_list.Sort();   // Nem tudjuk hogy alapból rendezett-e vagy sem, ezért rendezzük mi abcsorrend szerint
        this.word_list = word_list;

        char prevChar = word_list[0][0];    // A könyvjelző mentéshez eltárolja a legutóbbi kezdőbetűt, először az első szó legelső betüjét

        letter_bookmarks.Add(prevChar, 0);
        for (int i = 0; i < word_list.Count; i++)
        {
            if (word_list[i][0] != prevChar)
            {
                prevChar = word_list[i][0];
                letter_bookmarks.Add(prevChar, i);
            }
        }
    }

    public void Start()
    {
        foreach (string word in WordList) // A szótár minden betüjét végigjárjuk hogy biztosan ne hadjuk ki a lehetséges kezdőszót
        {

            List<string>? KeysFromWord = (List<string>?)Checker(word + " "); // A Checker() minden lehetséges kulcsot vissza ad nekünk ezzel a kezdőszóval. Ha nem talált egyet sem akkor Null

            List<string>? perword = new();

            /*
                Hogyha talált számunkra kulcsot a Checker(), akkor végig nézzük egyesével az összeset,
                hogy tényleg csak létező szavakat kapunk-e vissza az eredeti üzeneteinkből 
            */

            if (KeysFromWord != null)
            {
                for (int i = 0; i < KeysFromWord.Count; i++)
                {
                    string[][] messages = new string[][] {
                        Decrypt(message1.FullMessage.Substring(0, MaxKeyLength), KeysFromWord[i]).TrimEnd(' ').Split(' '),
                        Decrypt(message2.FullMessage.Substring(0, MaxKeyLength), KeysFromWord[i]).TrimEnd(' ').Split(' ')
                        };

                    bool KeyIsCorrect = true;

                    for (int j = 0; j < 2; j++)
                    {
                        foreach (string segment in messages[j])
                        {
                            if (segment != messages[j][^1])
                            {
                                if (!word_list.Contains(segment))
                                {
                                    KeyIsCorrect = false;
                                }
                            }
                            else
                            {
                                if (FindFirstWord(segment) == null)
                                {
                                    KeyIsCorrect = false;
                                }
                            }
                        }
                    }

                    if (KeyIsCorrect) 
                    {
                        perword.Add(KeysFromWord[i]); // Ha sehol se volt hibás szó, akkor elmentjük ezt a kulcsot
                    }

                };
            }

            // Mindig az első mondattal kezdünk, és ezt itt állítjuk vissza az eredti helyzetére
            cmsg = true;

            if (perword.Count > 0)
            {
                possible_keys.Add(perword); //  Ha volt lehetséges kulcs, akkor elmentjük
            }
        }
    }

    private object? Checker(string inputText, bool isKnownKeyPart = false) //object? mert három különböző lehetőséget kapunk vissza belőle, null, string vagy List<string>
    {
        if (inputText.Length >= this.MaxKeyLength && isKnownKeyPart) // Ha a megdott szöveg nagyobb vagy olyan hosszú mint a maximum hossz, és kulcs akkor vissza adja mint string
        {
            return inputText;
        }

        string possibleSegment; //Ez fogja tárolni hogy a megadott kezdőszavunk az minek felel meg a másik üzenetben
        bool local_cmsg = cmsg; //elmentjük hogy milyen kulccsal kezdődött ez a loop
        List<string> list = new();

        if (isKnownKeyPart == false) //Ha a megadott üzenet nem kulcs, akkor kezdőszóként kezeljük
        {
            string? KeySegment = null;
            KeySegment = Decrypt(
                CurrentMessage.GetSegment(inputText.Length),
                inputText
            );

            possibleSegment = Decrypt(
                OppositeMessage.GetSegment(KeySegment.Length),
                KeySegment
            );

            string[]? possibleWords = new string[0]; //Ez tartalmaz minden lehetséges szót a feltörés folytatásához
            if (possibleSegment.Contains(' ')) //Ha az ellenkező üzenetből vissza kapott rész tartalmaz ' '-t akkor tudjuk hogy a következő szó kisebb
            {
                string[] wordsInSegment = possibleSegment.TrimEnd(' ').Split(' '); 
                foreach (string word in wordsInSegment)
                {
                    if (FindFirstWord(word) == null && FindWordsEndsWith(word) == null) //ha a szegmensben lévő szavak benne vannak a szótárban akkor folytatjuk
                    {
                        return null;
                    }
                }

                try
                {
                    possibleWords = new[] { FindFirstWord(wordsInSegment[^1]) }; //Ellenőrzést követően, az utolsó szegmenst mentjük csak el mint "következő szó"
                    possibleSegment = possibleSegment.Substring(0, possibleSegment.Length - possibleWords[0].Length); //a korábbi szegmensből csak azt tartjuk meg ami kell
                }
                catch (System.Exception)
                {
                    return null;
                }

            }
            else //ha a ellenkező mondatbóli szegmens az nem tartalmaz ' '-t akkor a keresett szavunk nagyobb
            {
                possibleWords = FindWordsStartingWith(possibleSegment); //Megnézzük hogy létezik-e szó abban a szegmensben amit kaptunk
                if (possibleWords == null)
                {
                    return null;
                }


            }


            foreach (string word in possibleWords) // Egyesével ellenőrizve az összes lehetséges következő szót
            {
                string newKeySegment = KeySegment;

                if (KeySegment.Length < word.Length)
                {
                    newKeySegment += Decrypt(
                        OppositeMessage.FullMessage.Substring(
                            possibleSegment.Length,
                            word.Length - possibleSegment.Length + 1
                        ),
                        word.Substring(possibleSegment.Length) + " "
                    );
                }
                else
                {
                    newKeySegment = KeySegment.Substring(0, possibleSegment.Length) + Decrypt(
                        OppositeMessage.FullMessage.Substring(
                            possibleSegment.Length,
                            word.Length + 1
                        ), word + " "
                    );
                }

                cmsg = !local_cmsg; // A kezdő mondatunk ellenkezőjével ellenőrizzük majd a következő szegmenst
                switch (Checker(newKeySegment, true)) // mivel kulcs részleget adunk meg, ezért megadjuk az is hogy igen egy kulcs
                {
                    case string ret: // ha csak egy stringet kapunk vissza, akkor is listaként vár választ, így listába mentjük
                        list.Add(ret);
                        break;

                    case List<string> ret: //ha listát kapunk vissza, akkor kimentjük az eredményeket egy új listába
                        foreach (string item in ret)
                        {
                            list.Add(item);
                        }
                        break;

                    case null: //ha nem kaptunk vissza semmit akkor sikertelen volt a kezdőszó
                        break;

                }

            }
        }
        else // Ha kulcsot kaptunk akkor máshogy kezeljük az inputText-et
        {
            try
            {
                possibleSegment = Decrypt(OppositeMessage.GetSegment(inputText.Length), inputText); //kulcsként kezeljük

                string[] possibleNextWords = FindWordsStartingWith(possibleSegment.Split(' ')[^1]); //feltételezzük hogy a mondat többi része helyes és az utolsó szegmenssel foglalkozunk

                string newKeySegment = inputText; //az eredeti inputot elmentjük egy saját változóba

                foreach (string word in possibleNextWords)
                {
                    string workingWord = word.Substring(possibleSegment.Split(' ')[^1].Length) + " ";

                    int partLength = int.Clamp(workingWord.Length + inputText.Length - MaxKeyLength, 0, MaxKeyLength);

                    if (word.Length >= MaxKeyLength - inputText.Length) // ha az új szegmenssel túl lépnénk a maximum kulcs hosszon, akkor új szóval próbálkozunk
                    {                                                   // vagy visszavonjuk a ' '-t a végéről
                        if (partLength > 1)
                        {
                            continue;
                        }
                        else if (partLength == 1)
                        {
                            workingWord = word.Substring(possibleSegment.Split(' ')[^1].Length);
                        }

                    }

                    //Hozzáadjuk a kapott kulcsrészhez az új feltételezett kulcs részt, ami után ezt adjuk tovább
                    newKeySegment += Decrypt(                       
                        OppositeMessage.FullMessage.Substring(inputText.Length, workingWord.Length),
                        workingWord
                    );

                    //a következő alkalommal megint az ellenkező szöveggel akarunk foglalkozni
                    cmsg = !local_cmsg;
                    switch (Checker(newKeySegment, true))
                    {
                        case string ret:
                            list.Add(ret);
                            break;

                        case List<string> ret:
                            foreach (string item in ret)
                            {
                                list.Add(item);
                            }
                            break;

                        case null:

                            break;

                    }
                    //a következő szó ellenőrzése érdekében, vissza állítjuk a két fő értéket az eredeti állapotára
                    newKeySegment = newKeySegment.Substring(0, newKeySegment.Length - workingWord.Length);
                    cmsg = local_cmsg;
                }
            }
            catch (System.Exception)
            {
                return null;
            }

        }

        return (list.Count > 0) ? list : null; //ha van lista elem, akkor lsitát ad vissza, különben viszont sikertelen volt az ellenőrzés
    }

    private string? FindFirstWord(string inputSegment) //vissza adja az első találatot ami a megadott szegmenssel kezdődik
    {
        if (inputSegment.Length == 0 || inputSegment[0] == ' ' || !letter_bookmarks.ContainsKey(inputSegment[0]))
        {
            return null;
        }
        
        int starting_from = charvals.IndexOf(inputSegment[0]);
        int ending_at = starting_from + 1;
        while(!letter_bookmarks.ContainsKey(charvals[ending_at]) && ending_at < 26)
        {
            ending_at++;
        }
        int index = letter_bookmarks[charvals[starting_from]];
        string? ret = null;
        while (word_list.Count > index && (starting_from + 1 != 26 ? letter_bookmarks[charvals[ending_at]] : word_list.Count) > index)
        {
            if (word_list[index].StartsWith(inputSegment))
            {
                ret = word_list[index];
                break;
            }
            index++;
        }
        return ret;
    }

    private string[]? FindWordsStartingWith(string inputLetter) //vissza ad minden találatot ami a megadott szegmenssel kezdődik
    {
        List<string> ret = new();
        if (inputLetter.Length == 0 || inputLetter[0] == ' ' || !letter_bookmarks.ContainsKey(inputLetter[0]))
        {
            return null;
        }

        int starting_from = charvals.IndexOf(inputLetter[0]);

        int index = letter_bookmarks[charvals[starting_from]];

        try
        {
            while (word_list.Count > index && (starting_from + 1 != 26 ? letter_bookmarks[charvals[starting_from + 1]] : word_list.Count) > index)
            {
                if (word_list[index].StartsWith(inputLetter))
                {
                    ret.Add(word_list[index]);
                }
                index++;
            }
        }
        catch (Exception)
        {
            return ret.ToArray();
        }
        return ret.ToArray();
    }

    private string[]? FindWordsEndsWith(string inputSegment) //vissza ad minden lehetséges szót ami a szó részlettel végződik
    {
        List<string> words = new();
        string[] seperates = inputSegment.Split(' ');

        for (int i = 0; i < seperates.Length; i++)
        {
            if (seperates[i] == "")
            {
                break;
            }
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
            else
            {
                try
                {
                    int starting_from = charvals.IndexOf(seperates[i][0]);
                    if (starting_from == 26)
                    {
                        continue;
                    }
                    int index = letter_bookmarks[charvals[starting_from]];
                    while (letter_bookmarks[charvals[starting_from + 1]] > index && word_list.Count > index)
                    {
                        if (word_list[index].EndsWith(seperates[i]))
                        {
                            words.Add(word_list[index]);
                        }
                        index++;
                    }
                }
                catch (System.Exception)
                {
                    continue;
                }

            }

        }

        return (words.Count > 0) ? words.ToArray() : null;
    }
}

public class Message
{
    private string msg;
    public string FullMessage
    {
        get => msg;
    }
    public int MaxLength
    {
        get => msg.Length;
    }

    private int charPos = 0;
    public int CharacterPosition
    {
        get => charPos;
        set => charPos = int.Clamp(value, 0, this.MaxLength);
    }

    public string? GetSegment(int? inpos = null) //karakter pozíciótól függően vissza adja az előtte vagy utána jövő n karaktert string ként
    {
        if (inpos == null)
        {
            return null;
        }
        else
        {
            int endpos = int.Clamp((int)inpos, this.CharacterPosition * -1, this.MaxLength - this.CharacterPosition);
            if (endpos < 0)
            {
                if (this.charPos + (int)endpos >= 0)
                {
                    endpos += charPos;
                }
                else
                {
                    endpos = 0;
                }
                return msg.Substring((int)endpos, charPos);
            }
            else
            {
                endpos = (endpos >= MaxLength) ? MaxLength - 1 : endpos;
                return msg.Substring(charPos, (int)endpos);
            }
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