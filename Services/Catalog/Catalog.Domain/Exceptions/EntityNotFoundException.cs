using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Domain.Exceptions
{
    public class EntityNotFoundException(string message) : Exception(message);
}
