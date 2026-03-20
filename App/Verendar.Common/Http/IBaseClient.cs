namespace Verendar.Common.Http
{
    /// <summary>
    /// Marker interface for typed HTTP clients used for inter-service communication.
    /// All service clients should extend <see cref="BaseServiceClient{TClient}"/>,
    /// which implements this interface automatically.
    /// </summary>
    public interface IBaseClient { }
}
