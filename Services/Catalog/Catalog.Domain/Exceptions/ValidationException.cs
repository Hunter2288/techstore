using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Domain.Exceptions
{
    public class ValidationException(string message) : Exception(message);
}
