using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OpenSilver.IO;

namespace OpenSilver.Controls
{
    // If possible, use <a> with 'Content-Disposition: attachment' server response header instead.
    // This will make downloads from the server be entirely managed by the browser (progress, folder, etc). Example:
    // OpenSilver.Interop.ExecuteJavaScript($@"const downloadElement = document.createElement('a');
    //    downloadElement.download = '{filename}';
    //    downloadElement.href = '{filepath}';
    //    downloadElement.target = '_blank';
    //    downloadElement.click();");

    // This uses File System API whenever possible.
    // In browsers like Firefox and Safari, fallback to StreamSaver.js is used.

    public sealed class SaveFileDialog
    {
        private object _handle;

        private static bool? _isFileSystemApiAvailable;
        internal static bool IsFileSystemApiAvailable
        {
            get
            {
                if (_isFileSystemApiAvailable == null)
                {
                    _isFileSystemApiAvailable = !string.IsNullOrEmpty(
                        Interop.ExecuteJavaScript("window.showSaveFilePicker").ToString());
                }
                return _isFileSystemApiAvailable.GetValueOrDefault();
            }
            set => _isFileSystemApiAvailable = value;
        }

        public string Filter { get; set; }

        [OpenSilver.NotImplemented]
        public int FilterIndex
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the file name for the selected file associated with the SaveFileDialog.
        /// </summary>
        public string SafeFileName => Path.GetFileName(GetFilename());

        // Summary:
        //     Gets or sets the default file name extension applied to files that are saved
        //     with the System.Windows.Controls.SaveFileDialog.
        //
        // Returns:
        //     The default file name extension applied to files that are saved with the System.Windows.Controls.SaveFileDialog,
        //     which can optionally include the dot character (.).
        public string DefaultExt { get; set; }

        //
        // Summary:
        //     Gets or sets the file name used if a file name is not specified by the user.
        //
        // Returns:
        //     The file name used if a file name is not specified by the user.System.String.
        //
        // Exceptions:
        //   T:System.ArgumentException:
        //     Occurs if the specified file name is null or contains invalid characters such
        //     as quotes ("), less than (<), greater than (>), pipe (|), backspace (\b), null
        //     (\0), tab (\t), colon (:), asterisk(*), question mark (?), and slashes (\\, /).
        public string DefaultFileName { get; set; }

        //
        // Summary:
        //     Opens the file specified by the System.Windows.Controls.SaveFileDialog.SafeFileName
        //     property.
        //
        // Returns:
        //     A read-write stream for the file specified by the System.Windows.Controls.SaveFileDialog.SafeFileName
        //     property.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     No file was selected in the dialog box.
        public async Task<Stream> OpenFile(long? size = null)
        {
            var fileSaver = new FileSaver();
            await fileSaver.OpenFile(_handle, SafeFileName, size);

            MemoryFileStream memoryFileStream = new MemoryFileStream(bytes => fileSaver.Write(bytes),
                () => fileSaver.Close());
            return memoryFileStream;
        }

        private string GetFilename()
        {
            if (DefaultFileName != null && Path.HasExtension(DefaultFileName))
            {
                return DefaultFileName;
            }
            return Path.ChangeExtension(DefaultFileName ?? "data", DefaultExt ?? "dat");
        }

        //
        // Summary:
        //     Displays a System.Windows.Controls.SaveFileDialog that is modal to the Web browser
        //     or main window.
        //
        // Returns:
        //     true if the user clicked Save; false if the user clicked Cancel or closed the
        //     dialog box.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     Silverlight was unable to display the dialog box due to an improperly formatted
        //     filter, an invalid filter index or other reasons.
        //
        //   T:System.Security.SecurityException:
        //     Active Scripting in Internet Explorer is disabled.-or-The call to the System.Windows.Controls.OpenFileDialog.ShowDialog
        //     method was not made from user-initiated code or too much time passed between
        //     user-initiation and the display of the dialog.
        public async Task<bool?> ShowDialog()
        {
            if (!IsFileSystemApiAvailable)
            {
                return true;
            }

            TaskCompletionSource<IDisposable> taskCompletionSource = new TaskCompletionSource<IDisposable>();

            _handle = Interop.ExecuteJavaScript(@"
                try {
                    (async () => {
                        const opts = {
                            startIn: 'downloads',
                            suggestedName: $2
                        };
                        return await window.showSaveFilePicker(opts);
                    })().then(handle => { $0(); return handle; }, error => { $1(error.toString()) });
                } catch (error) {
                    console.error(error);
                    throw error;
                }",
                () => taskCompletionSource.SetResult(null),
                (Action<string>)(error =>
                {
                    // Errors thrown when dialogs are canceled are suppressed
                    if (!error.StartsWith("AbortError"))
                    {
                        taskCompletionSource.SetException(new Exception(error));
                    }
                }),
                SafeFileName);

            try
            {
                await taskCompletionSource.Task;

                return true;
            }
            catch (OperationCanceledException)
            {
                // Cancel button was hit on file dialog
                return false;
            }
            catch (Exception)
            {
                // Falling back to not using File System API as it might not be supported on this browser
                IsFileSystemApiAvailable = false;
                return true;
            }
        }
    }

