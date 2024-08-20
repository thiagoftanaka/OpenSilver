using System;
using System.ComponentModel;
using System.ServiceModel;
using System.ServiceModel.Channels;

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
        }

        public class GetTestStringCompletedEventArgs : AsyncCompletedEventArgs
        {
            private object[] _result;

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
    }
}
