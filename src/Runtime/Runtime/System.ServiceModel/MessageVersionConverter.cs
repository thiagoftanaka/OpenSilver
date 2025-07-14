
/*===================================================================================
* 
*   Copyright (c) Userware/OpenSilver.net
*      
*   This file is part of the OpenSilver Runtime (https://opensilver.net), which is
*   licensed under the MIT license: https://opensource.org/licenses/MIT
*   
*   As stated in the MIT license, "the above copyright notice and this permission
*   notice shall be included in all copies or substantial portions of the Software."
*  
\*====================================================================================*/


using System.ServiceModel.Channels;

namespace System.ServiceModel
{
    internal class MessageVersionConverter
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
