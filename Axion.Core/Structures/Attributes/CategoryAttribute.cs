using Axion.Core.Commands;
using System;

namespace Axion.Core.Structures.Attributes
{
	public class CategoryAttribute : Attribute
	{
		public Category Category { get; }

		public CategoryAttribute(Category category)
		{
			this.Category = category;
		}
	}
}