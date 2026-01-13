using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenMarkets.Api.Dto;
using OpenMarkets.Core.DomainModels.Exceptions;
using OpenMarkets.Core.Services;

namespace OpenMarkets.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CompaniesController : ControllerBase
{
    private readonly IAsxCompanyService _companyService;
    private readonly ILogger<CompaniesController> _logger;

    public CompaniesController(IAsxCompanyService companyService, ILogger<CompaniesController> logger)
    {
        _companyService = companyService;
        _logger = logger;
    }

    [HttpGet("{asxCode}")]
    [ProducesResponseType(typeof(CompanyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<CompanyResponse>> GetCompanyByCode(string asxCode, CancellationToken cancellationToken)
    {
        try
        {
            var company = await _companyService.GetCompanyByCodeAsync(asxCode, cancellationToken);

            var response = new CompanyResponse
            {
                AsxCode = company.AsxCode,
                CompanyName = company.CompanyName,
                ListingDate = company.ListingDate,
                GicsIndustry = company.GicsIndustry
            };

            return Ok(response);
        }
        catch (CompanyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Company not found: {AsxCode}", asxCode);
            return NotFound(new { message = ex.Message, asxCode });
        }
        catch (AsxDataUnavailableException ex)
        {
            _logger.LogError(ex, "ASX data unavailable for request: {AsxCode}", asxCode);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
        }
    }
}
