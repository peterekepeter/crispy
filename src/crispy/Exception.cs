namespace Crispy
{
    /// <summary> Root of all Crispy related exceptions </summary>
    [System.Serializable] public class CrispyException : System.Exception
    {
        /// <summary> Default constructor </summary>
        public CrispyException() { } 
        
        /// <summary> Constructor with message </summary>
        public CrispyException(string message) : base(message) { }

        /// <summary> Constructor with message and inner exception </summary>
        public CrispyException(string message, System.Exception inner) : base(message, inner) { }
        
        /// <summary> Serialization </summary>
        protected CrispyException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}