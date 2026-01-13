using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using OpenMarkets.Core.DomainModels;

namespace OpenMarkets.Infrastructure.Csv;

public sealed class AsxCompanyCsvMap : ClassMap<AsxCompany>
{
    public AsxCompanyCsvMap()
    {
        Map(m => m.CompanyName).Name("Company name");
        Map(m => m.AsxCode).Name("ASX code");
        Map(m => m.GicsIndustry).Name("GICS industry group").Optional();
        Map(m => m.ListingDate).Name("Listing date").Optional().TypeConverter<AsxDateConverter>();
    }
}

public class AsxDateConverter : DefaultTypeConverter
{
    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var formats = new[] { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd" };

        foreach (var format in formats)
        {
            if (DateTime.TryParseExact(text, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date;
            }
        }

        return null;
    }
}
