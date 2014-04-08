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


using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Node.Cs.Lib.Contexts
{
	internal class NodeCsHttpFileCollection : HttpFileCollectionBase
	{
		private readonly Dictionary<string, NodeCsFile> _filesList;

		public NodeCsHttpFileCollection(Dictionary<string, NodeCsFile> filesList)
		{
			_filesList = filesList;
		}
		public override IEnumerator GetEnumerator()
		{
			return _filesList.GetEnumerator();
		}

		public override HttpPostedFileBase Get(string name)
		{
			if (!_filesList.ContainsKey(name)) return null;
			return _filesList[name];
		}

		public override string[] AllKeys
		{
			get { return _filesList.Keys.ToArray(); }
		}

		public override HttpPostedFileBase this[string name]
		{
			get { return Get(name); }
		}
	}
}