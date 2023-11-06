using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OpenSilver.IO;

namespace System.Windows.Controls
{
    public sealed class SaveFileDialog
    {
        [OpenSilver.NotImplemented]
        public string Filter
        {
            get;
            set;
        }

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
        public Stream OpenFile()
        {
            var fileSaver = new FileSaver(GetFilename());

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
        public bool? ShowDialog()
        {
            return true;
        }
    }

    public class FileSaver
    {
        private const int BufferSize = 262144;

        private static bool _jsLibraryWasLoaded;

        private readonly string _filename;

        private IDisposable _streamSaverWriter;
        private bool _isClosed = false;

        public FileSaver(string filename)
        {
            _filename = filename;
        }

        ~FileSaver()
        {
            Close();
        }

        internal async Task OpenFile(long size)
        {
            if (await Initialize())
            {
                _streamSaverWriter = OpenSilver.Interop.ExecuteJavaScript(@"
                    let streamSaverWriter = streamSaver.createWriteStream($0, { size: $1 }).getWriter();

                    // If the download has been canceled, the closed Promise will go call the rejected callback.
                    streamSaverWriter.closed.then(() => { },
                    (error) => { $2() });
                    streamSaverWriter;", _filename, size, () => _isClosed = true);
            }
        }

        internal async Task Write(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            if (string.IsNullOrEmpty(_filename))
            {
                throw new InvalidOperationException("Filename is empty");
            }

            if (_streamSaverWriter == null)
            {
                await OpenFile(bytes.Length);
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

                string base64 = Convert.ToBase64String(bytes.Skip(i).Take(bytesToWrite).ToArray());
                OpenSilver.Interop.ExecuteJavaScriptVoid(@"const binaryString = atob($0);
                    var uInt8 = new Uint8Array(binaryString.length);
                    for (var i = 0; i < binaryString.length; i++) uInt8[i] = binaryString.charCodeAt(i);
                    $1.write(uInt8);", base64, _streamSaverWriter);

                // This loop could be long running for large files, so the file is written in
                // chunks and Delay is called to give control back to UI so it does not freeze.
                await Task.Delay(1);

                i += bytesToWrite;
            } while (i < bytes.Length - 1);
        }

        internal void Close()
        {
            if (_streamSaverWriter != null && !_isClosed)
            {
                OpenSilver.Interop.ExecuteJavaScriptVoid(@"$0.close()", _streamSaverWriter);
                _streamSaverWriter.Dispose();
                _streamSaverWriter = null;
                _isClosed = true;
            }
        }

        internal static async Task<bool> Initialize()
        {
            if (!_jsLibraryWasLoaded)
            {
                await OpenSilver.Interop.LoadJavaScriptFile("https://cdn.jsdelivr.net/npm/streamsaver@2.0.6/StreamSaver.min.js");
                _jsLibraryWasLoaded = true;
            }
            return true;
        }
    }
}
