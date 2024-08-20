using System.ServiceModel;
using System.Threading.Tasks;

namespace TestApplication.OpenSilver.Tests.ServiceReference
{
    public class CustomBasicHttpServiceReferenceClient : CSHTML5_ClientBase<BasicHttpServiceReference.BasicHttpService>,
        BasicHttpServiceReference.BasicHttpService
    {
        public Task<string> GetTestStringAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}
