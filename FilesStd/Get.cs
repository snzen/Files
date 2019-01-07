using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Utils.Files
{
	public class Get : IUtil
	{
		public string Name => "get";
		public string Info =>
			"Downloads resources listed in a map file with each link on a separate line. " + Environment.NewLine +
			"Args: not interactive (-ni), links file (-f), base url (-base), destination dir (-dest), max req/sec [5.0] (-rps) " +
			"from file row [0] (-from), to file row [last] (-to)";


		public void Run(RunArgs ra)
		{
			bool interactive = !ra.InArgs.ContainsKey("-ni");
			var baseUrl = string.Empty;
			var linksFile = string.Empty;
			var reqPerSec = 5.0;
			var fromIdx = 0;
			var toIdx = -1;

			if (interactive)
			{
				Utils.ReadString("links file: ", ref linksFile, true);
				Utils.ReadString("destination dir: ", ref ra.State.DestinationDir, true);
				Utils.ReadString("base url: ", ref baseUrl);
				Utils.ReadDouble("req/sec: ", ref reqPerSec);
				Utils.ReadInt("from row: ", ref fromIdx);
				Utils.ReadInt("to row: ", ref toIdx);
			}
			else
			{
				linksFile = ra.InArgs.GetFirstValue("-f");
				baseUrl = ra.InArgs.GetFirstValue("-base");
				ra.State.DestinationDir = ra.InArgs.GetFirstValue("-dest");
				if (ra.InArgs.ContainsKey("-rps"))
					reqPerSec = double.Parse(ra.InArgs.GetFirstValue("-rps"));
				if (ra.InArgs.ContainsKey("-from"))
					fromIdx = int.Parse(ra.InArgs.GetFirstValue("-from"));
				if (ra.InArgs.ContainsKey("-to"))
					toIdx = int.Parse(ra.InArgs.GetFirstValue("-to"));
			}

			var links = File.ReadAllLines(linksFile);
			if (toIdx < 0) toIdx = links.Length;

			if (fromIdx > 0 && fromIdx > links.Length) throw new ArgumentOutOfRangeException("from");
			if (toIdx > 0 && (toIdx > links.Length || toIdx <= fromIdx)) throw new ArgumentOutOfRangeException("to");

			links = links.Skip(fromIdx).Take(toIdx - fromIdx).ToArray();

			var LINKS_COUNT = links.Length;
			var c = 0;
			var counter = new CountdownEvent(LINKS_COUNT);
			var startTime = DateTime.Now;


			if (links != null && LINKS_COUNT > 0)
				for (int i = 0; i < LINKS_COUNT; i++)
				{
					var rps = i / DateTime.Now.Subtract(startTime).TotalSeconds;

					while (rps > reqPerSec)
					{
						Thread.Sleep(100);
						rps = i / DateTime.Now.Subtract(startTime).TotalSeconds;
					}

					new Task(async (o) =>
					{
						try
						{
							using (var webClient = new HttpClient())
							{
								var idx = (int)o;
								var link = links[idx];
								var url = string.Format("{0}/{1}", baseUrl, link);
								var fn = Path.Combine(ra.State.DestinationDir, link);
								var bytes = await webClient.GetByteArrayAsync(url);
								using (var fs = new FileStream(
									fn, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, bytes.Length, true))
									await fs.WriteAsync(bytes, 0, bytes.Length);
								string.Format("{0, 6}/{1}| {2, 6}|K {3}",
									Interlocked.Increment(ref c), LINKS_COUNT, bytes.Length / 1000, fn).PrintInfo(true);
							}
						}
						catch (Exception ex)
						{
							$"@ link {links[i]}".PrintSysError(true);
							ex.Message.PrintSysError(true);
						}
						finally
						{
							counter.Signal();
						}
					}, i).Start();
				}

			counter.Wait();
			Console.WriteLine();
			var dur = DateTime.Now.Subtract(startTime);
			$"Done [{dur.Hours}h {dur.Minutes}m {dur.Seconds}s].".PrintInfo(true);
		}
	}
}
