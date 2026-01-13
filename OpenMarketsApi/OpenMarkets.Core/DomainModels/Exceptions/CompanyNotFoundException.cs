namespace OpenMarkets.Core.DomainModels.Exceptions;

public class CompanyNotFoundException : Exception
{
    public string AsxCode { get; }

    public CompanyNotFoundException(string asxCode)
        : base($"Company with ASX code '{asxCode}' was not found.")
    {
        AsxCode = asxCode;
    }

    public CompanyNotFoundException(string asxCode, string message)
        : base(message)
    {
        AsxCode = asxCode;
    }

    public CompanyNotFoundException(string asxCode, string message, Exception innerException)
        : base(message, innerException)
    {
        AsxCode = asxCode;
    }
}
