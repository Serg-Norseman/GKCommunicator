using System;

namespace GKLocations.Blockchain
{
    public class MethodArgumentException : ArgumentException
    {
        public MethodArgumentException(string paramName, string message)
            : base($"The argument {paramName} does not match the conditions. {message}", paramName)
        {
        }
    }

    public class MethodResultException : ArgumentException
    {
        public MethodResultException(string paramName, string message)
            : base($"The result {paramName} does not match the conditions. {message}", paramName)
        {
        }
    }
}
