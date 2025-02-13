using System.ServiceModel;
using System.ServiceModel.Channels;

namespace TestApplication.Tests.ServiceReference
{
    [ServiceContract(Namespace = "", ConfigurationName = "LegacyBasicHttpServiceReference.BasicHttpService")]
    public interface IBasicHttpService
    {
        // Action is "*" to test if the actual Action will be retrieved from the Message parameter Header
        [OperationContract(AsyncPattern = true, Action = "*", ReplyAction = "*")]
        System.IAsyncResult BeginBodyMember(Message message, System.AsyncCallback callback, object asyncState);

        Message EndBodyMember(System.IAsyncResult result);
    }
}
