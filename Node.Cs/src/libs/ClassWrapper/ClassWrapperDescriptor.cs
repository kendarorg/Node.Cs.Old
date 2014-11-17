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
using System.Reflection;
using System.Text;
using ExpressionBuilder.Enums;
using ExpressionBuilder.Fluent;
using ExpressionBuilder.Utils;
using ExpressionBuilder;

namespace ClassWrapper
{
	public class ClassWrapperDescriptor
	{
		private readonly Type _toWrap;
		private readonly bool _ignoreCase;
		private readonly Dictionary<string, PropertyWrapperDescriptor> _properties;
		private readonly Dictionary<string, ReadOnlyCollection<MethodWrapperDescriptor>> _methodGroups;
		public ReadOnlyCollection<string> Properties { get; private set; }
		public ReadOnlyCollection<string> Methods { get; private set; }

		public ClassWrapperDescriptor(Type toWrap, bool ignoreCase = false)
		{
			_toWrap = toWrap;
			_ignoreCase = ignoreCase;

			if (_ignoreCase)
			{
				_properties = new Dictionary<string, PropertyWrapperDescriptor>(StringComparer.OrdinalIgnoreCase);
				_methodGroups = new Dictionary<string, ReadOnlyCollection<MethodWrapperDescriptor>>(StringComparer.OrdinalIgnoreCase);
			}
			else
			{
				_properties = new Dictionary<string, PropertyWrapperDescriptor>();
				_methodGroups = new Dictionary<string, ReadOnlyCollection<MethodWrapperDescriptor>>();
			}
		}

		public ClassWrapper CreateWrapper(object instance)
		{
			return new ClassWrapper(this, instance);
		}

		public ClassWrapper CreateWrapper()
		{
			return new ClassWrapper(this);
		}

		public bool ContainsMethod(string methodName)
		{
			return _methodGroups.ContainsKey(methodName);
		}

		public ReadOnlyCollection<MethodWrapperDescriptor> GetMethodGroup(string methodName)
		{
			return _methodGroups[methodName];
		}

		public bool ContainsProperty(string propertyName)
		{
			return _properties.ContainsKey(propertyName);
		}

		public PropertyWrapperDescriptor GetProperty(string propertyName)
		{
			if (!ContainsProperty(propertyName)) return null;
			return _properties[propertyName];
		}

		internal T Get<T>(object instance, string propertyName)
		{
			return (T)_properties[propertyName].Getter(instance);
		}

		internal object GetObject(object instance, string propertyName)
		{
			return Get<object>(instance, propertyName);
		}


		internal void Set(object instance, string propertyName, object value)
		{
			_properties[propertyName].Setter(instance, value);
		}

		internal T InvokeReturn<T>(object instance, string methodName, params object[] values)
		{
			return InvokePrivate<T>(true, instance, methodName, values);
		}

		internal object InvokeReturnObject(object instance, string methodName, params object[] values)
		{
			return InvokePrivate<object>(true, instance, methodName, values);
		}

		internal void Invoke(object instance, string methodName, params object[] values)
		{
			InvokePrivate<object>(false, instance, methodName, values);
		}

		internal bool TryInvoke(object instance, MethodWrapperDescriptor meth, out object result, params object[] valuesParams)
		{
			result = null;
			var values = new List<object>(valuesParams);
			int i = 0;
			bool isValid = true;
			for (; i < values.Count && isValid; i++)
			{
				var value = values[i];
				var par = meth.Parameters[i];
				if (value != null && par.ParameterType.IsAssignableFrom(value.GetType())) continue;
				if (value == null) continue;
				isValid = false;
			}
			for (; i < meth.Parameters.Count && isValid; i++)
			{
				var parameter = meth.Parameters[i];
				if (parameter.HasDefault)
				{
					values.Add(parameter.Default);
					continue;
				}
				if (parameter.ParameterType.IsValueType) isValid = false;
			}
			if (!isValid)
			{
				return false;
			}
			if (!meth.IsVoid)
			{
				result = meth.Call(instance, values.ToList());
			}
			meth.Call(instance, values.ToList());
			return true;
		}

		private T InvokePrivate<T>(bool shouldReturn, object instance, string methodName, params object[] valuesParams)
		{
			object result;
			if (!_methodGroups.ContainsKey(methodName)) throw new MissingMethodException(instance.GetType().FullName, methodName);

			var values = new List<object>(valuesParams);
			var methodGroup = _methodGroups[methodName];

			foreach (var meth in methodGroup)
			{
				if (meth.IsVoid && shouldReturn) continue;
				if (meth.Parameters.Count < values.Count) continue;

				if (TryInvoke(instance, meth, out result, valuesParams))
				{
					if (shouldReturn)
					{
						return (T)result;
					}
					return default(T);
				}
			}

			throw new MissingMethodException(instance.GetType().FullName, methodName);
		}

		public void Load()
		{
			foreach (var property in ReflectionUtil.GetPublicProperties(_toWrap))
			{
				var pw = CreateProperty(property);
				_properties.Add(pw.Name, pw);
			}
			Properties = new ReadOnlyCollection<string>(_properties.Select(p => p.Key).ToList());

			var methods = new Dictionary<string, List<MethodWrapperDescriptor>>();
			foreach (var method in ReflectionUtil.GetMethods(_toWrap))
			{
				try
				{
					var mw = CreateMethod(method);
					if (!methods.ContainsKey(mw.Name))
					{
						methods.Add(mw.Name, new List<MethodWrapperDescriptor>());
					}
					methods[mw.Name].Add(mw);
				}
				catch (Exception)
				{
					
				}
			}
			foreach (var methodGroup in methods)
			{
				var methodList = new ReadOnlyCollection<MethodWrapperDescriptor>(methodGroup.Value);
				_methodGroups.Add(methodGroup.Key, methodList);
			}

			Methods = new ReadOnlyCollection<string>(methods.Keys.ToList());
		}

