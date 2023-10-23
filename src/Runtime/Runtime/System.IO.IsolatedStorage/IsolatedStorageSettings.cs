
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
using System.Linq;
using System.Windows;
using CSHTML5.Types;
using OpenSilver;

namespace System.IO.IsolatedStorage
{
    /// <summary>
    /// Provides a System.Collections.Generic.Dictionary&lt;TKey,TValue&gt; that stores
    /// key-value pairs in isolated storage.
    /// </summary>
    /// <example>
    /// Here is how to use the IsolatedStorageSettings:
    /// <code lang="C#">
    /// //Write in the IsolatedStorageSettings:
    /// IsolatedStorageSettings.ApplicationSettings["someKey"] = "someValue";
    /// //Read from it:
    /// string value;
    /// string myString = IsolatedStorageSettings.ApplicationSettings.TryGetValue("someKey", out value);
    /// </code>
    /// </example>
    public sealed partial class IsolatedStorageSettings : IEnumerable, IEnumerable<KeyValuePair<string, object>>
    {
        string _fullApplicationName = null;
        private AppDomain _appDomain;

        dynamic GetLocalStorage()
        {
            return Interop.ExecuteJavaScript("window.localStorage");
        } 

        string GetKeysFirstPart()
        {
            return $"storage_{_fullApplicationName}{(_appDomain != null ? $"_{_appDomain.Id}" : "")}_settings_";
        }

        private IsolatedStorageSettings() : this(null)
        {
        }

        private IsolatedStorageSettings(AppDomain appDomain)
        {
            _fullApplicationName = Application.Current.ToString();
            _appDomain = appDomain;
        }

        static IsolatedStorageSettings _applicationSettings = null;
        static IsolatedStorageSettings _domainSettings;

        /// <summary>
        /// Gets an instance of System.IO.IsolatedStorage.IsolatedStorageSettings that
        /// contains the contents of the application's System.IO.IsolatedStorage.IsolatedStorageFile,
        /// scoped at the application level, or creates a new instance of System.IO.IsolatedStorage.IsolatedStorageSettings
        /// if one does not exist.
        /// </summary>
        public static IsolatedStorageSettings ApplicationSettings
        {
            get
            {
                if (_applicationSettings == null)
                {
                    _applicationSettings = new IsolatedStorageSettings();
                }
                return _applicationSettings;
            }
        }

        /// <summary>
        /// Gets the number of key-value pairs that are stored in the dictionary.
        /// </summary>
        public int Count
        {
            get
            {
                if (!Interop.IsRunningInTheSimulator)
                {
                    return Interop.ExecuteJavaScriptInt32(
                            $"Object.keys(window.localStorage).filter(k => k.startsWith('{GetKeysFirstPart()}')).length");
                }
                else
                {
                    return GetSettingsForCSharpForApplicationOrSite().Count;
                }
            }
        }

        private IsolatedStorageSettingsForCSharp GetSettingsForCSharpForApplicationOrSite()
        {
            return _appDomain != null
                ? IsolatedStorageSettingsForCSharp.DomainInstance
                : IsolatedStorageSettingsForCSharp.Instance;
        }

        /// <summary>
        /// Gets a collection that contains the keys in the dictionary.
        /// </summary>
        public ICollection Keys
        {
            get
            {
                if (!Interop.IsRunningInTheSimulator)
                {
                    string keys = Interop.ExecuteJavaScriptString(
                            $"Object.keys(window.localStorage).filter(k => k.startsWith('{GetKeysFirstPart()}')).join(';')");
                    return keys?.Replace(GetKeysFirstPart(), "").Split(';');
                }
                else
                {
                    return GetSettingsForCSharpForApplicationOrSite().Keys.ToList<string>();
                }
            }
        }

        ////
        //// Summary:
        ////     Gets an instance of System.IO.IsolatedStorage.IsolatedStorageSettings that
        ////     contains the contents of the application's System.IO.IsolatedStorage.IsolatedStorageFile,
        ////     scoped at the domain level, or creates a new instance of System.IO.IsolatedStorage.IsolatedStorageSettings
        ////     if one does not exist.
        ////
        //// Returns:
        ////     An System.IO.IsolatedStorage.IsolatedStorageSettings object that contains
        ////     the contents of the application's System.IO.IsolatedStorage.IsolatedStorageFile,
        ////     scoped at the domain level. If an instance does not already exist, a new
        ////     instance is created.
        public static IsolatedStorageSettings SiteSettings =>
            _domainSettings ??= new IsolatedStorageSettings(AppDomain.CurrentDomain);

