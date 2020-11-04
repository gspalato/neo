using Spade.Core.Commands;
using System;

namespace Spade.Core.Structures.Attributes
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