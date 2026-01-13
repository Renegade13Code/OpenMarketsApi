namespace OpenMarkets.Core.DomainModels.Exceptions;

public class AsxDataUnavailableException : Exception
{
    public AsxDataUnavailableException()
        : base("ASX company data is currently unavailable.")
    {
    }

    public AsxDataUnavailableException(string message)
        : base(message)
    {
    }

    public AsxDataUnavailableException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
