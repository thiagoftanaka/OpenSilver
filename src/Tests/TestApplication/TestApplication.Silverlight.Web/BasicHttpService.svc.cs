using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.Xml;

namespace TestApplication.Silverlight.Web
{
    [ServiceContract(Namespace = "")]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class BasicHttpService
    {
        [OperationContract]
        public string GetTestString()
        {
            return "This is a basic http test.";
        }

        [OperationContract]
        [WebInvoke]
        public Message Echo(Message message)
        {
            XmlDictionaryReader reader = message.GetReaderAtBodyContents();
            return Message.CreateMessage(message.Version, "urn:BasicHttpService/EchoResponse", reader);
        }
    }
}
