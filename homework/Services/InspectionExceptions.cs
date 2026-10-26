namespace homework.Services;

public class ReportNotFoundException : Exception
{
    public ReportNotFoundException(string message) : base(message)
    {
    }
}

public class ReportAlreadyConfirmedException : Exception
{
    public ReportAlreadyConfirmedException(string message) : base(message)
    {
    }
}

public class StockFullException : Exception
{
    public StockFullException(string message) : base(message)
    {
    }
}
