using System.Globalization;
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

    public static GarageBranchSummaryResponse ToSummaryResponse(this GarageBranch entity)
    {
        return new GarageBranchSummaryResponse
        {
            Id = entity.Id,
            Name = entity.Name,
            Slug = entity.Slug,
            Address = new AddressDto
            {
                ProvinceCode = entity.Address.ProvinceCode,
                WardCode = entity.Address.WardCode,
                HouseNumber = entity.Address.HouseNumber,
                StreetDetail = entity.Address.StreetDetail
            },
            PhoneNumber = entity.PhoneNumber,
            Latitude = entity.Latitude,
            Longitude = entity.Longitude,
            Status = entity.Status
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

    public static BranchMapItemResponse ToBranchMapItemResponse(this GarageBranch entity)
    {
        return new BranchMapItemResponse
        {
            Id = entity.Id,
            Name = entity.Name,
            Slug = entity.Slug,
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
            MapLinks = BuildMapLinks(entity.Latitude, entity.Longitude),
            PhoneNumber = entity.PhoneNumber,
            Status = entity.Status,
            Garage = new GarageInfoDto
            {
                Id = entity.Garage.Id,
                BusinessName = entity.Garage.BusinessName,
                Slug = entity.Garage.Slug,
                LogoUrl = entity.Garage.LogoUrl,
                Status = entity.Garage.Status
            }
        };
    }

    private static MapLinksDto? BuildMapLinks(double lat, double lng)
    {
        if (lat == 0 && lng == 0) return null;
        var latStr = lat.ToString(CultureInfo.InvariantCulture);
        var lngStr = lng.ToString(CultureInfo.InvariantCulture);
        return new MapLinksDto
        {
            GoogleMaps = $"https://www.google.com/maps?q={latStr},{lngStr}",
            AppleMaps = $"https://maps.apple.com/?ll={latStr},{lngStr}",
            Waze = $"https://waze.com/ul?ll={latStr},{lngStr}&navigate=yes",
            OpenStreetMap = $"https://www.openstreetmap.org/?mlat={latStr}&mlon={lngStr}&zoom=17"
        };
    }
}
