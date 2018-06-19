using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leash.Common.Infrastructure.Extensions
{
    public static class DialogHelper
    {
        public static bool GetFolderDirectory(out string directory, string defaultDirectory = "")
        {
            directory = string.Empty;
            var dialog = new CommonOpenFileDialog()
            {
                EnsurePathExists = true,
                IsFolderPicker = true,
                Multiselect = false,
                NavigateToShortcut = true,
            };

            if (!string.IsNullOrWhiteSpace(defaultDirectory))
                dialog.DefaultDirectory = defaultDirectory;

            if (dialog.ShowDialog() != CommonFileDialogResult.Ok) return false;

            directory = dialog.FileName;
            return true;
        }

        public static bool GetFile(string fileType, string extension, out string path)
        {
            path = string.Empty;
            var dialog = new CommonOpenFileDialog()
            {
                EnsurePathExists = true,
                Multiselect = false,
                NavigateToShortcut = true
            };

            dialog.Filters.Add(new CommonFileDialogFilter(fileType, extension));

            if (dialog.ShowDialog() != CommonFileDialogResult.Ok) return false;
            path = dialog.FileName;
            return true;
        }

        public static bool GetFiles(string fileType, string extension, out List<string> paths)
        {
            paths = new List<string>();
            var dialog = new CommonOpenFileDialog()
            {
                EnsurePathExists = true,
                Multiselect = true,
                NavigateToShortcut = true
            };

            dialog.Filters.Add(new CommonFileDialogFilter(fileType, extension));

            if (dialog.ShowDialog() != CommonFileDialogResult.Ok) return false;
            paths = new List<string>(dialog.FileNames);
            return true;
        }

        public static bool SaveFile(string fileType, string extension, out string savePath)
        {
            savePath = "";
            var dialog = new CommonSaveFileDialog()
            {
                EnsureFileExists = true,
                OverwritePrompt = true,
                AlwaysAppendDefaultExtension = true,
            };
            
            dialog.Filters.Add(new CommonFileDialogFilter(fileType, extension));

            if (dialog.ShowDialog() != CommonFileDialogResult.Ok) return false;

            savePath = dialog.FileName;
            
            return true;
        }

        public static bool SaveFile(out string savePath, params string[] filetypes)
        {
            savePath = "";
            
            var dialog = new CommonSaveFileDialog()
            {
                EnsureFileExists = true,
                OverwritePrompt = true,
                AlwaysAppendDefaultExtension = true,
            };
            
            for (int i = 0; i < filetypes.Length; i++)
            {
                var a = filetypes[i].Split(',');
                if (a.Length != 2) throw new FormatException("The File types should be formatted as follows: \n 'Name,extension'");

                dialog.Filters.Add(new CommonFileDialogFilter(a[0], a[1]));
            }

            
            if (dialog.ShowDialog() != CommonFileDialogResult.Ok) return false;

            if (string.IsNullOrWhiteSpace(Path.GetExtension(savePath)))
                savePath = dialog.FileName + Core.SUPPORTED_EXPORTS[dialog.SelectedFileTypeIndex-1].Split(',')[1];

            return true;
        }
    }
}