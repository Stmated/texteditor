using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eliason.TextEditor
{
    public interface INotifier
    {
        void Success(String title, String message);
        void Info(String title, String message);
        void Warning(String title, String message);
        void Error(String title, String message, Exception exception = null);

        AskResult AskYesNoCancel(String title, String message);
        NotifierInputResponse<T> AskInput<T>(NotifierInputRequest<T> request);
        NotifierSelectResponse<T> AskSelect<T>(NotifierSelectRequest<T> notifierSelectRequest);
        bool AskYesNo(String title, String message);
    }
}
