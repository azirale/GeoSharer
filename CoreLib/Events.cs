using System;

namespace net.azirale.geosharer.core
{
    /***** MESSAGE EVENT MEMBERS ************************************************/
    #region Message Event Members
    /// <summary>
    /// Interface to send text messages between objects. Primarily implemented in this library
    /// as a way of sending log/report messages out to the UI application
    /// </summary>
    public interface IMessageSender
    {
        /// <summary>
        /// Holds subscribers to message events for the object
        /// </summary>
        event Message Messaging;
        /// <summary>
        /// Return the subscribers to the Messaging event of the object
        /// </summary>
        /// <returns></returns>
        Message GetMessagingList();
    }

    /// <summary>
    /// Delegate for sending message events
    /// </summary>
    /// <param name="sender">Object that sent the event</param>
    /// <param name="message">MessagePacket oject containing the message being sent</param>
    public delegate void Message(object sender, MessagePacket message);

    /// <summary>
    /// A derived class of EventArgs that can be used to send a text message to another object
    /// </summary>
    public class MessagePacket : EventArgs
    {
        /// <summary>
        /// The text of the message
        /// </summary>
        public string Text { get; private set; }
        /// <summary>
        /// What verbosity channel the message should be sent through - to allow the receiving object
        /// to decide whether to display it or not
        /// </summary>
        public MessageVerbosity Verbosity { get; private set; }
        /// <summary>
        /// Create a new MessagePacket object with the given message text and verbosity channel
        /// </summary>
        /// <param name="verbosity">Verbosity channel message should be sent to</param>
        /// <param name="text">Text of the message</param>
        public MessagePacket(MessageVerbosity verbosity, string text)
        {
            this.Text = text;
            this.Verbosity = verbosity;
        }
    }
    #endregion

    /***** PROGRESS EVENT MEMBERS ***********************************************/
    #region Progress Event Members

    /// <summary>
    /// Interface for an object that provides updates as to its current progress
    /// </summary>
    public interface IProgressSender
    {
        /// <summary>
        /// Holds subscribers to the Progress update event of this object
        /// </summary>
        event Progress Progressing;
    }

    /// <summary>
    /// Delegate for sending progress update events
    /// </summary>
    /// <param name="sender">object sending the event</param>
    /// <param name="progress">ProgressPacket object detailing the current progress</param>
    public delegate void Progress(object sender, ProgressPacket progress);
    
    /// <summary>
    /// Object for sending progress update details. Includes current and maximum progress for
    /// a progress bar and a string value for any more relevant information for a user
    /// </summary>
    public class ProgressPacket : EventArgs
    {
        /// <summary>
        /// The current absolute progress as a long
        /// </summary>
        public long Current { get; private set; }
        /// <summary>
        /// The maximum absolute progress value as a long
        /// </summary>
        public long Maximum { get; private set; }
        /// <summary>
        /// Description of the current progress in a string
        /// </summary>
        public string Text { get; private set; }
        /// <summary>
        /// Create a new ProgressPacket object with the given progrss indicators and update text
        /// </summary>
        /// <param name="current">Current absolute progress value</param>
        /// <param name="maximum">Maximum absolute progress value</param>
        /// <param name="text">Description of the current progress</param>
        public ProgressPacket(long current, long maximum, string text)
        {
            this.Current = current;
            this.Maximum = maximum;
            this.Text = text;
        }
    }
    #endregion
}