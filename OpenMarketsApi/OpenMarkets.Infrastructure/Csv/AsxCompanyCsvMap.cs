using CsvHelper.Configuration;
using OpenMarkets.Core.DomainModels;

namespace OpenMarkets.Infrastructure.Csv;

public sealed class AsxCompanyCsvMap : ClassMap<AsxCompany>
{
    public AsxCompanyCsvMap()
    {
        Map(m => m.CompanyName).Name("Company name");
        Map(m => m.AsxCode).Name("ASX code");
        Map(m => m.GicsIndustry).Name("GICS industry group").Optional();
    }
}
