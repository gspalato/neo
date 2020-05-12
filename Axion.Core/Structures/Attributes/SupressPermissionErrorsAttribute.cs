using System;

namespace Axion.Core.Structures.Attributes
{
    public class SupressPermissionErrorsAttribute : Attribute
    {
        public readonly bool Supress;

        public SupressPermissionErrorsAttribute(bool supress)
        {
            Supress = supress;
        }
    }
}
