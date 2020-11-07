using System;

namespace Spade.Common.Structures.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CacheKeyFormatAttribute : Attribute
    {
        public string Format { get; init; }

        public CacheKeyFormatAttribute(string format)
        {
            Format = format;
        }
    }
}
