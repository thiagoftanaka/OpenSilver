using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;

namespace TestApplication.Silverlight.Web
{
    [ServiceContract(Namespace = "")]
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
        public string Echo(string message)
        {
            return message;
        }
    }
}
