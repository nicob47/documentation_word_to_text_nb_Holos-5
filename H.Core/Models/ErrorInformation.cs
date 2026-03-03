using H.Infrastructure;

namespace H.Core.Models
{
    public class ErrorInformation : MessageBase
    {
        #region Properties
        /// <summary>
        /// Gets the exception associated with the message object.
        /// Is not required and may be null.
        /// </summary>
        public Exception? Exception { get; }
        /// <summary>
        /// Indicates whether error is critical and if system should attempt to recover or halt operation.
        /// Is not required and defaults to false.
        /// </summary>
        public bool IsCritical { get; }

        #endregion 

        #region Constructors

        public ErrorInformation(string message, Exception? exception = null, bool isCritical = false)
        {
            base.Message = message;
            Exception = exception;
            IsCritical = isCritical;
        }

        #endregion
    }
}