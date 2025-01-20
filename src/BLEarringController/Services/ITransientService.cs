namespace BLEarringController.Services
{
    /// <summary>
    /// An interface indicating that the implementing class is a transient service, and should be
    /// registered with the dependency injection container.
    /// <para>
    /// Transient services are instantiated every time they are requested. This is useful for
    /// services do not need to be state aware.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Note that this must be public to be visible during assembly scanning with Scrutor.
    /// </remarks>
    public interface ITransientService
    {
        // This interface is intentionally blank, as it is only used to register classes with the
        // dependency injection container using Scrutor. This allows any class to implement this
        // interface to be automatically registered with the container.
    }
}