        /// <summary>
        /// Gets a collection that contains the values in the dictionary.
        /// </summary>
        public ICollection Values
        {
            get
            {
                if (!Interop.IsRunningInTheSimulator)
                {
                    string keys = Interop.ExecuteJavaScriptString(
                            $"Object.entries(window.localStorage).filter(([k, v], index) => k.startsWith('{GetKeysFirstPart()}')).map(([k, v], index) => v).join(';')");
                    return keys.Split(';');
                }
                else
                {
                    return IsolatedStorageSettingsForCSharp.Instance.Values.ToList();
                }
            }
        }

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the item to get or set.</param>
        /// <returns>
        /// The value associated with the specified key. If the specified key is not
        /// found, a get operation throws a System.Collections.Generic.KeyNotFoundException,
        /// and a set operation creates a new element that has the specified key.
        /// </returns>
        public object this[string key]
        {
            get
            {
                if (!Interop.IsRunningInTheSimulator)
                {
                    var result = Interop.ExecuteJavaScript("window.localStorage[$0]", GetKeysFirstPart() + key)
                        as INTERNAL_JSObjectReference;
                    return result?.GetActualValue();
                }
                else
                {
                    return GetSettingsForCSharpForApplicationOrSite()[key];
                }
            }
            set
            {
                if (!Interop.IsRunningInTheSimulator)
                {
                    Interop.ExecuteJavaScriptVoidAsync("window.localStorage[$0] = $1", GetKeysFirstPart() + key, value);
                }
                else
                {
                    GetSettingsForCSharpForApplicationOrSite()[key] = value;
                }
            }
        }

        /// <summary>
        /// Adds an entry to the dictionary for the key-value pair.
        /// </summary>
        /// <param name="key">The key for the entry to be stored.</param>
        /// <param name="value">The value to be stored.</param>
        public void Add(string key, object value)
        {
            if (!Interop.IsRunningInTheSimulator)
            {
                Interop.ExecuteJavaScriptVoidAsync("window.localStorage[$0] = $1", GetKeysFirstPart() + key, value);
            }
            else
            {
                GetSettingsForCSharpForApplicationOrSite().Add(key, value);
                GetSettingsForCSharpForApplicationOrSite().Save();
            }
        }

        /// <summary>
        /// Resets the count of items stored in System.IO.IsolatedStorage.IsolatedStorageSettings
        /// to zero and releases all references to elements in the collection.
        /// </summary>
        public void Clear()
        {
            if (!Interop.IsRunningInTheSimulator)
            {
                foreach (string key in Keys)
                {
                    Interop.ExecuteJavaScriptVoidAsync("delete window.localStorage[$0]", GetKeysFirstPart() + key);
                }
            }
            else
            {
                GetSettingsForCSharpForApplicationOrSite().Clear();
            }
        }

        /// <summary>
        /// Determines if the application settings dictionary contains the specified
        /// key.
        /// </summary>
        /// <param name="key">The key for the entry to be located.</param>
        /// <returns>true if the dictionary contains the specified key; otherwise, false.</returns>
        public bool Contains(string key)
        {
            if (!Interop.IsRunningInTheSimulator)
            {
                var result = Interop.ExecuteJavaScript("window.localStorage[$0]", GetKeysFirstPart() + key)
                    as INTERNAL_JSObjectReference;
                return result?.GetActualValue() != null;
            }
            else
            {
                return GetSettingsForCSharpForApplicationOrSite().ContainsKey(key);
            }
        }

