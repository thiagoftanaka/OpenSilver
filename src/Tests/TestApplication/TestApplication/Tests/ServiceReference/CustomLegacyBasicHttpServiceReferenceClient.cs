using System;
using System.ComponentModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;

namespace TestApplication.OpenSilver.Tests.ServiceReference
{
    public class CustomLegacyBasicHttpServiceReferenceClient :
#if OPENSILVER
        CSHTML5_ClientBase<LegacyBasicHttpServiceReference.BasicHttpService>,
#else
        ClientBase<LegacyBasicHttpServiceReference.BasicHttpService>,
#endif
        LegacyBasicHttpServiceReference.BasicHttpService
    {
        public event EventHandler<GetTestStringCompletedEventArgs> GetTestStringCompleted;
        public event EventHandler<GetTestStringCompletedEventArgs> EchoCompleted;

        protected override LegacyBasicHttpServiceReference.BasicHttpService CreateChannel()
        {
            return new CustomLegacyBasicHttpServiceReferenceClientChannel(this);
        }

        public void GetTestStringAsync()
        {
            InvokeAsync(OnBeginGetTestString, new object[0], OnEndGetTestString, OnGetTestStringCompleted, null);
        }

        private IAsyncResult OnBeginGetTestString(object[] parameters, AsyncCallback callback, object asyncState)
        {
            return BeginGetTestString(callback, asyncState);
        }

        private object[] OnEndGetTestString(IAsyncResult result)
        {
            return new object[] { EndGetTestString(result) };
        }

        private void OnGetTestStringCompleted(object state)
        {
            InvokeAsyncCompletedEventArgs e = state as InvokeAsyncCompletedEventArgs;
            if (e != null)
            {
                GetTestStringCompleted?.Invoke(this,
                    new GetTestStringCompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
            }
        }

        public IAsyncResult BeginGetTestString(AsyncCallback callback, object asyncState)
        {
            return Channel.BeginGetTestString(callback, asyncState);
        }

        public string EndGetTestString(IAsyncResult result)
        {
            return Channel.EndGetTestString(result);
        }

        public void EchoUsingMessageAsync(string message)
        {
            Message messageInstance = Message.CreateMessage(Endpoint?.Binding?.MessageVersion ?? MessageVersion.Default,
                "urn:BasicHttpService/Echo", new EchoBodyWriter(message));
            InvokeAsync(OnBeginEcho, new object[] { messageInstance }, OnEndEcho, OnEchoCompleted, null);
        }

        private IAsyncResult OnBeginEcho(object[] parameters, AsyncCallback callback, object asyncState)
        {
            return BeginEcho(parameters[0] as string, callback, asyncState);
        }

        private object[] OnEndEcho(IAsyncResult result)
        {
            return new object[] { EndEcho(result) };
        }

        private void OnEchoCompleted(object state)
        {
            InvokeAsyncCompletedEventArgs e = state as InvokeAsyncCompletedEventArgs;
            if (e != null)
            {
                EchoCompleted?.Invoke(this,
                    new GetTestStringCompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
            }
        }

        public IAsyncResult BeginEcho(string message, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginEcho(message, callback, asyncState);
        }

        public string EndEcho(IAsyncResult result)
        {
            return Channel.EndEcho(result);
        }

        private class CustomLegacyBasicHttpServiceReferenceClientChannel :
            ChannelBase<LegacyBasicHttpServiceReference.BasicHttpService>,
            LegacyBasicHttpServiceReference.BasicHttpService
        {
            public CustomLegacyBasicHttpServiceReferenceClientChannel(
#if OPENSILVER
                CSHTML5_ClientBase<LegacyBasicHttpServiceReference.BasicHttpService> client) :
#else
                ClientBase<LegacyBasicHttpServiceReference.BasicHttpService> client) :
#endif
                base(client)
            {
            }

            public IAsyncResult BeginGetTestString(AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("GetTestString", new object[0], callback, asyncState);
            }

            public string EndGetTestString(IAsyncResult result)
            {
                return (string)EndInvoke("GetTestString", new object[0], result);
            }

            public IAsyncResult BeginEcho(string message, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("Echo", new object[] { message }, callback, asyncState);
            }

            public string EndEcho(IAsyncResult result)
            {
                return (string)EndInvoke("Echo", new object[0], result);
            }
        }

        public class GetTestStringCompletedEventArgs : AsyncCompletedEventArgs
        {
            private readonly object[] _result;

            public string Result
            {
                get
                {
                    RaiseExceptionIfNecessary();
                    return _result[0] as string;
                }
            }

            public GetTestStringCompletedEventArgs(object[] result, Exception error, bool cancelled, object userState) : base(error, cancelled, userState)
            {
                if (error == null && !cancelled)
                {
                    _result = result;
                }
            }
        }

        private class EchoBodyWriter : BodyWriter
        {
            private readonly string _message;

            public EchoBodyWriter(string message) : base(true)
            {
                _message = message;
            }

            protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
            {
                writer.WriteStartElement("Echo");
                writer.WriteStartElement("message");
                writer.WriteString(_message);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }
    }
}
