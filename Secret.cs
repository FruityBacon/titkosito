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

    private int MaxKeyLength
    {
        get
        {
            return (message1.MaxLength > message2.MaxLength) ? message2.MaxLength : message1.MaxLength;
        }
    }

    private Message ShorterMessage
    {
        get
        {
            return (message1.MaxLength > message2.MaxLength) ? message2 : message1;
        }
    }

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
            //("{0}. {1}", roundCount, word);
            List<string>? KeysFromWord = (List<string>?)Checker(word + " ");
            List<string>? perword = new();

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
                        perword.Add(KeysFromWord[i]);
                    }

                };
            }
            roundCount++;
            loopCount = 0;
            cmsg = true;
            message1.CharacterPosition = 0;
            message2.CharacterPosition = 0;

            if (perword.Count > 0)
            {
                possible_keys.Add(perword);
            }
        }
    }

    private object? Checker(string inputText, bool isKnownKeyPart = false)
    {
        if (inputText.Length >= this.MaxKeyLength && isKnownKeyPart)
        {
            return inputText;
        }
        string writeout = "";
        string possibleSegment;
        bool local_cmsg = cmsg;
        List<string> list = new();

        if (isKnownKeyPart == false)
        {
            string? KeySegment = null;
            KeySegment = Decrypt(
                CurrentMessage.GetSegment(inputText.Length),
                inputText
            );
            writeout += "\nKeySegment: " + KeySegment;

            possibleSegment = Decrypt(
                OppositeMessage.GetSegment(KeySegment.Length),
                KeySegment
            );
            writeout += "\npossibleSegment: " + possibleSegment;

            writeout += "\npS words:";
            string[]? possibleWords = new string[0];
            if (possibleSegment.Contains(' '))
            {
                string[] wordsInSegment = possibleSegment.TrimEnd(' ').Split(' ');
                foreach (string word in wordsInSegment)
                {
                    if (FindFirstWord(word) == null && FindWordsEndsWith(word) == null)
                    {
                        return null;
                    }
                }
                try
                {
                    possibleWords = new[] { FindFirstWord(wordsInSegment[^1]) };
                    possibleSegment = possibleSegment.Substring(0, possibleSegment.Length - possibleWords[0].Length);
                    writeout += "\nNewPossibleSegment: " + possibleSegment + '"';
                }
                catch (System.Exception)
                {
                    return null;
                }

            }
            else
            {
                possibleWords = FindWordsStartingWith(possibleSegment);
                if (possibleWords == null)
                {
                    return null;
                }


            }


            string writtenout = writeout;
            foreach (string word in possibleWords)
            {
                writeout = writtenout;
                string newKeySegment = KeySegment;
                writeout += "\n\t" + word;

                if (KeySegment.Length < word.Length)
                {
                    writeout += "\n\t" + Repeater(" ", possibleSegment.Length - 1) + "'" + word.Substring(possibleSegment.Length) + " '";

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
                writeout += "\nNewKeySegment: " + newKeySegment + "\n";

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

            }
        }
        else
        {
            try
            {
                possibleSegment = Decrypt(OppositeMessage.GetSegment(inputText.Length), inputText);

                string[] possibleNextWords = FindWordsStartingWith(possibleSegment.Split(' ')[^1]);

                string newKeySegment = inputText;

                foreach (string word in possibleNextWords)
                {
                    string workingWord = word.Substring(possibleSegment.Split(' ')[^1].Length) + " ";

                    int partLength = int.Clamp(workingWord.Length + inputText.Length - MaxKeyLength, 0, MaxKeyLength);

                    if (word.Length >= MaxKeyLength - inputText.Length)
                    {
                        if (partLength > 1)
                        {
                            continue;
                        }
                        else if (partLength == 1)
                        {
                            workingWord = word.Substring(possibleSegment.Split(' ')[^1].Length);
                        }

                    }

                    newKeySegment += Decrypt(
                        OppositeMessage.FullMessage.Substring(inputText.Length, workingWord.Length),
                        workingWord
                    );


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
                    newKeySegment = newKeySegment.Substring(0, newKeySegment.Length - workingWord.Length);
                    cmsg = local_cmsg;
                }
            }
            catch (System.Exception)
            {
                return null;
            }

        }

        return (list.Count > 0) ? list : null;
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

    private string? FindFirstWord(string inputSegment)
    {
        if (inputSegment.Length == 0 || inputSegment[0] == ' ' || !letter_bookmarks.ContainsKey(inputSegment[0]))
        {
            return null;
        }
        
        int starting_from = charvals.IndexOf(inputSegment[0]);
        int ending_at = letter_bookmarks.ContainsKey(charvals[starting_from + 1]) ? 1 : 2;
        int index = letter_bookmarks[charvals[starting_from]];
        string? ret = null;
        while (word_list.Count > index && (starting_from + 1 != 26 ? letter_bookmarks[charvals[starting_from + ending_at]] : word_list.Count) > index)
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

    private string[]? FindWordsStartingWith(string inputLetter)
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

    private string[]? FindWordsEndsWith(string inputSegment) //vissza ad minden lehetséges szót ami a szó részlettel kezdődik
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