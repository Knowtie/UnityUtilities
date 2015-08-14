using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Code.Utility.FileIO;

namespace Assets.Code.Utility.FileIO {

    /// <summary>
    /// A data filter is used to persist a single type of data to a file stream.  It can persist any kind of
    /// data format that can support the Persister and Loader formats, which should be almost all data.
    /// </summary>
    /// <typeparam name="T">Identifies the data that can be persisted.</typeparam>
    public class DataFiler<T> {
        private readonly Persister<T> _Persister;
        private readonly Loader<T> _Loader;

        public DataFiler(Persister<T> aPersister, Loader<T> aLoader) {
            _Persister = aPersister;
            _Loader = aLoader;
        }

        public void Persist(int aIdentifier , T aData, FileStreamIO IO) {
            IO.Write(aIdentifier, BitConverter.GetBytes);
            _Persister(IO, aData);
        }

        public T Load(FileStreamIO IO) {
            return _Loader(IO);
        }
    }
}
