

/*===================================================================================
* 
*   Copyright (c) Userware/OpenSilver.net
*      
*   This file is part of the OpenSilver Runtime (https://opensilver.net), which is
*   licensed under the MIT license: https://opensource.org/licenses/MIT
*   
*   As stated in the MIT license, "the above copyright notice and this permission
*   notice shall be included in all copies or substantial portions of the Software."
*  
\*====================================================================================*/


using CSHTML5.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.Serialization;
#if !BRIDGE
using System.Runtime.Serialization.Formatters.Binary;
#endif
using System.Text;
using System.Threading.Tasks;

namespace System.IO.IsolatedStorage
{
    internal partial class IsolatedStorageSettingsForCSharp : IDictionary<string, Object>
    {
        #region Constants/Variables

        private const string Filename = "Settings.bin";
        private readonly Dictionary<string, object> _appDictionary = new Dictionary<string, object>();

        private static readonly IsolatedStorageSettingsForCSharp StaticIsolatedStorageSettings = new IsolatedStorageSettingsForCSharp();

        //TODO implemente bellow with BRIDGE (seems long)
#if !BRIDGE
        private static readonly IFormatter Formatter = new BinaryFormatter();

        private static readonly IsolatedStorageSettingsForCSharp StaticDomainIsolatedStorageSettings = new(isForDomain: true);
#endif

        #endregion

        #region Singleton Implementation

#if !BRIDGE
        private IsolatedStorageSettingsForCSharp(bool isForDomain)
        {
            LoadData(isForDomain);
        }

        private IsolatedStorageSettingsForCSharp() : this(false) { }
#else
        /// <summary>
        ///     Its a private constructor.
        /// </summary>
        private IsolatedStorageSettingsForCSharp()
        {
        }
#endif

        /// <summary>
        ///     Its a static singleton instance.
        /// </summary>
        public static IsolatedStorageSettingsForCSharp Instance
        {
            get { return StaticIsolatedStorageSettings; }
        }


#if !BRIDGE
        public static IsolatedStorageSettingsForCSharp DomainInstance => StaticDomainIsolatedStorageSettings;

        //TODO : verify we don't need the method below using Bridge
        // public acces´s for tests
        public void LoadData(bool isForDomain)
        {
            // IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            IsolatedStorageFile isoStore = isForDomain ?
                IsolatedStorageFile.GetMachineStoreForDomain() :
                IsolatedStorageFile.GetMachineStoreForApplication();

            if (isoStore.GetFileNames(Filename).Length == 0)
            {
                // File not exists. Let us NOT try to DeSerialize it.        
                return;
            }

            // Read the stream from Isolated Storage.    
            Stream stream = new IsolatedStorageFileStream(Filename, FileMode.OpenOrCreate, isoStore);
            try
            {
                // DeSerialize the Dictionary from stream.    
                object bytes = Formatter.Deserialize(stream);

                var appData = (Dictionary<string, object>) bytes;

                // Enumerate through the collection and load our Dictionary.            
                IDictionaryEnumerator enumerator = appData.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    _appDictionary[enumerator.Key.ToString()] = enumerator.Value;
                }
            }
            finally
            {
                stream.Close();
            }
        }
#endif

        #endregion

        #region Methods

        /// <summary>
        ///     It Checks if Dictionary object has item corresponding to passed key,
        ///     if True then it returns that object else it returns default value.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultvalue"></param>
        /// <returns></returns>
        public object this[string key, Object defaultvalue]
        {
            get
            {
                return _appDictionary.ContainsKey(key)
                           ? _appDictionary[key]
                           : defaultvalue;
            }
            set
            {
                _appDictionary[key] = value;
#if !BRIDGE
                Save();
#endif
            }
        }

        /// <summary>
        ///     It serializes dictionary in binary format and stores it in a binary file.
        /// </summary>
        public void Save()
        {
            //BRIDGETODO : implemente the code below
#if !BRIDGE
            // IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForAssembly();

            Stream stream = new IsolatedStorageFileStream(Filename, FileMode.Create, isoStore);
            try
            {
                // Serialize dictionary into the IsolatedStorage.                                
                Formatter.Serialize(stream, _appDictionary);
            }
            finally
            {
                stream.Close();
            }      
#endif
        }

        #endregion

        #region IDictionary<string, object> Members

        public void Add(string key, object value)
        {
            _appDictionary.Add(key, value);
#if !BRIDGE
            Save();
#endif
        }

        public bool ContainsKey(string key)
        {
            return _appDictionary.ContainsKey(key);
        }

        public ICollection<string> Keys
        {
            get { return _appDictionary.Keys; }
        }

        public bool Remove(string key)
        {
            try
            {
#if !BRIDGE
                Save();
#endif
                _appDictionary.Remove(key);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool TryGetValue(string key, out object value)
        {
            return _appDictionary.TryGetValue(key, out value);
        }

        public ICollection<object> Values
        {
            get
            {
#if BRIDGE
                return INTERNAL_BridgeWorkarounds.GetDictionaryValues_SimulatorCompatible(_appDictionary).ToList();
#else
                return _appDictionary.Values;
#endif
            }
        }

        public object this[string key]
        {
            get { return _appDictionary[key]; }
            set
            {
                _appDictionary[key] = value;
#if !BRIDGE
                Save();
#endif
            }
        }


        public void Add(KeyValuePair<string, object> item)
        {
            _appDictionary.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _appDictionary.Clear();
#if !BRIDGE
            Save();
#endif
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return _appDictionary.ContainsKey(item.Key);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return _appDictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return _appDictionary.Remove(item.Key);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _appDictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _appDictionary.GetEnumerator();
        }

        #endregion
    }
}
