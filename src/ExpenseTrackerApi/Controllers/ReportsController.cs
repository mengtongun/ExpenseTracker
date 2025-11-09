using ExpenseTracker.Application.Common.Interfaces.Services;
using ExpenseTracker.Application.Contracts.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTrackerApi.Controllers;

[Authorize]
[Route("api/[controller]")]
public class ReportsController : ApiControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("summary")]
    [ProducesResponseType(typeof(ReportSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReportSummaryDto>> GetSummary([FromQuery] ReportQueryParameters parameters, CancellationToken cancellationToken)
    {
        var summary = await _reportService.GetSummaryAsync(CurrentUserId, parameters, cancellationToken);
        return Ok(summary);
    }

    [HttpGet("export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Export([FromQuery] ReportExportRequest request, CancellationToken cancellationToken)
    {
        var (content, contentType, fileName) = await _reportService.ExportAsync(CurrentUserId, request, cancellationToken);
        return File(content, contentType, fileName);
    }

    [HttpGet("history")]
    [ProducesResponseType(typeof(IEnumerable<ReportDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ReportDto>>> GetHistory(CancellationToken cancellationToken)
    {
        var history = await _reportService.GetHistoryAsync(CurrentUserId, cancellationToken);
        return Ok(history);
    }
}

