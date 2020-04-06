using System;

namespace Muon.Kernel.Structures
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