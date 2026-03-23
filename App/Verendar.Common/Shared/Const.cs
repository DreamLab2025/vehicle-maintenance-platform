namespace Verendar.Common.Shared
{
    public class Const
    {
        public const string RabbitMQ = "rabbitmq";

        public const string Redis = "redis";

        public const string IdentityDatabase = "identity-db";

        public const string VehicleDatabase = "vehicle-db";

        public const string MediaDatabase = "media-db";

        public const string NotificationDatabase = "notification-db";

        public const string AiDatabase = "ai-db";

        public const string LocationDatabase = "location-db";

        public const string GarageDatabase = "garage-db";

        public const string CorrelationIdHeaderName = "X-Correlation-ID";

        public const string CorrelationIdKey = "CorrelationId";
    }

    public enum RoleType
    {
        Admin,
        User,
        GarageOwner,
        Mechanic
    }
}
