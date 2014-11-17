// ===========================================================
// Copyright (C) 2014-2015 Kendar.org
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation 
// files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, 
// modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software 
// is furnished to do so, subject to the following conditions:
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS 
// BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF 
// OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ===========================================================


using System.Collections.Generic;

namespace Node.Cs.Lib.Utils
{
	public static class ByteMatch
	{
		private static readonly int[] Empty = new int[0];

		public static int IndexOf(byte[] self, byte candidate, int startAt = 0, int length = -1)
		{
			if (startAt >= self.Length)
			{
				return -1;
			}
			if (length == -1) length = self.Length;
			else length = startAt + length;

			for (int i = startAt; i < length; i++)
			{
				if (candidate != self[i])
				{
					continue;
				}

				return i;
			}
			return -1;
		}

		public static int IndexOf(byte[] self, byte[] candidate, int startAt = 0, int length = -1)
		{
			if (IsEmptyLocate(self, candidate))
			{
				return -1;
			}
			if (startAt >= self.Length)
			{
				return -1;
			}
			if (length == -1) length = self.Length;
			else length = startAt + length;

			for (int i = startAt; i < length; i++)
			{
				if (!IsMatch(self, i, candidate))
				{
					continue;
				}

				return i;
			}
			return -1;
		}

		public static IEnumerable<int> Matches(byte[] self, byte[] candidate, int startAt = 0, int length = -1)
		{
			if (IsEmptyLocate(self, candidate))
			{
				yield break;
			}

			if (startAt >= self.Length)
			{
				yield break;
			}
			if (length == -1) length = self.Length;
			else length = startAt + length;

			for (int i = startAt; i < length; i++)
			{
				if (!IsMatch(self, i, candidate))
				{
					continue;
				}

				yield return i;
			}
		}

		private static bool IsMatch(byte[] array, int position, byte[] candidate)
		{
			if (candidate.Length > (array.Length - position))
			{
				return false;
			}

			for (int i = 0; i < candidate.Length; i++)
			{
				if (array[position + i] != candidate[i])
				{
					return false;
				}
			}

			return true;
		}

		private static bool IsEmptyLocate(byte[] array, byte[] candidate)
		{
			return array == null
				|| candidate == null
				|| array.Length == 0
				|| candidate.Length == 0
				|| candidate.Length > array.Length;
		}
	}
}
