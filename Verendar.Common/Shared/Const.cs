namespace Verendar.Common.Shared
{
    public class Const
    {
        public const string RabbitMQ = "rabbitmq";

        public const string Redis = "redis-cache";

        public const string IdentityDatabase = "identity-db";

        public const string VehicleDatabase = "vehicle-db";

        public const string MediaDatabase = "media-db";

        public const string NotificationDatabase = "notification-db";
    }

    public enum RoleType
    {
        Admin,
        User
    }
}
