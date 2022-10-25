using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenSilver.IO
{
    public class MemoryFileStream : MemoryStream
    {
        private readonly List<byte> _bytes = new List<byte>();
        private readonly Action<byte[]> _writeCallback;
        private bool _isFlushed = true;

        public MemoryFileStream(Action<byte[]> writeCallback)
        {
            _writeCallback = writeCallback;
        }

        public override void Flush()
        {
            _writeCallback?.Invoke(_bytes.ToArray());
            _isFlushed = true;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _bytes.AddRange(buffer.Skip(offset).Take(count));
            _isFlushed = false;
        }

        protected override void Dispose(bool disposing)
        {
            if (!_isFlushed)
            {
                Flush();
            }

            base.Dispose(disposing);
        }
    }
}
