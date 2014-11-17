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


using System;

namespace GenericHelpers
{
	public static class WildcardCompareExtension
	{
		/// <summary>
		/// Thanks to:
		/// http://stackoverflow.com/questions/2433998/system-stringcomparer-that-supports-wildcard
		/// </summary>
		public static bool WildcardCompare(this string toCheck, string mask, StringComparison stringComparison = StringComparison.Ordinal)
		{
			int i = 0, k = 0;

			while (k != toCheck.Length)
			{
				if (i > mask.Length - 1)
					return false;

				switch (mask[i])
				{
					case '*':
						{
							if ((i + 1) == mask.Length)
							{
								return true;
							}

							while (k != toCheck.Length)
							{
								if (string.Compare(toCheck.Substring(k + 1), mask.Substring(i + 1), stringComparison) == 0)
								{
									return true;
								}
								k += 1;
							}
						}
						return false;
					case '?':
						break;
					default:
						if (string.Compare(toCheck.Substring(k, 1), mask.Substring(i, 1), stringComparison) != 0)
						{
							return false;
						}
						break;
				}

				i += 1;
				k += 1;
			}

			if (k == toCheck.Length)
			{
				if (i == mask.Length || mask[i] == ';' || mask[i] == '*')
					return true;
			}

			return false;
		}
	}
}
