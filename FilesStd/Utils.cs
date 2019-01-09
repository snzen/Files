using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

namespace Utils.Files
{
	static class Utils
	{
		public static void ReadString(this string msg, ref string target, bool loop = false)
		{
			do
			{
				msg.PrintInfo();
				var s = Console.ReadLine();
				if (string.IsNullOrEmpty(s)) continue;
				target = s;
				return;
			} while (loop);
		}


		public static bool PickOption(this string msg, ref string target, bool loop, params string[] options)
		{
			if (options == null || options.Length < 1) throw new ArgumentNullException("options");

			do
			{
				msg.PrintInfo();
				var s = Console.ReadLine();
				if (string.IsNullOrEmpty(s)) continue;
				if (!options.Contains(s)) continue;
				target = s;
				return true;
			} while (loop);

			return false;
		}

		public static bool ReadWord(this string msg, string matchword)
		{
			msg.PrintInfo();
			return Console.ReadLine() == matchword;
		}

		public static void ReadInt(this string msg, ref int target, bool loop = false)
		{
			do
			{
				msg.PrintInfo();
				int temp = 0;
				if (!int.TryParse(Console.ReadLine(), out temp)) continue;
				target = temp;
				return;
			} while (loop);
		}

		public static void ReadDouble(this string msg, ref double target, bool loop = false)
		{
			do
			{
				msg.PrintInfo();
				double temp = 0;
				if (!double.TryParse(Console.ReadLine(), out temp)) continue;
				target = temp;
				return;
			} while (loop);
		}

		public static void ReadIntIn(this string msg, ref int target, int[] accepted, bool loop = false)
		{
			do
			{
				msg.PrintInfo();
				int temp = 0;
				if (!int.TryParse(Console.ReadLine(), out temp) && accepted.Contains(temp)) continue;
				target = temp;
				return;
			} while (loop);
		}


		public static void PrintHeader(this string msg, bool nl = false) =>
		Print(msg, ConsoleColor.Cyan, null, nl);
		public static void PrintSysInfo(this string msg, bool nl = false) =>
			Print(msg, ConsoleColor.White, ConsoleColor.DarkBlue, nl);
		public static void PrintSysError(this string msg, bool nl = false) =>
			Print(msg, ConsoleColor.Red, ConsoleColor.Black, nl);
		public static void PrintSysWarn(this string msg, bool nl = false) =>
			Print(msg, ConsoleColor.Yellow, ConsoleColor.DarkBlue, nl);
		public static void PrintInfo(this string msg, bool nl = false) =>
			Print(msg, ConsoleColor.Yellow, null, nl);

		public static void Print(this string msg, ConsoleColor? fc = null, ConsoleColor? bc = null, bool newLine = false)
		{
			bool acq = false;
			sync.Enter(ref acq);

			if (acq)
			{
				var cfc = Console.ForegroundColor;
				var cbc = Console.BackgroundColor;
				if (fc.HasValue) Console.ForegroundColor = fc.Value;
				if (bc.HasValue) Console.BackgroundColor = bc.Value;
				Console.Write(msg);
				Console.ForegroundColor = cfc;
				Console.BackgroundColor = cbc;
				if (newLine) Console.WriteLine();

				sync.Exit();
			}
		}

		public static void PrintLine(this string msg, ConsoleColor? fc = null, ConsoleColor? bc = null) =>
			Print(msg, fc, bc, true);

		public static IEnumerable<string> Slice(this string line, int maxLen)
		{
			var sb = new StringBuilder();
			var len = 0;

			foreach (var l in line.Split(WSEP, StringSplitOptions.None))
			{
				foreach (var w in l.Split(' '))
				{
					if (len + w.Length > maxLen)
					{
						yield return sb.ToString();
						sb.Clear();
						len = 0;
					}

					sb.Append(w + ' ');
					len += w.Length;
				}
				yield return sb.ToString();
				sb.Clear();
				len = 0;
			}


			if (sb.Length > 0)
				yield return sb.ToString();
		}

		public static List<int> Randomize(int len)
		{
			var R = new List<int>(Enumerable.Range(0, len));
			var rdm = new Random();
			for (var i = 0; i < R.Count; i++)
			{
				var j = rdm.Next(0, R.Count);
				var t = R[i];
				R[i] = R[j];
				R[j] = t;
			}

			return R;
		}

		public static void Randomize<T>(T[] arr)
		{
			var R = Randomize(arr.Length);
			for (var i = 0; i < arr.Length; i++)
			{
				var ri = R[i];
				var tmp = arr[i];
				arr[i] = arr[ri];
				arr[ri] = tmp;
			}
		}

		public static string ToJson<T>(this T o)
		{
			using (var ms = new MemoryStream())
			{
				var s = new DataContractJsonSerializer(typeof(T));

				s.WriteObject(ms, o);
				var bytes = ms.ToArray();

				return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
			}
		}

		public static string ToXml<T>(this T o)
		{
			using (var ms = new MemoryStream())
			{
				var s = new XmlSerializer(typeof(T));

				s.Serialize(ms, o);
				var bytes = ms.ToArray();

				return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
			}
		}

		static readonly string[] WSEP = new string[] { Environment.NewLine };
		static SpinLock sync = new SpinLock();
	}
}
