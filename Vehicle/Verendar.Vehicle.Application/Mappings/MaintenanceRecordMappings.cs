using Verendar.Common.Databases.Base;

namespace Verendar.Vehicle.Application.Mappings
{
    public static class MaintenanceRecordMappings
    {
        public static MaintenanceRecord ToMaintenanceRecord(this CreateMaintenanceRecordRequest request, Guid userVehicleId)
        {
            return new MaintenanceRecord
            {
                UserVehicleId = userVehicleId,
                ServiceDate = request.ServiceDate,
                OdometerAtService = request.OdometerAtService,
                GarageName = request.GarageName,
                TotalCost = request.TotalCost ?? 0,
                Notes = request.Notes,
                InvoiceImageUrl = request.InvoiceImageUrl,
                Status = EntityStatus.Active,
            };
        }

        public static MaintenanceRecordItem ToMaintenanceRecordItem(
            this MaintenanceRecordItemInput input,
            Guid maintenanceRecordId,
            Guid partCategoryId,
            Guid? partProductId)
        {
            return new MaintenanceRecordItem
            {
                MaintenanceRecordId = maintenanceRecordId,
                PartCategoryId = partCategoryId,
                PartProductId = partProductId,
                CustomPartName = input.CustomPartName,
                InstanceIdentifier = input.InstanceIdentifier,
                Price = input.Price ?? 0,
                Notes = input.ItemNotes,
                UpdatesTracking = input.UpdatesTracking,
            };
        }

        public static VehiclePartTracking ToNewVehiclePartTracking(
            Guid userVehicleId,
            Guid partCategoryId,
            string? instanceIdentifier,
            int lastOdometer,
            DateOnly lastDate,
            int? predictedNextOdometer,
            DateOnly? predictedNextDate,
            int? customKmInterval,
            int? customMonthsInterval,
            Guid? currentPartProductId)
        {
            return new VehiclePartTracking
            {
                UserVehicleId = userVehicleId,
                PartCategoryId = partCategoryId,
                InstanceIdentifier = instanceIdentifier,
                CurrentPartProductId = currentPartProductId,
                LastReplacementOdometer = lastOdometer,
                LastReplacementDate = lastDate,
                PredictedNextOdometer = predictedNextOdometer,
                PredictedNextDate = predictedNextDate,
                CustomKmInterval = customKmInterval,
                CustomMonthsInterval = customMonthsInterval,
                IsDeclared = true,
                Status = EntityStatus.Active,
            };
        }

        public static void ApplyMaintenanceRecordUpdate(
            this VehiclePartTracking tracking,
            Guid? currentPartProductId,
            string? instanceIdentifier,
            int lastOdometer,
            DateOnly lastDate,
            int? predictedNextOdometer,
            DateOnly? predictedNextDate,
            int? customKmInterval,
            int? customMonthsInterval)
        {
            tracking.CurrentPartProductId = currentPartProductId;
            if (instanceIdentifier != null)
                tracking.InstanceIdentifier = instanceIdentifier;
            tracking.LastReplacementOdometer = lastOdometer;
            tracking.LastReplacementDate = lastDate;
            tracking.PredictedNextOdometer = predictedNextOdometer;
            tracking.PredictedNextDate = predictedNextDate;
            tracking.CustomKmInterval = customKmInterval;
            tracking.CustomMonthsInterval = customMonthsInterval;
            tracking.IsDeclared = true;
        }

        public static CreateMaintenanceRecordItemResult ToCreateMaintenanceRecordItemResult(
            Guid maintenanceRecordItemId,
            string partCategoryCode,
            VehiclePartTrackingSummary tracking)
        {
            return new CreateMaintenanceRecordItemResult
            {
                MaintenanceRecordItemId = maintenanceRecordItemId,
                PartCategoryCode = partCategoryCode,
                Tracking = tracking,
            };
        }

        public static CreateMaintenanceRecordResponse ToCreateMaintenanceRecordResponse(
            Guid maintenanceRecordId,
            List<CreateMaintenanceRecordItemResult> items)
        {
            return new CreateMaintenanceRecordResponse
            {
                MaintenanceRecordId = maintenanceRecordId,
                Items = items,
            };
        }


        public static MaintenanceRecordItemDto ToMaintenanceRecordItemDto(this MaintenanceRecordItem item)
        {
            var partName = item.PartProduct?.Name ?? item.CustomPartName ?? item.PartCategory?.Code ?? string.Empty;
            return new MaintenanceRecordItemDto
            {
                Id = item.Id,
                PartName = partName,
                PartCategoryCode = item.PartCategory?.Code ?? string.Empty,
                PartProductId = item.PartProductId,
                InstanceIdentifier = item.InstanceIdentifier,
                Price = item.Price,
                Notes = item.Notes,
            };
        }

        public static MaintenanceRecordSummaryDto ToMaintenanceRecordSummaryDto(this MaintenanceRecord record)
        {
            return new MaintenanceRecordSummaryDto
            {
                Id = record.Id,
                UserVehicleId = record.UserVehicleId,
                ServiceDate = record.ServiceDate,
                OdometerAtService = record.OdometerAtService,
                GarageName = record.GarageName,
                TotalCost = record.TotalCost,
                Notes = record.Notes,
                InvoiceImageUrl = record.InvoiceImageUrl,
                ItemCount = record.Items?.Count ?? 0,
            };
        }

        public static MaintenanceRecordDetailDto ToMaintenanceRecordDetailDto(this MaintenanceRecord record)
        {
            var items = (record.Items ?? [])
                .Select(i => i.ToMaintenanceRecordItemDto())
                .ToList();
            return new MaintenanceRecordDetailDto
            {
                Id = record.Id,
                UserVehicleId = record.UserVehicleId,
                ServiceDate = record.ServiceDate,
                OdometerAtService = record.OdometerAtService,
                GarageName = record.GarageName,
                TotalCost = record.TotalCost,
                Notes = record.Notes,
                InvoiceImageUrl = record.InvoiceImageUrl,
                Items = items,
            };
        }
    }
}
