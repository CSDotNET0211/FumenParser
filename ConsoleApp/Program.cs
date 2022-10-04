using Fumen;
using System.Text;

var url = "";

while (true)
{
    var test = 0;
    //Console.WriteLine(FumenParser.DecodeComment("DAFLrAA", ref test));

    //v115@9gwhBeQ4EeAtwhBeR4CeBtwhRpg0Q4CeAtglwhRpi0?AeilJeAgHvhA8iB
    // Console.WriteLine(FumenParser.PollRevert(2,8*220+219));
    Console.WriteLine("input");
    url = Console.ReadLine();

    var decode = FumenParser.Decode(url);
    var encode = FumenParser.Encode(decode);
    Console.WriteLine("ori:" + url.Replace("?", ""));
    Console.WriteLine("enc:" + encode);

    var decode2 = FumenParser.Decode(encode);
    var encode2 = FumenParser.Encode(decode2);
    Console.WriteLine("en2:" + encode2);
    if (url.Replace("?", "") == encode && encode == encode2)
        Console.WriteLine("true");
    else
        Console.WriteLine("false");

    Console.WriteLine();

}

