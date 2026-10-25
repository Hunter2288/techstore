using System;
using System.Collections.Generic;
using System.Text;

namespace Orders.Domain.Exceptions
{
    public class ValidationException(string message) : Exception(message);
}
