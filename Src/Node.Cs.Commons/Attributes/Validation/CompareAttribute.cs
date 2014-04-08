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
	public class CompareAttribute : ValidationAttribute, IValidationAttribute
	{
		private readonly string _withField;

		public string WithField
		{
			get
			{
				return _withField;
			}
		}

		public CompareAttribute(string withField)
		{
			_withField = withField;
		}

		public bool IsValid(object value, Type type)
		{
			return false;
		}

		public bool IsValid(object value, object toCompareValue)
		{
			if (value == null && toCompareValue == null) return true;
			if (value == null || toCompareValue == null) return false;
			return value.Equals(toCompareValue);
		}
	}
}