namespace VMP.Common.Shared
{
    public class Const
    {
        public const string RabbitMQ = "rabbitmq";

        public const string Redis = "redis-cache";

        public const string IdentityDatabase = "identity-db";

        public const string VehicleDatabase = "vehicle-db";

        public const string MediaDatabase = "media-db";
    }

    public enum RoleType
    {
        Admin,
        User
    }
}
