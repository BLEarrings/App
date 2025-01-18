using System.ComponentModel;

namespace BLEarringController.ViewModels
{
    /// <summary>
    /// An interface for all ViewModels, which must implement <see cref="INotifyPropertyChanged"/>
    /// for bindings and <see cref="IQueryAttributable"/> to get data attributes from navigation.
    /// </summary>
    /// <remarks>
    /// Note that this must be public to be visible during assembly scanning with Scrutor.
    /// </remarks>
    public interface IViewModel : INotifyPropertyChanged, IQueryAttributable
    {
        // This interface is intentionally blank, as it is only used to enforce the implementations
        // of other interfaces for the ViewModels, and to register ViewModels with the dependency
        // injection container using Scrutor.
    }
}
