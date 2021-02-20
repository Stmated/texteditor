using System;

namespace Eliason.TextEditor
{
    /// <summary>
    /// Deprecated?
    /// </summary>
    [Flags]
    public enum ByInterface
    {
        Unknown = 0,
        ByKeyboard = 1,
        ByMouse = 2,
        ByMouseDouble = 4,
        ByMouseRight = 8,
        Manually = 16,

        MaskForceSeparate = 32
    }
}