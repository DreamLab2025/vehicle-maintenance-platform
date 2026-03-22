namespace Verendar.Ai.Application.Mappings
{
    public static class VehicleServiceMappings
    {

        public static VehicleInfoDto ToVehicleInfoDto(this VehicleServiceUserVehicleResponse vehicle)
        {
            return new VehicleInfoDto
            {
                Brand = vehicle.UserVehicleVariant.Model.BrandName,
                Model = vehicle.UserVehicleVariant.Model.Name,
                CurrentOdometer = vehicle.CurrentOdometer,
                PurchaseDate = vehicle.PurchaseDate ?? DateTime.UtcNow
            };
        }


        public static DefaultScheduleDto ToDefaultScheduleDto(
            this VehicleServiceDefaultScheduleResponse schedule,
            string partCategoryCode)
        {
            return new DefaultScheduleDto
            {
                PartCategoryCode = partCategoryCode,
                PartCategoryName = partCategoryCode, // Will be filled from mapping if needed
                InitialKm = schedule.InitialKm,
                KmInterval = schedule.KmInterval,
                MonthsInterval = schedule.MonthsInterval,
                RequiresOdometerTracking = schedule.RequiresOdometerTracking,
                RequiresTimeTracking = schedule.RequiresTimeTracking
            };
        }
    }
}
