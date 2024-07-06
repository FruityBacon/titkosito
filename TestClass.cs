using Xunit;

namespace titkosito;
public class TestClass
{
    [Fact]
    public void helloworld_Test()
    {
        Secret secret = new Secret();
        Assert.Equal("hfnosauytm", secret.Encrypt("helloworld", "abcdefghijklmnop"));
        Assert.Equal("helloworld", secret.Decrypt("hfnosauytm", "abcdefghijklmnop"));
    }

    [Fact]
    public void Cracker_Test()
    {
        StreamReader sr = new StreamReader("words.txt");
        List<string> list = new();
        while (!sr.EndOfStream)
        {
            list.Add(sr.ReadLine());
        }
        string key = "abcdefghijklmnopqrstuvwx";
        string message1 = "ebtobehpzmjnmfqwuirlazvslpm";
        string message2 = "cvtlsxo fiutxysspjzxtxwp";
        Cracker cracker = new Cracker(message1, message2, list);

        cracker.Start();

        Assert.Contains(key,cracker.PossibleKeys);
    }

}