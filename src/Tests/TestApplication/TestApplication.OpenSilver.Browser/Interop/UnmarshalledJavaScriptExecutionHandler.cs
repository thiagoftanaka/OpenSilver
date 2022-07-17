using DotNetForHtml5;
using Microsoft.JSInterop;
using Microsoft.JSInterop.WebAssembly;

namespace TestApplication.OpenSilver.Browser.Interop
{
    public class UnmarshalledJavaScriptExecutionHandler : IJavaScriptExecutionHandler
    {
        private const string MethodName = "callJSUnmarshalled";
        private readonly WebAssemblyJSRuntime _runtime;

        public UnmarshalledJavaScriptExecutionHandler(IJSRuntime runtime)
        {
            _runtime = runtime as WebAssemblyJSRuntime;
        }

        public void ExecuteJavaScript(string javaScriptToExecute)
        {
            _runtime.InvokeUnmarshalled<string, object>(MethodName, javaScriptToExecute);
        }

        public object ExecuteJavaScriptWithResult(string javaScriptToExecute)
        {
            return _runtime.InvokeUnmarshalled<string, object>(MethodName, javaScriptToExecute);
        }

        public void SendJavaScriptBinaryXmlHttpRequest(string id, string base64Body)
        {
            _runtime.InvokeUnmarshalled<string, string, object>("sendBinaryXmlHttpRequest", id, base64Body);
        }
    }
}