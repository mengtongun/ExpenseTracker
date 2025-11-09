using System.Globalization;
using System.Text;
using System.Text.Json;
using AutoMapper;
using CsvHelper;
using ExpenseTracker.Application.Common.Exceptions;
using ExpenseTracker.Application.Common.Interfaces.Persistence;
using ExpenseTracker.Application.Common.Interfaces.Services;
using ExpenseTracker.Application.Contracts.Reports;
using ExpenseTracker.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ExpenseTracker.Application.Services.Reports;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    static ReportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public ReportService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ReportSummaryDto> GetSummaryAsync(int userId, ReportQueryParameters parameters, CancellationToken cancellationToken = default)
    {
        var (start, end) = NormalizeDateRange(parameters.StartDate, parameters.EndDate);
        var expenses = await _unitOfWork.Expenses.GetByUserAsync(userId, start, end, null, cancellationToken);

        return BuildSummary(expenses);
    }

    public async Task<(byte[] Content, string ContentType, string FileName)> ExportAsync(int userId, ReportExportRequest request, CancellationToken cancellationToken = default)
    {
        var (start, end) = NormalizeDateRange(request.StartDate, request.EndDate);
        var expenses = await _unitOfWork.Expenses.GetByUserAsync(userId, start, end, null, cancellationToken);

        if (expenses.Count == 0)
        {
            throw new NotFoundException("No expenses found for the selected period.");
        }

        var summary = BuildSummary(expenses);
        var normalizedGroupBy = request.GroupBy.ToLowerInvariant();
        var fileBaseName = $"expense-report-{normalizedGroupBy}-{start:yyyyMMdd}-{end:yyyyMMdd}";

        (byte[] Content, string ContentType, string FileName) result = request.Format switch
        {
            ReportExportFormat.Csv => (
                GenerateCsv(expenses),
                "text/csv",
                $"{fileBaseName}.csv"),
            ReportExportFormat.Pdf => (
                GeneratePdf(expenses, summary, start, end, normalizedGroupBy),
                "application/pdf",
                $"{fileBaseName}.pdf"),
            _ => throw new BadRequestException("Unsupported export format.")
        };

        var parametersJson = JsonSerializer.Serialize(new
        {
            startDate = start,
            endDate = end,
            request.GroupBy,
            request.Format
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        var report = new Report
        {
            UserId = userId,
            ReportType = $"Summary-{normalizedGroupBy}",
            Parameters = parametersJson,
            FileName = result.FileName,
            ContentType = result.ContentType,
            Content = result.Content
        };

        await _unitOfWork.Reports.AddAsync(report, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task<IReadOnlyList<ReportDto>> GetHistoryAsync(int userId, CancellationToken cancellationToken = default)
    {
        var reports = await _unitOfWork.Reports.GetByUserAsync(userId, cancellationToken);
        return reports.Select(_mapper.Map<ReportDto>).ToList();
    }

    private static (DateOnly Start, DateOnly End) NormalizeDateRange(DateOnly? start, DateOnly? end)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var normalizedStart = start ?? new DateOnly(today.Year, today.Month, 1);
        var normalizedEnd = end ?? today;

        if (normalizedStart > normalizedEnd)
        {
            (normalizedStart, normalizedEnd) = (normalizedEnd, normalizedStart);
        }

        return (normalizedStart, normalizedEnd);
    }

    private static ReportSummaryDto BuildSummary(IEnumerable<Expense> expenses)
    {
        var expenseList = expenses.ToList();
        var summary = new ReportSummaryDto
        {
            Total = expenseList.Sum(e => e.Amount)
        };

        summary.ByCategory = expenseList
            .GroupBy(e => e.Category?.Name ?? "Uncategorized")
            .OrderByDescending(g => g.Sum(x => x.Amount))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        summary.ByMonth = expenseList
            .GroupBy(e => e.ExpenseDate.ToString("yyyy-MM"))
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        summary.ByCurrency = expenseList
            .GroupBy(e => e.Currency)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        return summary;
    }

    private static byte[] GenerateCsv(IEnumerable<Expense> expenses)
    {
        using var memoryStream = new MemoryStream();
        using (var writer = new StreamWriter(memoryStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true), leaveOpen: true))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteField("Date");
            csv.WriteField("Category");
            csv.WriteField("Description");
            csv.WriteField("Amount");
            csv.WriteField("Currency");
            csv.NextRecord();

            foreach (var expense in expenses.OrderBy(e => e.ExpenseDate).ThenBy(e => e.Id))
            {
                csv.WriteField(expense.ExpenseDate.ToString("yyyy-MM-dd"));
                csv.WriteField(expense.Category?.Name ?? "Uncategorized");
                csv.WriteField(expense.Description ?? string.Empty);
                csv.WriteField(expense.Amount);
                csv.WriteField(expense.Currency);
                csv.NextRecord();
            }
        }

        return memoryStream.ToArray();
    }

    private static byte[] GeneratePdf(IEnumerable<Expense> expenses, ReportSummaryDto summary, DateOnly start, DateOnly end, string groupBy)
    {
        var expenseList = expenses.OrderBy(e => e.ExpenseDate).ThenBy(e => e.Id).ToList();
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.Header().Column(column =>
                {
                    column.Item().Text("Expense Tracker Report").SemiBold().FontSize(20);
                    column.Item().Text($"Period: {start:yyyy-MM-dd} - {end:yyyy-MM-dd}");
                    column.Item().Text($"Group By: {groupBy}");
                });

                page.Content().Column(column =>
                {
                    column.Spacing(15);

                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Medium).Padding(10).Column(item =>
                        {
                            item.Item().Text("Total Spend").SemiBold();
                            item.Item().Text(summary.Total.ToString("C", CultureInfo.CurrentCulture));
                        });

                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Medium).Padding(10).Column(item =>
                        {
                            item.Item().Text("Top Category").SemiBold();
                            var topCategory = summary.ByCategory.FirstOrDefault();
                            if (topCategory.Key is null)
                            {
                                item.Item().Text("N/A");
                            }
                            else
                            {
                                item.Item().Text($"{topCategory.Key}: {topCategory.Value:C}");
                            }
                        });
                    });

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(4);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderCell).Text("Date");
                            header.Cell().Element(HeaderCell).Text("Category");
                            header.Cell().Element(HeaderCell).Text("Description");
                            header.Cell().Element(HeaderCell).Text("Amount");
                            header.Cell().Element(HeaderCell).Text("Curr");
                        });

                        foreach (var expense in expenseList)
                        {
                            table.Cell().Element(BodyCell).Text(expense.ExpenseDate.ToString("yyyy-MM-dd"));
                            table.Cell().Element(BodyCell).Text(expense.Category?.Name ?? "Uncategorized");
                            table.Cell().Element(BodyCell).Text(expense.Description ?? string.Empty);
                            table.Cell().Element(BodyCell).Text(expense.Amount.ToString("N2"));
                            table.Cell().Element(BodyCell).Text(expense.Currency);
                        }
                    });
                });

                page.Footer().AlignRight().Text(text =>
                {
                    text.Span("Generated ");
                    text.Span(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm"));
                    text.Span(" UTC | Page ");
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        });

        return document.GeneratePdf();

        static IContainer HeaderCell(IContainer container)
            => container.BorderBottom(1).BorderColor(Colors.Black).Padding(5).DefaultTextStyle(x => x.SemiBold());

        static IContainer BodyCell(IContainer container)
            => container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5);
    }
}

