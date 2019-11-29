using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Utils.Files
{
	public class Files
	{
		public const string GA_NOPRINT = "-noprint";
		const int INFO_LINE_WIDTH = 65;
		static RunArgs runArgs = new RunArgs();

		public static int Run(string[] args)
		{
			if (args.Length < 1)
			{
				var t = typeof(Files);
				var v = t.Assembly.GetName().Version;
				var vs = $"v{v.Major}.{v.Minor}.{v.Build}";
				$"File utils {vs}".PrintHeader();
			}
			else if (Array.Exists(args, (x) => x == GA_NOPRINT)) Volatile.Write(ref Utils.SuppressPrint, true);

			Utils.PrintLine("");
			Utils.PrintLine("");

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
					$"{Environment.NewLine} ({u.Value.Name}): {Environment.NewLine}".PrintHeader();
					foreach (var line in u.Value.Info.Slice(INFO_LINE_WIDTH))
						$"   {line}".PrintLine();
				}

				var a = String.Empty;
				"".PrintLine();

				while (true)
				{
					do
					{
						if (a == "q") return 0;
						Utils.ReadString("Choose an action or type 'q' to exit: ", ref a);
					} while (!pMap.ContainsKey(a));
					TryRunProgram(pMap[a]);
				}
			}
			else if (inArgs.Map.ContainsKey("-p"))
			{
				var subProg = inArgs.Map["-p"];
				if (subProg != null && subProg.Count > 0)
					if (pMap.ContainsKey(subProg[0])) return TryRunProgram(pMap[subProg[0]]);
					else throw new Exception("Unknown subprogram.");
			}

			return 0;
		}

		static int TryRunProgram(IUtil utl)
		{
			var r = -1;

			try
			{
				$"[{utl.Name}]".PrintLine(ConsoleColor.Green);
				r = utl.Run(runArgs);
			}
			catch (Exception ex)
			{
				ex.Message.PrintLine(ConsoleColor.Red, ConsoleColor.White);
			}

			return r;
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
