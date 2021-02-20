namespace Eliason.TextEditor
{
    public interface ITextSegment
    {
        /// <summary>
        /// Gets the text that this segment contains.
        /// </summary>
        string[] Texts { get; }

        string GetText(int textColumnIndex);

        /// <summary>
        /// Gets or sets the index this segment has in offset to its parent, and not the document.
        /// </summary>
        int Index { get; set; }

        /// <summary>
        /// Gets the length that this segment has, which most often is the length of <see cref="Texts"/>[textColumnIndex].
        /// </summary>
        /// <param name="textColumnIndex">The text column index that is being targeted.</param>
        /// <returns></returns>
        int GetLength(int textColumnIndex);

        /// <summary>
        /// Sets the length that this segment has on the specified text column index.
        /// </summary>
        /// <param name="textColumnIndex">The text column index that is being targeted.</param>
        /// <param name="value">The value of the length.</param>
        /// <returns></returns>
        void SetLength(int textColumnIndex, int value);

        /// <summary>
        /// Returns true if this segment intersects with the specified range.
        /// </summary>
        /// <param name="index">The index that is relative to its parent.</param>
        /// <param name="length">The length of the check for an intersection.</param>
        /// <param name="startIncluded">True if should include touching starts and not just intersections.</param>
        /// <param name="endIncluded">True if shoudl include touching ends and not just intersections.</param>
        /// <returns></returns>
        bool Contains(int index, int length, bool startIncluded, bool endIncluded);
    }
}