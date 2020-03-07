using System;

namespace Muon.Core.Structures
{
	public class CategoryAttribute : Attribute
	{
		public string Category { get; }

		public CategoryAttribute(string category)
		{
			this.Category = category;
		}
	}
}