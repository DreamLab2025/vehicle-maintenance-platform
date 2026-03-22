namespace Verendar.Vehicle.Application.Mappings
{
    public static class TypeMappings
    {
        public static VehicleType ToEntity(this TypeRequest request)
        {
            return new VehicleType
            {
                Name = request.Name,
                Description = request.Description,
                ImageUrl = request.ImageUrl
            };
        }

        public static TypeResponse ToResponse(this VehicleType entity)
        {
            return new TypeResponse
            {
                Id = entity.Id,
                Name = entity.Name,
                Slug = entity.Slug,
                Description = entity.Description,
                ImageUrl = entity.ImageUrl,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public static TypeSummary ToSummary(this VehicleType entity)
        {
            return new TypeSummary
            {
                Id = entity.Id,
                Name = entity.Name,
                Slug = entity.Slug,
                ImageUrl = entity.ImageUrl
            };
        }

        public static void UpdateEntity(this VehicleType entity, TypeRequest request)
        {
            entity.Name = request.Name;
            entity.Description = request.Description;
            entity.ImageUrl = request.ImageUrl;
        }
    }
}
