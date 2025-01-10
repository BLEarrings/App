using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BLEarringController.ViewModels
{
    /// <summary>
    /// Base class for all view models.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        #region Events

        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region Methods

        #region Protected

        /// <summary>
        /// Notify the client that a property has changed, by invoking the
        /// <see cref="PropertyChanged"/> event for a specified property.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property that changed.
        /// </param>
        protected void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #endregion
    }
}
