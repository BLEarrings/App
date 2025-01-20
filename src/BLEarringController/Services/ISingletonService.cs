namespace BLEarringController.Services
{
    /// <summary>
    /// An interface indicating that the implementing class is a singleton service, and should be
    /// registered with the dependency injection container.
    /// <para>
    /// Singletons are created once and exist for the entire lifetime of the app. They are shared
    /// by all consumers, so are useful for services that need to control an aspect of the app
    /// centrally.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Note that this must be public to be visible during assembly scanning with Scrutor.
    /// </remarks>
    public interface ISingletonService
    {
        // This interface is intentionally blank, as it is only used to register classes with the
        // dependency injection container using Scrutor. This allows any class to implement this
        // interface to be automatically registered with the container.
    }
}
