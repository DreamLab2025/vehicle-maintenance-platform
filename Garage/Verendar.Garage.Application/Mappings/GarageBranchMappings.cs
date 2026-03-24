using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Application.Mappings;

public static class GarageBranchMappings
{
    public static GarageBranch ToEntity(this GarageBranchRequest request, Guid garageId)
    {
        return new GarageBranch
        {
            GarageId = garageId,
            Name = request.Name,
            Description = request.Description,
            CoverImageUrl = request.CoverImageUrl,
            PhoneNumber = request.PhoneNumber,
            TaxCode = request.TaxCode,
            Address = new Address
            {
                ProvinceCode = request.Address.ProvinceCode,
                WardCode = request.Address.WardCode,
                HouseNumber = request.Address.HouseNumber,
                StreetDetail = request.Address.StreetDetail
            },
            WorkingHours = new WorkingHours
            {
                Schedule = request.WorkingHours.Schedule
                    .ToDictionary(
                        kv => kv.Key,
                        kv => new DaySchedule
                        {
                            OpenTime = kv.Value.OpenTime,
                            CloseTime = kv.Value.CloseTime,
                            IsClosed = kv.Value.IsClosed
                        })
            }
        };
    }

    public static GarageBranchResponse ToResponse(this GarageBranch entity)
    {
        return new GarageBranchResponse
        {
            Id = entity.Id,
            GarageId = entity.GarageId,
            Name = entity.Name,
            Slug = entity.Slug,
            Description = entity.Description,
            CoverImageUrl = entity.CoverImageUrl,
            Address = new AddressDto
            {
                ProvinceCode = entity.Address.ProvinceCode,
                WardCode = entity.Address.WardCode,
                HouseNumber = entity.Address.HouseNumber,
                StreetDetail = entity.Address.StreetDetail
            },
            Latitude = entity.Latitude,
            Longitude = entity.Longitude,
            WorkingHours = new WorkingHoursDto
            {
                Schedule = entity.WorkingHours.Schedule
                    .ToDictionary(
                        kv => kv.Key,
                        kv => new DayScheduleDto
                        {
                            OpenTime = kv.Value.OpenTime,
                            CloseTime = kv.Value.CloseTime,
                            IsClosed = kv.Value.IsClosed
                        })
            },
            PhoneNumber = entity.PhoneNumber,
            TaxCode = entity.TaxCode,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
