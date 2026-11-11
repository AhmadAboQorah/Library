using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApplication.Exceptions
{
    public class BusinessRuleException : Exception
    {
        public BusinessRuleException(string message) : base(message) { }
    }
}
