using System.ServiceModel;
using System.ServiceModel.Activation;

namespace TestApplication.Silverlight.Web
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class BinaryService
    {
        [OperationContract]
        public string GetTestString()
        {
            return "This is a binary test.";
        }
    }
}
