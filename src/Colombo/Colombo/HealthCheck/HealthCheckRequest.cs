namespace Colombo.HealthCheck
{
    /// <summary>
    /// Request that can be send to a Colombo server, and for which a response will indicate that this server is up & running.
    /// </summary>
    public class HealthCheckRequest : SideEffectFreeRequest<ACKResponse>
    {
    }
}
