using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace CustomVisionCompanion.Services
{
    public static class DialogService
    {
        public static Task ShowAsync(string message, string title = null)
        {
            var dialog = new MessageDialog(message, title);
            return dialog.ShowAsync().AsTask();
        }
    }
}
