using System.ServiceModel;
using System.ServiceModel.Channels;

namespace TestApplication.Tests.ServiceReference
{
    [ServiceContract(Namespace = "", ConfigurationName = "LegacyBasicHttpServiceReference.BasicHttpService")]
    public interface IBasicHttpService
    {
        [OperationContract(AsyncPattern = true, Action = "urn:BasicHttpService/BodyMember",
            ReplyAction = "urn:BasicHttpService/BodyMemberResponse")]
        System.IAsyncResult BeginBodyMember(Message message, System.AsyncCallback callback, object asyncState);

        Message EndBodyMember(System.IAsyncResult result);
    }
}
