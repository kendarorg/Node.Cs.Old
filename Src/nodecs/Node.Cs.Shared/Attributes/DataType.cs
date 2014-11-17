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


namespace NodeCs.Shared.Attributes
{
	// Summary:
	//     Represents an enumeration of the data types associated with data fields and
	//     parameters.
	public enum DataType
	{
		// Summary:
		//     Represents a custom data type.
		Custom = 0,
		//
		// Summary:
		//     Represents an instant in time, expressed as a date and time of day.
		DateTime = 1,
		//
		// Summary:
		//     Represents a date value.
		Date = 2,
		//
		// Summary:
		//     Represents a time value.
		Time = 3,
		//
		// Summary:
		//     Represents a continuous time during which an object exists.
		Duration = 4,
		//
		// Summary:
		//     Represents a phone number value.
		PhoneNumber = 5,
		//
		// Summary:
		//     Represents a currency value.
		Currency = 6,
		//
		// Summary:
		//     Represents text that is displayed.
		Text = 7,
		//
		// Summary:
		//     Represents an HTML file.
		Html = 8,
		//
		// Summary:
		//     Represents multi-line text.
		MultilineText = 9,
		//
		// Summary:
		//     Represents an e-mail address.
		EmailAddress = 10,
		//
		// Summary:
		//     Represent a password value.
		Password = 11,
		//
		// Summary:
		//     Represents a URL value.
		Url = 12,
		//
		// Summary:
		//     Represents a URL to an image.
		ImageUrl = 13,
		//
		// Summary:
		//     Represents a credit card number.
		CreditCard = 14,
		//
		// Summary:
		//     Represents a postal code.
		PostalCode = 15,
		//
		// Summary:
		//     Represents file upload data type.
		Upload = 16,
	}
}