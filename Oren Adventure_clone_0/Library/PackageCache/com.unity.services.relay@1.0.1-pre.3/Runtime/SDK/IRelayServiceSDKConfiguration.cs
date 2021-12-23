
namespace Unity.Services.Relay
{
    /// <summary>
    /// Interface used for editing the configuration of the relay service SDK.
    /// Primary usage is for testing purposes.
    /// </summary>
    public interface IRelayServiceSDKConfiguration
    {
        /// <summary>
        /// Sets the base path in configuration.
        /// </summary>
        /// <param name="basePath">The base path to set in configuration.</param>
        void SetBasePath(string basePath);
    }
}
