using System;
using CuddleKit.Serialization;

namespace Cuddle.Cli
{
	class Program
	{
		static void Main(string[] args)
		{
			const string script = @"   \ // dsssd
\ // уа2""##";

			//using var stream = new StringReader(script);
			using var reader = new Reader(script);

			using var document = Document.Deserialize("a; b; c");
			document.Read("d; e; f");

			Console.WriteLine("Hello World! \u10ff33d");
		}
	}
}
