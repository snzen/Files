using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TestSurface;
using Utils.Files;

namespace Test
{
	class StreamReductorMultiCase : ITestSurface
	{
		public string Info => "Test the Files.StreamReductor";
		public string FailureMessage { get; set; }
		public bool? Passed { get; set; }
		public bool IsComplete { get; set; }
		public bool IndependentLaunchOnly => false;

		public async Task Run(IDictionary<string, List<string>> args)
		{
			NormalReduction();
			if (!Passed.HasValue || Passed.Value) NonSeekableMemoryStream();
			if (!Passed.HasValue || Passed.Value) OutOfBounds();
			if (!Passed.HasValue || Passed.Value) SmallerThanTake();
			if (!Passed.HasValue || Passed.Value) SmallerThanSkipTake();
		}

		public void NormalReduction()
		{
			"> Normal Reduction:".AsInfo();

			var bytes = new byte[1000];
			byte b = 0;

			for (int i = 0; i < bytes.Length; i++)
				bytes[i] = b++;

			const int SKIP = 100;
			const int TAKE = 20;
			const int READ = 77;
			var readBuff = new byte[READ];

			using (var ms = new MemoryStream(bytes))
			using (var rs = new StreamReductor(ms, SKIP, TAKE))
			{
				rs.Read(readBuff, 0, READ);

				for (int i = 0; i < TAKE; i++)
					if (readBuff[i] != i)
					{
						Passed = false;
						break;
					}

				for (int i = 0; i < TAKE; i++)
					if (readBuff[TAKE + i] != TAKE + SKIP + i)
					{
						Passed = false;
						break;
					}

				if (Passed.HasValue && !Passed.Value) FailureMessage = "Normal Reduction on seek-able stream failed to read the correct bytes";
				else
				{
					Passed = true;
					"[OK] Normal Reduction".AsSuccess();
				}
			}
		}

		public void SmallerThanTake()
		{
			"> The data is smaller than a TAKE:".AsInfo();

			var bytes = new byte[100];
			byte b = 0;

			for (int i = 0; i < bytes.Length; i++)
				bytes[i] = b++;

			const int SKIP = 400;
			const int TAKE = 200;
			const int READ = 1000;
			var readBuff = new byte[READ];

			using (var ms = new MemoryStream(bytes))
			using (var rs = new StreamReductor(ms, SKIP, TAKE))
			{
				rs.Read(readBuff, 0, TAKE);

				for (int i = 0; i < bytes.Length; i++)
					if (readBuff[i] != i)
					{
						Passed = false;
						break;
					}

				if (readBuff[bytes.Length] != 0) Passed = false;

				if (Passed.HasValue && !Passed.Value) FailureMessage = "Smaller than a TAKE fails.";
				else
				{
					Passed = true;
					"[OK] SmallerThanTake".AsSuccess();
				}
			}
		}

		public void SmallerThanSkipTake()
		{
			"> The data is smaller than a SKIP + TAKE:".AsInfo();

			var bytes = new byte[100];
			byte b = 0;

			for (int i = 0; i < bytes.Length; i++)
				bytes[i] = b++;

			const int SKIP = 80;
			const int TAKE = 40;
			const int READ = 200;
			var readBuff = new byte[READ];

			using (var ms = new MemoryStream(bytes))
			using (var rs = new StreamReductor(ms, SKIP, TAKE))
			{
				rs.Read(readBuff, 0, READ);

				var LIM = Math.Min(TAKE, bytes.Length);

				for (int i = 0; i < LIM; i++)
					if (readBuff[i] != i)
					{
						Passed = false;
						break;
					}

				if (readBuff[LIM] != 0) Passed = false;

				if (Passed.HasValue && !Passed.Value) FailureMessage = "Smaller than SKIP + TAKE fails.";
				else
				{
					Passed = true;
					"[OK] SmallerThanSkipTake".AsSuccess();
				}
			}
		}

		public void NonSeekableMemoryStream()
		{
			"> NonSeekableMemoryStream:".AsInfo();

			var bytes = new byte[1000];
			byte b = 0;

			for (int i = 0; i < bytes.Length; i++)
				bytes[i] = b++;

			const int SKIP = 100;
			const int TAKE = 20;
			const int READ = 77;
			var readBuff = new byte[READ];

			using (var ms = new NonSeekableMemoryStream(bytes))
			using (var rs = new StreamReductor(ms, SKIP, TAKE))
			{
				rs.Read(readBuff, 0, READ);

				for (int i = 0; i < TAKE; i++)
					if (readBuff[i] != i)
					{
						Passed = false;
						break;
					}

				for (int i = 0; i < TAKE; i++)
					if (readBuff[TAKE + i] != TAKE + SKIP + i)
					{
						Passed = false;
						break;
					}

				if (Passed.HasValue && !Passed.Value) FailureMessage = "Normal Reduction on seek-able stream failed to read the correct bytes";
				else
				{
					Passed = true;
					"[OK] NonSeekableMemoryStream".AsSuccess();
				}
			}
		}

		public void OutOfBounds()
		{
			"> OutOfBounds:".AsInfo();
			"Will attempt to take more data than there is after the reduction. The Cut will happen on a TAKE.".AsInnerInfo();

			var bytes = new byte[100];
			byte b = 0;

			for (int i = 0; i < bytes.Length; i++)
				bytes[i] = b++;

			const int SKIP = 20;
			const int TAKE = 25;
			const int READ = 100; // More than there is after the reduction
			var readBuff = new byte[READ];

			using (var ms = new NonSeekableMemoryStream(bytes))
			using (var rs = new StreamReductor(ms, SKIP, TAKE))
			{
				rs.Read(readBuff, 0, READ);

				var nonZero = 0;

				for (int i = 1; i < readBuff.Length; i++)
					if (readBuff[i] != 0)
						nonZero++;

				if (nonZero != 59)
				{
					FailureMessage = "OutOfBounds read more or less data than it should.";
					Passed = false;
				}

				if (!Passed.HasValue) Passed = true;
				if (Passed.Value) "[OK] OutOfBounds".AsSuccess();
			}
		}
	}

	public class NonSeekableMemoryStream : MemoryStream
	{
		public NonSeekableMemoryStream(byte[] bytes) : base(bytes) { }

		public override bool CanSeek => false;
	}
}
