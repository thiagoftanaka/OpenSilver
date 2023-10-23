
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

using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace System.IO.IsolatedStorage
{
    internal sealed class IsolatedStorageSettingsForCSharp : IDictionary<string, object>
    {
        #region Constants/Variables

        private const string Filename = "Settings.bin";
        private readonly Dictionary<string, object> _appDictionary = new Dictionary<string, object>();

        private static readonly IsolatedStorageSettingsForCSharp StaticIsolatedStorageSettings;

        private static readonly IFormatter Formatter;

        private static readonly IsolatedStorageSettingsForCSharp StaticDomainIsolatedStorageSettings;

        static IsolatedStorageSettingsForCSharp()
        {
            // Formatter has to be instantiated first because it is used during the instance constructor execution
            Formatter = new BinaryFormatter();
            StaticIsolatedStorageSettings = new IsolatedStorageSettingsForCSharp();
            StaticDomainIsolatedStorageSettings = new IsolatedStorageSettingsForCSharp(isForDomain: true);
        }

        #endregion

        #region Singleton Implementation

        private IsolatedStorageSettingsForCSharp(bool isForDomain)
        {
            LoadData(isForDomain);
        }

        private IsolatedStorageSettingsForCSharp() : this(false) { }

        /// <summary>
        ///     Its a static singleton instance.
        /// </summary>
        public static IsolatedStorageSettingsForCSharp Instance
        {
            get { return StaticIsolatedStorageSettings; }
        }

        public static IsolatedStorageSettingsForCSharp DomainInstance => StaticDomainIsolatedStorageSettings;

        //TODO : verify we don't need the method below using Bridge
        // public acces´s for tests
        public void LoadData(bool isForDomain)
        {
            // IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            IsolatedStorageFile isoStore;
            if (!OpenSilver.Interop.IsRunningInTheSimulator)
            {
                isoStore = isForDomain
                    ? IsolatedStorageFile.GetUserStoreForDomain()
                    : IsolatedStorageFile.GetUserStoreForApplication();
            }
            else
            {
                // Using GetUserStoreForAssembly for Simulator because it threw Application Identity-related exception
                isoStore = IsolatedStorageFile.GetUserStoreForAssembly();
            }

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
                Save();
            }
        }

        /// <summary>
        ///     It serializes dictionary in binary format and stores it in a binary file.
        /// </summary>
        public void Save()
        {
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
        }

        #endregion

        #region IDictionary<string, object> Members

        public void Add(string key, object value)
        {
            _appDictionary.Add(key, value);
            Save();
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
                Save();
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
                return _appDictionary.Values;
            }
        }

        public object this[string key]
        {
            get { return _appDictionary[key]; }
            set
            {
                _appDictionary[key] = value;
                Save();
            }
        }


        public void Add(KeyValuePair<string, object> item)
        {
            _appDictionary.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _appDictionary.Clear();
            Save();
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
