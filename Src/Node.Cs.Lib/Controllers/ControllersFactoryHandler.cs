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


using ClassWrapper;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Node.Cs.Lib.Exceptions;
using Node.Cs.Lib.Utils;

namespace Node.Cs.Lib.Controllers
{
	public class ControllersFactoryHandler
	{
		private static IControllersFactory _controllerFactory;
		private readonly static Dictionary<Type, ControllerWrapperDescriptor> _controllerDescriptors = new Dictionary<Type, ControllerWrapperDescriptor>();

		public void Initialize(string controllersFactoryClassFullName)
		{
			var type = Type.GetType(controllersFactoryClassFullName);
			if (type == null) throw new NodeCsException("IControllersFactory '{0}' not found",controllersFactoryClassFullName);

			//This is called once, so no problem with activator
			_controllerFactory = (IControllersFactory)Activator.CreateInstance(type);
			var types = _controllerFactory.Initialize();
			InitializeDescriptors(types);
		}

		public void Initialize(IControllersFactory controllersFactoryClassFullName)
		{
			_controllerFactory = controllersFactoryClassFullName;
			var types = _controllerFactory.Initialize();
			InitializeDescriptors(types);
		}

		private void InitializeDescriptors(IEnumerable<Type> types)
		{
			foreach (var type in types)
			{
				var cld = new ClassWrapperDescriptor(type, true);
				cld.Load();
				_controllerDescriptors.Add(type, new ControllerWrapperDescriptor(cld));
				foreach (var method in cld.Methods)
				{
					var methodGroup = cld.GetMethodGroup(method);
					foreach (var meth in methodGroup)
					{
						if (meth.Visibility != ItemVisibility.Public) continue;
						if (meth.Parameters.Count == 0) continue;
						foreach (var param in meth.Parameters)
						{
							var paramType = param.ParameterType;
							if (!paramType.IsValueType && !paramType.Namespace.StartsWith("System"))
							{
								ValidationAttributesService.RegisterModelType(param.ParameterType);
							}
						}
					}
				}
			}
		}

		public static ControllerWrapperDescriptor GetContainer(Type type)
		{
			return _controllerDescriptors[type];
		}

		public static ControllerWrapperInstance Create(string name)
		{
			var classInstance = _controllerFactory.Create(name);
			if (classInstance == null) return null;
			var descriptor = _controllerDescriptors[classInstance.GetType()];
			return descriptor.CreateWrapper(classInstance);
		}

		internal static void Release(IController controller)
		{
			_controllerFactory.Release(controller);
		}
	}
}
