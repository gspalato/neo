using System;

namespace Oculus.Core.Structures.Attributes
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
