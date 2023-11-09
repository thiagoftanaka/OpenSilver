using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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
            set
            {
                _isFileSystemApiAvailable = value;
            }
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
                MessageBox.Show("The download will start once the data is fetched from the server.");

                return true;
            }

            TaskCompletionSource<IDisposable> taskCompletionSource = new TaskCompletionSource<IDisposable>();

            _handle = Interop.ExecuteJavaScript(@"
                (async () => {
                    const opts = {
                        startIn: 'downloads',
                        suggestedName: $2
                    };
                    return await window.showSaveFilePicker(opts);
                })().then(handle => { $0(); return handle; }, error => $1(error.toString()));",
                () => taskCompletionSource.SetResult(null),
                (Action<string>)((error) =>
                {
                    taskCompletionSource.SetException(
                    error.StartsWith("AbortError") ? new OperationCanceledException(error) : new Exception(error));
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
                MessageBox.Show("The download will start once the data is fetched from the server.");

                // Falling back to not using File System API as it might not be supported on this browser
                IsFileSystemApiAvailable = false;
                return true;
            }
        }
    }

    public class FileSaver
    {
        private const int BufferSize = 262144;

        private static bool _jsLibraryWasLoaded;

        private IDisposable _writableStream;
        private bool _isClosed = false;

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
                $0.then(async handle => await handle.createWritable())
                .then(stream => { $1(); return stream; }, error => $2(error.toString()));",
                    handle,
                    () => taskCompletionSource.SetResult(null),
                    (Action<string>)((error) => taskCompletionSource.SetException(new Exception(error))));

                await taskCompletionSource.Task;
            }
            else
            {
                if (await Initialize())
                {
                    _writableStream = Interop.ExecuteJavaScript($@"
                    let streamSaverWriter = streamSaver.createWriteStream($0 {(fallbackSize != null ? ", { size: $1 }" : "")}).getWriter();
 
                    // If the download has been canceled, the closed Promise will go call the rejected callback.
                    streamSaverWriter.closed.then(() => {{ }}, (error) => $2());
                    streamSaverWriter;", fallbackFilename, fallbackSize, () => _isClosed = true);
                }
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
                    $1.then(async writableStream => {
                        const binaryString = atob($0);
                        var uInt8 = new Uint8Array(binaryString.length);
                        for (var i = 0; i < binaryString.length; i++) uInt8[i] = binaryString.charCodeAt(i);
                        await writableStream.write(uInt8);
                    }).then(() => $2(), error => $3(error.toString()));",
                        base64,
                        _writableStream,
                        () => taskCompletionSource.SetResult(null),
                        (Action<string>)((error) => taskCompletionSource.SetException(new Exception(error))));

                    await taskCompletionSource.Task;
                }
                else
                {
                    OpenSilver.Interop.ExecuteJavaScriptVoid(@"const binaryString = atob($0);
                    var uInt8 = new Uint8Array(binaryString.length);
                    for (var i = 0; i < binaryString.length; i++) uInt8[i] = binaryString.charCodeAt(i);
                    $1.write(uInt8);", base64, _writableStream);
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

                    Interop.ExecuteJavaScriptVoid(@"$0.then(async writableStream => await writableStream.close())
                    .then(() => $1(), error => $2(error.toString()));",
                        _writableStream,
                        () => taskCompletionSource.SetResult(null),
                        (Action<string>)((error) => taskCompletionSource.SetException(new Exception(error))));

                    await taskCompletionSource.Task;
                }
                else
                {
                    Interop.ExecuteJavaScriptVoid(@"$0.close()", _writableStream);
                }

                _writableStream.Dispose();
                _writableStream = null;
                _isClosed = true;
            }
        }

        internal static async Task<bool> Initialize()
        {
            if (!_jsLibraryWasLoaded)
            {
                await Interop.LoadJavaScriptFile("https://cdn.jsdelivr.net/npm/streamsaver@2.0.6/StreamSaver.min.js");
                Interop.ExecuteJavaScriptVoid("streamSaver.mitm = './libs/mitm.html'");
                _jsLibraryWasLoaded = true;
            }
            return true;
        }
    }
}
