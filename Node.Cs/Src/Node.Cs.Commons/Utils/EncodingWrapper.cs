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


using System.Text;

namespace Node.Cs.Lib.Utils
{
	public class EncodingWrapper : Encoding
	{
		private readonly Encoding _encoding;

		public EncodingWrapper(Encoding encoding)
		{
			_encoding = encoding;
		}

		public override byte[] GetPreamble()
		{
			return new byte[0];
		}

		public override int GetByteCount(char[] chars, int index, int count)
		{
			return _encoding.GetByteCount(chars, index, count);
		}

		public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
		{
			return _encoding.GetBytes(chars, charIndex, charCount, bytes, byteIndex);
		}

		public override int GetCharCount(byte[] bytes, int index, int count)
		{
			return _encoding.GetCharCount(bytes, index, count);
		}

		public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
		{
			return _encoding.GetChars(bytes, byteIndex, byteCount, chars, charIndex);
		}

		public override int GetMaxByteCount(int charCount)
		{
			return _encoding.GetMaxByteCount(charCount);
		}

		public override int GetMaxCharCount(int byteCount)
		{
			return _encoding.GetMaxCharCount(byteCount);
		}

		public override string BodyName
		{
			get { return _encoding.BodyName; }
		}

		public override object Clone()
		{
			return new EncodingWrapper((Encoding)_encoding.Clone());
		}

		public override int CodePage
		{
			get { return _encoding.CodePage; }
		}

		public override string EncodingName
		{
			get { return _encoding.EncodingName; }
		}

		public override Decoder GetDecoder()
		{
			return _encoding.GetDecoder();
		}

		public override Encoder GetEncoder()
		{
			return _encoding.GetEncoder();
		}

		public override string HeaderName
		{
			get { return _encoding.HeaderName; }
		}

		public override bool IsAlwaysNormalized(NormalizationForm form)
		{
			return _encoding.IsAlwaysNormalized(form);
		}

		public override bool IsBrowserDisplay
		{
			get { return _encoding.IsBrowserDisplay; }
		}

		public override bool IsBrowserSave
		{
			get { return _encoding.IsBrowserSave; }
		}

		public override bool IsMailNewsDisplay
		{
			get { return _encoding.IsMailNewsDisplay; }
		}

		public override bool IsMailNewsSave
		{
			get { return _encoding.IsMailNewsSave; }
		}

		public override bool IsSingleByte
		{
			get { return _encoding.IsSingleByte; }
		}

		public override string WebName
		{
			get { return _encoding.WebName; }
		}

		public override int WindowsCodePage
		{
			get { return _encoding.WindowsCodePage; }
		}
	}
}