namespace Js2Cs
{
    /// <summary> Defines the varios supported module types. </summary>
    public enum ModuleLoaderType
    {
        /// <summary> Adds extra JS code for detection. </summary>
        Autodetect = 0,

        /// <summary> CommonJs format, useful for nodejs. </summary>
        CommonJs = 1,

        /// <summary> Asyncronous Module Definition, used by require.js </summary>
        Amd = 2,

        /// <summary> ES6 modules, for modern projects. </summary>
        Es6 = 3,

        /// <summary> Plain old browser javascript, adds methods to a global varible. </summary>
        GlobalVariable = 4
    }
}
