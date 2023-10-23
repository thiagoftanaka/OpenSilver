﻿using System;
using System.Collections;

namespace OpenSilver.Internal
{
    /// <summary>
    ///    This allows callers to "foreach" through a WeakReferenceList.
    ///    Each weakreference is checked for liveness and "current"
    ///    actually returns a strong reference to the current element.
    /// </summary>
    /// <remarks>
    ///    Due to the way enumerators function, this enumerator often
    ///    holds a cached strong reference to the "Current" element.
    ///    This should not be a problem unless the caller stops enumerating
    ///    before the end of the list AND holds the enumerator alive forever.
    /// </remarks>
    internal struct WeakReferenceListEnumerator : IEnumerator
    {
        public WeakReferenceListEnumerator(ArrayList List)
        {
            _i = 0;
            _List = List;
            _StrongReference = null;
        }

        object IEnumerator.Current
        { get { return Current; } }

        public object Current
        {
            get
            {
                if (null == _StrongReference)
                {
                    throw new InvalidOperationException("No current object to return.");
                }
                return _StrongReference;
            }
        }

        public bool MoveNext()
        {
            object obj = null;
            while (_i < _List.Count)
            {
                WeakReference weakRef = (WeakReference)_List[_i++];
                obj = weakRef.Target;
                if (null != obj)
                    break;
            }
            _StrongReference = obj;
            return (null != obj);
        }

        public void Reset()
        {
            _i = 0;
            _StrongReference = null;
        }

        int _i;
        ArrayList _List;
        object _StrongReference;
    }
}