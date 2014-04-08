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
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CSharp.RuntimeBinder;

namespace Node.Cs.Lib.Utils
{
	public class ItemData
	{
		public string PropertyName;
		public object PropertyValue;
		public IEnumerable<Attribute> PropertyAttributes;
		public object MainValue;
		public Type MainType;
	}

	public class LambdaHelper
	{
		public LambdaHelper()
		{
			_memberExpressions = new Dictionary<Type, FieldInfo>();
			_memberAttributes = new Dictionary<PropertyInfo, object[]>();
		}

		public Dictionary<Type, FieldInfo> _memberExpressions;
		private readonly Dictionary<PropertyInfo, object[]> _memberAttributes;

		public object GetPropertyValue<T>(Expression<Func<T>> e)
		{
			return e.Compile()();
		}

		public object GetObject<T>(Expression<Func<T>> e)
		{
			var me = (MemberExpression)((MemberExpression)e.Body).Expression;
			var ce = (ConstantExpression)me.Expression;
			var valueType = ce.Value.GetType();
			if (!_memberExpressions.ContainsKey(valueType))
			{
				var fieldInfo = ce.Value.GetType().GetField(me.Member.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				_memberExpressions.Add(valueType, fieldInfo);
			}

			return _memberExpressions[valueType].GetValue(ce.Value);
		}

		public Type GetObjectType<T>(Expression<Func<T>> e)
		{
			var member = (MemberExpression)e.Body;
			Expression strExpr = member.Expression;
			return strExpr.Type;
		}

		public string GetPropertyName<T>(Expression<Func<T>> e)
		{
			var member = (MemberExpression)e.Body;
			return member.Member.Name;
		}

		public PropertyInfo GetProperty<T>(Expression<Func<T>> e)
		{
			var member = (MemberExpression)e.Body;
			return member.Member as PropertyInfo;;
		}
		/*
		public PropertyInfo Property<T>(Expression<Func<T>> e)
		{
			var member = e.Body as MemberExpression;

			// Check if there is a cast to object in first position
			if (member == null)
			{
				var unary = e.Body as UnaryExpression;
				if (unary != null)
				{
					member = unary.Operand as MemberExpression;
				}
			}
			return member.Member as PropertyInfo;
		}*/

		public object[] GetCustomAttribues<TK>(Expression<Func<TK>> expression)
		{
			var pinfo = GetProperty(expression);
			if (!_memberAttributes.ContainsKey(pinfo))
			{
				_memberAttributes.Add(pinfo, pinfo.GetCustomAttributes(true));
			}
			return _memberAttributes[pinfo];
		}

		public object GetProperty(object o, string member)
		{
			if (o == null) throw new ArgumentNullException("o");
			if (member == null) throw new ArgumentNullException("member");
			Type scope = o.GetType();
			IDynamicMetaObjectProvider provider = o as IDynamicMetaObjectProvider;
			if (provider != null)
			{
				ParameterExpression param = Expression.Parameter(typeof(object));
				DynamicMetaObject mobj = provider.GetMetaObject(param);
				GetMemberBinder binder = (GetMemberBinder)Microsoft.CSharp.RuntimeBinder.Binder.GetMember(0, member, scope, new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(0, null) });
				DynamicMetaObject ret = mobj.BindGetMember(binder);
				BlockExpression final = Expression.Block(
						Expression.Label(CallSiteBinder.UpdateLabel),
						ret.Expression
				);
				LambdaExpression lambda = Expression.Lambda(final, param);
				Delegate del = lambda.Compile();
				return del.DynamicInvoke(o);
			}
			else
			{
				return o.GetType().GetProperty(member, BindingFlags.Public | BindingFlags.Instance).GetValue(o, null);
			}
		}
	}
}
