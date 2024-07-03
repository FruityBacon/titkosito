using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Dynamic;

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
    private bool cmsg = true;
    private Message CurrentMessage
    {
        get => cmsg ? message1 : message2;
    }
    private Message OppositeMessage
    {
        get => !cmsg ? message1 : message2;
    }
    private int loopCount = 0;
    private int roundCount = 0;

    #endregion

    #region arrays

    private List<List<string>> possible_keys = new(); // [i] = kezdőszó, [j] = minden lehetőség ezzel a kezdőszóval
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
    private List<string> word_list = new();
    private Dictionary<char, int> letter_bookmarks = new();
    public string[] WordList
    {
        get => word_list.ToArray();
    }

    #endregion

    public Cracker(string message1, string message2, List<string> word_list)
    {
        this.message1 = new(message1);
        this.message2 = new(message2);
        word_list.Sort();
        this.word_list = word_list;
        char prevChar = word_list[0][0];
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
        foreach (string word in WordList)
        {
            //Console.WriteLine("{0}. {1}", roundCount, word);
            List<string>? perword = (List<string>?)Checker(word, message1);
            roundCount++;
            loopCount = 0;
            cmsg = true;
            message1.CharacterPosition = 0;
            message2.CharacterPosition = 0;
            if (perword == null)
            {
                continue;
            }
            possible_keys.Add(perword);
        }
    }

    private object? Checker(string inputText, Message message)
    {
        //Console.Write("{0} \t{1}\n", loopCount, inputText);
        inputText = inputText+' ';
        if (message.MaxLength <= inputText.Length + message.CharacterPosition)
        {
            return new List<string> { inputText };
        }

        

        string? KeySegment = FindSegment(inputText, message);
        string? possibleWord = null;
        string[]? currentPossibleWords = null;

        if (KeySegment != null)
        {
            possibleWord = message.GetSegment(KeySegment.Length);

            if (possibleWord != null)
            {
                currentPossibleWords = FindWords(Decrypt(possibleWord, KeySegment));
            }
        }

        if (currentPossibleWords == null || KeySegment == null || possibleWord == null)
        {
            return null;
        }


        bool shownkey = false;
        List<string> list = new();
        int curMsgCharPos = CurrentMessage.CharacterPosition;
        int oppMsgCharPos = OppositeMessage.CharacterPosition;
        foreach (string? item in currentPossibleWords)
        {
            if (item != null)
            {
                string nextword = Decrypt(OppositeMessage.GetSegment(KeySegment.Length), KeySegment);
                string[] nextwords = nextword.Split(' ');

                if (FindWords(nextwords[0]) != null && IsValidMessageSegment(nextword))
                {
                    string[]? nextInputWord;

                    if (nextwords.Length > 1)
                    {
                        string? temp = FindFirstWord(nextwords[^1]);
                        if (temp == null)
                        {
                            continue;
                        }
                        nextInputWord = new[] { temp.Substring(nextwords[^1].Length) };
                    }
                    else
                    {
                        nextInputWord = FindWords(nextword);
                        if (nextInputWord == null)
                        {
                            return null;
                        }
                        else
                        {
                            for (int i = 0; i < nextInputWord.Length; i++)
                            {
                                System.Console.WriteLine("{0}..{1}",loopCount,nextInputWord[i]);
                                nextInputWord[i] = (nextInputWord[i].Length > inputText.Length) ?
                                    nextInputWord[i].Substring(inputText.Length) :
                                    inputText.Substring(nextInputWord[i].Length);

                            }
                        }
                    }

                    foreach (var niw in nextInputWord)
                    {
                        if (niw.Length+1 == inputText.Length)
                        {
                            continue;
                        }

                        CurrentMessage.CharacterPosition = curMsgCharPos + inputText.Length + 1;
                        OppositeMessage.CharacterPosition = oppMsgCharPos + inputText.Length + 1;

                        cmsg = !cmsg;
                        Console.WriteLine("'{0}' -- {1}", niw+' ', CurrentMessage);
                        object? result = Checker(niw, CurrentMessage);
                        if (result != null)
                        {
                            foreach (var ret in (List<string>)result)
                            {
                                System.Console.WriteLine(niw + ret);
                                list.Add(niw + ret);
                            }
                        }
                    }

                    if (!shownkey)
                    {
                        Console.WriteLine("{0}.{1} : {2}({3}) - {4}", roundCount, loopCount++, inputText, CurrentMessage.CharacterPosition, (cmsg) ? "msg1" : "msg2");
                        System.Console.WriteLine("'{0}' '{1}' :", KeySegment, possibleWord);
                        shownkey = true;
                    }
                    string leftover = (item.Length > KeySegment.Length) ? item.Substring(KeySegment.Length) : KeySegment.Substring(item.Length);
                    Console.WriteLine("\t{0}\n{3}{1} - {2}", item, nextword, leftover, Repeater("\t", loopCount));

                }


            }
        }
        if (shownkey == true)
        {
            System.Console.WriteLine();
        }
        if (list.Count > 0)
        {
            foreach (var item in list)
            {
                System.Console.WriteLine(item);
            }
        }

        return null;
    }

    private string Repeater(string wordToRepeat, int count)
    {
        count--;
        if (count > 0)
        {
            wordToRepeat += Repeater(wordToRepeat, count);
        }
        return wordToRepeat;
    }

    private string? FindSegment(string inputWord, Message message) //vissza ad egy kulcs részleget amit fel lehet használni keresésre
    {
        string? segment = message.GetSegment(inputWord.Length);

        if (segment == null)
        {
            return null;
        }

        return this.Decrypt(
            segment,
            inputWord
        );

    }

    private bool IsValidMessageSegment(string inputSegment)
    {
        string[] inwords = inputSegment.Split(' ');
        int index_wl = 0;
        int index_is = 0;
        bool found = false;
        while (index_is < inwords.Length && !found)
        {
            while (index_wl < WordList.Length && !found)
            {
                if (inwords[^1] == inwords[index_is])
                {
                    if (WordList[index_wl].StartsWith(inwords[index_is]))
                    {
                        found = true;
                    }
                }
                else
                {
                    if (WordList[index_wl] != inwords[index_is])
                    {
                        break;
                    }
                }
                index_wl++;
            }
            index_is++;
        }

        return found;
    }

    private string? FindFirstWord(string inputSegment)
    {
        if (inputSegment.Length == 0 || inputSegment[0] == ' ')
        {
            return null;
        }
        int starting_from = charvals.IndexOf(inputSegment[0]);
        int index = letter_bookmarks[charvals[starting_from]];
        string? ret = null;
        while (word_list.Count > index && letter_bookmarks[charvals[starting_from + 1]] > index)
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

    private string[]? FindWords(string inputSegment) //vissza ad minden lehetséges szót ami a szó részlettel kezdődik
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
                        if (word_list[index].StartsWith(seperates[i]))
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
        return msg.Substring(0, pos);
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