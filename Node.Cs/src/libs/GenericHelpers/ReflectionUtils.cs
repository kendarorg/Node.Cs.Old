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
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GenericHelpers
{
	public static class ReflectionUtils
	{
		public static bool IsSystemType(Type type)
		{
			if (type == typeof(object)) return false;
			if (type.IsPrimitive) return true;
			if (type == typeof(string)) return true;
			if (type == typeof(DateTime)) return true;
			if (type == typeof(TimeSpan)) return true;
			return false;
		}
		private const BindingFlags DynamicBindingFlags = BindingFlags.Public | BindingFlags.Instance;

		public static Dictionary<string, object> ObjectToDictionary(object data)
		{
			var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

			if (data == null) return dict;
			var stri = data.ToString();
			if (string.IsNullOrWhiteSpace(stri)) return dict;

			try
			{
				foreach (var property in data.GetType().GetProperties(DynamicBindingFlags))
				{
					if (property.CanRead)
					{
						dict.Add(property.Name, property.GetValue(data, null));
					}
				}
			}
			catch (Exception)
			{

			}
			return dict;
		}
	}
}
