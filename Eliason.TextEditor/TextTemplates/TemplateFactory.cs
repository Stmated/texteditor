using System;
using System.Collections.Generic;
using System.IO;

namespace Eliason.TextEditor.TextTemplates
{
    public class TemplateFactory
    {
        public static IEnumerable<Template> Get(ITextView textView, string key = null)
        {
            foreach (var resourceDir in textView.Settings.ResourceDirectoryPaths)
            {
                var tempateDir = Path.Combine(resourceDir, "Text templates");
                if (Directory.Exists(tempateDir) == false)
                {
                    yield break;
                }

                foreach (var file in Directory.GetFiles(tempateDir))
                {
                    if (file.EndsWith(".txt", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var name = file.Substring(tempateDir.Length + 1, file.Length - (tempateDir.Length + 5));

                        if (key != null && key != name)
                        {
                            continue;
                        }

                        yield return new Template(name, File.ReadAllText(file));
                    }
                    else
                    {
                        textView.Settings.Notifier.Error(null, Strings.TextControl_Template_FileExtensionMustBeTxt);
                        //Bridge.Get().Error(AppResources.TextControl_Template_FileExtensionMustBeTxt, null);
                    }
                }
            }
        }
    }
}