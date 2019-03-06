using System;
using System.IO;

namespace Utils.Files
{
	public class StreamReductor : Stream
	{
		public StreamReductor(Stream original, int skip, int take)
		{
			if (!original.CanRead) throw new ArgumentException("original", "The original stream must support reading.");
			if (skip < 0) throw new ArgumentOutOfRangeException("skip", "skip is negative.");
			if (take < 1) throw new ArgumentOutOfRangeException("take", "take is less than 1.");

			this.original = original;
			this.skip = skip;
			this.take = take;

			if (original.CanSeek)
			{
				// How many take+skip tiles fit in the original stream length
				long totalTakes = original.Length / (skip + take);
				if (totalTakes > 0)
				{
					// The total bytes to be read - rem
					length = (totalTakes * take);
					// The last tile is cut either on the take or the skip side
					var rem = original.Length % (skip + take);
					// If the remainder is less than the take - get the remainder, otherwise the cut is on skip.
					length += (rem < take) ? rem : take;
				}
				else length = original.Length > take ? take : original.Length;
			}
			else length = -1;
		}

		public override bool CanRead => true;
		public override bool CanSeek => original.CanSeek;
		public override bool CanWrite => false;
		public override long Length => original.CanSeek ? length : -1;
		public override long Position
		{
			get => pos;
			set
			{
				if (!original.CanSeek) throw new NotSupportedException("The original stream cannot seek.");
				if (value < 0 || value > length) throw new ArgumentOutOfRangeException("Position");

				pos = value;
			}
		}

		public override void Flush() => original.Flush();

		public override int Read(byte[] buffer, int offset, int count)
		{
			int nTakes = (int)(pos / take); // will truncate by casting
			long sPos = pos + (nTakes * skip); // always starts with a take thus the skips are -1

			int totalRead = 0, read = 0;
			int toRead = take; // can't read more without seeking the original stream
			int left = (int)(pos % take); // initially set as the tile remainder of the tile
			if (left < 1) left = count - totalRead; // in case the remainder is 0 set the real left count

			while (left > 0)
			{
				// Seek the original one way or the other
				if (original.CanSeek) original.Seek(sPos, SeekOrigin.Begin);
				else
					// this is terribly slow, should make use of a bigger array
					while (original.Position < sPos)
						original.Read(onebyte, 0, 1);

				// read up to the end of the tile
				while (toRead > 0)
				{
					read = original.Read(buffer, offset, toRead);

					if (read > 0)
					{
						offset += read;
						totalRead += read;
						pos += read;
						sPos += read;
						toRead -= read;
					}
					else break;
				}

				// There is no more data, break
				if (toRead > 0 && read < 1) break;

				left = count - totalRead;
				// Take the remainder or the whole tile
				toRead = left < take ? left : take;

				// Can't check for out-of-range if the stream cannot seek
				if (left > 0) sPos += skip;
			}

			return totalRead;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			if (!original.CanSeek) throw new NotSupportedException("The original stream doesn't support seeking.");
			if (offset < 0 || offset > length) throw new ArgumentOutOfRangeException("offset");

			var updPos = pos;

			switch (origin)
			{
				case SeekOrigin.Begin:
				updPos = offset;
				break;
				case SeekOrigin.Current:
				updPos += offset;
				if (updPos > length) throw new ArgumentOutOfRangeException("offset", "The position greater than the length.");
				break;
				case SeekOrigin.End:
				updPos = length + offset;
				if (updPos < 0) throw new ArgumentOutOfRangeException("offset", "The position is negative.");
				break;
				default: throw new ArgumentException();
			}

			pos = updPos;

			return pos;
		}

		public override void SetLength(long value) => throw new NotSupportedException();
		public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

		readonly byte[] onebyte = new byte[1];
		readonly Stream original;
		long pos;
		readonly long length;
		readonly int skip;
		readonly int take;
	}
}
