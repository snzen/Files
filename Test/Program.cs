using System;

namespace Test
{
	class Program
	{
		static void Main(string[] args)
		{
			var runner = new TestSurface.Runner();

			runner.Run(args);

			Console.ReadLine();
		}
	}
}
