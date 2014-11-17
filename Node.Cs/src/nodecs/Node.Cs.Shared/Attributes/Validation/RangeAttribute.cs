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
using System.Collections.Generic;

namespace NodeCs.Shared.Attributes.Validation
{
	public class RangeAttribute : ValidationAttribute, IValidationAttribute
	{
		private readonly object _min;
		private readonly object _max;

		public object Min
		{
			get { return _min; }
		}
		public object Max
		{
			get { return _max; }
		}

		public RangeAttribute(object min, object max = null)
		{
			_min = min;
			_max = max;
		}

		public bool IsValid(object value, Type type)
		{
			try
			{
				if (value == null) return false;

				var partial = false;
				var typeName = _min.GetType().Name;
				switch (typeName)
				{
					case ("Decimal"):
						partial = Convert.ToDecimal(value) >= Convert.ToDecimal(_min);
						break;
					case ("Int64"):
					case ("Int32"):
					case ("Int16"):
						partial = Convert.ToInt64(value) >= Convert.ToInt64(_min);
						break;
					case ("UInt64"):
					case ("UInt32"):
					case ("UInt16"):
					case ("Boolean"):
						partial = Convert.ToUInt64(value) >= Convert.ToUInt64(_min);
						break;
					case ("Double"):
						partial = Convert.ToDouble(value) >= Convert.ToDouble( _min);
						break;
					case ("Single"):
						partial = Convert.ToSingle(value) >= Convert.ToSingle( _min);
						break;
					case ("DateTime"):
						partial = Convert.ToDateTime(value).ToUniversalTime() >= Convert.ToDateTime(_min).ToUniversalTime();
						break;
				}

				if (!partial) return false;
				if (_max == null) return true;

				typeName = _max.GetType().Name;
				switch (typeName)
				{
					case ("Decimal"):
						partial = Convert.ToDecimal(value) <= Convert.ToDecimal(_max);
						break;
					case ("Int64"):
					case ("Int32"):
					case ("Int16"):
						partial = Convert.ToInt64(value) <=Convert.ToInt64( _max);
						break;
					case ("UInt64"):
					case ("UInt32"):
					case ("UInt16"):
					case ("Boolean"):
						partial = Convert.ToUInt64(value) <= Convert.ToUInt64(_max);
						break;
					case ("Double"):
						partial = Convert.ToDouble(value) <=Convert.ToDouble( _max);
						break;
					case ("Single"):
						partial = Convert.ToSingle(value) <= Convert.ToSingle(_max);
						break;
					case ("DateTime"):
						partial = Convert.ToDateTime(value).ToUniversalTime() <= Convert.ToDateTime(_max).ToUniversalTime();
						break;
				}

				return partial;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}