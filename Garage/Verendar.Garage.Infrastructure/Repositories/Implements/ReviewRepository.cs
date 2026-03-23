namespace Verendar.Garage.Infrastructure.Repositories.Implements;

public class ReviewRepository(GarageDbContext context)
    : PostgresRepository<GarageReview>(context), IReviewRepository
{
}
