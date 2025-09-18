using System.ServiceModel.Channels;

namespace System.ServiceModel
{
    internal class MessageVersionHelper
    {
        internal static MessageVersion ToMessageVersion(string soapVersion)
        {
            switch (soapVersion)
            {
                case "1.1":
                    return MessageVersion.Soap11;
                case "1.2":
                    return MessageVersion.Soap12;
                default:
                    throw new InvalidOperationException($"SOAP version not supported: {soapVersion}");
            }
        }

        internal static string ToString(MessageVersion messageVersion)
        {
            if (MessageVersion.Soap11.Equals(messageVersion) ||
                MessageVersion.Soap11WSAddressing10.Equals(messageVersion) ||
                MessageVersion.Soap11WSAddressingAugust2004.Equals(messageVersion))
            {
                return "1.1";
            }
            if (MessageVersion.Soap12.Equals(messageVersion) ||
                     MessageVersion.Soap12WSAddressing10.Equals(messageVersion) ||
                     MessageVersion.Soap12WSAddressingAugust2004.Equals(messageVersion))
            {
                return "1.2";
            }
            throw new InvalidOperationException($"SOAP version not supported: {messageVersion}");
        }
    }
}