    public class FileSaver
    {
        // This is the size in which writes don't seem to block for too long
        private const int BufferSize = 131072;

        private IDisposable _writableStream;
        private bool _isClosed = false;
        private string _filename;

        ~FileSaver()
        {
            Close();
        }

        /// <summary>
        /// </summary>
        /// <param name="handle">JS Promise object that fulfills to a FileSystemWritableFileStream if File System API is available.</param>
        /// <param name="fallbackFilename">Filename to save in case downloads are managed by the browser.</param>
        /// <param name="fallbackSize">File size to show progress in case downloads are managed by the browser.</param>
        /// <returns></returns>
        internal async Task OpenFile(object handle, string fallbackFilename, long? fallbackSize)
        {
            if (SaveFileDialog.IsFileSystemApiAvailable)
            {
                TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>();

                _writableStream = Interop.ExecuteJavaScript(@"
                    try {
                        $0.then(async handle => await handle.createWritable())
                        .then(stream => { $1(); return stream; }, error => { console.error(error); $2(error.toString()) });
                    } catch (error) {
                        console.error(error);
                        throw error;
                    }",
                    handle,
                    () => taskCompletionSource.SetResult(null),
                    (Action<string>)(error => taskCompletionSource.SetException(new Exception(error))));

                await taskCompletionSource.Task;
            }
            else
            {
                _writableStream = Interop.ExecuteJavaScript(@"[];");
                _filename = fallbackFilename;
            }
        }

        internal async Task Write(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            if (_writableStream == null)
            {
                throw new InvalidOperationException("Unable to write to an unopened file.");
            }

            int bytesToWrite = BufferSize;
            int i = 0;
            do
            {
                if (_isClosed)
                {
                    break;
                }

                if (i + bytesToWrite > bytes.Length)
                {
                    bytesToWrite = bytes.Length - i;
                }

                TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>();

                string base64 = Convert.ToBase64String(bytes.Skip(i).Take(bytesToWrite).ToArray());

                if (SaveFileDialog.IsFileSystemApiAvailable)
                {
                    Interop.ExecuteJavaScriptVoid(@"
                        try {
                            $1.then(async writableStream => {
                                const binaryString = atob($0);
                                let uint8 = new Uint8Array(binaryString.length);
                                for (let i = 0; i < binaryString.length; i++) uint8[i] = binaryString.charCodeAt(i);
                                await writableStream.write(uint8);
                            }).then(() => $2(), error => { console.error(error); $3(error.toString()) });
                        } catch (error) {
                            console.error(error);
                            throw error;
                        }",
                        base64,
                        _writableStream,
                        () => taskCompletionSource.SetResult(null),
                        (Action<string>)(error => taskCompletionSource.SetException(new Exception(error))));

                    await taskCompletionSource.Task;
                }
                else
                {
                    Interop.ExecuteJavaScriptVoid(@"
                        try {
                            const binaryString = atob($0);
                            for (let i in binaryString) {
                                $1.push(binaryString[i]);
                            }
                        } catch (error) {
                            console.error(error);
                            throw error;
                        }", base64, _writableStream);
                }

                // This loop could be long running for large files, so the file is written in
                // chunks and Delay is called to give control back to UI so it does not freeze.
                await Task.Delay(1);

                i += bytesToWrite;
            } while (i < bytes.Length - 1);
        }

        internal async void Close()
        {
            if (_writableStream != null && !_isClosed)
            {
                if (SaveFileDialog.IsFileSystemApiAvailable)
                {
                    TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>();

                    Interop.ExecuteJavaScriptVoid(@"
                        try {
                            $0.then(async writableStream => await writableStream.close())
                            .then(() => $1(), error => { $2(error.toString()) });
                        } catch (error) {
                            console.error(error);
                            throw error;
                        }",
                        _writableStream,
                        () => taskCompletionSource.SetResult(null),
                        (Action<string>)(error =>
                        {
                            /* Errors are suppressed because it could just be that downloads were canceled */
                        }));

                    await taskCompletionSource.Task;
                }
                else
                {
                    Interop.ExecuteJavaScriptVoid(@"
                        try {
                            let uint8Array = new Uint8Array($0.length);
                            for (let i = 0; i < $0.length; i++) uint8Array[i] = $0[i].charCodeAt(0);

                            // Must pass TypedArray as regular Array to Blob constructor, that's why it's wrapped in []
                            saveAs(new Blob([uint8Array]), $1);
                        } catch (error) {
                            console.error(error);
                            throw error;
                        }", _writableStream, _filename);
                }

                _writableStream.Dispose();
                _writableStream = null;
                _isClosed = true;
            }
        }
    }
}
