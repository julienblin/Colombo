namespace Colombo.HealthCheck
{
    /// <summary>
    /// Handler for <see cref="HealthCheckRequest"/>. Merely do anything than respond.
    /// </summary>
    public class HealthCheckRequestHandler : SideEffectFreeRequestHandler<HealthCheckRequest, ACKResponse>
    {
        /// <summary>
        /// Handles the request.
        /// </summary>
        protected override void Handle()
        { }
    }
}
