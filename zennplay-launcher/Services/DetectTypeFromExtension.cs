using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zennplay_launcher.Services
{
    public static class DetectTypeFromExtension
    {
        public static string DetectFileType(string? filePathOrTarget)
        {
            if (string.IsNullOrWhiteSpace(filePathOrTarget))
            {
                return "Other";
            }

            string extension = Path.GetExtension(filePathOrTarget).ToLowerInvariant();

            return extension switch
            {
                ".exe" or ".lnk" or ".url" or ".uri" or ".bat" or ".cmd" or ".ps1" or ".application"
                    => "Executable",

                ".txt" or ".cfg" or ".json" or ".ini" or ".xml" or ".yaml" or ".yml"
                    => "Text/Config File",

                ".pdf" or ".doc" or ".docx" or ".chm" or ".html"
                    => "Document",

                _ => "Other"
            };
        }

    }
}
