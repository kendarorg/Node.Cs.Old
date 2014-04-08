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
	public class RangeAttribute : ValidationAttribute, IValidationAttribute
	{
		private readonly object _min;
		private readonly object _max;

		public RangeAttribute(object min, object max = null)
		{
			_min = min;
			_max = max;
		}

		public bool IsValid(object value, Type type)
		{
			if (value == null) return false;

			var partial = false;
			if (_min is Int32) partial = Convert.ToInt32(value) >= (Int32)_min;
			else if (_min is Decimal) partial = Convert.ToDecimal(value) >= (Decimal)_min;
			else if (_min is Int64) partial = Convert.ToInt64(value) >= (Int64)_min;
			else if (_min is Double) partial = Convert.ToDouble(value) >= (Double)_min;
			else if (_min is Single) partial = Convert.ToSingle(value) >= (Single)_min;

			if (!partial) return false;
			if (_max == null) return true;

			if (_max is Int32) partial = Convert.ToInt32(value) <= (Int32)_max;
			else if (_max is Decimal) partial = Convert.ToDecimal(value) <= (Decimal)_max;
			else if (_max is Int64) partial = Convert.ToInt64(value) <= (Int64)_max;
			else if (_max is Double) partial = Convert.ToDouble(value) <= (Double)_max;
			else if (_max is Single) partial = Convert.ToSingle(value) <= (Single)_max;
			return partial;
		}
	}
}