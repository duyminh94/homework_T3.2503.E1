namespace homework.Services;

public class UnclearImageException : Exception
{
    public UnclearImageException(string message) : base(message)
    {
    }
}

public class AiValidationException : Exception
{
    public AiValidationException(string message) : base(message)
    {
    }
}

public class AiConfigurationException : Exception
{
    public AiConfigurationException(string message) : base(message)
    {
    }
}

public class AiRateLimitException : Exception
{
    public AiRateLimitException(string message) : base(message)
    {
    }
}

public class AiServiceException : Exception
{
    public AiServiceException(string message) : base(message)
    {
    }
}
