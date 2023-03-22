

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


#if (!FOR_DESIGN_TIME) && CORE
extern alias ToBeReplacedAtRuntime;
#endif

#if BRIDGE
using Bridge;
#else
using JSIL.Meta;
#endif

using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Diagnostics;
using System.Reflection;
using System.ComponentModel;
using System.Configuration;
using System.Threading;
using System.ServiceModel.Channels;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml;
using CSHTML5.Internal;
using static System.ServiceModel.INTERNAL_WebMethodsCaller;
using Binding = System.ServiceModel.Channels.Binding;

#if MIGRATION
using System.Windows;
using System.Text;
using System.Windows.Data;
using System.Windows.Shapes;
#else
using Windows.UI.Xaml;
#endif

#if OPENSILVER
using DataContractSerializerCustom = System.Runtime.Serialization.DataContractSerializer_CSHTML5Ver;
#else // BRIDGE
using DataContractSerializerCustom = System.Runtime.Serialization.DataContractSerializer;
#endif

namespace System.ServiceModel
{
    /// <summary>
    /// Provides the base implementation used to create Windows Communication Foundation
    /// (WCF) client objects that can call services.
    /// </summary>
    /// <typeparam name="TChannel">The channel to be used to connect to the service.</typeparam>
    /// <example>
    /// Here is an example on how you can use a WebService to receive data:
    /// <code lang="C#">
    /// //We create a new instance of the ServiceClient
    /// MyServiceClient soapClient =
    ///     new MyServiceClient(
    ///         new BasicHttpBinding(),
    ///         new EndpointAddress(
    ///             new Uri("http://MyServiceAddress.com/MyService.svc")));
    ///
    /// //We call the method that will give us the data we want:
    /// var result = await soapClient.GetToDosAsync(_ownerId);
    /// //We get the data from the response:
    /// ToDoItem[] todos = result.Body.GetToDosResult;
    /// </code>
    /// Here is another example that shows how you can send data to a WebService:
    /// <code lang="C#">
    /// //We create an item to send to the WebService:
    /// ToDoItem todo = new ToDoItem()
    /// {
    ///     Description = MyTextBox.Text,
    ///     Id = Guid.NewGuid(),
    /// };
    ///
    /// //We create a new instance of the ServiceClient
    /// MyServiceClient soapClient =
    ///     new MyServiceClient(
    ///         new BasicHttpBinding(),
    ///         new EndpointAddress(
    ///             new Uri("http://MyServiceAddress.com/MyService.svc")));
    ///
    /// //We send the data by calling a method implemented for that purpose in the WebService:
    /// await soapClient.AddOrUpdateToDoAsync(todo);
    /// </code>
    /// </example>
#if BRIDGE
    public abstract partial class CSHTML5_ClientBase<TChannel> : ICommunicationObject where TChannel : class
#elif OPENSILVER
    public abstract partial class CSHTML5_ClientBase<TChannel> /*: ICommunicationObject, IDisposable*/ where TChannel : class
#endif
    {
#if OPENSILVER
        //Note: Adding this because they are in the file generated when adding a Service Reference through the "Add Connected Service" for OpenSilver.
        public System.ServiceModel.Description.ServiceEndpoint Endpoint => ChannelFactory?.Endpoint;
        public System.ServiceModel.Description.ClientCredentials ClientCredentials { get; } = new Description.ClientCredentials();
#endif
        string _remoteAddressAsString;

        private TChannel channel;
        public TChannel Channel
        {
            get
            {
                if (channel == null)
                {
                    channel = CreateChannel();
                }
                return channel;
            }
        }

        private ChannelFactory<TChannel> _channelFactory;
        public ChannelFactory<TChannel> ChannelFactory => _channelFactory;

        /// <summary>
        /// Provides support for implementing the event-based asynchronous pattern.
        /// </summary>
        /// <param name="beginOperationDelegate">A delegate that is used for calling the asynchronous operation.</param>
        /// <param name="inValues">The input values to the asynchronous call.</param>
        /// <param name="endOperationDelegate">A delegate that is used to end the asynchronous call after it has completed.</param>
        /// <param name="operationCompletedCallback">
        /// A client-supplied callback that is invoked when the asynchronous method is
        /// complete. The callback is passed to the BeginOperationDelegate.
        /// </param>
        /// <param name="userState">The userState object to associate with the asynchronous call.</param>
        protected void InvokeAsync(BeginOperationDelegate beginOperationDelegate, object[] inValues,
          EndOperationDelegate endOperationDelegate, SendOrPostCallback operationCompletedCallback, object userState)
        {
            AsyncOperation asyncOperation = AsyncOperationManager.CreateOperation(userState);
            AsyncOperationContext context = new AsyncOperationContext(asyncOperation, endOperationDelegate, operationCompletedCallback);
            IAsyncResult result = beginOperationDelegate(inValues, OnAsyncCallCompleted, context);
        }

        static void OnAsyncCallCompleted(IAsyncResult result)
        {
            AsyncOperationContext context = (AsyncOperationContext)result.AsyncState;
            Exception error = null;
            object[] results = null; //todo: fix type?
            try
            {
                results = context.EndDelegate(result);
            }
            catch (Exception e)
            {
                error = e;
            }
            CompleteAsyncCall(context, results, error);
        }

        static void CompleteAsyncCall(AsyncOperationContext context, object[] results, Exception error)
        {
            if (context.CompletionCallback != null)
            {
                InvokeAsyncCompletedEventArgs e = new InvokeAsyncCompletedEventArgs(results, error, false, context.AsyncOperation.UserSuppliedState);
                context.AsyncOperation.PostOperationCompleted(context.CompletionCallback, e);
            }
            else
            {
                context.AsyncOperation.OperationCompleted();
            }
        }

        class AsyncOperationContext
        {
            AsyncOperation asyncOperation;
            EndOperationDelegate endDelegate;
            SendOrPostCallback completionCallback;

            internal AsyncOperationContext(AsyncOperation asyncOperation, EndOperationDelegate endDelegate, SendOrPostCallback completionCallback)
            {
                this.asyncOperation = asyncOperation;
                this.endDelegate = endDelegate;
                this.completionCallback = completionCallback;
            }

            internal AsyncOperation AsyncOperation
            {
                get
                {
                    return this.asyncOperation;
                }
            }

            internal EndOperationDelegate EndDelegate
            {
                get
                {
                    return this.endDelegate;
                }
            }

            internal SendOrPostCallback CompletionCallback
            {
                get
                {
                    return this.completionCallback;
                }
            }

        }

        /// <summary>
        /// A delegate that is used by ClientBase&lt;TChannel&gt;.InvokeAsync(...)
        /// for calling asynchronous operations on the client.
        /// </summary>
        /// <param name="inValues"></param>
        /// <param name="asyncCallback"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected delegate IAsyncResult BeginOperationDelegate(object[] inValues, AsyncCallback asyncCallback, object state);

        /// <summary>
        /// A delegate that is invoked by ClientBase&lt;TChannel&gt;.InvokeAsync(...)
        /// on successful completion of the call made by ClientBase&lt;TChannel&gt;.InvokeAsync(...)
        /// to ClientBase&lt;TChannel&gt;.BeginOperationDelegate.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        protected delegate object[] EndOperationDelegate(IAsyncResult result);

        /// <summary>
        /// Stores the results from an asynchronous call made by the client.
        /// </summary>
        protected class InvokeAsyncCompletedEventArgs : AsyncCompletedEventArgs
        {
            object[] results;

            internal InvokeAsyncCompletedEventArgs(object[] results, Exception error, bool cancelled, object userState)
                : base(error, cancelled, userState)
            {
                this.results = results;
            }

            public object[] Results
            {
                get
                {
                    return this.results;
                }
            }
        }

        public string INTERNAL_RemoteAddressAsString
        {
            get
            {
                return _remoteAddressAsString;
            }
        }

        public virtual string INTERNAL_SoapVersion
        {
            get
            {
                MessageVersion messageVersion = _channelFactory?.Endpoint?.Binding?.MessageVersion;
                if (messageVersion == MessageVersion.Soap11 ||
                    messageVersion == MessageVersion.Soap11WSAddressingAugust2004)
                {
                    return "1.1";
                }
                else if (messageVersion == MessageVersion.Soap12WSAddressing10 ||
                    messageVersion == MessageVersion.Soap12WSAddressing10)
                {
                    return "1.2";
                }
                return null;
            }
        }

        public IList<MessageHeader> MessageHeaders => (Channel as ChannelBase<TChannel>)?.MessageHeaders;

        //#if !FOR_DESIGN_TIME && CORE
        //        [JSIgnore]
        //        INTERNAL_RealClientBaseImplementation<TChannel> _realClientBase;
        //#endif

        /// <summary>
        /// Initializes a new instance of the System.ServiceModel.ClientBase`1
        /// class using the default target endpoint from the application configuration
        /// file.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Either there is no default endpoint information in the configuration file,
        /// more than one endpoint in the file, or no configuration file.
        /// </exception>
        protected CSHTML5_ClientBase()
        {
            // we get the name of the contract we are looking for
            string contractConfigurationName = null;
            Type interfacetype = this.GetType().BaseType.GetGenericArguments()[0];
            foreach (ServiceContractAttribute attr in interfacetype.GetCustomAttributes(typeof(ServiceContractAttribute), false))
            {
                if (attr.ConfigurationName != null)
                {
                    contractConfigurationName = attr.ConfigurationName;
                    break;
                }
            }
            if (contractConfigurationName == null)
            {
                throw new Exception(
                    string.Format("Could not find a suitable ServiceContractAttribute to get the default endpoint in the type: '{0}'.",
                                  interfacetype.FullName));
            }

            // Attempt to read the WCF endpoint address by first looking into the 
            // "ServiceReferences.ClientConfig" file, and then the "App.Config" file
            string endpointAddress;
            Binding binding;
            using (var serviceReferencesClientConfig = OpenSilver.Interop.ExecuteJavaScript("window.ServiceReferencesClientConfig")) {
                if (TryReadEndpoint(serviceReferencesClientConfig,
                        "ServiceReferences.ClientConfig",
                        contractConfigurationName,
                        false /* throw if not found */,
                        out endpointAddress,
                        out binding))
                {
                    _remoteAddressAsString = endpointAddress;
                }
                else
                {
                    using (var appConfig = OpenSilver.Interop.ExecuteJavaScript("window.AppConfig")) {
                        if (TryReadEndpoint(appConfig,
                                "App.Config",
                                contractConfigurationName,
                                true /* throw if not found */,
                                out endpointAddress,
                                out binding))
                        {
                            _remoteAddressAsString = endpointAddress;
                        }
                        else
                        {
                            throw new Exception(
                                string.Format("Could not find the default WCF endpoint element that references the contract '{0}' in the ServiceModel client configuration section.",
                                    contractConfigurationName));
                        }
                    }
                }
            }

            _channelFactory = new ChannelFactory<TChannel>(binding, new EndpointAddress(endpointAddress));
        }

    private static bool TryReadEndpoint(
            object configFileContent,
            string fileName,
            string contractConfigurationName,
            bool throwIfFileNotFound,
            out string endpointAddress,
            out Binding binding)
        {
            bool isNullOrUndefined = OpenSilver.Interop.ExecuteJavaScriptBoolean(
                $"!{CSHTML5.INTERNAL_InteropImplementation.GetVariableStringForJS(configFileContent)}");
            if (!isNullOrUndefined)
            {
                string fileContentAsString = Convert.ToString(configFileContent);

                XDocument doc = XDocument.Parse(fileContentAsString);

                endpointAddress = null;
                binding = null;
                string bindingConfiguration = null;
                // Attempt to find the elements that contain the correct Binding and endpointAddress:
                foreach (XElement endpointXElement in doc.Descendants("endpoint"))
                {
                    XAttribute contractXAttribute = endpointXElement.Attributes("contract").FirstOrDefault();
                    if (contractXAttribute != null &&
                        contractXAttribute.Value == contractConfigurationName)
                    {
                        XAttribute addressXAttribute = endpointXElement.Attributes("address").FirstOrDefault();
                        if (addressXAttribute != null)
                        {
                            endpointAddress = addressXAttribute.Value;
                        }

                        XAttribute bindingConfigurationXAttribute = endpointXElement
                            .Attributes("bindingConfiguration").FirstOrDefault();
                        if (bindingConfigurationXAttribute != null)
                        {
                            bindingConfiguration = bindingConfigurationXAttribute.Value;
                        }
                    }
                }

                if (bindingConfiguration != null)
                {
                    foreach (XElement bindingXElement in doc.Descendants("binding"))
                    {
                        XAttribute bindingNameXAttribute = bindingXElement.Attributes("name").FirstOrDefault();
                        if (bindingNameXAttribute != null &&
                            bindingNameXAttribute.Value == bindingConfiguration)
                        {
                            BindingElementCollection collection = new BindingElementCollection();
                            if (bindingXElement.Descendants("binaryMessageEncoding").Any())
                            {
                                collection.Add(new BinaryMessageEncodingBindingElement());
                            }

                            binding = new CustomBinding(collection.ToArray());
                            break;
                        }
                    }
                }

                if (endpointAddress != null && binding != null)
                {
                    return true;
                }
                return false;
            }
            else
            {
                //--------------------------
                // FILE NOT FOUND
                //--------------------------

                if (throwIfFileNotFound)
                {
                    throw new FileNotFoundException(
                        string.Format("Could not find the '{0}' file to get the default endpoint. Please make sure that you have added your service reference through 'Add service reference' or use the constructor that allows you to set the Binding and Endpoint programmatically.\nSee http://www.cshtml5.com/links/wcf-limitations-and-tutorials.aspx for details.",
                                      fileName));
                }
                else
                {
                    endpointAddress = null;
                    binding = null;
                    return false;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the System.ServiceModel.ClientBase`1
        /// class using the configuration information specified in the application configuration
        /// file by endpointConfigurationName.
        /// </summary>
        /// <param name="endpointConfigurationName">
        /// The name of the endpoint in the application configuration file.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// The specified endpoint information is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The endpoint cannot be found or the endpoint contract is not valid.
        /// </exception>
        protected CSHTML5_ClientBase(string endpointConfigurationName)
        {
            throw new NotSupportedException("Please specify the Binding and Endpoint programmatically. See http://www.cshtml5.com/links/wcf-limitations-and-tutorials.aspx for details.");
            //todo
        }

        /// <summary>
        /// Initializes a new instance of the System.ServiceModel.ClientBase`1
        /// class using the specified binding and target address.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteAddress">The address of the service endpoint.</param>
        /// <exception cref="ArgumentNullException">
        /// The binding is null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// The remote address is null.
        /// </exception>
        protected CSHTML5_ClientBase(Binding binding, EndpointAddress remoteAddress)
        {
            if (remoteAddress == null)
            {
                throw new ArgumentNullException("remoteAddress");
            }

            _remoteAddressAsString = remoteAddress.Uri.OriginalString;

            _channelFactory = new ChannelFactory<TChannel>(binding, remoteAddress);
        }

        /// <summary>
        /// Initializes a new instance of the System.ServiceModel.ClientBase`1
        /// class using the specified target address and endpoint information.
        /// </summary>
        /// <param name="endpointConfigurationName">The name of the endpoint in the application configuration file.</param>
        /// <param name="remoteAddress">The address of the service.</param>
        /// <exception cref="ArgumentNullException">
        /// The endpoint is null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// The remote address is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The endpoint cannot be found or the endpoint contract is not valid.
        /// </exception>
        protected CSHTML5_ClientBase(string endpointConfigurationName, EndpointAddress remoteAddress)
        {
            throw new NotSupportedException("Please specify the Binding and Endpoint programmatically. See http://www.cshtml5.com/links/wcf-limitations-and-tutorials.aspx for details.");
            //todo
        }

        /// <summary>
        /// Initializes a new instance of the System.ServiceModel.ClientBase`1
        /// class.
        /// </summary>
        /// <param name="endpointConfigurationName">The name of the endpoint in the application configuration file.</param>
        /// <param name="remoteAddress">The address of the service.</param>
        /// <exception cref="ArgumentNullException">
        /// The endpoint is null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// The remote address is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The endpoint cannot be found or the endpoint contract is not valid.
        /// </exception>
        protected CSHTML5_ClientBase(string endpointConfigurationName, string remoteAddress)
        {
            throw new NotSupportedException("Please specify the Binding and Endpoint programmatically. See http://www.cshtml5.com/links/wcf-limitations-and-tutorials.aspx for details.");
            //todo
        }

#if !FOR_DESIGN_TIME
        /// <summary>
        /// Provides an API to call web methods defined in a WebService
        /// </summary>
        public partial class WebMethodsCaller
        {
            protected const string SoapVersion11 = "1.1";
            protected const string SoapVersion12 = "1.2";

            string _addressOfService;

            INTERNAL_WebRequestHelper_JSOnly _webRequestHelper_JSVersion = new INTERNAL_WebRequestHelper_JSOnly();

            /// <summary>
            /// Constructor for the WebMethodsCaller's class
            /// </summary>
            /// <param name="addressOfService">The address of the WebService</param>
            public WebMethodsCaller(string addressOfService)
            {
                _addressOfService = addressOfService;
            }

            public void BeginCallWebMethod(
                string webMethodName,
                Type interfaceType,
                Type methodReturnType,
                IDictionary<string, object> originalRequestObject,
                Action<string> callback,
                string soapVersion,
                CSHTML5_ClientBase<TChannel> client)
            {
                BeginCallWebMethod(webMethodName, interfaceType, methodReturnType, null, "", originalRequestObject,
                    callback, soapVersion, client);
            }

#if OPENSILVER
            public void BeginCallWebMethod(
                string webMethodName,
                Type interfaceType,
                Type methodReturnType,
                IEnumerable<MessageHeader> outgoingMessageHeaders,
                IDictionary<string, object> originalRequestObject,
                Action<string> callback,
                string soapVersion,
                CSHTML5_ClientBase<TChannel> client)
            {
                BeginCallWebMethod(webMethodName, interfaceType, methodReturnType, null,
                    GetEnvelopeHeaders(outgoingMessageHeaders?.ToList(), soapVersion), originalRequestObject,
                    callback, soapVersion, client);
            }
#endif

            public void BeginCallWebMethod(
                string webMethodName,
                Type interfaceType,
                Type methodReturnType,
                string messageHeaders,
                IDictionary<string, object> originalRequestObject,
                Action<string> callback,
                string soapVersion,
                CSHTML5_ClientBase<TChannel> client)
            {
                BeginCallWebMethod(webMethodName, interfaceType, methodReturnType, null,
                    messageHeaders, originalRequestObject, callback, soapVersion, client);
            }

            public void BeginCallWebMethod(
                string webMethodName,
                Type interfaceType,
                Type methodReturnType,
                IReadOnlyList<Type> knownTypes,
                string messageHeaders,
                IDictionary<string, object> originalRequestObject,
                Action<string> callback,
                string soapVersion,
                CSHTML5_ClientBase<TChannel> client)
            {
                MethodInfo method = ResolveMethod(interfaceType, webMethodName, "Begin" + webMethodName);
                bool isXmlSerializer = IsXmlSerializer(webMethodName, methodReturnType, method);

                Dictionary<string, string> headers;
                object request;
                PrepareRequest(
                    webMethodName,
                    method,
                    interfaceType,
                    methodReturnType,
                    knownTypes,
                    messageHeaders,
                    originalRequestObject,
                    soapVersion,
                    isXmlSerializer,
                    out headers,
                    out request,
                    client);

                Uri address = INTERNAL_UriHelper.EnsureAbsoluteUri(_addressOfService);

                // Make the actual web service call
                _webRequestHelper_JSVersion.MakeRequest(
                    address,
                    "POST",
                    this,
                    headers,
                    request,
                    (sender, e) =>
                    {
                        string xmlReturnedFromTheServer = e.Result;
                        callback(xmlReturnedFromTheServer);
                    },
                    true,
                    Application.Current.Host.Settings.DefaultSoapCredentialsMode);
            }

            public object EndCallWebMethod(
               string webMethodName,
               Type interfaceType,
               Type methodReturnType,
               string xmlReturnedFromTheServer,
               string soapVersion,
               CSHTML5_ClientBase<TChannel> client)
            {
                return EndCallWebMethod(webMethodName,
                     interfaceType,
                     methodReturnType,
                     null,
                     xmlReturnedFromTheServer,
                     soapVersion,
                     client);
            }

            public object EndCallWebMethod(
                     string webMethodName,
                     Type interfaceType,
                     Type methodReturnType,
                     IReadOnlyList<Type> knownTypes,
                     string xmlReturnedFromTheServer,
                     string soapVersion,
                     CSHTML5_ClientBase<TChannel> client)
            {
                MethodInfo beginMethod = ResolveMethod(interfaceType, webMethodName, "Begin" + webMethodName);
                bool isXmlSerializer = IsXmlSerializer(webMethodName,
                                                       methodReturnType,
                                                       beginMethod);

                object requestResponse = ReadAndPrepareResponse(
                    xmlReturnedFromTheServer,
                    interfaceType,
                    methodReturnType,
                    knownTypes,
                    faultException =>
                    {
                        throw faultException;
                    },
                    isXmlSerializer,
                    soapVersion,
                    client);

                return requestResponse;
            }

            public RETURN_TYPE EndCallWebMethod<RETURN_TYPE>(
                string webMethodName,
                Type interfaceType,
                string xmlReturnedFromTheServer,
                string soapVersion,
                CSHTML5_ClientBase<TChannel> client)
            {
                return (RETURN_TYPE)EndCallWebMethod(
                    webMethodName,
                    interfaceType,
                    typeof(RETURN_TYPE),
                    xmlReturnedFromTheServer,
                    soapVersion,
                    client);
            }

            internal Task<T> CallWebMethodAsyncBeginEnd<T>(
                string webMethodName,
                Type interfaceType,
                Type methodReturnType,
                IDictionary<string, object> originalRequestObject,
                string soapVersion,
                CSHTML5_ClientBase<TChannel> client)
            {
                TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();

                AsyncCallback callback = new AsyncCallback(delegate (IAsyncResult asyncResponseResult)
                {
                    try
                    {
                        T result = EndCallWebMethod<T>(
                            webMethodName,
                            interfaceType,
                            ((WebMethodAsyncResult)asyncResponseResult).XmlReturnedFromTheServer,
                            soapVersion,
                            client);
                        tcs.SetResult(result);
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                });
                object asyncState = null;

                WebMethodAsyncResult webMethodAsyncResult = new WebMethodAsyncResult(callback, asyncState);

                BeginCallWebMethod(
                    webMethodName,
                    interfaceType,
                    methodReturnType,
                    originalRequestObject,
                    (xmlReturnedFromTheServer) =>
                    {
                        // After server call has finished (not deserialized yet)
                        webMethodAsyncResult.XmlReturnedFromTheServer = xmlReturnedFromTheServer;

                        // This causes a call to "EndCallWebMethod" which will deserialize the response.
                        webMethodAsyncResult.Completed();
                    },
                    soapVersion,
                    client);

                return tcs.Task;
            }

#if OPENSILVER
            internal Task<(T, MessageHeaders)> CallWebMethodAsyncBeginEnd<T>(
                string webMethodName,
                Type interfaceType,
                Type methodReturnType,
                IEnumerable<MessageHeader> outgoingMessageHeaders,
                IDictionary<string, object> originalRequestObject,
                string soapVersion,
                CSHTML5_ClientBase<TChannel> client)
            {
                TaskCompletionSource<(T, MessageHeaders)> tcs = new TaskCompletionSource<(T, MessageHeaders)>();

                AsyncCallback callback = new AsyncCallback(delegate (IAsyncResult asyncResponseResult)
                {
                    try
                    {
                        T result = EndCallWebMethod<T>(
                            webMethodName,
                            interfaceType,
                            ((WebMethodAsyncResult)asyncResponseResult).XmlReturnedFromTheServer,
                            soapVersion,
                            client);

                        var messageHeaders = GetEnvelopeHeaders(((WebMethodAsyncResult)asyncResponseResult).XmlReturnedFromTheServer, soapVersion);

                        tcs.SetResult((result, messageHeaders));
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                });
                object asyncState = null;

                WebMethodAsyncResult webMethodAsyncResult = new WebMethodAsyncResult(callback, asyncState);

                BeginCallWebMethod(
                    webMethodName,
                    interfaceType,
                    methodReturnType,
                    outgoingMessageHeaders,
                    originalRequestObject,
                    (xmlReturnedFromTheServer) =>
                    {
                        // After server call has finished (not deserialized yet)
                        webMethodAsyncResult.XmlReturnedFromTheServer = xmlReturnedFromTheServer;

                        // This causes a call to "EndCallWebMethod" which will deserialize the response.
                        webMethodAsyncResult.Completed();
                    },
                    soapVersion,
                    client);

                return tcs.Task;
            }

            /// <summary>
            /// Asynchronously calls a WebMethod.
            /// </summary>
            /// <typeparam name="T">The return type of the WebMethod</typeparam>
            /// <param name="webMethodName">The name of the WebMethod</param>
            /// <param name="interfaceType">The Type of the interface</param>
            /// <param name="methodReturnType">The return Type of the method</param>
            /// <param name="outgoingMessageHeaders">The outgoing message headers</param>
            /// <param name="originalRequestObject">The additional arguments of the method</param>
            /// <param name="soapVersion">The SOAP Version of the request</param>
            /// <returns>The result of the call of the method and the incoming message headers.</returns>
            public Task<(T, MessageHeaders)> CallWebMethodAsync<T>(
                string webMethodName,
                Type interfaceType,
                Type methodReturnType,
                IEnumerable<MessageHeader> outgoingMessageHeaders,
                IDictionary<string, object> originalRequestObject,
                string soapVersion,
                CSHTML5_ClientBase<TChannel> client) // Note: we don't arrive here using c#
            {
                // todo: find out what happens with methods that take multiple arguments 
                // (if possible) and change the parameterName to a string[].
                MethodInfo method = ResolveMethod(interfaceType, webMethodName, webMethodName + "Async");
                bool isXmlSerializer = IsXmlSerializer(webMethodName, methodReturnType, method);
                string outgoingMessageHeadersString = GetEnvelopeHeaders(outgoingMessageHeaders?.ToList(), soapVersion);

                Dictionary<string, string> headers;
                object request;
                PrepareRequest(
                    webMethodName,
                    method,
                    interfaceType,
                    methodReturnType,
                    null,
                    outgoingMessageHeadersString,
                    originalRequestObject,
                    soapVersion,
                    isXmlSerializer,
                    out headers,
                    out request,
                    client);

                var tcs = new TaskCompletionSource<(T, MessageHeaders)>(); //todo: here we need to change object to the return type

                string response = _webRequestHelper_JSVersion.MakeRequest(
                    new Uri(_addressOfService),
                    "POST",
                    this,
                    headers,
                    request,
                    (sender, args2) =>
                    {
                        ReadAndPrepareResponseGeneric_JSVersion(
                            tcs,
                            args2,
                            interfaceType,
                            methodReturnType,
                            null,
                            isXmlSerializer,
                            soapVersion
                            ,
                            client);
                    },
                    true,
                    Application.Current.Host.Settings.DefaultSoapCredentialsMode);

                return tcs.Task;
            }
#endif

            /// <summary>
            /// Asynchronously calls a WebMethod.
            /// </summary>
            /// <typeparam name="T">The return type of the WebMethod</typeparam>
            /// <param name="webMethodName">The name of the WebMethod</param>
            /// <param name="interfaceType">The Type of the interface</param>
            /// <param name="methodReturnType">The return Type of the method</param>
            /// <param name="originalRequestObject">The additional arguments of the method</param>
            /// <param name="soapVersion">The SOAP Version of the request</param>
            /// <returns>The result of the call of the method.</returns>
            public Task<T> CallWebMethodAsync<T>(
                string webMethodName,
                Type interfaceType,
                Type methodReturnType,
                IDictionary<string, object> originalRequestObject,
                string soapVersion,
                CSHTML5_ClientBase<TChannel> client) // Note: we don't arrive here using c#
            {
                // todo: find out what happens with methods that take multiple arguments 
                // (if possible) and change the parameterName to a string[].
                MethodInfo method = ResolveMethod(interfaceType, webMethodName, webMethodName + "Async");
                bool isXmlSerializer = IsXmlSerializer(webMethodName, methodReturnType, method);

                Dictionary<string, string> headers;
                object request;
                PrepareRequest(
                    webMethodName,
                    method,
                    interfaceType,
                    methodReturnType,
                    null,
                    "",
                    originalRequestObject,
                    soapVersion,
                    isXmlSerializer,
                    out headers,
                    out request,
                    client);

                var tcs = new TaskCompletionSource<T>(); //todo: here we need to change object to the return type

                string response = _webRequestHelper_JSVersion.MakeRequest(
                    new Uri(_addressOfService),
                    "POST",
                    this,
                    headers,
                    request,
                    (sender, args2) =>
                    {
                        ReadAndPrepareResponseGeneric_JSVersion(
                            tcs,
                            args2,
                            interfaceType,
                            methodReturnType,
                            null,
                            isXmlSerializer,
                            soapVersion,
                            client);
                    },
                    true,
                    Application.Current.Host.Settings.DefaultSoapCredentialsMode);

                return tcs.Task;
            }

            /// <summary>
            /// Calls a WebMethod
            /// </summary>
            /// <param name="webMethodName">The name of the Method</param>
            /// <param name="interfaceType">The Type of the interface</param>
            /// <param name="methodReturnType">The return Type of the method</param>
            /// <param name="originalRequestObject">The additional arguments of the method</param>
            /// <param name="soapVersion">The SOAP Version of the request</param>
            /// <returns>The result of the call of the method.</returns>
            public object CallWebMethod(
                string webMethodName,
                Type interfaceType,
                Type methodReturnType,
                IDictionary<string, object> originalRequestObject,
                string soapVersion,
                CSHTML5_ClientBase<TChannel> client) // Note: we don't arrive here using c#.
            {
                //**************************************
                // What the request should look like in case of classes or strings:
                //**************************************
                //<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
                //  <s:Body>
                //    <GetCurrentTime xmlns="http://tempuri.org/"/>
                //  </s:Body>
                //</s:Envelope>
                //**************************************
                // What the request should look like with parameters:
                //<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
                //  <s:Body>
                //    <AddTodo xmlns="http://tempuri.org/">
                //    <id>1</id>
                //    <todo>zsfzef</todo>
                //    <priority>1</priority>
                //    <dueDate i:nil="true" xmlns:i="http://www.w3.org/2001/XMLSchema-instance"/>
                //    </AddTodo>
                //  </s:Body>
                //</s:Envelope>
                //**************************************

                //**************************************
                // What the request should look like in case of value types (eg. int MethodName(int)):
                //**************************************

                //<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
                //    <s:Body>
                //        <MethodName xmlns="http://tempuri.org/">
                //            <value>10</value>
                //        </MethodName>
                //    </s:Body>
                //</s:Envelope>
                //**************************************

                MethodInfo method = ResolveMethod(interfaceType, webMethodName, webMethodName, "Begin" + webMethodName);
                bool isXmlSerializer = IsXmlSerializer(webMethodName, methodReturnType, method);

                Dictionary<string, string> headers;
                object request;
                PrepareRequest(
                    webMethodName,
                    method,
                    interfaceType,
                    methodReturnType,
                    null,
                    "",
                    originalRequestObject,
                    soapVersion,
                    isXmlSerializer,
                    out headers,
                    out request,
                    client);

                string response = _webRequestHelper_JSVersion.MakeRequest(
                        new Uri(_addressOfService),
                        "POST",
                        this,
                        headers,
                        request,
                        null,
                        false,
                        Application.Current.Host.Settings.DefaultSoapCredentialsMode);

                return ReadAndPrepareResponse(
                    response,
                    interfaceType,
                    methodReturnType,
                    null,
                    faultException =>
                    {
                        throw faultException;
                    },
                    isXmlSerializer,
                    soapVersion,
                    client);
            }

#if OPENSILVER
            /// <summary>
            /// Calls a WebMethod
            /// </summary>
            /// <param name="webMethodName">The name of the Method</param>
            /// <param name="interfaceType">The Type of the interface</param>
            /// <param name="methodReturnType">The return Type of the method</param>
            /// <param name="outgoingMessageHeaders">The outgoing message headers</param>
            /// <param name="originalRequestObject">The additional arguments of the method</param>
            /// <param name="soapVersion">The SOAP Version of the request</param>
            /// <returns>The result of the call of the method and the incoming message headers.</returns>
            public (object, MessageHeaders) CallWebMethod(
                string webMethodName,
                Type interfaceType,
                Type methodReturnType,
                IEnumerable<MessageHeader> outgoingMessageHeaders,
                IDictionary<string, object> originalRequestObject,
                string soapVersion,
                CSHTML5_ClientBase<TChannel> client) // Note: we don't arrive here using c#.
            {
                //**************************************
                // What the request should look like in case of classes or strings:
                //**************************************
                //<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
                //  <s:Body>
                //    <GetCurrentTime xmlns="http://tempuri.org/"/>
                //  </s:Body>
                //</s:Envelope>
                //**************************************
                // What the request should look like with parameters:
                //<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
                //  <s:Body>
                //    <AddTodo xmlns="http://tempuri.org/">
                //    <id>1</id>
                //    <todo>zsfzef</todo>
                //    <priority>1</priority>
                //    <dueDate i:nil="true" xmlns:i="http://www.w3.org/2001/XMLSchema-instance"/>
                //    </AddTodo>
                //  </s:Body>
                //</s:Envelope>
                //**************************************

                //**************************************
                // What the request should look like in case of value types (eg. int MethodName(int)):
                //**************************************

                //<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
                //    <s:Body>
                //        <MethodName xmlns="http://tempuri.org/">
                //            <value>10</value>
                //        </MethodName>
                //    </s:Body>
                //</s:Envelope>
                //**************************************

                MethodInfo method = ResolveMethod(interfaceType, webMethodName, webMethodName, "Begin" + webMethodName);
                bool isXmlSerializer = IsXmlSerializer(webMethodName, methodReturnType, method);
                var outgoingMessageHeadersString = GetEnvelopeHeaders(outgoingMessageHeaders?.ToList(), soapVersion);

                Dictionary<string, string> headers;
                object request;
                PrepareRequest(
                    webMethodName,
                    method,
                    interfaceType,
                    methodReturnType,
                    null,
                    outgoingMessageHeadersString,
                    originalRequestObject,
                    soapVersion,
                    isXmlSerializer,
                    out headers,
                    out request,
                    client);

                string response = _webRequestHelper_JSVersion.MakeRequest(
                        new Uri(_addressOfService),
                        "POST",
                        this,
                        headers,
                        request,
                        null,
                        false,
                        Application.Current.Host.Settings.DefaultSoapCredentialsMode);

                var typedResponseBody = ReadAndPrepareResponse(
                    response,
                    interfaceType,
                    methodReturnType,
                    null,
                    faultException =>
                    {
                        throw faultException;
                    },
                    isXmlSerializer,
                    soapVersion,
                    client);

                var incomingMessageHeaders = GetEnvelopeHeaders(response, soapVersion);

                return (typedResponseBody, incomingMessageHeaders);
            }
#endif

            public static MethodInfo ResolveMethod(Type interfaceType, string webMethodName, params string[] methodNames)
            {
                MethodInfo method = null;
                if (methodNames != null)
                {
                    for (int i = 0; i < methodNames.Length; i++)
                    {
                        if ((method = interfaceType.GetMethod(methodNames[i])) != null)
                            break;
                    }
                }
                return method ?? throw new MissingMethodException(
                    string.Format("Cannot find an operation named '{0}'.", webMethodName));
            }

            private static bool IsXmlSerializer(
                string webMethodName,
                Type methodReturnType,
                MethodInfo method)
            {
                if (methodReturnType != null)
                {
                    if (Regex.IsMatch(methodReturnType.Name, $@"{webMethodName}Response\d*$"))
                    {
                        if (methodReturnType.GetField("Body") != null)
                        {
                            return true;
                        }

                        if (methodReturnType.GetCustomAttribute<MessageContractAttribute>(true) != null)
                        {
                            return true;
                        }
                    }
                }

                if (method != null)
                {
                    ParameterInfo[] parameterInfos = method.GetParameters();
                    if (methodReturnType == typeof(IAsyncResult) &&
                       (parameterInfos != null && parameterInfos.Length > 0) &&
                        Regex.IsMatch(parameterInfos[0].ParameterType.Name, $@"{webMethodName}Request\d*$"))
                    {
                        if (parameterInfos[0].ParameterType
                            .GetCustomAttribute<MessageContractAttribute>(true) == null)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

#if OPENSILVER
            public static string GetEnvelopeHeaders(ICollection<MessageHeader> messageHeaders, string soapVersion)
            {
                if (messageHeaders == null || !messageHeaders.Any())
                {
                    return "";
                }

                var settings = new XmlWriterSettings { OmitXmlDeclaration = true };

                return string.Join("", messageHeaders.Select(mh =>
                {
                    using (var sw = new StringWriter())
                    using (var xw = XmlWriter.Create(sw, settings))
                    {
                        mh.WriteHeader(xw, soapVersion == "1.1" ? MessageVersion.Soap11 : MessageVersion.Soap12WSAddressing10);

                        xw.Flush();
                        return sw.ToString();
                    }
                }));
            }

            private static MessageHeaders GetEnvelopeHeaders(string incomingMessageString, string soapVersion)
            {
                using (var reader = XmlReader.Create(new StringReader(incomingMessageString)))
                {
                    var incomingMessage = Message.CreateMessage(reader, int.MaxValue, soapVersion == "1.1" ? MessageVersion.Soap11 : MessageVersion.Soap12WSAddressing10);
                    return incomingMessage.Headers;
                }
            }
#endif

            private void ProcessNode(XElement node, Action<XElement> action)
            {
                action(node);
                foreach (XElement child in node.Elements())
                {
                    ProcessNode(child, action);
                }
            }

            private void PrepareRequest(
                string webMethodName, // webMethod
                MethodInfo method, // method to look for in 'interfaceType'
                Type interfaceType,
                Type methodReturnType,
                IReadOnlyList<Type> knownTypes,
                string envelopeHeaders,
                IDictionary<string, object> requestParameters,
                string soapVersion,
                bool isXmlSerializer,
                out Dictionary<string, string> headers,
                out object request,
                CSHTML5_ClientBase<TChannel> client)
            {
                soapVersion = client?.INTERNAL_SoapVersion ?? soapVersion;
                headers = new Dictionary<string, string>();
                string requestFormat = null;

                string interfaceTypeName = interfaceType.Name; // default value
                string interfaceTypeNamespace = "http://tempuri.org/"; // default value
                string soapAction = string.Empty;


                ServiceContractAttribute serviceContractAttr =
                    (ServiceContractAttribute)interfaceType.GetCustomAttributes(typeof(ServiceContractAttribute), false)
                                                           .FirstOrDefault(); // note: there should never be more than one.

                if (serviceContractAttr != null)
                {
                    if (serviceContractAttr.Namespace != null &&
                        serviceContractAttr.Namespace != "http://tempuri.org") // default value if namespace is not set explicitly.
                    {
                        interfaceTypeNamespace = serviceContractAttr.Namespace;
                    }
                    if (!string.IsNullOrEmpty(serviceContractAttr.Name))
                    {
                        interfaceTypeName = serviceContractAttr.Name;
                    }
                }
                
                // Look for the soapAction.
                OperationContractAttribute operationContractAttr =
                    (OperationContractAttribute)method.GetCustomAttributes(typeof(OperationContractAttribute), false)
                                                        .FirstOrDefault(); // note: there should never be more than one.

                if (operationContractAttr != null)
                {
                    soapAction = operationContractAttr.Action;
                }

                if (string.IsNullOrEmpty(soapAction))
                {
                    soapAction = string.Format("{0}/{1}/{2}",
                                                interfaceTypeNamespace.Trim('/'),
                                                interfaceTypeName.Trim('/'),
                                                webMethodName);
                }

                BinaryMessageEncodingBindingElement binaryBindingElement = client?.ChannelFactory?.Endpoint?.Binding?
                    .CreateBindingElements().Find<BinaryMessageEncodingBindingElement>();
                bool isBinaryBinding = binaryBindingElement != null;

                switch (soapVersion)
                {
                    case SoapVersion11:
                        headers.Add("Content-Type", @"text/xml; charset=utf-8");
                        headers.Add("SOAPAction", soapAction);

                        requestFormat = "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\">{0}{1}</s:Envelope>";
                        break;

                    case SoapVersion12:
                        if (isBinaryBinding)
                        {
                            headers.Add("Content-Type", @"application/soap+msbin1");
                        }
                        else
                        {
                            headers.Add("Content-Type", @"application/soap+xml; charset=utf-8");
                        }

                        requestFormat = "<s:Envelope xmlns:a=\"http://www.w3.org/2005/08/addressing\" xmlns:s=\"http://www.w3.org/2003/05/soap-envelope\">{0}{1}</s:Envelope>";
                        break;

                    default:
                        throw new InvalidOperationException(
                            string.Format("SOAP version not supported: {0}",
                                          soapVersion));
                }

                // in every case, we want the name of the method as a XElement
                XElement methodNameElement =
                    new XElement(XNamespace.Get(interfaceTypeNamespace)
                                           .GetName(webMethodName));

                request = null;

                Message message = requestParameters?.Values.OfType<Message>().FirstOrDefault();

                // Note: now we want to add the parameters of the method
                // to do that, we basically get the serialized version of the objects, 
                // and replace their tag that should have the type with the parameter name.
                if (message == null && requestParameters != null)
                {
                    ParameterInfo[] parameterInfos = method.GetParameters();
                    int parametersCount = requestParameters != null ?
                                          requestParameters.Count :
                                          0;

                    for (int i = 0; i < parametersCount; ++i)
                    {
                        object requestBody = requestParameters[parameterInfos[i].Name];
                        if (requestBody != null)
                        {
                            var types = new List<Type>(knownTypes ?? Enumerable.Empty<Type>());
                            types.AddRange(
                                interfaceType.GetCustomAttributes(typeof(ServiceKnownTypeAttribute), true)
                                             .Select(o => ((ServiceKnownTypeAttribute)o).Type));

                            DataContractSerializerCustom dataContractSerializer =
                                new DataContractSerializerCustom(
                                    parameterInfos[i].ParameterType,
                                    types,
                                    isXmlSerializer);

                            XDocument xdoc = dataContractSerializer.SerializeToXDocument(requestBody);

                            XElement paramNameElement =
                                new XElement(XNamespace.Get(interfaceTypeNamespace)
                                                       .GetName(parameterInfos[i].Name));
                            if (!isXmlSerializer)
                            {
                                bool isBodyMemberSerialization = false;
                                if (requestBody.GetType().GetCustomAttribute<MessageContractAttribute>(true) != null)
                                {
                                    FieldInfo fieldInfo = requestBody.GetType()
                                        .GetFields()
                                        .FirstOrDefault(p => Attribute.IsDefined(p, typeof(MessageBodyMemberAttribute)));
                                    if (fieldInfo == null)
                                    {
                                        throw new ArgumentException(
                                            "Unable to find MessageBodyMemberAttribute of MessageContractAttribute contract");
                                    }

                                    isBodyMemberSerialization = true;
                                }

                                // we don't want to add this in the case of an XmlSerializer 
                                // because it would be <request> which is not what we want. 
                                // The correct parameter name is already in the Request body.
                                if (!isBodyMemberSerialization)
                                {
                                    methodNameElement.Add(paramNameElement);
                                }

                                foreach (XNode currentNode in xdoc.Root.Nodes())
                                {
                                    if (!isBodyMemberSerialization)
                                    {
                                        paramNameElement.Add(currentNode);
                                    }
                                    else if(currentNode is XElement currentElement)
                                    {
                                        currentElement.Name = methodNameElement.Name.Namespace +
                                                              currentElement.Name.LocalName;
                                        methodNameElement.Add(currentElement);
                                    }
                                }
                                foreach (XAttribute currentAttribute in xdoc.Root.Attributes())
                                {
                                    // we don't want to keep the "xmlns="http://schemas.microsoft.com/2003/10/Serialization/" 
                                    // because it breaks the request.
                                    if (currentAttribute.Name.LocalName != "xmlns")
                                    {
                                        if (!isBodyMemberSerialization)
                                        {
                                            paramNameElement.Add(currentAttribute);
                                        }
                                        else
                                        {
                                            methodNameElement.Add(currentAttribute);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //we assume that it always has the same structure 
                                // <root>
                                //   <AddOrUpdateToDoRequest xmlns="http://schemas.datacontract.org/2004/07/">
                                //      <Body>
                                //         <toDoItem
                                // so we want to go to xdoc.Root.Nodes()[0].Nodes()
                                foreach (XNode currentNode in xdoc.Root.Nodes())
                                {
                                    XElement xElement = currentNode as XElement;

                                    if (xElement != null)
                                    {
                                        foreach (XElement node in xElement.Elements())
                                        {
                                            ProcessNode(node, x => x.Name = XNamespace.Get(string.IsNullOrEmpty(x.Name.NamespaceName) ?
                                                                                           interfaceTypeNamespace :
                                                                                           x.Name.NamespaceName)
                                                                                      .GetName(x.Name.LocalName));
                                            methodNameElement.Add(node);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            // the value is null so we simply need to put the parameter name with i:nil="true" and we're good
                            XElement paramNameElement =
                                new XElement(XNamespace.Get(interfaceTypeNamespace)
                                                       .GetName(parameterInfos[i].Name));
                            XAttribute attribute =
                                new XAttribute(XNamespace.Get(DataContractSerializer_Helpers.XMLSCHEMA_NAMESPACE)
                                                         .GetName("nil"),
                                               "true");
                            paramNameElement.Add(attribute);
                            methodNameElement.Add(paramNameElement);
                        }
                    }
                }

#if OPENSILVER
                if (message == null)
                {
                    if (soapVersion == SoapVersion12)
                    {
                        envelopeHeaders = string.Format("<a:Action>{0}</a:Action>", soapAction) + (envelopeHeaders ?? "");
                    }
                    request = string.Format(requestFormat,
                                            !string.IsNullOrEmpty(envelopeHeaders) ?
                                                string.Format("<s:Header>{0}</s:Header>", envelopeHeaders) :
                                                "",
                                            string.Format("<s:Body>{0}</s:Body>",
                                            methodNameElement.ToString(SaveOptions.DisableFormatting)));
                }
                else
                {
                    string body;
                    using (MemoryStream memoryStream = new MemoryStream())
                    using (XmlDictionaryWriter xmlDictionaryWriter =
                        XmlDictionaryWriter.CreateTextWriter(memoryStream, Text.Encoding.UTF8, false))
                    using (StreamReader streamReader = new StreamReader(memoryStream))
                    {
                        message.WriteBody(xmlDictionaryWriter);
                        xmlDictionaryWriter.Flush();
                        memoryStream.Position = 0;
                        body = streamReader.ReadToEnd();
                    }

                    StringBuilder messageHeaders = new StringBuilder();
                    foreach (MessageHeader header in message.Headers)
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        using (XmlDictionaryWriter xmlDictionaryWriter =
                            XmlDictionaryWriter.CreateTextWriter(memoryStream, Text.Encoding.UTF8, false))
                        using (StreamReader streamReader = new StreamReader(memoryStream))
                        {
                            header.WriteHeader(xmlDictionaryWriter, MessageVersion.Default);
                            xmlDictionaryWriter.Flush();
                            memoryStream.Position = 0;
                            messageHeaders.Append(streamReader.ReadToEnd());
                        }
                    }

                    request = string.Format(requestFormat,
                                    string.Format("<s:Header>{0}</s:Header>", messageHeaders.ToString() + envelopeHeaders),
                                    body);
                }

                if (isBinaryBinding)
                {
                    Message binaryMessage;
                    if (message != null)
                    {
                        binaryMessage = message;
                    }
                    else
                    {
                        binaryMessage = Message.CreateMessage(client.ChannelFactory.Endpoint.Binding.MessageVersion,
                            soapAction, methodNameElement);

                        string xmlMessage = string.Format(requestFormat,
                                        !string.IsNullOrEmpty(envelopeHeaders) ?
                                            string.Format("<s:Header>{0}</s:Header>", envelopeHeaders) :
                                            "",
                                        string.Format("<s:Body>{0}</s:Body>",
                                        methodNameElement.ToString(SaveOptions.DisableFormatting)));
                        MessageHeaders messageHeaders = GetEnvelopeHeaders(xmlMessage, soapVersion);

                        binaryMessage.Headers.Clear();
                        foreach (MessageHeader messageHeader in messageHeaders)
                        {
                            binaryMessage.Headers.Add(messageHeader);
                        }

                        if (binaryMessage.Headers.To == null)
                        {
                            binaryMessage.Headers.To = client.ChannelFactory.Endpoint.Address.Uri;
                        }
                    }
                    MessageEncoder messageEncoder = binaryBindingElement.CreateMessageEncoderFactory().Encoder;

                    request = messageEncoder.WriteMessage(binaryMessage, int.MaxValue,
                        BufferManager.CreateBufferManager(2147483647, 2147483647)).ToArray();
                }
#else
            request = string.Format(requestFormat, DataContractSerializerCustom.XElementToString(methodNameElement));
#endif
        }

        private void ReadAndPrepareResponseGeneric_JSVersion<T>(
                TaskCompletionSource<T> taskCompletionSource,
                INTERNAL_WebRequestHelper_JSOnly_RequestCompletedEventArgs e,
                Type interfaceType,
                Type requestResponseType,
                IReadOnlyList<Type> knownTypes,
                bool isXmlSerializer,
                string soapVersion,
                CSHTML5_ClientBase<TChannel> client)
            {
                if (e.Error != null && string.IsNullOrEmpty(e.Result))
                {
                    taskCompletionSource.TrySetException(e.Error);
                }
                else
                {
                    T requestResponse = (T)ReadAndPrepareResponse(
                        e.Result,
                        interfaceType,
                        requestResponseType,
                        knownTypes,
                        faultException =>
                        {
                            taskCompletionSource.TrySetException(faultException);
                        },
                        isXmlSerializer,
                        soapVersion,
                        client);

                    // Note: this Task.IsCompleted can be true if we met an exception 
                    // which triggered a call to TrySetException (above).
                    if (!taskCompletionSource.Task.IsCompleted)
                    {
                        taskCompletionSource.SetResult(requestResponse);
                    }
                }
            }

#if OPENSILVER
            private void ReadAndPrepareResponseGeneric_JSVersion<T>(
                TaskCompletionSource<(T, MessageHeaders)> taskCompletionSource,
                INTERNAL_WebRequestHelper_JSOnly_RequestCompletedEventArgs e,
                Type interfaceType,
                Type requestResponseType,
                IReadOnlyList<Type> knownTypes,
                bool isXmlSerializer,
                string soapVersion,
                CSHTML5_ClientBase<TChannel> client)
            {
                if (e.Error == null)
                {
                    T requestResponse = (T)ReadAndPrepareResponse(
                        e.Result,
                        interfaceType,
                        requestResponseType,
                        knownTypes,
                        faultException =>
                        {
                            taskCompletionSource.TrySetException(faultException);
                        },
                        isXmlSerializer,
                        soapVersion,
                        client);

                    // Note: this Task.IsCompleted can be true if we met an exception 
                    // which triggered a call to TrySetException (above).
                    if (!taskCompletionSource.Task.IsCompleted)
                    {
                        taskCompletionSource.SetResult((requestResponse, GetEnvelopeHeaders(e.Result, soapVersion)));
                    }
                }
                else
                {
                    taskCompletionSource.TrySetException(e.Error);
                }
            }


            private FaultException GetFaultException(string response, bool useXmlSerializerFormat)
            {
                const string ns = "http://schemas.xmlsoap.org/soap/envelope/";

                VerifyThatResponseIsNotNullOrEmpty(response);
                var faultElement = DataContractSerializerCustom.ParseToXDocument(response).Root
                                                 .Element(XName.Get("Body", ns))
                                                 .Element(XName.Get("Fault", ns));

                if (faultElement == null)
                {
                    return new FaultException();
                }

                var faultStringElement = faultElement.Element(XName.Get("faultstring"));
                var faultReasonValue = faultStringElement?.Value;
                var lang = faultStringElement?.Attribute(XName.Get("lang", XNamespace.Xml.NamespaceName))?.Value;
                var faultReasonText = string.IsNullOrEmpty(lang)
                    ? new FaultReasonText(faultReasonValue)
                    : new FaultReasonText(faultReasonValue, lang);
                var reason = new FaultReason(faultReasonText);

                var faultCodeElement = faultElement.Element(XName.Get("faultcode"));
                var code = new FaultCode(faultCodeElement?.Value);

                var detailElement = faultElement.Element(XName.Get("detail"));
                if (detailElement == null)
                {
                    return new FaultException(reason, code, null);
                }

                detailElement = detailElement.Elements().First();
                var detailType = ResolveType(detailElement.Name, useXmlSerializerFormat);

                var serializer = new DataContractSerializerCustom(detailType);

                var detail = serializer.DeserializeFromXElement(detailElement);

                var type = typeof(FaultException<>).MakeGenericType(detailType);

                return (FaultException)Activator.CreateInstance(type, detail, reason, code, null);
            }

            private static Type ResolveType(XName name, bool useXmlSerializerFormat)
            {
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < assemblies.Length; ++i)
                {
                    Type[] types = assemblies[i].GetTypes();
                    for (int j = 0; j < types.Length; ++j)
                    {
                        Type type = types[j];
                        object[] attrs = type.GetCustomAttributes(typeof(DataContractAttribute), false);
                        DataContractAttribute attr = attrs != null && attrs.Length > 0 ?
                                                     (DataContractAttribute)attrs[0] :
                                                     null;

                        if (attr != null)
                        {
                            bool nameMatch = attr.IsNameSetExplicitly ?
                                         attr.Name == name.LocalName :
                                         type.Name == name.LocalName;

                            if(nameMatch)
                            {
                                bool namespaceMatch = attr.IsNamespaceSetExplicitly ?
                                    attr.Namespace == name.NamespaceName :
                                    DataContractSerializer_Helpers.GetDefaultNamespace(type.Namespace, useXmlSerializerFormat) == name.NamespaceName;

                                if (namespaceMatch)
                                    return type;
                            }
                        }
                    }
                }

                throw new InvalidOperationException(string.Format("Could not resolve type {0}", name));
            }
#endif

            private object ReadAndPrepareResponse(
                string responseAsString,
                Type interfaceType,
                Type requestResponseType,
                IReadOnlyList<Type> knownTypes,
                Action<FaultException> raiseFaultException,
                bool isXmlSerializer,
                string soapVersion,
                CSHTML5_ClientBase<TChannel> client)
            {
                //**************************************
                // What the response should look like in case of classes or strings:
                //**************************************
                //<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
                //  <s:Body xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
                //    <METHODNAMEResponse xmlns="http://tempuri.org/">
                //      <METHODNAMEResult>
                //        <BoolValue>true</BoolValue>
                //        <StringValue>44</StringValue>
                //      </METHODNAMEResult>
                //    </METHODNAMEResponse>
                //  </s:Body>
                //</s:Envelope>
                //**************************************
                // What the response should look like in case of value types (eg. int MethodName(int)):
                //**************************************
                //<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
                //    <s:Body xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
                //        <MethodNameResponse xmlns="http://tempuri.org/">
                //            <MethodNameResult>10</MethodNameResult>
                //        </MethodNameResponse>
                //    </s:Body>
                //</s:Envelope>

                //-----------------------------------------------------------
                // handle the case where the server has sent a FaultException: 
                // (this is what it looks like when the exception is of type FaultException, 
                // we need to try with a custom FaultException)
                //-----------------------------------------------------------
                //<s:Fault>
                //    <faultcode>s:Client</faultcode>
                //    <faultstring xml:lang="fr-FR">id already exists</faultstring>
                //</s:Fault>
                // we make our own exception. Because it is easier that way (it will probably
                // require an actual deserialization of the users' FaultException later on, to 
                // be able to support their custom ones).

                soapVersion = client?.INTERNAL_SoapVersion ?? soapVersion;

                VerifyThatResponseIsNotNullOrEmpty(responseAsString);
                string NS;
                if (soapVersion == "1.1")
                {
                    NS = "http://schemas.xmlsoap.org/soap/envelope/";
                }
                else
                {
                    Debug.Assert(soapVersion == "1.2",
                                    string.Format("Unexpected soap version ({0}) !", soapVersion));
                    NS = "http://www.w3.org/2003/05/soap-envelope";
                }

                BinaryMessageEncodingBindingElement binaryBindingElement = client?.ChannelFactory?.Endpoint?.Binding?
                    .CreateBindingElements().Find<BinaryMessageEncodingBindingElement>();
                bool isBinaryBinding = binaryBindingElement != null;

                if (isBinaryBinding)
                {
                    string base64response = responseAsString;
                    byte[] response = Convert.FromBase64String(base64response);
                    MessageEncoder messageEncoder = binaryBindingElement.CreateMessageEncoderFactory().Encoder;

                    Message message;
                    using (MemoryStream readingMemoryStream = new MemoryStream(response))
                    using (MemoryStream writingMemoryStream = new MemoryStream())
                    using (XmlDictionaryWriter xmlDictionaryWriter =
                        XmlDictionaryWriter.CreateTextWriter(writingMemoryStream, Text.Encoding.UTF8, false))
                    using (StreamReader streamReader = new StreamReader(writingMemoryStream))
                    {
                        message = messageEncoder.ReadMessage(readingMemoryStream, int.MaxValue);

                        message.WriteMessage(xmlDictionaryWriter);
                        xmlDictionaryWriter.Flush();
                        writingMemoryStream.Position = 0;
                        responseAsString = streamReader.ReadToEnd();
                    }
                }

                XElement envelopeElement = DataContractSerializerCustom.ParseToXDocument(responseAsString).Root;
                XElement headerElement = envelopeElement.Element(XName.Get("Header", NS));
                XElement bodyElement = envelopeElement.Element(XName.Get("Body", NS));


#if OPENSILVER
                // Error parsing, if applicable
                if (soapVersion == "1.2")
                {
                    XElement faultElement = bodyElement.Element(XName.Get("Fault", NS));

                    if (faultElement != null)
                    {
                        XElement codeElement = faultElement.Element(XName.Get("Code", NS));
                        XElement reasonElement = faultElement.Element(XName.Get("Reason", NS));
                        XElement detailElement = faultElement.Element(XName.Get("Detail", NS));

                        FaultCode faultCode = new FaultCode(codeElement.Elements().First().Value);
                        FaultReason faultReason = new FaultReason(reasonElement.Elements().First().Value);
                        string action = headerElement.Element(XName.Get("Action", "http://www.w3.org/2005/08/addressing")).Value;

                        FaultException faultException;

                        if (detailElement != null)
                        {
                            XElement innerExceptionElement = detailElement.Elements().First();

                            object innerException = ParseException(innerExceptionElement, innerExceptionElement.Name.LocalName);

                            Type faultExceptionType = typeof(FaultException<>).MakeGenericType(innerException.GetType());

                            faultException = (FaultException)Activator.CreateInstance(faultExceptionType, innerException, faultReason, faultCode, action);
                        }
                        else
                        {
                            faultException = new FaultException(faultReason, faultCode, action);
                        }

                        raiseFaultException(faultException);
                        return null;
                    }

                    object ParseException(XElement exceptionElement, string exceptionTypeName)
                    {
                        Type exceptionType = ResolveType(exceptionTypeName);

                        object exception = Activator.CreateInstance(exceptionType);

                        foreach (XElement element in exceptionElement.Elements())
                        {
                            PropertyInfo property = exceptionType.GetProperty(element.Name.LocalName);

                            XAttribute isNullAttribute = element.Attributes().FirstOrDefault(a => a.Name.LocalName == "nil");
                            if (isNullAttribute != null && isNullAttribute.Value == "true")
                            {
                                property.SetValue(exception, null);
                            }
                            else
                            {
                                if (property.Name == "InnerException")
                                    property.SetValue(exception, ParseException(element, "SoaUnknownException"));
                                else
                                    property.SetValue(exception, element.Value);
                            }
                        }

                        return exception;
                    }

                    Type ResolveType(string name)
                    {
                        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                        int asemblyCount = assemblies.Length;
                        for (int i = 0; i < asemblyCount; i++)
                        {
                            Type[] types = assemblies[i].GetTypes();
                            int typeCount = types.Length;
                            for (int j = 0; j < typeCount; j++)
                            {
                                if (types[j].Name == name)
                                    return types[j];
                            }
                        }

                        throw new InvalidOperationException(string.Format("Could not resolve type {0}", name));
                    }
                }
                else
                {
                    int m = responseAsString.IndexOf(":Fault>");
                    if (m == -1)
                    {
                        m = responseAsString.IndexOf("<Fault>");
                    }
                    if (m != -1)
                    {
                        FaultException fe = GetFaultException(responseAsString, isXmlSerializer);
                        raiseFaultException(fe);
                        return null;
                    }
                }
#else
                int m = responseAsString.IndexOf(":Fault>");
                if (m == -1)
                {
                    m = responseAsString.IndexOf("<Fault>");
                }
                if (m != -1)
                {
                    m = responseAsString.IndexOf("<faultstring", m);
                    if (m != -1) //this might seem redundant but with that we have more chances to have an actual FaultException
                    {
                        m = responseAsString.IndexOf('>', m);
                        responseAsString = responseAsString.Remove(0, m + 1);
                        m = responseAsString.IndexOf("</faultstring");
                        responseAsString = responseAsString.Remove(m);
                        FaultException fe = new FaultException(responseAsString);
                        raiseFaultException(fe);
                        return null;
                    }
                }
#endif

                object requestResponse = null;

                if (!requestResponseType.IsValueType)
                {
                    // we deserialize the response
                    // in the case of an XmlSerializer:
                    // - change the xsi:nil="true" or add the xsi namespace (probably better to add the namespace)
                    // - directly use xDoc.Root instead of its Nodes (I think)
                    // - change the type to deserialize to the type with the name : requestResponseType.Name + "Body"
                    FieldInfo bodyFieldInfo = null;

                    // we make sure this is not a method with no return type
                    // Note: we test for the "Object" type since it is what we put instead of "void"
                    // to allow passing it as Generic type argument when calling CallWebMethod.
                    if (requestResponseType == typeof(object))
                    {
                        if (bodyElement != null && bodyElement.Nodes().Count() == 0)
                        {
                            // Note: there might be a more efficient way of checking if the method has a return 
                            // type (possibly through a smart use of responseAsString.IndexOf but it seems 
                            // complicated and not necessarily more efficient).
                            // this is a method with no return type, there is no need to read the response 
                            // after checking that there was no FaultException.
                            return null;
                        }
                    }

                    bool isBodyMemberSerialization = false;
                    Type typeToDeserialize = requestResponseType;
                    if (isXmlSerializer)
                    {
                        bodyFieldInfo = requestResponseType.GetField("Body");
                        if (bodyFieldInfo != null)
                        {
                            typeToDeserialize = bodyFieldInfo.FieldType;
                        }
                        else if (requestResponseType.GetCustomAttribute<MessageContractAttribute>(true) != null)
                        {
                            isBodyMemberSerialization = true;

                            FieldInfo fieldInfo = requestResponseType
                                .GetFields()
                                .FirstOrDefault(p => Attribute.IsDefined(p, typeof(MessageBodyMemberAttribute)));
                            if (fieldInfo == null)
                            {
                                throw new ArgumentException(
                                    "Unable to find MessageBodyMemberAttribute of MessageContractAttribute contract");
                            }

                            typeToDeserialize = fieldInfo.FieldType;
                            bodyFieldInfo = fieldInfo;
                        }
                    }

                    // get the known types from the interface type
                    IEnumerable<Type> serviceKnownTypes =
                        interfaceType.GetCustomAttributes(typeof(ServiceKnownTypeAttribute), true)
                                     .Select(o => ((ServiceKnownTypeAttribute)o).Type);

                    List<Type> types = new List<Type>(knownTypes ?? Enumerable.Empty<Type>());
                    foreach (Type t in serviceKnownTypes)
                    {
                        types.Add(t);
                    }

                    DataContractSerializerCustom deSerializer = new DataContractSerializerCustom(typeToDeserialize, types);
                    XElement xElement = envelopeElement;

                    //exclude the parts that are <Enveloppe><Body>... since they are useless 
                    // and would keep the deserialization from working properly
                    // they should always be the two outermost elements
                    if (soapVersion == "1.1")
                    {
                        xElement = bodyElement ?? xElement;
                    }
                    else
                    {
                        xElement = bodyElement;
                    }

                    // we check if the type is defined in the next XElement because 
                    // it changes the XElement we want to use in that case.
                    // The reason is that the response uses one less XElement in the 
                    // case where we use XmlSerializer and the method has the return 
                    // Type object.
                    bool isTypeSpecified =
                        xElement.Attributes(XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance").GetName("type"))
                                .Any();
                    if (!isXmlSerializer || !isTypeSpecified)
                    {
                        // we are either not in the XmlSerializer version or we have 
                        // the "extra" XElement so we move in once.
                        xElement = xElement.Elements().FirstOrDefault() ?? xElement; //move inside of the <Body> tag
                    }

                    if (requestResponseType == typeof(Message))
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            XmlDictionaryReader xmlDictionaryReader = XmlDictionaryReader.CreateTextReader(
                                Text.Encoding.UTF8.GetBytes(responseAsString),
                                new XmlDictionaryReaderQuotas());
                            Message message = Message.CreateMessage(xmlDictionaryReader, 4096, MessageVersion.Default);
                            requestResponse = message;
                        }
                    }
                    else if (!isXmlSerializer)
                    {
                        if (typeToDeserialize.GetCustomAttribute<MessageContractAttribute>() != null)
                        {
                            // DataContractSerializer needs correct namespace instead of http://tempuri.org/
                            XNamespace ns = DataContractSerializer_Helpers.GetDefaultNamespace(typeToDeserialize.Namespace, false);
                            xElement.Name = ns + xElement.Name.LocalName;
                            xElement.Attributes("xmlns").Remove();
                            foreach (var childElement in xElement.Elements())
                            {
                                childElement.Name = ns + childElement.Name.LocalName;
                            }
                        }
                        else
                        {
                            xElement = xElement.Elements().FirstOrDefault() ?? xElement;
                        }
                        requestResponse = deSerializer.DeserializeFromXElement(xElement);
                    }
                    else
                    {
                        if (isBodyMemberSerialization)
                        {
                            xElement = xElement.Elements().FirstOrDefault() ?? xElement;
                        }

                        requestResponse = Activator.CreateInstance(requestResponseType);
                        object requestResponseBody = deSerializer.DeserializeFromXElement(xElement);
                        bodyFieldInfo.SetValue(requestResponse, requestResponseBody);
                    }
                }
                else
                {
                    // we remove the parts of the response string that are not the 
                    // response itself. That is, we remove the "<s:Body>" so as to 
                    // keep only its content
                    string keyWord = ":Body";
                    int i = responseAsString.IndexOf(keyWord);
                    int k = responseAsString.IndexOf('>', i);
                    responseAsString = responseAsString.Remove(0, k + 1);
                    i = responseAsString.LastIndexOf(keyWord);
                    k = responseAsString.Substring(0, i).LastIndexOf('<');
                    responseAsString = responseAsString.Remove(k);

                    // Here we are dealing with a value type.
                    // Example:
                    //     <METHODNAMEResponse xmlns="http://tempuri.org/">
                    //         <METHODNAMEResult>10</METHODNAMEResult>
                    //     </METHODNAMEResponse>


                    // we look for :nil="true". Since we have a value type, it will
                    // mean that the value type is nullable and the value is null
                    int indexOfNil = responseAsString.IndexOf(":nil=\"true\"");
                    if (indexOfNil == -1)
                    {
                        // we remove the <MethodNameResponse> tag
                        int ii = responseAsString.IndexOf('>', 0);
                        int jj = responseAsString.IndexOf('>', ii + 1);
                        int kk = responseAsString.IndexOf('<', jj + 1);
                        responseAsString = responseAsString.Substring(jj + 1, (kk - jj - 1));

                        //todo: support more value types?
                        //todo: handle nullables that are null
                        if (requestResponseType == typeof(int) || requestResponseType == typeof(int?))
                            requestResponse = int.Parse(responseAsString);
                        else if (requestResponseType == typeof(long) || requestResponseType == typeof(long?))
                            requestResponse = long.Parse(responseAsString);
                        else if (requestResponseType == typeof(bool) || requestResponseType == typeof(bool?))
                            requestResponse = bool.Parse(responseAsString);
                        else if (requestResponseType == typeof(float) || requestResponseType == typeof(float?))
                            requestResponse = float.Parse(responseAsString); //todo: ensure this is the culture-invariant parsing!
                        else if (requestResponseType == typeof(double) || requestResponseType == typeof(double?))
                            requestResponse = double.Parse(responseAsString); //todo: ensure this is the culture-invariant parsing!
                        else if (requestResponseType == typeof(decimal) || requestResponseType == typeof(decimal?))
                            requestResponse = decimal.Parse(responseAsString); //todo: ensure this is the culture-invariant parsing!
                        else if (requestResponseType == typeof(char) || requestResponseType == typeof(char?))
                            requestResponse = (char)(int.Parse(responseAsString)); //todo: support encodings
                        else if (requestResponseType == typeof(DateTime) || requestResponseType == typeof(DateTime?))
                            requestResponse = INTERNAL_DateTimeHelpers.ToDateTime(responseAsString); //todo: ensure this is the culture-invariant parsing!
                        else if (requestResponseType.IsEnum)
                            requestResponse = Enum.Parse(requestResponseType, responseAsString);
                        else if (requestResponseType == typeof(void))
                        {
                            // Do nothing so null object will be returned
                        }
                        else
                            throw new NotSupportedException($"The following type is not supported in the current WCF implementation: '{requestResponseType}', string value is {responseAsString}. " +
                                $"\nPlease report this issue to support@cshtml5.com");
                    }
                    else
                    {
                        if (requestResponseType.FullName.StartsWith("System.Nullable`1"))
                        {
                            return null;
                        }
                        else
                        {
                            return Activator.CreateInstance(requestResponseType);
                        }
                    }
                }

                return requestResponse;
            }

            static void VerifyThatResponseIsNotNullOrEmpty(string responseAsString)
            {
                // Check that the response is not empty:
                if (string.IsNullOrEmpty(responseAsString))
                {
                    throw new CommunicationException("The remote server returned an error. To debug, look at the browser Console output, or use a tool such as Fiddler.");
                }
            }

        }
#endif

        #region work in progress

        #region Not Supported Stuff

        /// <summary>
        /// Gets the underlying System.ServiceModel.IClientChannel implementation.
        /// </summary>
		[OpenSilver.NotImplemented]
        public IClientChannel InnerChannel
        {
            get
            {
                return (IClientChannel)Channel;
            }
        }

        [OpenSilver.NotImplemented]
        public void Abort()
        {

        }

        [OpenSilver.NotImplemented]
        public CommunicationState State
        {
            get { return CommunicationState.Created; }
        }


        //    /// <summary>
        //    /// Returns a new channel to the service.
        //    /// </summary>
        //    /// <returns>A channel of the type of the service contract.</returns>
        protected virtual TChannel CreateChannel()
        {
            return null;
        }

        //    /// <summary>
        //    /// Replicates the behavior of the default keyword in C#.
        //    /// </summary>
        //    /// <typeparam name="T">The type that is identified as reference or numeric by the keyword.</typeparam>
        //    /// <returns>Returns null if T is a reference type and zero if T is a numeric value type.</returns>
        [OpenSilver.NotImplemented]
        protected T GetDefaultValueForInitialization<T>()
        {
            return default(T);
        }

        /// <summary>
        /// Generic ChannelBase class.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        //protected class ChannelBase<T> : IOutputChannel, IRequestChannel, IClientChannel, IDisposable, IContextChannel, IChannel, ICommunicationObject, IExtensibleObject<IContextChannel> where T : class
        protected class ChannelBase<T> : IOutputChannel, IRequestChannel, IClientChannel, IDisposable, IContextChannel, IChannel, ICommunicationObject, IExtensibleObject<IContextChannel> where T : class
        {
            private CSHTML5_ClientBase<T> _client;

            public IList<MessageHeader> MessageHeaders
            {
                get;
            } = new List<MessageHeader>();

            /// <summary>
            /// Initializes a new instance of the <see cref="System.ServiceModel.ClientBase{TChannel}.ChannelBase{T}"/>
            /// class from an existing instance of the class.
            /// </summary>
            /// <param name="client">The object used to initialize the new instance of the class.</param>
            protected ChannelBase(CSHTML5_ClientBase<T> client)
            {
                _client = client;
            }

            /// <summary>
            /// Starts an asynchronous call of a specified method by name.
            /// </summary>
            /// <param name="methodName">The name of the method to be called asynchronously.</param>
            /// <param name="args">An array of arguments for the method invoked.</param>
            /// <param name="callback">The System.AsyncCallback delegate.</param>
            /// <param name="state">The state object.</param>
            /// <returns>The System.IAsyncResult that references the asynchronous method invoked.</returns>
            //[SecuritySafeCritical]
            protected IAsyncResult BeginInvoke(string methodName, object[] args, AsyncCallback callback, object state)
            {
                MethodInfo methodInfo = CSHTML5_ClientBase<T>.WebMethodsCaller.ResolveMethod(typeof(T), methodName,
                    "Begin" + methodName);
                ParameterInfo[] parameterInfos = methodInfo.GetParameters()
                    .Where(p => p.Name != CallbackParameterName && p.Name != AsyncStateParameterName)
                    .ToArray();
                if (parameterInfos.Length != args.Length)
                {
                    throw new ArgumentException("Arg count does not match method parameter count");
                }

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                for (int i = 0; i < parameterInfos.Length; ++i)
                {
                    parameters[parameterInfos[i].Name] = args[i];
                }
                parameters[CallbackParameterName] = callback;
                parameters[AsyncStateParameterName] = state;

                return BeginCallWebMethod<T>(_client.INTERNAL_RemoteAddressAsString,
                    methodName,
                    typeof(IAsyncResult),
                    null,
                    CSHTML5_ClientBase<T>.WebMethodsCaller.GetEnvelopeHeaders(MessageHeaders, _client.INTERNAL_SoapVersion),
                    parameters,
                    _client.INTERNAL_SoapVersion,
                    _client);
            }

            /// <summary>
            /// Completes an asynchronous invocation by name of a specified method.
            /// </summary>
            /// <param name="methodName">The name of the method called asynchronously.</param>
            /// <param name="args">An array of arguments for the method invoked.</param>
            /// <param name="result">The result returned by a call.</param>
            /// <returns>The System.Object output by the method invoked.</returns>
            //[SecuritySafeCritical]
            protected object EndInvoke(string methodName, object[] args, IAsyncResult result)
            {
                MethodInfo methodInfo = CSHTML5_ClientBase<T>.WebMethodsCaller.ResolveMethod(typeof(T), methodName,
                    "End" + methodName);

                return EndCallWebMethod(_client.INTERNAL_RemoteAddressAsString,
                    methodName,
                    methodInfo.ReturnType,
                    new Dictionary<string, object>()
                    {
                        { "result", result },
                    },
                    _client.INTERNAL_SoapVersion,
                    _client);
            }

            public bool AllowInitializationUI { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public bool DidInteractiveInitialization => throw new NotImplementedException();

            public Uri Via => _client?.Endpoint?.Address?.Uri ?? new Uri(_client.INTERNAL_RemoteAddressAsString);

            public bool AllowOutputBatching { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public IInputSession InputSession => throw new NotImplementedException();

            public EndpointAddress LocalAddress => throw new NotImplementedException();

            public TimeSpan OperationTimeout { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public IOutputSession OutputSession => throw new NotImplementedException();

            public EndpointAddress RemoteAddress => throw new NotImplementedException();

            public string SessionId => throw new NotImplementedException();

            public CommunicationState State => throw new NotImplementedException();

            public IExtensionCollection<IContextChannel> Extensions => throw new NotImplementedException();

            public event EventHandler<UnknownMessageReceivedEventArgs> UnknownMessageReceived;
            public event EventHandler Closed;
            public event EventHandler Closing;
            public event EventHandler Faulted;
            public event EventHandler Opened;
            public event EventHandler Opening;

            public void Abort()
            {
                throw new NotImplementedException();
            }

            public IAsyncResult BeginClose(AsyncCallback callback, object state)
            {
                throw new NotImplementedException();
            }

            public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                throw new NotImplementedException();
            }

            public IAsyncResult BeginDisplayInitializationUI(AsyncCallback callback, object state)
            {
                throw new NotImplementedException();
            }

            public IAsyncResult BeginOpen(AsyncCallback callback, object state)
            {
                throw new NotImplementedException();
            }

            public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                throw new NotImplementedException();
            }

            public void Close()
            {
                throw new NotImplementedException();
            }

            public void Close(TimeSpan timeout)
            {
                throw new NotImplementedException();
            }

            public void DisplayInitializationUI()
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public void EndClose(IAsyncResult result)
            {
                throw new NotImplementedException();
            }

            public void EndDisplayInitializationUI(IAsyncResult result)
            {
                throw new NotImplementedException();
            }

            public void EndOpen(IAsyncResult result)
            {
                throw new NotImplementedException();
            }

            public T1 GetProperty<T1>() where T1 : class
            {
                throw new NotImplementedException();
            }

            public void Open()
            {
                throw new NotImplementedException();
            }

            public void Open(TimeSpan timeout)
            {
                throw new NotImplementedException();
            }

            public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
            {
                throw new NotImplementedException();
            }

            public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                throw new NotImplementedException();
            }

            public void EndSend(IAsyncResult result)
            {
                throw new NotImplementedException();
            }

            public void Send(Message message)
            {
                throw new NotImplementedException();
            }

            public void Send(Message message, TimeSpan timeout)
            {
                throw new NotImplementedException();
            }

            public IAsyncResult BeginRequest(Message message, AsyncCallback callback, object state)
            {
                throw new NotImplementedException();
            }

            public IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                throw new NotImplementedException();
            }

            public Message EndRequest(IAsyncResult result)
            {
                throw new NotImplementedException();
            }

            public Message Request(Message message)
            {
                throw new NotImplementedException();
            }

            public Message Request(Message message, TimeSpan timeout)
            {
                throw new NotImplementedException();
            }
        }


        #endregion

#if BRIDGE
        #region ICommunicationObject methods

		[OpenSilver.NotImplemented]
        CommunicationState ICommunicationObject.State
        {
            get { return CommunicationState.Created; }
        }

		[OpenSilver.NotImplemented]
        event EventHandler ICommunicationObject.Closed
        {
            add { }
            remove { }
        }

		[OpenSilver.NotImplemented]
        event EventHandler ICommunicationObject.Closing
        {
            add { }
            remove { }
        }

		[OpenSilver.NotImplemented]
        event EventHandler ICommunicationObject.Faulted
        {
            add { }
            remove { }
        }

		[OpenSilver.NotImplemented]
        event EventHandler ICommunicationObject.Opened
        {
            add { }
            remove { }
        }

		[OpenSilver.NotImplemented]
        event EventHandler ICommunicationObject.Opening
        {
            add { }
            remove { }
        }

		[OpenSilver.NotImplemented]
        void ICommunicationObject.Abort()
        {

        }

		[OpenSilver.NotImplemented]
        void ICommunicationObject.Close()
        {

        }

		[OpenSilver.NotImplemented]
        void ICommunicationObject.Close(TimeSpan timeout)
        {

        }

		[OpenSilver.NotImplemented]
        IAsyncResult ICommunicationObject.BeginClose(AsyncCallback callback, object state)
        {
            return null;
        }

		[OpenSilver.NotImplemented]
        IAsyncResult ICommunicationObject.BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return null;
        }

		[OpenSilver.NotImplemented]
        void ICommunicationObject.EndClose(IAsyncResult result)
        {

        }

		[OpenSilver.NotImplemented]
        void ICommunicationObject.Open()
        {

        }

		[OpenSilver.NotImplemented]
        void ICommunicationObject.Open(TimeSpan timeout)
        {

        }

		[OpenSilver.NotImplemented]
        IAsyncResult ICommunicationObject.BeginOpen(AsyncCallback callback, object state)
        {
            return null;
        }

		[OpenSilver.NotImplemented]
        IAsyncResult ICommunicationObject.BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return null;
        }

		[OpenSilver.NotImplemented]
        void ICommunicationObject.EndOpen(IAsyncResult result)
        {

        }

        //void ICommunicationObject.Dispose()
        //{

        //}
        #endregion
#endif

        #endregion work in progress
    }
}