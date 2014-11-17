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



#if NOMORE
using System;
using System.Web;
using RazorEngine.Templating;

namespace Node.Cs.Razor
{
	public class NodeCsTemplateService : TemplateService
	{
		public NodeCsTemplateService(RazorEngine.Configuration.TemplateServiceConfiguration templateConfig)
			:base(templateConfig)
		{
		}
		public override ITemplate CreateTemplate(string razorTemplate, Type templateType, object model)
		{
			if (model == null)
			{
				model = new object();
			}
			
			var dymodel = model as dynamic;

			dynamic resulting = base.CreateTemplate(razorTemplate, templateType, (object)dymodel);
			resulting.Model = dymodel;

			return resulting as ITemplate; 
		}

		/// <summary>
		/// Sets the model for the template.
		/// </summary>
		/// <typeparam name="T">The model type.</typeparam>
		/// <param name="template">The template instance.</param>
		/// <param name="model">The model instance.</param>
		private static void SetModel<T>(ITemplate template, T model)
		{
			if (model == null) return;

			var dynamicTemplate = template as ITemplate<dynamic>;
			if (dynamicTemplate != null)
			{
				dynamicTemplate.Model = model;
				return;
			}

			var staticModel = template as ITemplate<T>;
			if (staticModel != null)
			{
				staticModel.Model = model;
				return;
			}

			SetModelExplicit(template, model);
		}

		/// <summary>
		/// Sets the model for the template.
		/// </summary>
		/// <remarks>
		/// This method uses reflection to set the model property. As we can't guaruntee that we know
		/// what model type they will be using, we have to do the hard graft. The preference would be
		/// to use the generic <see cref="SetModel{T}"/> method instead.
		/// </remarks>
		/// <param name="template">The template instance.</param>
		/// <param name="model">The model instance.</param>
		private static void SetModelExplicit(ITemplate template, object model)
		{
			var type = template.GetType();
			var prop = type.GetProperty("Model");

			if (prop != null)
				prop.SetValue(template, model, null);
		}

	}
}
#endif
