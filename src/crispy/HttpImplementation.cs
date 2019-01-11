namespace Crispy
{
    /// <summary> The type of library to use. </summary>
    public enum HttpImplementation
    {
        /// <summary> XHR based implementation, that uses callbacks. </summary>
        Xhr = 0,
        /// <summary> Promise based implementation. </summary>
        Promise = 1
    }
}
