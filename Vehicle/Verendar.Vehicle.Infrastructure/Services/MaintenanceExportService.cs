using System.Globalization;
using CsvHelper;
using Verendar.Common.Shared;
using CsvHelper.Configuration;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Verendar.Vehicle.Application.Dtos;
using Verendar.Vehicle.Application.Services.Interfaces;
using Verendar.Vehicle.Domain.Entities;
using Verendar.Vehicle.Domain.Repositories.Interfaces;

namespace Verendar.Vehicle.Infrastructure.Services
{
    public class MaintenanceExportService(
        IUnitOfWork unitOfWork,
        ILogger<MaintenanceExportService> logger) : IMaintenanceExportService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<MaintenanceExportService> _logger = logger;

        private static readonly List<string> AllColumns =
        [
            "ServiceDate", "OdometerAtService", "GarageName", "TotalCost",
            "Notes", "InvoiceImageUrl", "Items"
        ];

        public async Task<ApiResponse<(byte[] Data, string ContentType, string FileName)>> ExportAsync(
            Guid userId,
            ExportMaintenanceRequest request,
            CancellationToken cancellationToken = default)
        {
            var vehicle = await _unitOfWork.UserVehicles
                .FindOneAsync(v => v.Id == request.UserVehicleId && v.UserId == userId);

            if (vehicle == null)
            {
                _logger.LogWarning("ExportMaintenance: vehicle {VehicleId} not found for user {UserId}", request.UserVehicleId, userId);
                return ApiResponse<(byte[], string, string)>.NotFoundResponse("Không tìm thấy xe");
            }

            var allRecords = (await _unitOfWork.MaintenanceRecords.GetByUserVehicleIdWithItemsAsync(request.UserVehicleId, cancellationToken)).ToList();

            var records = allRecords
                .Where(r => (!request.From.HasValue || r.ServiceDate >= request.From.Value)
                         && (!request.To.HasValue || r.ServiceDate <= request.To.Value))
                .OrderBy(r => r.ServiceDate)
                .ToList();

            var columns = (request.Columns?.Count > 0)
                ? request.Columns.Intersect(AllColumns, StringComparer.OrdinalIgnoreCase).ToList()
                : AllColumns;

            if (columns.Count == 0)
                columns = AllColumns;

            var plate = string.IsNullOrWhiteSpace(vehicle.LicensePlate) ? vehicle.Id.ToString("N")[..8] : vehicle.LicensePlate;
            var dateStr = DateTime.UtcNow.ToString("yyyyMMdd");
            var fileBase = $"maintenance-{plate}-{dateStr}";

            try
            {
                if (request.Format == ExportFormat.Pdf)
                {
                    var pdfBytes = GeneratePdf(vehicle, records, columns);
                    return ApiResponse<(byte[], string, string)>.SuccessResponse(
                        (pdfBytes, "application/pdf", $"{fileBase}.pdf"));
                }
                else
                {
                    var csvBytes = GenerateCsv(records, columns);
                    return ApiResponse<(byte[], string, string)>.SuccessResponse(
                        (csvBytes, "text/csv", $"{fileBase}.csv"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExportMaintenance: failed to generate {Format} for vehicle {VehicleId}", request.Format, request.UserVehicleId);
                return ApiResponse<(byte[], string, string)>.FailureResponse("Lỗi khi tạo file xuất. Vui lòng thử lại.");
            }
        }

        private static byte[] GeneratePdf(UserVehicle vehicle, List<MaintenanceRecord> records, List<string> columns)
        {
            var plate = vehicle.LicensePlate ?? vehicle.Id.ToString("N")[..8];
            var totalCost = records.Sum(r => r.TotalCost);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Lịch sử bảo dưỡng xe").FontSize(18).Bold().AlignCenter();
                        col.Item().Text($"Biển số: {plate}").FontSize(12).AlignCenter();
                        col.Item().Text($"Ngày xuất: {DateTime.UtcNow:dd/MM/yyyy}").FontSize(10).AlignCenter();
                        col.Item().PaddingVertical(5).LineHorizontal(1);
                    });

                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        if (records.Count == 0)
                        {
                            col.Item().Text("Không có dữ liệu bảo dưỡng trong khoảng thời gian này.").Italic();
                            return;
                        }

                        col.Item().Table(table =>
                        {
                            // Define columns
                            table.ColumnsDefinition(def =>
                            {
                                foreach (var c in columns)
                                {
                                    switch (c)
                                    {
                                        case "ServiceDate": def.ConstantColumn(75); break;
                                        case "OdometerAtService": def.ConstantColumn(70); break;
                                        case "GarageName": def.RelativeColumn(2); break;
                                        case "TotalCost": def.ConstantColumn(80); break;
                                        case "Notes": def.RelativeColumn(2); break;
                                        case "InvoiceImageUrl": def.RelativeColumn(2); break;
                                        case "Items": def.RelativeColumn(3); break;
                                        default: def.RelativeColumn(); break;
                                    }
                                }
                            });

                            // One header row with one cell per column (QuestPDF: each Header() call is a row)
                            table.Header(header =>
                            {
                                foreach (var c in columns)
                                {
                                    header.Cell().Background(Colors.Blue.Medium)
                                        .Padding(4)
                                        .Text(GetColumnLabel(c)).Bold().FontColor(Colors.White).FontSize(9);
                                }
                            });

                            // Data rows
                            var rowIndex = 0;
                            foreach (var record in records)
                            {
                                var bg = rowIndex % 2 == 0 ? Colors.White : Colors.Grey.Lighten3;
                                rowIndex++;

                                foreach (var c in columns)
                                {
                                    table.Cell().Background(bg).Padding(4).Text(GetCellValue(record, c)).FontSize(9);
                                }
                            }
                        });
                    });

