using System;
using System.ComponentModel;

namespace TestApplication.Tests.ServiceReference
{
    public class RequestCompletedEventArgs : AsyncCompletedEventArgs
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

        public RequestCompletedEventArgs(object[] result, Exception error, bool cancelled, object userState) :
            base(error, cancelled, userState)
        {
            if (error == null && !cancelled)
            {
                _result = result;
            }
        }
    }
}
