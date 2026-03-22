namespace Verendar.Vehicle.Application.Mappings
{
    public static class MaintenanceRecordMappings
    {
        public static MaintenanceRecord ToMaintenanceRecord(this CreateRecordRequest request, Guid userVehicleId)
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
            };
        }

        public static MaintenanceRecordItem ToMaintenanceRecordItem(
            this RecordItemInput input,
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

        public static PartTracking ToNewVehiclePartTracking(
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
            return new PartTracking
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
            };
        }

        public static void ApplyMaintenanceRecordUpdate(
            this PartTracking tracking,
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

        public static RecordItemResult ToRecordItemResult(
            Guid maintenanceRecordItemId,
            string partCategorySlug,
            PartTrackingSummary tracking)
        {
            return new RecordItemResult
            {
                MaintenanceRecordItemId = maintenanceRecordItemId,
                PartCategorySlug = partCategorySlug,
                Tracking = tracking,
            };
        }

        public static CreateRecordResponse ToCreateRecordResponse(
            Guid maintenanceRecordId,
            List<RecordItemResult> items)
        {
            return new CreateRecordResponse
            {
                MaintenanceRecordId = maintenanceRecordId,
                Items = items,
            };
        }


        public static RecordItemDto ToRecordItemDto(this MaintenanceRecordItem item)
        {
            return new RecordItemDto
            {
                Id = item.Id,
                PartCategoryId = item.PartCategoryId,
                PartCategorySlug = item.PartCategory?.Slug ?? string.Empty,
                PartProductId = item.PartProductId,
                PartProductName = item.PartProduct?.Name,
                CustomPartName = item.CustomPartName,
                InstanceIdentifier = item.InstanceIdentifier,
                Price = item.Price,
                Notes = item.Notes,
                UpdatesTracking = item.UpdatesTracking,
            };
        }

        public static RecordSummaryDto ToRecordSummaryDto(this MaintenanceRecord record)
        {
            return new RecordSummaryDto
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

        public static RecordDetailDto ToRecordDetailDto(this MaintenanceRecord record)
        {
            var items = (record.Items ?? [])
                .Select(i => i.ToRecordItemDto())
                .ToList();
            return new RecordDetailDto
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
