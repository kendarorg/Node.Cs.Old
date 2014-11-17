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


// Guids.cs
// MUST match guids.h
using System;

namespace Kendar.Node_Cs_Extension
{
    static class GuidList
    {
        public const string guidNode_Cs_ExtensionPkgString = "41fe144f-db28-4d93-8efa-094829fa7c7b";
        public const string guidNode_Cs_ExtensionCmdSetString = "fedf5a0c-deae-4cd7-92fa-e7c7d080cac1";

        public static readonly Guid guidNode_Cs_ExtensionCmdSet = new Guid(guidNode_Cs_ExtensionCmdSetString);
    };
}