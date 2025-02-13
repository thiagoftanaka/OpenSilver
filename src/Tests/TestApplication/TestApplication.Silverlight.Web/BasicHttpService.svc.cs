using System.ServiceModel;
using System.ServiceModel.Activation;

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
            return new BodyMemberResponseMessage
            {
                Response = $"Response to '{message.Request}'."
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
