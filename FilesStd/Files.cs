using System;
using System.Collections.Generic;
using System.Linq;

namespace Utils.Files
{
	public class Files
	{
		const int INFO_LINE_WIDTH = 65;
		static RunArgs runArgs = new RunArgs();

		public static void Run(string[] args)
		{
			var P = GetPrograms();
			var pMap = new Dictionary<string, IUtil>();
			var inArgs = new ArgsParser(args);
			runArgs.InArgs = inArgs.Map;

			foreach (var p in P)
			{
				var utl = Activator.CreateInstance(p) as IUtil;
				if (utl != null)
					pMap.Add(utl.Name, utl);
			}

			if (args == null || args.Length < 1)
			{
				"Available subprograms:".PrintHeader(true);
				"   Launch directly with the -p switch: files -p take ...".PrintLine();

				foreach (var u in pMap)
				{
					Console.WriteLine();
					$" ({u.Value.Name}):".PrintHeader();
					Console.WriteLine();
					foreach (var line in u.Value.Info.Slice(INFO_LINE_WIDTH))
						$"   {line}".PrintLine();
				}

				var a = String.Empty;
				Console.WriteLine();

				while (true)
				{
					do
					{
						if (a == "q") return;
						Utils.ReadString("Choose an action or type 'q' to exit: ", ref a);
					} while (!pMap.ContainsKey(a));
					TryRunProgram(pMap[a]);
				}

			}
			else if (inArgs.Map.ContainsKey("-p"))
			{
				var subProg = inArgs.Map["-p"];
				if (subProg != null && subProg.Count > 0)
					if (pMap.ContainsKey(subProg[0])) TryRunProgram(pMap[subProg[0]]);
					else throw new Exception("Unknown subprogram.");
			}

			Console.ReadLine();
		}

		static void TryRunProgram(IUtil utl)
		{
			try
			{
				$"[{utl.Name}]".PrintLine(ConsoleColor.Green);
				utl.Run(runArgs);
			}
			catch (Exception ex)
			{
				ex.Message.PrintLine(ConsoleColor.Red, ConsoleColor.White);
			}
		}

		static List<Type> GetPrograms()
		{
			return AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(s => s.GetTypes())
				.Where(p => typeof(IUtil).IsAssignableFrom(p) && p.IsClass)
				.ToList();
		}
	}
}
