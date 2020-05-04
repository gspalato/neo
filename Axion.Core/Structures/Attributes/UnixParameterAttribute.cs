using System;

namespace Axion.Core.Structures.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class UnixParameterAttribute : Attribute
    {
        public string Name;

        public UnixParameterAttribute()
        {
        }

        public UnixParameterAttribute(string name)
        {
            Name = name;
        }
    }
}
