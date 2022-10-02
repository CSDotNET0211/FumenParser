// See https://aka.ms/new-console-template for more information


using Fumen;
using System.Text;

var url = "v115@khA8JeAgWDAmS8CAvhAAAPAA";

while (true)
{
    Console.WriteLine("input");
    url = Console.ReadLine();

    //        "v115@vhAgWDAmS8CA"
    var decode = FumenParser.Decode(url);
    var encode = FumenParser.Encode(decode);
    Console.WriteLine(FumenParser.Poll(2, "Je")/240);
    Console.WriteLine("ori:"+url.Replace("?",""));
    Console.WriteLine("enc:"+encode);
    Console.WriteLine(decode.Pages[0].Comment);

    var decode2 = FumenParser.Decode(encode);
    var encode2 = FumenParser.Encode(decode2);
    Console.WriteLine("en2:"+encode2);
    if (url.Replace("?", "") == encode&&encode == encode2)
        Console.WriteLine("true");
    else
        Console.WriteLine("false");
    Console.WriteLine(decode2.Pages[0].Comment);

    Console.WriteLine();

}

