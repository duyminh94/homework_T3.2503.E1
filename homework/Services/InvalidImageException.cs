namespace homework.Services;

public class InvalidImageException : Exception
{
    public InvalidImageException(string message) : base(message)
    {
    }
}
