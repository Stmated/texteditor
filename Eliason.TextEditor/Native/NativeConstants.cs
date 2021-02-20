namespace Eliason.TextEditor.Native
{
    public enum SECURITY_IMPERSONATION_LEVEL
    {
        SecurityAnonymous = 0,
        SecurityIdentification = 1,
        SecurityImpersonation = 2,
        SecurityDelegation = 3
    }

    public enum TOKEN_TYPE
    {
        TokenPrimary = 1,
        TokenImpersonation = 2
    }

    public static class NativeConstants
    {
        public const uint PROCESS_QUERY_INFORMATION = 0x0400;

        public const uint SEE_MASK_CLASSKEY = 0x00000003;
        public const uint SEE_MASK_CLASSNAME = 0x00000001;
        public const uint SEE_MASK_CONNECTNETDRV = 0x00000080;
        public const uint SEE_MASK_DOENVSUBST = 0x00000200;
        public const uint SEE_MASK_FLAG_DDEWAIT = 0x00000100;
        public const uint SEE_MASK_FLAG_LOG_USAGE = 0x04000000;
        public const uint SEE_MASK_FLAG_NO_UI = 0x00000400;
        public const uint SEE_MASK_HMONITOR = 0x00200000;
        public const uint SEE_MASK_HOTKEY = 0x00000020;
        public const uint SEE_MASK_ICON = 0x00000010;
        public const uint SEE_MASK_IDLIST = 0x00000004;
        public const uint SEE_MASK_INVOKEIDLIST = 0x0000000c;
        public const uint SEE_MASK_NO_CONSOLE = 0x00008000;
        public const uint SEE_MASK_NOCLOSEPROCESS = 0x00000040;
        public const uint SEE_MASK_NOQUERYCLASSSTORE = 0x01000000;
        public const uint SEE_MASK_NOZONECHECKS = 0x00800000;
        public const uint SEE_MASK_UNICODE = 0x00004000;
        public const uint SEE_MASK_WAITFORINPUTIDLE = 0x02000000;

        public const int SW_FORCEMINIMIZE = 11;
        public const int SW_HIDE = 0;
        public const int SW_MAX = 11;
        public const int SW_MAXIMIZE = 3;
        public const int SW_MINIMIZE = 6;
        public const int SW_NORMAL = 1;
        public const int SW_RESTORE = 9;
        public const int SW_SHOW = 5;
        public const int SW_SHOWDEFAULT = 10;
        public const int SW_SHOWMAXIMIZED = 3;
        public const int SW_SHOWMINIMIZED = 2;
        public const int SW_SHOWMINNOACTIVE = 7;
        public const int SW_SHOWNA = 8;
        public const int SW_SHOWNOACTIVATE = 4;
        public const int SW_SHOWNORMAL = 1;

        public const uint READ_CONTROL = 0x00020000;

        public const uint STANDARD_RIGHTS_EXECUTE = READ_CONTROL;
        public const uint STANDARD_RIGHTS_READ = READ_CONTROL;
        public const uint STANDARD_RIGHTS_REQUIRED = 0x000F0000;
        public const uint STANDARD_RIGHTS_WRITE = READ_CONTROL;

        public const uint TOKEN_ASSIGN_PRIMARY = 0x0001;
        public const uint TOKEN_DUPLICATE = 0x0002;
        public const uint TOKEN_EXECUTE = STANDARD_RIGHTS_EXECUTE;
        public const uint TOKEN_IMPERSONATE = 0x0004;
        public const uint TOKEN_QUERY = 0x0008;
        public const uint TOKEN_QUERY_SOURCE = 0x0010;
        public const uint TOKEN_READ = STANDARD_RIGHTS_READ | TOKEN_QUERY;

        public const uint MAXIMUM_ALLOWED = 0x02000000;

        public const int ERROR_ALREADY_EXISTS = 183;
        public const int ERROR_CANCELLED = 1223;
        public const int ERROR_IO_PENDING = 0x3e5;
        public const int ERROR_NO_MORE_ITEMS = 259;
        public const int ERROR_SUCCESS = 0;
        public const int ERROR_TIMEOUT = 1460;

        public const int WM_WINDOWPOSCHANGING = 0x0046;
        public const int WM_WINDOWPOSCHANGED = 0x0047;

        public const int WM_IME_STARTCOMPOSITION = 0x010D;
        public const int WM_IME_ENDCOMPOSITION = 0x010E;
        public const int WM_IME_COMPOSITION = 0x010F;
        public const int WM_IME_KEYLAST = 0x010F;

        public const int CFS_CANDIDATEPOS = 64;
        public const int CFS_EXCLUDE = 128;

        public const int ATTR_INPUT = 0;
        public const int ATTR_TARGET_CONVERTED = 1;
        public const int ATTR_CONVERTED = 2;
        public const int ATTR_TARGET_NOTCONVERTED = 3;
        public const int ATTR_INPUT_ERROR = 4;
        public const int ATTR_FIXEDCONVERTED = 5;

        public const int TRANSPARENT = 1;
        public const int OPAQUE = 2;

        public const uint SRCAND = 0x008800C6; /* dest = source AND dest */
        public const uint SRCCOPY = 0x00CC0020; /* dest = source  */
        public const uint SRCERASE = 0x00440328; /* dest = source AND (NOT dest ) */
        public const uint SRCINVERT = 0x00660046; /* dest = source XOR dest */
        public const uint SRCPAINT = 0x00EE0086; /* dest = source OR dest */

        public const int PS_ALTERNATE = 8;
        public const int PS_COSMETIC = 0x00000000;
        public const int PS_DASH = 1; /* -------  */
        public const int PS_DASHDOT = 3; /* _._._._  */
        public const int PS_DASHDOTDOT = 4; /* _.._.._  */
        public const int PS_DOT = 2; /* .......  */
        public const int PS_ENDCAP_FLAT = 0x00000200;
        public const int PS_ENDCAP_MASK = 0x00000F00;
        public const int PS_ENDCAP_ROUND = 0x00000000;
        public const int PS_ENDCAP_SQUARE = 0x00000100;
        public const int PS_GEOMETRIC = 0x00010000;
        public const int PS_INSIDEFRAME = 6;
        public const int PS_JOIN_BEVEL = 0x00001000;
        public const int PS_JOIN_MASK = 0x0000F000;
        public const int PS_JOIN_MITER = 0x00002000;
        public const int PS_JOIN_ROUND = 0x00000000;
        public const int PS_NULL = 5;
        public const int PS_SOLID = 0;
        public const int PS_TYPE_MASK = 0x000F0000;
        public const int PS_USERSTYLE = 7;

        public const int HS_HORIZONTAL = 0;
        public const int HS_VERTICAL = 1;
        public const int HS_FDIAGONAL = 2;
        public const int HS_BDIAGONAL = 3;
        public const int HS_CROSS = 4;
        public const int HS_DIAGCROSS = 5;
    }
}