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
using ExpressionBuilder;
using GenericHelpers;

namespace Node.Cs.Lib.Controllers
{
	public class BasicControllersFactory:IControllersFactory
	{
		private ReadOnlyDictionary<string, Func<IController>> _funcStorage;

		public IController Create(string controllerName)
		{
			if (!_funcStorage.ContainsKey(controllerName)) return null;
			return _funcStorage[controllerName]();
		}

		public void Release(IController controllerName)
		{
			
		}

		public IEnumerable<Type> Initialize()
		{
			var creator = new Dictionary<string, Func<IController>>(StringComparer.OrdinalIgnoreCase);
			var allTypes = AssembliesManager.LoadTypesInheritingFrom(typeof (IController));
			var types = new List<Type>();
			
			foreach (var type in allTypes)
			{
				var function = Function.Create()
						.WithBody(
							CodeLine.CreateVariable<IController>("result"),
							CodeLine.Assign("result",
								Operation.CreateInstance(type))
						)
						.Returns("result");
				var lambda = function.ToLambda<Func<IController>>();

				var func = new Func<IController>(lambda);
				creator.Add(type.Name,func);
				types.Add(type);
			}
			_funcStorage = new ReadOnlyDictionary<string, Func<IController>>(creator);
			return types;
		}
	}
}
