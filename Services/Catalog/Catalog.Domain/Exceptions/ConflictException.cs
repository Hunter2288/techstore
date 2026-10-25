using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Domain.Exceptions
{
    public class ConflictException(string message) : Exception(message);
}
