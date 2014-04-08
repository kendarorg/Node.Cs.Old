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

namespace Node.Cs.Lib.Attributes.Validation
{
	public class StringLengthAttribute : ValidationAttribute, IValidationAttribute
	{

		public int MinimumLength { get; set; }
		private readonly int _length;
		public int MaximumLength { get { return _length; } }

		public StringLengthAttribute(int length)
		{
			MinimumLength = 0;
			_length = length;
			ErrorMessage = "Wrong length. Must be between ";
			ErrorMessage += MinimumLength;
			ErrorMessage += " and ";
			ErrorMessage += _length;
			ErrorMessage += ".";
		}

		public bool IsValid(object value, Type type)
		{
			var strValue = value as string;
			if (strValue == null && MinimumLength == 0) return true;

			return strValue.Length >= MinimumLength && strValue.Length <= _length;
		}
	}
}