        /// <summary>
        /// Removes the entry with the specified key.
        /// </summary>
        /// <param name="key">The key for the entry to be deleted.</param>
        /// <returns>true if the specified key was removed; otherwise, false.</returns>
        public bool Remove(string key)
        {
            if (!Interop.IsRunningInTheSimulator)
            {
                return Interop.ExecuteJavaScriptBoolean(
                    $@"let existedBefore = Object.keys(window.localStorage).includes('{GetKeysFirstPart() + key}');
delete window.localStorage['{GetKeysFirstPart() + key}'];
existedBefore && !Object.keys(window.localStorage).includes('{GetKeysFirstPart() + key}');");
            }
            else
            {
                bool ret = GetSettingsForCSharpForApplicationOrSite().Remove(key);
                GetSettingsForCSharpForApplicationOrSite().Save();
                return ret;
            }
        }

        //below is commented because the data is directly saved when changed.
        ///// <summary>
        ///// Saves data written to the current System.IO.IsolatedStorage.IsolatedStorageSettings
        ///// object.
        ///// </summary>
        //public void Save()
        //{
        //    //todo.
        //}


        /// <summary>
        /// Gets a value for the specified key.
        /// </summary>
        /// <typeparam name="T">The System.Type of the value parameter.</typeparam>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">
        /// When this method returns, the value associated with the specified key if
        /// the key is found; otherwise, the default value for the type of the value
        /// parameter. This parameter is passed uninitialized.
        /// </param>
        /// <returns>true if the specified key is found; otherwise, false.</returns>
        public bool TryGetValue<T>(string key, out T value)
        {
            if (!Interop.IsRunningInTheSimulator)
            {
                object valueAttempt = this[key];
                if (valueAttempt != null)
                {
                    value = (T)valueAttempt;
                    return true;
                }
                value = default;
                return false;
            }
            else
            {
                object temp;
                bool ret = GetSettingsForCSharpForApplicationOrSite().TryGetValue(key, out temp);
                value = (T)temp;
                return ret;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (KeyValuePair<string, object> kv in EnumerateKeyValues())
            {
                yield return kv;
            }
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            foreach (KeyValuePair<string, object> kv in EnumerateKeyValues())
            {
                yield return kv;
            }
        }

        IEnumerable<KeyValuePair<string, object>> EnumerateKeyValues()
        {
            if (!Interop.IsRunningInTheSimulator)
            {
                foreach (var keyValuePair in ((IEnumerable<string>)Keys).Zip((IEnumerable<object>)Values,
                             (key, value) => new KeyValuePair<string, object>(key, value)))
                {
                    yield return keyValuePair;
                }
            }
            else
            {
                foreach (KeyValuePair<string, object> kv in GetSettingsForCSharpForApplicationOrSite())
                {
                    yield return kv;
                }
            }
        }

#region for the interfaces that we remove for now
        //public void Add(KeyValuePair<string, object> item)
        //{
        //    throw new NotImplementedException();
        //}

        //public bool Contains(KeyValuePair<string, object> item)
        //{
        //    throw new NotImplementedException();
        //}

        //public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        //{
        //    throw new NotImplementedException();
        //}

        //public bool IsReadOnly
        //{
        //    get { throw new NotImplementedException(); }
        //}

        //public bool Remove(KeyValuePair<string, object> item)
        //{
        //    throw new NotImplementedException();
        //}





        //IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        //{
        //    throw new NotImplementedException();
        //}

        //public void Add(object key, object value)
        //{
        //    throw new NotImplementedException();
        //}

        //public bool Contains(object key)
        //{
        //    throw new NotImplementedException();
        //}

        //IDictionaryEnumerator IDictionary.GetEnumerator()
        //{
        //    throw new NotImplementedException();
        //}

        //public bool IsFixedSize
        //{
        //    get { throw new NotImplementedException(); }
        //}

        //public void Remove(object key)
        //{
        //    throw new NotImplementedException();
        //}

        //public object this[object key]
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }
        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public void CopyTo(Array array, int index)
        //{
        //    throw new NotImplementedException();
        //}

        //public bool IsSynchronized
        //{
        //    get { throw new NotImplementedException(); }
        //}

        //public object SyncRoot
        //{
        //    get { throw new NotImplementedException(); }
        //}


        //public bool ContainsKey(string key)
        //{
        //    throw new NotImplementedException();
        //}

        //ICollection<string> IDictionary<string, object>.Keys
        //{
        //    get { throw new NotImplementedException(); }
        //}

        //public bool TryGetValue(string key, out object value)
        //{
        //    throw new NotImplementedException();
        //}

        //ICollection<object> IDictionary<string, object>.Values
        //{
        //    get { throw new NotImplementedException(); }
        //}

        //void ICollection.CopyTo(Array array, int index)
        //{
        //    throw new NotImplementedException();
        //}

        //int ICollection.Count
        //{
        //    get { throw new NotImplementedException(); }
        //}

        //bool ICollection.IsSynchronized
        //{
        //    get { throw new NotImplementedException(); }
        //}

        //object ICollection.SyncRoot
        //{
        //    get { throw new NotImplementedException(); }
        //}
#endregion
    }
}
