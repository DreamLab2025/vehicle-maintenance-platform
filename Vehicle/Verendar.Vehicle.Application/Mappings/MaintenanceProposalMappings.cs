using Verendar.Vehicle.Application.Dtos;
using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Application.Mappings
{
    public static class MaintenanceProposalMappings
    {
        public static MaintenanceProposalItemResponse ToResponse(
            this MaintenanceProposalItem item,
            string? partCategoryName = null) => new()
        {
            Id = item.Id,
            PartCategoryId = item.PartCategoryId,
            GarageProductId = item.GarageProductId,
            GarageServiceId = item.GarageServiceId,
            PartCategoryName = partCategoryName,
            ItemName = item.ItemName,
            UpdatesTracking = item.UpdatesTracking,
            RecommendedKmInterval = item.RecommendedKmInterval,
            RecommendedMonthsInterval = item.RecommendedMonthsInterval,
            Price = item.Price
        };

        public static MaintenanceProposalResponse ToResponse(
            this MaintenanceProposal proposal,
            IReadOnlyDictionary<Guid, string>? categoryNames = null) => new()
        {
            Id = proposal.Id,
            BookingId = proposal.BookingId,
            UserVehicleId = proposal.UserVehicleId,
            BranchName = proposal.BranchName,
            ServiceDate = proposal.ServiceDate,
            OdometerAtService = proposal.OdometerAtService,
            Notes = proposal.Notes,
            TotalAmount = proposal.TotalAmount,
            Status = proposal.Status,
            CreatedAt = proposal.CreatedAt,
            Items = proposal.Items
                .Select(i => i.ToResponse(
                    i.PartCategoryId.HasValue && categoryNames != null
                        ? categoryNames.GetValueOrDefault(i.PartCategoryId.Value)
                        : null))
                .ToList()
        };
    }
}
