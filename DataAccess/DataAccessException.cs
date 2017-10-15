using System;

namespace PreskriptorAPI.DataAccess
{
    public class DataAccessException : Exception
    {
        public  DataAccessException (string ErrorMessage) : base (ErrorMessage) {}

    }   
}

