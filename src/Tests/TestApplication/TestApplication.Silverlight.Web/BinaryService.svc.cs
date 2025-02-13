using System.ServiceModel;
using System.ServiceModel.Activation;

namespace TestApplication.Silverlight.Web
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class BinaryService
    {
        [OperationContract]
        public string Echo(string message)
        {
            return $"Binary response to '{message}'";
        }

        [OperationContract]
        public BodyMemberResponseMessage BodyMember(BodyMemberRequestMessage message)
        {
            return new BodyMemberResponseMessage
            {
                Response = $"Binary response to '{message.Request}'."
            };
        }
    }
}
