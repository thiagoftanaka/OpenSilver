using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Xml;

namespace TestApplication.Silverlight.Web
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class BasicHttpService
    {
        [OperationContract]
        public string Echo(string message)
        {
            return $"Response to '{message}'";
        }

        [OperationContract]
        public BodyMemberResponseMessage BodyMember(BodyMemberRequestMessage message)
        {
            int headerIndex = OperationContext.Current.IncomingMessageHeaders.FindHeader("CustomHeader", "");
            string headerContent = null;
            if (headerIndex != -1)
            {
                using (XmlDictionaryReader xmlDictionaryReader = OperationContext.Current.IncomingMessageHeaders
                           .GetReaderAtHeader(headerIndex))
                {
                    headerContent = xmlDictionaryReader.ReadString();
                }
            }

            return new BodyMemberResponseMessage
            {
                Response = $"Response to '{message.Request}'. Custom header: {(headerIndex != -1 ? headerContent : "no custom header" )}"
            };
        }
    }

    [MessageContract]
    public class BodyMemberRequestMessage
    {
        [MessageBodyMember(
            Name = "BodyMemberRequest",
            Namespace = ""
        )]
        public string Request { get; set; }
    }

    [MessageContract]
    public class BodyMemberResponseMessage
    {
        [MessageBodyMember(
            Name = "BodyMemberResponse",
            Namespace = "")]
        public string Response { get; set; }
    }
}
