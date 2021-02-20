#region

using System;

#endregion

namespace Eliason.TextEditor
{
    /// <summary>
    ///   i cloneable.
    /// </summary>
    /// <typeparam name = "T">
    /// </typeparam>
    public interface ICloneable<out T> : ICloneable
    {
        /// <summary>
        ///   clone.
        /// </summary>
        /// <returns>
        /// </returns>
        new T Clone();
    }
}