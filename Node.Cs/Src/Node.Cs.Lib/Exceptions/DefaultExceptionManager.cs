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
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using ConcurrencyHelpers.Monitor;
using ExpressionBuilder;
using Node.Cs.Lib.Loggers;
using Node.Cs.Lib.Settings;
using Node.Cs.Lib.Utils;

namespace Node.Cs.Lib.Exceptions
{
	public class DefaultExceptionManager : IGlobalExceptionManager
	{
		public void HandleException(Exception exception, HttpContextBase context)
		{
			PerfMon.SetMetric(PerfMonConst.NodeCs_Status_Exceptions, 1);
			Logger.Error(exception, "Exception during request to: {0}", context.Request.Url);
			var nodeCsException = exception as NodeCsException;
			if (nodeCsException == null)
			{
				WriteException(exception, context);
			}
			else if (nodeCsException.HttpCode == 500)
			{
				WriteException(nodeCsException, context);
			}
			else
			{
				WriteError(nodeCsException, context, nodeCsException.HttpCode);
			}
		}


		public void Initialize(NodeCsSettings settings)
		{

		}

		private void WriteError(Exception exception, HttpContextBase context, int httpCode)
		{
			try
			{
				var exceptionText = BuildException(exception, httpCode);
				context.Response.StatusCode = httpCode;
				var encoding = context.Response.ContentEncoding ?? Encoding.UTF8;
				var byteEncoded = encoding.GetBytes(exceptionText);
				context.Response.OutputStream.Write(byteEncoded, 0, byteEncoded.Length);
			}
			catch (Exception ex)
			{
				Logger.Error(ex, "Error writing...error!");
			}
		}

		private void WriteException(Exception exception, HttpContextBase context)
		{
			WriteError(exception, context, 500);
		}

		public static string BuildException(Exception ex, int statusCode)
		{
			return string.Format(@"
<html>
  <head>
		<title>Error {2}:{3}</title>
<style type='text/css'> 
<!-- 
body{{margin:0;font-size:.7em;font-family:Verdana,Arial,Helvetica,sans-serif;}} 
code{{margin:0;color:#006600;font-size:1.1em;font-weight:bold;}} 
.config_source code{{font-size:.8em;color:#000000;}} 
pre{{margin:0;font-size:1.4em;word-wrap:break-word;}} 
ul,ol{{margin:10px 0 10px 5px;}} 
ul.first,ol.first{{margin-top:5px;}} 
fieldset{{padding:0 15px 10px 15px;word-break:break-all;}} 
.summary-container fieldset{{padding-bottom:5px;margin-top:4px;}} 
legend.no-expand-all{{padding:2px 15px 4px 10px;margin:0 0 0 -12px;}} 
legend{{color:#333333;;margin:4px 0 8px -12px;_margin-top:0px; 
font-weight:bold;font-size:1em;}} 
a:link,a:visited{{color:#007EFF;font-weight:bold;}} 
a:hover{{text-decoration:none;}} 
h1{{font-size:2.4em;margin:0;color:#FFF;}} 
h2{{font-size:1.7em;margin:0;color:#CC0000;}} 
h3{{font-size:1.4em;margin:10px 0 0 0;color:#CC0000;}} 
h4{{font-size:1.2em;margin:10px 0 5px 0; 
}}#header{{width:96%;margin:0 0 0 0;padding:6px 2% 6px 2%;font-family:'trebuchet MS',Verdana,sans-serif; 
 color:#FFF;background-color:#5C87B2; 
}}#content{{margin:0 0 0 2%;position:relative;}} 
.summary-container,.content-container{{background:#FFF;width:96%;margin-top:8px;padding:10px;position:relative;}} 
.content-container p{{margin:0 0 10px 0; 
}}#details-left{{width:35%;float:left;margin-right:2%; 
}}#details-right{{width:63%;float:left;overflow:hidden; 
}}#server_version{{width:96%;_height:1px;min-height:1px;margin:0 0 5px 0;padding:11px 2% 8px 2%;color:#FFFFFF; 
 background-color:#5A7FA5;border-bottom:1px solid #C1CFDD;border-top:1px solid #4A6C8E;font-weight:normal; 
 font-size:1em;color:#FFF;text-align:right; 
}}#server_version p{{margin:5px 0;}} 
table{{margin:4px 0 4px 0;width:100%;border:none;}} 
td,th{{vertical-align:top;padding:3px 0;text-align:left;font-weight:normal;border:none;}} 
th{{width:30%;text-align:right;padding-right:2%;font-weight:bold;}} 
thead th{{background-color:#ebebeb;width:25%; 
}}#details-right th{{width:20%;}} 
table tr.alt td,table tr.alt th{{}} 
.highlight-code{{color:#CC0000;font-weight:bold;font-style:italic;}} 
.clear{{clear:both;}} 
.preferred{{padding:0 5px 2px 5px;font-weight:normal;background:#006633;color:#FFF;font-size:.8em;}} 
--> 
</style> 
  </head>
	<body>
<div id='content'> 
<div class='content-container'> 
<h3>Exception: {0}</h3>
<h4>Exception Type: {2}, Status Code: {3}</h4>
</div>
{1}
<!--<div class='content-container'> 
<fieldset><h4>Detailed Error Information:</h4> 
  <div id='details-left'> 
   <table border='0' cellpadding='0' cellspacing='0'> 
    <tr class='alt'><th>Module</th><td>&nbsp;&nbsp;&nbsp;DirectoryListingModule</td></tr> 
    <tr><th>Notification</th><td>&nbsp;&nbsp;&nbsp;ExecuteRequestHandler</td></tr> 
    <tr class='alt'><th>Handler</th><td>&nbsp;&nbsp;&nbsp;StaticFile</td></tr> 
    <tr><th>Error Code</th><td>&nbsp;&nbsp;&nbsp;0x00000000</td></tr> 
     
   </table> 
  </div> 
  <div id='details-right'> 
   <table border='0' cellpadding='0' cellspacing='0'> 
    <tr class='alt'><th>Requested URL</th><td>&nbsp;&nbsp;&nbsp;http://localhost:8012/</td></tr> 
    <tr><th>Physical Path</th><td>&nbsp;&nbsp;&nbsp;C:\Users\enrico.daros\Desktop\Efw\Samples\SimpleWebSite</td></tr> 
    <tr class='alt'><th>Logon Method</th><td>&nbsp;&nbsp;&nbsp;Anonymous</td></tr> 
    <tr><th>Logon User</th><td>&nbsp;&nbsp;&nbsp;Anonymous</td></tr> 
    <tr class='alt'><th>Request Tracing Directory</th><td>&nbsp;&nbsp;&nbsp;C:\Users\enrico.daros\Documents\IISExpress\TraceLogFiles\SIMPLEWEBSITE</td></tr> 
   </table> 
   <div class='clear'></div> 
  </div> 
 </fieldset> 
</div>-->
	<body>
<html>",
				ex.Message,
				SetupStackTrace(ex.StackTrace, statusCode),
				ex.GetType().Name,
				statusCode);
		}

		private static string SetupStackTrace(string st, int statusCode)
		{
			if (statusCode != 500) return string.Empty;

			const string content = @"
<div class='content-container'> 
<fieldset>
<h4>Stack trace:</h4>
<ul>{0}</ul>
</fieldset>
</div>
";
			var splitted = st.Split(new[] { '\n', '\r', '\f' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(a => "<li>" + HttpUtility.HtmlEncode(a) + "</li>").ToArray();
			return string.Format(content, string.Join("", splitted));
		}
	}
}