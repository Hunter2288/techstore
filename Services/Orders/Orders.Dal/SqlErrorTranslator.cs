using Microsoft.Data.SqlClient;
using Orders.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orders.Dal
{
    public static class SqlErrorTranslator
    {
        public static bool TryTranslate(SqlException ex, out Exception? domain)
        {
            domain = ex.Number switch
            {
               
                50001 or 50010 or 50020 or 50030 => new NotFoundException(ex.Message),
                50003 => new ValidationException(ex.Message),
                50002 or 50011 or 50012 or 50021 or 50022 or 50023 or 50024 or 50031 or 50032
                    => new BusinessConflictException(ex.Message),
                2601 or 2627 => new BusinessConflictException("Record with the same key already exists"),
                547 => new BusinessConflictException("Operation violates a reference or check constraint"),
                _ => null
            };
            return domain is not null;
        }
    }
}
