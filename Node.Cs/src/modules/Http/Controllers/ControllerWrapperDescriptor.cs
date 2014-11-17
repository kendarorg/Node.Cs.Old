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
using System.Collections.ObjectModel;
using System.Linq;
using ClassWrapper;
using Http.Shared.Controllers;
using Http.Shared.Routing;

namespace Http.Controllers
{
	public class ControllerWrapperDescriptor
	{
		private readonly Dictionary<string, Dictionary<string, ReadOnlyCollection<MethodWrapperDescriptor>>> _methods;
		private readonly ClassWrapperDescriptor _cd;

		public ClassWrapperDescriptor WrapperDescriptor
		{
			get { return _cd; }
		}

		public ControllerWrapperDescriptor(ClassWrapperDescriptor cd)
		{
			_cd = cd;
			_methods = new Dictionary<string, Dictionary<string, ReadOnlyCollection<MethodWrapperDescriptor>>>(StringComparer.OrdinalIgnoreCase);
			InitializeMethodGroups();
		}

		public ControllerWrapperInstance CreateWrapper(IController classInstance)
		{
			return new ControllerWrapperInstance(this, _cd.CreateWrapper(classInstance));
		}

		public IEnumerable<MethodWrapperDescriptor> GetMethodGroup(string action, string verb)
		{
			if (!_methods.ContainsKey(verb) || !_methods[verb].ContainsKey(action))
			{
				if (!_methods.ContainsKey("ALL") || !_methods["ALL"].ContainsKey(action)) yield break;
				verb = "ALL";
			}
			foreach (var method in _methods[verb][action])
			{
				yield return method;
			}
		}

		private void InitializeMethodGroups()
		{
			foreach (var methodName in _cd.Methods)
			{
				foreach (var method in _cd.GetMethodGroup(methodName))
				{
					if (method.IsVoid || method.Visibility != ItemVisibility.Public) continue;
					var verb = "GET";
					var action = methodName;
					var attr = (HttpRequestTypeAttribute)method.Attributes.FirstOrDefault(a => a is HttpRequestTypeAttribute);
					var cho = (ChildActionOnly)method.Attributes.FirstOrDefault(a => a is ChildActionOnly);

					var attrName = (ActionName)method.Attributes.FirstOrDefault(a => a is ActionName);
					if (attr != null)
					{
						verb = attr.Verb;
						if (attrName != null)
						{
							action = attrName.Name;
						}
						else if (!string.IsNullOrWhiteSpace(attr.Action))
						{
							action = attr.Action;
						}
					}
					if (cho != null)
					{
						verb = "ALL";
					}
					SetupMethod(verb, action, method);
				}
			}
		}

		private void SetupMethod(string verb, string action, MethodWrapperDescriptor method)
		{
			if (!_methods.ContainsKey(verb))
			{
				_methods.Add(verb, new Dictionary<string, ReadOnlyCollection<MethodWrapperDescriptor>>(StringComparer.OrdinalIgnoreCase));
			}
			if (!_methods[verb].ContainsKey(action))
			{
				_methods[verb].Add(action, new ReadOnlyCollection<MethodWrapperDescriptor>(new List<MethodWrapperDescriptor>()));
			}
			var methods = new List<MethodWrapperDescriptor>(_methods[verb][action]);
			methods.Add(method);
			_methods[verb][action] = new ReadOnlyCollection<MethodWrapperDescriptor>(methods);
		}
	}
}
