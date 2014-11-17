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


using System.IO;
using System.Text;
using Microsoft.Win32;

namespace GenericHelpers
{
	public static class PathUtils
	{
		public static string GetExtension(string path)
		{
			var res = Path.GetExtension(path);
			if (res == null) return res;
			return res.Trim('.');
		}

		private readonly static string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
		private readonly static byte[] _preamble = Encoding.UTF8.GetPreamble();

		public static string RemoveBom(string result, byte[] data)
		{
			if (data.Length > _preamble.Length)
			{
				if (data[0] == _preamble[0] && data[1] == _preamble[1] && data[2] == _preamble[2])
				{
					return result.Remove(0, _byteOrderMarkUtf8.Length);
				}
			}
			return result;
		}

		public static string RemoveBom(string result)
		{
			if (result.Length < _byteOrderMarkUtf8.Length) return result;

			var data = Encoding.UTF8.GetBytes(result.Substring(0, _byteOrderMarkUtf8.Length));
			if (data[0] == _preamble[0] && data[1] == _preamble[1] && data[2] == _preamble[2])
			{
				return result.Remove(0, _byteOrderMarkUtf8.Length);
			}
			return result;
		}

		public static int HasBom(byte[] data)
		{
			if (data.Length < _preamble.Length) return 0;
			if (data[0] == _preamble[0] && data[1] == _preamble[1] && data[2] == _preamble[2])
			{
				return _preamble.Length;
			}
			return 0;
		}
	}
}
