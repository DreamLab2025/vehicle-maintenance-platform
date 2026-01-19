using VMP.Vehicle.Application.Dtos;
using VMP.Vehicle.Domain.Entities;

namespace VMP.Vehicle.Application.Mappings
{
    public static class OilMappings
    {
        public static Oil ToEntity(this OilRequest request)
        {
            return new Oil
            {
                VehiclePartId = request.VehiclePartId,
                ViscosityGrade = request.ViscosityGrade,
                ApiServiceClass = request.ApiServiceClass,
                JasoRating = request.JasoRating,
                BaseOilType = request.BaseOilType,
                RecommendedVolumeLiters = request.RecommendedVolumeLiters,
                VehicleUsage = request.VehicleUsage,
                RecommendedIntervalKmScooter = request.RecommendedIntervalKmScooter,
                RecommendedIntervalKmManual = request.RecommendedIntervalKmManual,
                RecommendedIntervalMonths = request.RecommendedIntervalMonths
            };
        }

        public static OilResponse ToResponse(this Oil entity)
        {
            return new OilResponse
            {
                Id = entity.Id,
                VehiclePartId = entity.VehiclePartId,
                VehiclePartName = entity.VehiclePart?.Name ?? string.Empty,
                CategoryName = entity.VehiclePart?.Category?.Name ?? string.Empty,
                ViscosityGrade = entity.ViscosityGrade,
                ApiServiceClass = entity.ApiServiceClass,
                JasoRating = entity.JasoRating,
                BaseOilType = entity.BaseOilType,
                RecommendedVolumeLiters = entity.RecommendedVolumeLiters,
                VehicleUsage = entity.VehicleUsage,
                VehicleUsageDisplay = GetVehicleUsageDisplay(entity.VehicleUsage),
                RecommendedIntervalKmScooter = entity.RecommendedIntervalKmScooter,
                RecommendedIntervalKmManual = entity.RecommendedIntervalKmManual,
                RecommendedIntervalMonths = entity.RecommendedIntervalMonths,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public static void UpdateEntity(this Oil entity, OilRequest request)
        {
            entity.ViscosityGrade = request.ViscosityGrade;
            entity.ApiServiceClass = request.ApiServiceClass;
            entity.JasoRating = request.JasoRating;
            entity.BaseOilType = request.BaseOilType;
            entity.RecommendedVolumeLiters = request.RecommendedVolumeLiters;
            entity.VehicleUsage = request.VehicleUsage;
            entity.RecommendedIntervalKmScooter = request.RecommendedIntervalKmScooter;
            entity.RecommendedIntervalKmManual = request.RecommendedIntervalKmManual;
            entity.RecommendedIntervalMonths = request.RecommendedIntervalMonths;
        }

        private static string GetVehicleUsageDisplay(OilVehicleUsage usage)
        {
            return usage switch
            {
                OilVehicleUsage.Scooter => "Xe ga",
                OilVehicleUsage.Manual => "Xe số",
                OilVehicleUsage.Both => "Cả hai loại",
                _ => "Không xác định"
            };
        }
    }
}
