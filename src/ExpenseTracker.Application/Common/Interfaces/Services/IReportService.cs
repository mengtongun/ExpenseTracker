using ExpenseTracker.Application.Contracts.Reports;

namespace ExpenseTracker.Application.Common.Interfaces.Services;

public interface IReportService
{
    Task<ReportSummaryDto> GetSummaryAsync(int userId, ReportQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<(byte[] Content, string ContentType, string FileName)> ExportAsync(int userId, ReportExportRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReportDto>> GetHistoryAsync(int userId, CancellationToken cancellationToken = default);
}

