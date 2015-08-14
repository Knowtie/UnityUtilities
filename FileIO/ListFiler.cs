using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Code.Utility.FileIO {

    public delegate void Persister<in T>(FileStreamIO IO, T aData);
    public delegate T Loader<out T>(FileStreamIO IO);

    public class ListFiler<T> {
        private readonly Persister<T> _Persister;
        private readonly Loader<T> _Loader;

        public ListFiler(Persister<T> aPersister, Loader<T> aLoader) {
            _Persister = aPersister;
            _Loader = aLoader;
        }

        public void Persist(int aIdentifier , List<T> aArray , FileStreamIO IO) {
            IO.Write(aIdentifier, BitConverter.GetBytes);
            IO.Write(aArray.Count, BitConverter.GetBytes);
            foreach (T ELEMENT in aArray) {
                _Persister(IO, ELEMENT);
            }
        }

        public List<T> Load(FileStreamIO IO) {
            List<T> LIST = new List<T>();
            int COUNT = FileUtility.ReadInt(IO);
            for (int IDX = 0; IDX < COUNT; IDX++) {
                LIST.Add(_Loader(IO));
            }
            return LIST;
            
        }
        
    }
}
