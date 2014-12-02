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


#region  Microsoft Public License
/* This code is part of Xipton.Razor v3.0
 * (c) Jaap Lamfers, 2013 - jaap.lamfers@xipton.net
 * Licensed under the Microsoft Public License (MS-PL) http://www.microsoft.com/en-us/openness/licenses.aspx#MPL
 */
#endregion

using System;
using System.IO;

namespace Http.Renderer.Razor.Utils
{
    public static class StringExtension
    {

        public static string FormatWith(this string format, params object[] args)
        {
            return format == null ? null : string.Format(format, args);
        }

        public static bool NullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static string EmptyAsNull(this string value) {
            return string.IsNullOrEmpty(value) ? null : value;
        }

        public static bool HasVirtualRootOperator(this string path)
        {
            return path != null && path.StartsWith("~");
        }

        public static string RemoveRoot(this string path)
        {
            if (path.NullOrEmpty()) return path;
            if (path[0] == '~') path = path.Substring(1);
            return path.TrimStart('\\').TrimStart('/');
        }

        public static bool IsAbsoluteVirtualPath(string path)
        {
            return path != null && (path.Contains(":") || path.StartsWith("/") || path.StartsWith("\\"));
        }

        public static string MakeAbsoluteDirectoryPath(this string path){
            var baseDir = Path.GetDirectoryName(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
            if (baseDir == null)
                return path;
            return Path.Combine(baseDir, path ?? ".");
        }

        public static string HtmlEncode(this string value)
        {
            return value == null ? null : System.Net.WebUtility.HtmlEncode(value);
        }

        internal static bool IsFileName(this string path) {
            return path != null && File.Exists(path);
        }
        internal static bool IsXmlContent(this string content) {
            return content != null && content.TrimStart().StartsWith("<");
        }
        internal static bool IsUrl(this string value) {
            return value != null && (value.HasVirtualRootOperator() || value.StartsWith("/"));
        }

    }
}