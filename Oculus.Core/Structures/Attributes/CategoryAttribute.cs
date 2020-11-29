using Oculus.Core.Commands;
using System;

namespace Oculus.Core.Structures.Attributes
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