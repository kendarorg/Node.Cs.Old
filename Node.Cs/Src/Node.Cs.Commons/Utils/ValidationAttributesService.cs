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
using ClassWrapper;
using Node.Cs.Lib.Attributes.Validation;
using Node.Cs.Lib.Contexts;
using Node.Cs.Lib.Controllers;
using System.Collections.Concurrent;

namespace Node.Cs.Lib.Utils
{
	public static class ValidationAttributesService
	{
		public static ConcurrentDictionary<Type, ClassWrapperDescriptor> _classWrappers =
			new ConcurrentDictionary<Type, ClassWrapperDescriptor>();

		public static void RegisterModelType(Type type)
		{
			if (_classWrappers.ContainsKey(type)) return;
			if (type == typeof(FormCollection)) return;
			var cld = new ClassWrapperDescriptor(type);
			cld.Load();
			_classWrappers.TryAdd(type, cld);
		}

		public static bool CanValidate(object model)
		{
			if (model == null) return false;
			var type = model.GetType();
			return _classWrappers.ContainsKey(type);
		}

		public static ClassWrapper.ClassWrapper GetWrapper(object model)
		{
			var type = model.GetType();
			if (!_classWrappers.ContainsKey(type))
			{
				if (type.Namespace.StartsWith("System") || type.IsValueType) return null;
				var clw = new ClassWrapperDescriptor(type);
				clw.Load();
				_classWrappers.AddOrUpdate(type, clw, (a, b) => b);
			}
			return _classWrappers[type].CreateWrapper(model);
		}


		public static ClassWrapperDescriptor GetWrapperDescriptor(object model)
		{
			var type = model.GetType();
			if (model.GetType() == typeof(Type))
			{
				type = (Type)model;
			}
			if (!_classWrappers.ContainsKey(type)) return null;
			return _classWrappers[type];
		}

		/*
		public static object InvokeGetProperty(object model, string property)
		{
			var type = model.GetType();
			var cld = _classWrappers[type];
			return cld.(property);
		}*/

		public static bool ValidateModel(object model, ModelStateDictionary modelStatDictionary)
		{
			if (model == null) return false;
			var type = model.GetType();
			var cld = _classWrappers[type];
			var cw = cld.CreateWrapper(model);
			foreach (var propName in cld.Properties)
			{
				var prop = cld.GetProperty(propName);
				if (prop == null || prop.GetterVisibility != ItemVisibility.Public || prop.SetterVisibility != ItemVisibility.Public) continue;
				foreach (var attr in prop.Attributes)
				{
					var validationAttr = attr as IValidationAttribute;
					if (validationAttr == null) continue;
					var compareAttr = attr as CompareAttribute;
					if (compareAttr != null)
					{
						var toCompareValue = cw.GetObject(compareAttr.WithField);
						if (!compareAttr.IsValid(cw.GetObject(propName), toCompareValue))
						{
							modelStatDictionary.AddModelError(propName, validationAttr.ErrorMessage);
							break;
						}
					}
					else if (!validationAttr.IsValid(cw.GetObject(propName), prop.PropertyType))
					{
						modelStatDictionary.AddModelError(propName, validationAttr.ErrorMessage);
						break;
					}
				}
			}
			return modelStatDictionary.IsValid;
		}
	}
}
