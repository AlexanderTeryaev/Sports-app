using System;
using System.Runtime.Serialization;

namespace WebApi.Exceptions
{
    public class NotFoundEntityException : Exception
    {
        public NotFoundEntityException()
        {
        }

        public NotFoundEntityException(string message) : base(message)
        {
        }

        public NotFoundEntityException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}