namespace Crispy
{
    /// <summary> Additional options when doing typescript generation </summary>
    public class TsOptions
    {
        /// <summary> Generate type info for readable fields/properties </summary>
        public bool Readable = true; 

        /// <summary> Generate type info for writeable fields/properties </summary>
        public bool Writeable = true;
        
        /// <summary> Generate type info for properties </summary>
        public bool Properties = true;

        /// <summary> Generate type info for fields </summary>
        public bool Fields = true;
       
        /// <summary> Generate type info for public fields/properties </summary>
        public bool Public = true;

        /// <summary> Generate type info for non-public fields/properties </summary>
        public bool NonPublic = false;

        /// <summary> Transform UpperCamelCase (used in C#) to lowerCamelCase (used in JS) </summary>
        public bool LowercaseFirstLetter = true;

        /// <summary> Export enum values as numbers (set false for strings) </summary>
        public bool EnumNumberValues = false;
    }
}