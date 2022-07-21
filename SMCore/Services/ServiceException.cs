using System;
using System.Collections.Generic;
using System.Text;

namespace SMCore.Services
{

    public class UnregisterServiceException : Exception
    {
        public UnregisterServiceException() { }
        public UnregisterServiceException(string message)
            : base(message){}
        public UnregisterServiceException(Exception exception)
            : base("", exception){}
        public UnregisterServiceException(string message, Exception exception)
            : base(message, exception){}
    }

    public class DuplicateRegisterServiceException : Exception
    {
        public DuplicateRegisterServiceException(){}

        public DuplicateRegisterServiceException(string message)
            : base(message){}

        public DuplicateRegisterServiceException(Exception exception)
            : base("", exception){}

        public DuplicateRegisterServiceException(string message, Exception exception)
            : base(message, exception){}
    }
}
