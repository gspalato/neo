using System;

namespace Axion.Structures.Attributes
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