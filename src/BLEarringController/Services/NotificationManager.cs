namespace BLEarringController.Services
{
    /// <inheritdoc cref="INotificationManager" />
    public class NotificationManager : INotificationManager
    {
        #region Constants

        #region Private

        /// <summary>
        /// The default text to show on an informational alert.
        /// </summary>
        /// <remarks>
        /// This is only used for alerts with a single button.
        /// </remarks>
        private const string DefaultAlertButtonText = "Ok";

        #endregion

        #endregion

        #region Methods

        #region Public

        /// <inheritdoc />
        public Task<string> DisplayActionSheet(
            string title,
            string cancelText,
            params string[] actions)
        {
            // Display the action sheet without the "destructive" button.
            return Shell.Current.CurrentPage.DisplayActionSheet(
                title,
                cancelText,
                null,
                actions);
        }

        /// <inheritdoc />
        public Task<string> DisplayActionSheet(
            string title,
            string cancelText,
            string destructiveText,
            params string[] actions)
        {
            return Shell.Current.CurrentPage.DisplayActionSheet(
                title,
                cancelText,
                destructiveText,
                actions);
        }

        /// <inheritdoc />
        public Task DisplayAlert(string title, string message)
        {
            // Note that the "Cancel" text refers to the button on the text to dismiss the alert,
            // so although it functions by cancelling the alert, it displays text to "Accept" the
            // alert.
            return Shell.Current.CurrentPage.DisplayAlert(
                title,
                message,
                DefaultAlertButtonText);
        }

        /// <inheritdoc />
        public Task DisplayAlert(string title, string message, string dismissText)
        {
            return Shell.Current.CurrentPage.DisplayAlert(title, message, dismissText);
        }

        /// <inheritdoc />
        public Task<bool> DisplayAlert(
            string title,
            string message,
            string acceptText,
            string cancelText)
        {
            return Shell.Current.CurrentPage.DisplayAlert(title, message, acceptText, cancelText);
        }

        /// <inheritdoc />
        public Task<string?> DisplayPrompt(
            string title,
            string message,
            string acceptText,
            string cancelText,
            string? placeholderText,
            int maxLength,
            Keyboard keyboard,
            string initialText)
        {
            return Shell.Current.CurrentPage.DisplayPromptAsync(
                title,
                message,
                acceptText,
                cancelText,
                placeholderText,
                maxLength,
                keyboard,
                initialText);
        }

        /// <inheritdoc />
        public Task<string?> DisplayPrompt(
            string title,
            string message,
            string acceptText,
            string cancelText,
            string placeholderText,
            int maxLength,
            Keyboard keyboard)
        {
            // Wrap the call to the full DisplayPrompt method and pass default value for missing
            // argument.
            return DisplayPrompt(
                title,
                message,
                acceptText,
                cancelText,
                placeholderText,
                maxLength,
                keyboard,
                string.Empty);
        }

        /// <inheritdoc />
        public Task<string?> DisplayPrompt(
            string title,
            string message,
            string acceptText,
            string cancelText,
            int maxLength,
            Keyboard keyboard,
            string initialText)
        {
            // Wrap the call to the full DisplayPrompt method and pass default value for missing
            // argument.
            return DisplayPrompt(
                title,
                message,
                acceptText,
                cancelText,
                null,
                maxLength,
                keyboard,
                initialText);
        }

        #endregion

        #endregion
    }
}