                    page.Footer().AlignRight().Text(text =>
                    {
                        text.Span($"Tổng chi phí: {totalCost:N0} VNĐ   |   Tổng {records.Count} lần bảo dưỡng   |   Trang ");
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        }

        private static byte[] GenerateCsv(List<MaintenanceRecord> records, List<string> columns)
        {
            using var ms = new MemoryStream();
            using var writer = new StreamWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true);
            using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));

            // Header
            foreach (var col in columns)
                csv.WriteField(GetColumnLabel(col));
            csv.NextRecord();

            // Rows
            foreach (var record in records)
            {
                foreach (var col in columns)
                    csv.WriteField(GetCellValue(record, col));
                csv.NextRecord();
            }

            writer.Flush();
            return ms.ToArray();
        }

        private static string GetColumnLabel(string column) => column switch
        {
            "ServiceDate" => "Ngày dịch vụ",
            "OdometerAtService" => "Số km (lúc DV)",
            "GarageName" => "Garage",
            "TotalCost" => "Tổng chi phí (VNĐ)",
            "Notes" => "Ghi chú",
            "InvoiceImageUrl" => "URL hóa đơn",
            "Items" => "Danh sách phụ tùng",
            _ => column
        };

        private static string GetCellValue(MaintenanceRecord record, string column) => column switch
        {
            "ServiceDate" => record.ServiceDate.ToString("dd/MM/yyyy"),
            "OdometerAtService" => record.OdometerAtService.ToString("N0"),
            "GarageName" => record.GarageName ?? string.Empty,
            "TotalCost" => record.TotalCost.ToString("N0"),
            "Notes" => record.Notes ?? string.Empty,
            "InvoiceImageUrl" => record.InvoiceImageUrl ?? string.Empty,
            "Items" => FormatItems(record.Items),
            _ => string.Empty
        };

        private static string FormatItems(List<MaintenanceRecordItem> items)
        {
            if (items == null || items.Count == 0) return string.Empty;
            return string.Join("; ", items.Select(i =>
            {
                var name = i.PartCategory?.Name ?? i.CustomPartName ?? "N/A";
                return $"{name}: {i.Price:N0} VNĐ";
            }));
        }
    }
}
