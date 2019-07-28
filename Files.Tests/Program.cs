using System;

namespace Test
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length < 1) args = new string[] { "+all"};

			new TestSurface.SurfaceLauncher().Start(args);

			Console.ReadLine();
		}
	}
}
