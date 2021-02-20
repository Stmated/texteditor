namespace Eliason.TextEditor
{
    public interface IAutoSaver
    {
        /// <summary>
        ///   Calls the code which auto-saves the object's content.
        /// </summary>
        void SaveAuto();
    }
}