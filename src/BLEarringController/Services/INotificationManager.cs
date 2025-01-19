namespace BLEarringController.Services
{
    /// <summary>
    /// A transient service that allows displaying pop-ups to the user. This is used to wrap
    /// methods that require a <see cref="Page"/>, so that ViewModels are able to display
    /// notifications without referencing the Views.
    /// <para>
    /// Following the conventions from the
    /// <see href="https://learn.microsoft.com/en-us/dotnet/maui/user-interface/pop-ups">MAUI
    /// documentation on pop-ups</see>, the following naming conventions are maintained:
    /// <list type="bullet">
    ///     <item>
    ///         <term>Alert</term>
    ///         <description>
    ///         A simple pop-up with either one or two buttons, presenting a user with information
    ///         and allowing them to make a choice.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term>Action Sheet</term>
    ///         <description>
    ///         A pop-up that presents the user with a list of choices, and up to two buttons. One
    ///         of the buttons may be used to represent a "destructive" action.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term>Prompt</term>
    ///         <description>
    ///         A pop-up that prompts the user and allows them to enter data using the keyboard.
    ///         </description>
    ///     </item>
    /// </list>
    /// </para>
    /// </summary>
    /// <remarks>
    /// For now, this only wraps the built-in MAUI pop-up methods on a <see cref="Page"/>, but this
    /// may be expanded in the future.
    /// </remarks>
    public interface INotificationManager : ISingletonService
    {
        #region Methods

        #region Public

        /// <summary>
        /// Display an action sheet over the current page. The action sheet presents the user with
        /// a list of options to pick from and a cancel button.
        /// </summary>
        /// <remarks>
        /// The action sheet can also display a "destructive" button using the
        /// <see cref="DisplayActionSheet(string,string,string,string[])"/> method.
        /// </remarks>
        /// <param name="title">The title at the top of the action sheet.</param>
        /// <param name="cancelText">The text to display on the "cancel" button.</param>
        /// <param name="actions">The "actions" to present the user with.</param>
        /// <returns>
        /// A <see cref="Task"/> that will be completed with the <see cref="string"/> on the button
        /// or action that the user clicked once the user makes a selection.
        /// </returns>
        Task<string> DisplayActionSheet(string title, string cancelText, params string[] actions);

        /// <summary>
        /// Display an action sheet over the current page. The action sheet presents the user with
        /// a list of options to pick from, a cancel button, and a "destructive" button.
        /// </summary>
        /// <remarks>
        /// The action sheet can also display without a "destructive" button using the
        /// <see cref="DisplayActionSheet(string,string,string[])"/> method.
        /// </remarks>
        /// <param name="title">The title at the top of the action sheet.</param>
        /// <param name="cancelText">The text to display on the "cancel" button.</param>
        /// <param name="destructiveText">The text to display on the "destructive" button.</param>
        /// <param name="actions">The "actions" to present the user with.</param>
        /// <returns>
        /// A <see cref="Task"/> that will be completed with the <see cref="string"/> on the button
        /// or action that the user clicked once the user makes a selection.
        /// </returns>
        Task<string> DisplayActionSheet(
            string title,
            string cancelText,
            string destructiveText,
            params string[] actions);

        /// <summary>
        /// Display an alert over the current page. This alert is intended to provide information
        /// to the user, and provides them with a single option to dismiss the alert.
        /// <para>
        /// The button to dismiss the alert will contain text equivalent to "OK".
        /// </para>
        /// </summary>
        /// <remarks>
        /// To customise the text on the button to dismiss the alert, use
        /// <see cref="DisplayAlert(string,string,string)"/>.
        /// </remarks>
        /// <param name="title">The title at the top of the alert.</param>
        /// <param name="message">The main body of text to display on the alert.</param>
        /// <returns>
        /// A <see cref="Task"/> that will be completed when the alert is dismissed.
        /// </returns>
        Task DisplayAlert(string title, string message);

        /// <summary>
        /// Display an alert over the current page. This alert is intended to provide information
        /// to the user, and provides them with a single option to dismiss the alert.
        /// </summary>
        /// <remarks>
        /// To use the default text on the button to dismiss the alert, use
        /// <see cref="DisplayAlert(string,string)"/>.
        /// </remarks>
        /// <param name="title">The title at the top of the alert.</param>
        /// <param name="message">The main body of text to display on the alert.</param>
        /// <param name="dismissText">
        /// The text to display on the button that dismisses the alert.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that will be completed when the alert is dismissed.
        /// </returns>
        Task DisplayAlert(string title, string message, string dismissText);

        /// <summary>
        /// Display an alert over the current page. This alert is intended to present the user with
        /// a choice to "accept" or "cancel". The text on the buttons can be customised.
        /// </summary>
        /// <remarks>
        /// To display an alert for information only, use <see cref="DisplayAlert(string,string)"/>
        /// or <see cref="DisplayAlert(string,string,string)"/>.
        /// </remarks>
        /// <param name="title">The title at the top of the alert.</param>
        /// <param name="message">The main body of text to display on the alert.</param>
        /// <param name="acceptText">
        /// The text to display on the button to "accept" the option presented on the alert.
        /// </param>
        /// <param name="cancelText">
        /// The text to display on the button to "reject" the option presented on the alert.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that will be completed when the user makes a choice with a
        /// <see cref="bool"/> indicating which choice they made.
        /// <para>
        /// <c>true</c> is used to indicate the user clicked the "accept" button.
        /// <c>false</c> is used to indicate the user clicked the "cancel" button.
        /// </para>
        /// </returns>
        Task<bool> DisplayAlert(
            string title,
            string message,
            string acceptText,
            string cancelText);

        /// <summary>
        /// Display a prompt that requests the user to enter some text data. The prompt will have a
        /// "cancel" and "accept" button, and can be customised to have a contained length,
        /// placeholder text when the prompt field is empty, and text to initialise the prompt
        /// with. The <see cref="Keyboard"/> displayed with the prompt should be set according to
        /// the type of data requested from the user.
        /// </summary>
        /// <param name="title">The title to display at the top of the prompt.</param>
        /// <param name="message">The message to explain the data the user should enter.</param>
        /// <param name="acceptText">The text to display on the "accept" button.</param>
        /// <param name="cancelText">The text to display on the "cancel" button.</param>
        /// <param name="placeholderText">
        /// Placeholder text to display in the entry when it is empty. Set this to <c>null</c> to
        /// disable the placeholder text.
        /// </param>
        /// <param name="maxLength">
        /// Constrain the length of the string that the user can enter. Set this to <c>-1</c> if
        /// the text length should not be constrained.
        /// </param>
        /// <param name="keyboard">The <see cref="Keyboard"/> to display with the prompt.</param>
        /// <param name="initialText">Text to pre-populate the prompt with.</param>
        /// <returns>
        /// A <see cref="Task"/> completed with the <see cref="string"/> containing the text in the
        /// prompt when the user clicks the "accept", or <c>null</c> if the user clicked cancel.
        /// </returns>
        Task<string?> DisplayPrompt(
            string title,
            string message,
            string acceptText,
            string cancelText,
            string? placeholderText,
            int maxLength,
            Keyboard keyboard,
            string initialText);

        /// <summary>
        /// Display a prompt that requests the user to enter some text data. The prompt will have a
        /// "cancel" and "accept" button, and can be customised to have a contained length and
        /// placeholder text when the prompt field is empty. The <see cref="Keyboard"/> displayed
        /// with the prompt should be set according to the type of data requested from the user.
        /// </summary>
        /// <param name="title">The title to display at the top of the prompt.</param>
        /// <param name="message">The message to explain the data the user should enter.</param>
        /// <param name="acceptText">The text to display on the "accept" button.</param>
        /// <param name="cancelText">The text to display on the "cancel" button.</param>
        /// <param name="placeholderText">
        /// Placeholder text to display in the entry when it is empty. Set this to <c>null</c> to
        /// disable the placeholder text.
        /// </param>
        /// <param name="maxLength">
        /// Constrain the length of the string that the user can enter. Set this to <c>-1</c> if
        /// the text length should not be constrained.
        /// </param>
        /// <param name="keyboard">The <see cref="Keyboard"/> to display with the prompt.</param>
        /// <returns>
        /// A <see cref="Task"/> completed with the <see cref="string"/> containing the text in the
        /// prompt when the user clicks the "accept", or <c>null</c> if the user clicked cancel.
        /// </returns>
        Task<string?> DisplayPrompt(
            string title,
            string message,
            string acceptText,
            string cancelText,
            string placeholderText,
            int maxLength,
            Keyboard keyboard);

        /// <summary>
        /// Display a prompt that requests the user to enter some text data. The prompt will have a
        /// "cancel" and "accept" button, and can be customised to have a contained length and text
        /// to initialise the prompt with. The <see cref="Keyboard"/> displayed with the prompt
        /// should be set according to the type of data requested from the user.
        /// </summary>
        /// <param name="title">The title to display at the top of the prompt.</param>
        /// <param name="message">The message to explain the data the user should enter.</param>
        /// <param name="acceptText">The text to display on the "accept" button.</param>
        /// <param name="cancelText">The text to display on the "cancel" button.</param>
        /// <param name="maxLength">
        /// Constrain the length of the string that the user can enter. Set this to <c>-1</c> if
        /// the text length should not be constrained.
        /// </param>
        /// <param name="keyboard">The <see cref="Keyboard"/> to display with the prompt.</param>
        /// <param name="initialText">Text to pre-populate the prompt with.</param>
        /// <returns>
        /// A <see cref="Task"/> completed with the <see cref="string"/> containing the text in the
        /// prompt when the user clicks the "accept", or <c>null</c> if the user clicked cancel.
        /// </returns>
        Task<string?> DisplayPrompt(
            string title,
            string message,
            string acceptText,
            string cancelText,
            int maxLength,
            Keyboard keyboard,
            string initialText);

        #endregion

        #endregion
    }
}
