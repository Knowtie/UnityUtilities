using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Assets.Code.Utility.FileIO {

    /// <summary>
    /// FileStreamIO is a helper class for persiting byte[] data to a filestream or reading from it.
    /// </summary>
    public class FileStreamIO {

        private readonly FileStream _FileStream;
        private int _Offset;

        public FileStreamIO(string aFilename, FileMode aMode) {
            _Offset = 0;
            _FileStream = new FileStream(aFilename, aMode, aMode == FileMode.Open ? FileAccess.Read : FileAccess.Write);
        }

        public byte[] Read(int aBytes) {
            byte[] DATA = new byte[aBytes];
            for (int INDEX = 0; INDEX < aBytes; INDEX++) {
                DATA[INDEX] = (byte) _FileStream.ReadByte();
                _Offset++;
            }
            return DATA;
        }

        public void Write<T>(T aData, Func<T, byte[]> aFunc) {
            foreach (byte BYTE in aFunc(aData)) {
                _FileStream.WriteByte(BYTE);
                _Offset++;
            }
        }

        public void Close() {
            _FileStream.Close();
        }
    }
}