		private PropertyWrapperDescriptor CreateProperty(PropertyInfo property)
		{
			var pw = new PropertyWrapperDescriptor();
			pw.PropertyType = property.PropertyType;


			

			pw.Name = property.Name;
			if (property.CanRead)
			{
				if (property.GetGetMethod().IsPrivate) pw.GetterVisibility = ItemVisibility.Private;
				else if (property.GetGetMethod().IsPublic) pw.GetterVisibility = ItemVisibility.Public;
				else pw.GetterVisibility = ItemVisibility.Protected;

				pw.Getter = Function.Create()
					.WithParameter<object>("instanceObject")
					.WithBody(
						CodeLine.CreateVariable(_toWrap, "instance"),
						CodeLine.Assign("instance", Operation.Cast("instanceObject", _toWrap)),
						CodeLine.CreateVariable<object>("result"),
						CodeLine.Assign("result", Operation.Get("instance", pw.Name))
					)
					.Returns("result")
					.ToLambda<Func<object, object>>();
			}
			if (property.CanWrite)
			{
				if (property.GetSetMethod().IsPrivate) pw.SetterVisibility = ItemVisibility.Private;
				else if (property.GetSetMethod().IsPublic) pw.SetterVisibility = ItemVisibility.Public;
				else pw.SetterVisibility = ItemVisibility.Protected;

				pw.Setter = Function.Create()
					.WithParameter<object>("instanceObject")
					.WithParameter<object>("value")
					.WithBody(
						CodeLine.CreateVariable(_toWrap, "instance"),
						CodeLine.Assign("instance", Operation.Cast("instanceObject", _toWrap)),
						CodeLine.CreateVariable(pw.PropertyType, "casted"),
						CodeLine.Assign("casted", Operation.Cast("value", pw.PropertyType)),
						Operation.Set("instance", pw.Name, Operation.Variable("casted"))
					)
					.ToLambda<Action<object, object>>();
			}
			foreach (Attribute attr in property.GetCustomAttributes(true))
			{
				pw.Attributes.Add(attr);
			}
			return pw;
		}

		private MethodWrapperDescriptor CreateMethod(MethodCallDescriptor method)
		{
			var mi = ((MethodInfo)method.Method);
			var mw = new MethodWrapperDescriptor();
			if (mi.IsPrivate) mw.Visibility = ItemVisibility.Private;
			else if (mi.IsPublic) mw.Visibility = ItemVisibility.Public;
			else mw.Visibility = ItemVisibility.Protected;

			mw.Name = method.Method.Name;
			if (mi.ReturnType != typeof(void))
			{
				mw.ReturnType = mi.ReturnType;
			}

			mw.IsVoid = mw.ReturnType == null;
			mw.Parameters = new List<MethodParameterDescriptor>();

			foreach (var param in mi.GetParameters())
			{
				mw.Parameters.Add(CreateMethodParameter(param));
			}
			foreach (Attribute attr in mi.GetCustomAttributes(true))
			{
				mw.Attributes.Add(attr);
			}

			var codeLines = new List<ICodeLine>();
			var parameters = new List<IOperation>();
			var paramsCount = mw.Parameters.Count;


			codeLines.Add(CodeLine.CreateVariable(_toWrap, "instance"));
			codeLines.Add(CodeLine.Assign("instance", Operation.Cast("instanceObject", _toWrap)));
			for (int index = 0; index < mw.Parameters.Count; index++)
			{
				var param = mw.Parameters[index];
				codeLines.Add(CodeLine.CreateVariable(param.ParameterType, param.Name));

				codeLines.Add(CodeLine.CreateIf(
					Condition.And(
						Condition.Compare(Operation.Get("params", "Count"), Operation.Constant(paramsCount), ComparaisonOperator.Smaller),
						Condition.Compare(Operation.Constant(true), Operation.Constant(param.HasDefault))))
					.Then(CodeLine.Assign(param.Name, Operation.Constant(param.Default)))
					.Else(CodeLine.Assign(param.Name,
						Operation.Cast(Operation.InvokeReturn("params", "get_Item", Operation.Constant((Int32)index)), param.ParameterType))));
				parameters.Add(Operation.Variable(param.Name));
			}
			if (!mw.IsVoid)
			{
				codeLines.Add(CodeLine.Assign("result",
					Operation.InvokeReturn("instance", mw.Name, parameters.ToArray())));
			}
			else
			{
				codeLines.Add(Operation.Invoke("instance", mw.Name, parameters.ToArray()));
			}

			mw.Call = Function.Create()
					.WithParameter<object>("instanceObject")
					.WithParameter<List<object>>("params")
					.WithBody(
						CodeLine.CreateVariable<object>("result"),
						codeLines.ToArray()
					)
					.Returns("result")
					.ToLambda<Func<object, List<object>, object>>();
			return mw;
		}

		private MethodParameterDescriptor CreateMethodParameter(ParameterInfo parameter)
		{
			var mp = new MethodParameterDescriptor();
			mp.Name = parameter.Name;
			if (parameter.IsOptional)
			{
				mp.HasDefault = true;
				mp.Default = parameter.DefaultValue;
			}
			mp.ParameterType = parameter.ParameterType;
			foreach (Attribute attr in parameter.GetCustomAttributes(true))
			{
				mp.Attributes.Add(attr);
			}
			return mp;
		}
	}
}
