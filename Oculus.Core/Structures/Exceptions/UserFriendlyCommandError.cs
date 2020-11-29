using System;

namespace Oculus.Core.Structures.Exceptions
{
    public class UserFriendlyCommandError : Exception
    {
        public UserFriendlyCommandError(string message) : base(message) { }
    }
}
