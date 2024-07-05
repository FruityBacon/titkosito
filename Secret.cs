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

    public void Test()
    {
        string key = "abcdefghijklmnopqrstuvwxyzabcdefg";
        string showKey = "";
        string original = Decrypt(message1.FullMessage, key);
        string word = original.Split(' ')[0] + ' ';
        string og = "";
        string write = "";
        string place = "";
        int counter = 0;
        Console.WriteLine("Message1:");
        foreach (char item in message1.FullMessage)
        {
            og += original[counter] + "\t";
            write += item + "\t";
            showKey += key[counter] + "\t";
            place += counter++ + "\t";
        }
        Console.WriteLine(og + "\n" + write + "\n" + place + "\n" + showKey);

        original = Decrypt(message2.FullMessage, key);
        og = "";
        write = "";
        place = "";
        counter = 0;
        Console.WriteLine("Message2:");
        foreach (char item in message2.FullMessage)
        {
            og += original[counter] + "\t";
            write += item + "\t";
            place += counter++ + "\t";
        }
        Console.WriteLine(og + "\n" + write + "\n" + place + "\n" + showKey);

        List<string>? perword = (List<string>?)NewChecker(word);
        roundCount++;
        loopCount = 0;
        cmsg = true;
        message1.CharacterPosition = 0;
        message2.CharacterPosition = 0;
        if (perword == null)
        {
            System.Console.WriteLine("\nFAILED");
            return;
        }
        else
        {
            System.Console.WriteLine("\nSUCCESS");
        }
        possible_keys.Add(perword);

    }

    public void Start()
    {
        foreach (string word in WordList)
        {
            //Console.WriteLine("{0}. {1}", roundCount, word);
            List<string>? perword = (List<string>?)Checker(word);
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

    private object? NewChecker(string inputText, bool isKnownKeyPart = false)
    {
        System.Console.WriteLine("NEW LOOP {0} '{1}' Using {2}",loopCount++,inputText,(isKnownKeyPart?"Key":"Starter Word"));
        System.Console.WriteLine("Working sentence: '{0}'",CurrentMessage);
        string writeout = "";
        string? KeySegment = null;
        string possibleSegment;

        if (isKnownKeyPart == false)
        {
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
                
                possibleWords = new[] { FindFirstWord(wordsInSegment[^1]) };
                possibleSegment = possibleSegment.Substring(0, possibleSegment.Length-possibleWords[0].Length);
                writeout += "\nNewPossibleSegment: "+possibleSegment+'"';
            }
            else
            {
                possibleWords = FindWordsStartingWith(possibleSegment);
                if (possibleWords == null)
                {
                    return null;
                }


            }

            bool local_cmsg = cmsg;
            string writtenout = writeout;
            foreach (string word in possibleWords)
            {
                writeout = writtenout;
                string newKeySegment = KeySegment;
                writeout += "\n\t" + word;

                if (KeySegment.Length < word.Length)
                {
                    writeout += "\n\t"+Repeater(" ",possibleSegment.Length-1)+"'"+word.Substring(possibleSegment.Length)+" '";

                    newKeySegment += Decrypt(
                        OppositeMessage.FullMessage.Substring(
                            possibleSegment.Length,
                            word.Length-possibleSegment.Length+1
                        ),
                        word.Substring(possibleSegment.Length)+" "
                    );
                }
                else
                {
                    newKeySegment = KeySegment.Substring(0,possibleSegment.Length) + Decrypt(
                        OppositeMessage.FullMessage.Substring(
                            possibleSegment.Length,
                            word.Length+1
                        ),word+" "
                    );
                }
                writeout += "\nNewKeySegment: "+newKeySegment+"\n";

                System.Console.WriteLine(writeout);
                cmsg = !local_cmsg;
                NewChecker(newKeySegment,true);

            }
        }
        else
        {
            System.Console.WriteLine("You just passed on a key, Congrats!");
        }

        
        return null;
    }

    private object? Checker(string inputText, string? knownKeyPart = null)
    {
        //Console.Write("{0} \t{1}\n", loopCount, inputText);
        Console.WriteLine("\n\nNEW LOOP - {0} {1}", loopCount++, ((knownKeyPart != null) ? knownKeyPart + "(" + (knownKeyPart.Length - 1) + ")" : ""));

        if (CurrentMessage.MaxLength <= inputText.Length + 1 + CurrentMessage.CharacterPosition)
        {
            return new List<string> { inputText };
        }

        System.Console.WriteLine("Input Text: '{0}'({1})", inputText, inputText.Length);


        string? KeySegment = FindSegment(inputText, CurrentMessage);
        Console.WriteLine("KeySegment: '{0}'", KeySegment);

        string? possibleWord = null;
        string[]? currentPossibleWords = null;

        if (KeySegment != null)
        {

            if (knownKeyPart != null) //ha van ismert kulcs akkor felhasználjuk ellenőrzésre
            {
                possibleWord = Decrypt(OppositeMessage.GetSegment(KeySegment.Length), KeySegment);

                System.Console.WriteLine("nextword start: {0}, keysegment: {1}, letter at msgcp: {2}", possibleWord, KeySegment, CurrentMessage.FullMessage[CurrentMessage.CharacterPosition]);
                if (possibleWord != null)
                {
                    currentPossibleWords = FindWordsStartingWith(possibleWord);
                    if (currentPossibleWords != null)
                    {
                        foreach (string item in currentPossibleWords)
                        {
                            System.Console.WriteLine(item);
                        }
                    }

                }
            }
            else //ha nincs ismert kulcs akkor tippelünk
            {
                possibleWord = CurrentMessage.GetSegment(KeySegment.Length);
                Console.WriteLine("'{0}' {1} {2}", KeySegment, possibleWord, CurrentMessage.CharacterPosition);

                if (possibleWord != null)
                {
                    currentPossibleWords = FindWords(Decrypt(possibleWord, KeySegment));
                }
            }

        }

        if (currentPossibleWords == null || KeySegment == null || possibleWord == null)
        {
            System.Console.WriteLine("returning null " + loopCount);
            return null;
        }

        bool shownkey = false;
        List<string> list = new();
        bool local_cmsg = cmsg;
        int curMsgCharPos = CurrentMessage.CharacterPosition;
        int oppMsgCharPos = OppositeMessage.CharacterPosition;
        System.Console.WriteLine(currentPossibleWords.Length);
        foreach (string? item in currentPossibleWords)
        {
            System.Console.WriteLine(item);
            if (item != null)
            {
                string segmenttouse = (knownKeyPart != null) ? Decrypt(OppositeMessage.GetSegment(item.Length), item) : KeySegment;
                System.Console.WriteLine("Segment Used: '{0}' - '{1}'", segmenttouse, KeySegment);
                string nextword = Decrypt(OppositeMessage.GetSegment(segmenttouse.Length), segmenttouse);
                System.Console.WriteLine("NextWord: '{0}'", nextword);
                string[] nextwords = nextword.Split(' ');

                if (FindWords(nextwords[0]) != null && IsValidMessageSegment(nextword))
                {
                    string[]? nextInputWord;

                    foreach (var items in nextwords)
                    {
                        System.Console.WriteLine(items);
                    }

                    if (nextwords.Length > 1)
                    {
                        System.Console.WriteLine("Only one word.");
                        string? temp = FindFirstWord(nextwords[^1]);

                        if (temp == null)
                        {
                            continue;
                        }
                        string temp2 = "";
                        for (int i = 0; i < nextwords.Length - 1; i++)
                        {
                            temp2 += nextwords[i] + " ";
                        }
                        temp = temp2 + temp;

                        nextInputWord = new[] { temp.Substring(KeySegment.Length) + " " };
                        System.Console.WriteLine("Possible words:\n\t" + temp + "\t" + nextInputWord[^1]);
                        System.Console.WriteLine("\t{0} {1}", KeySegment.Length, nextInputWord[^1].Length);
                        KeySegment += Decrypt(
                            OppositeMessage.FullMessage.Substring(
                                KeySegment.Length,
                                nextInputWord[^1].Length
                                ),
                            nextInputWord[^1]);
                    }
                    else
                    {
                        System.Console.WriteLine("Possibly more than one word.");
                        nextInputWord = FindWords(nextword);

                        if (nextInputWord == null)
                        {
                            return null;
                        }
                        else
                        {
                            for (int i = 0; i < nextInputWord.Length; i++)
                            {
                                System.Console.WriteLine("OG word -> {0} ", nextInputWord[i]);
                                /*nextInputWord[i] = (nextInputWord[i].Length > inputText.Length) ?
                                    nextInputWord[i].Substring(inputText.Length) :
                                    inputText.Substring(nextInputWord[i].Length);
                                System.Console.WriteLine("{0}", nextInputWord[i]);
                                */
                                if (nextInputWord[i].Length < inputText.Length)
                                {
                                    nextInputWord[i] = inputText.Substring(nextInputWord[i].Length);
                                }
                            }
                        }
                    }

                    int localloop = 0;
                    System.Console.WriteLine("{0} Testing possible words...", loopCount);
                    foreach (var niw in nextInputWord)
                    {

                        System.Console.WriteLine("{0}. '{1}'", localloop++, niw);
                        if (niw.Length + 1 == inputText.Length)
                        {
                            continue;
                        }

                        string tobesent = niw;
                        string originalInputText = inputText;
                        if (niw.Length > inputText.Length)
                        {
                            tobesent = niw.Substring(inputText.Length) + " ";
                            inputText = niw + " ";
                            KeySegment += Decrypt(
                                OppositeMessage.FullMessage.Substring(
                                    KeySegment.Length,
                                    tobesent.Length
                                    ),
                                tobesent);

                            tobesent = Decrypt(
                                CurrentMessage.GetSegment(KeySegment.Length),
                                KeySegment).Substring(originalInputText.Length) + " ";

                            System.Console.WriteLine("New niw/inputText = {0} / {1} - {2}", tobesent, inputText, KeySegment);
                        }

                        if (knownKeyPart == null)
                        {
                            CurrentMessage.CharacterPosition = 0 + inputText.Length + 1 - tobesent.Length;
                            OppositeMessage.CharacterPosition = 0 + inputText.Length + 1 - tobesent.Length;
                        }
                        else
                        {
                            CurrentMessage.CharacterPosition = knownKeyPart.Length - 1 + tobesent.Length;
                            OppositeMessage.CharacterPosition = knownKeyPart.Length - 1 + tobesent.Length;
                        }


                        if (nextInputWord.Length > 1)
                        {
                            System.Console.WriteLine("Lengths : {0} {1}", knownKeyPart.Length, tobesent.Length);
                            int howlong = tobesent.Length;
                            if (knownKeyPart.Length + tobesent.Length > CurrentMessage.MaxLength)
                            {
                                howlong = CurrentMessage.MaxLength - knownKeyPart.Length;
                            }
                            KeySegment = (knownKeyPart != null) ?
                                Decrypt(CurrentMessage.FullMessage.Substring(knownKeyPart.Length, howlong), tobesent) :
                                Decrypt(CurrentMessage.FullMessage.Substring(KeySegment.Length, howlong), tobesent);
                            System.Console.WriteLine(KeySegment);
                        }

                        this.cmsg = !local_cmsg;
                        Console.WriteLine("'{0}' -- {1} {2}:{3}", tobesent, CurrentMessage, OppositeMessage.CharacterPosition, CurrentMessage.CharacterPosition);
                        object? result = Checker(tobesent, (knownKeyPart == null) ? KeySegment : knownKeyPart + KeySegment);
                        if (result != null)
                        {
                            foreach (var ret in (List<string>)result)
                            {
                                System.Console.WriteLine(tobesent + ret);
                                list.Add(tobesent + ret);
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }

                    if (!shownkey)
                    {
                        Console.WriteLine("{0}.{1} : {2}({3}) - {4}", roundCount, loopCount++, inputText, CurrentMessage.CharacterPosition, (cmsg) ? "msg1" : "msg2");
                        System.Console.WriteLine("'{0}' '{1}' :", KeySegment, possibleWord);
                        shownkey = true;
                    }
                    string leftover = (item.Length > KeySegment.Length) ? item.Substring(KeySegment.Length) : KeySegment.Substring(item.Length);
                    Console.WriteLine("\t{0}\n{3}. {1} - {2}", item, nextword, leftover, loopCount);

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
        if (inputSegment.Length == 0 || inputSegment[0] == ' ' || !letter_bookmarks.ContainsKey(inputSegment[0]))
        {
            return null;
        }
        int starting_from = charvals.IndexOf(inputSegment[0]);
        int index = letter_bookmarks[charvals[starting_from]];
        string? ret = null;
        while (word_list.Count > index && (starting_from + 1 != 26 ? letter_bookmarks[charvals[starting_from + 1]] : word_list.Count) > index)
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
        catch (Exception e)
        {
            return ret.ToArray();
        }
        return ret.ToArray();
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