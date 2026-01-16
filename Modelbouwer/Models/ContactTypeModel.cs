using System;
using System.Collections.Generic;
using System.Text;

namespace Modelbouwer.Models;

    public class ContactTypeModel
    {
	public string? ContactTypeName { get; set; }
	public int ContactTypeId { get; set; }

	// Define the property that you want to use in TLists (for example in the errorList
	public string? Name => ContactTypeName;

	// Mapping dictionary for mapping Database Header to Property name
	public static readonly Dictionary<string, string> HeaderToPropertyMap = new()
	{
		{ DBNames.ContactTypeFieldNameId, "ContactTypeId" },
		{ DBNames.ContactTypeFieldNameName, "ContactTypeName" }
	};


	public static readonly Dictionary<string, string[]> ColumnMappings = new()
	{
		[nameof(ContactTypeId)] = [ "ID" ],

		[nameof(ContactTypeName)] = [
			"Functie",
			"Function",
			"Funktion" ]
	};
}
