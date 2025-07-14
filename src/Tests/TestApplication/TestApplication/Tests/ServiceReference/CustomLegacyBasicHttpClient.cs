
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


using System;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace TestApplication.Tests.ServiceReference
{
    public class CustomLegacyBasicHttpClient :
#if OPENSILVER
        CSHTML5_ClientBase<IBasicHttpService>,
#else
        ClientBase<IBasicHttpService>,
#endif
        IBasicHttpService
    {
        public event EventHandler<RequestCompletedEventArgs> BodyMemberCompleted;

        public IAsyncResult BeginBodyMember(Message message, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginBodyMember(message, callback, asyncState);
        }

        public Message EndBodyMember(IAsyncResult result)
        {
            return Channel.EndBodyMember(result);
        }

        public void BodyMemberAsync(string messageString)
        {
            Message messageInstance = Message.CreateMessage(Endpoint?.Binding?.MessageVersion ?? MessageVersion.Default,
                "urn:BasicHttpService/BodyMember", new BodyMemberBodyWriter(messageString));
            InvokeAsync(OnBeginBodyMember, new object[] { messageInstance }, OnEndBodyMember, OnBodyMemberCompleted, null);
        }

        private IAsyncResult OnBeginBodyMember(object[] parameters, AsyncCallback callback, object asyncState)
        {
            return BeginBodyMember(parameters[0] as Message, callback, asyncState);
        }

        private object[] OnEndBodyMember(IAsyncResult result)
        {
            var messageResult = EndBodyMember(result);
            string bodyMemberResult = BodyMemberBodyReader.Read(messageResult);
            return new object[] { bodyMemberResult };
        }

        private void OnBodyMemberCompleted(object state)
        {
            InvokeAsyncCompletedEventArgs e = state as InvokeAsyncCompletedEventArgs;
            if (e != null)
            {
                BodyMemberCompleted?.Invoke(this,
                    new RequestCompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
            }
        }

        protected override IBasicHttpService CreateChannel()
        {
            return new CustomMessageLegacyBasicHttpClientChannel(this);
        }

        private class CustomMessageLegacyBasicHttpClientChannel : ChannelBase<IBasicHttpService>, IBasicHttpService
        {
            public CustomMessageLegacyBasicHttpClientChannel(
#if OPENSILVER
                CSHTML5_ClientBase<IBasicHttpService> client) :
#else
                ClientBase<IBasicHttpService> client) :
#endif
            base(client)
            {
            }

            public IAsyncResult BeginBodyMember(Message message, AsyncCallback callback, object asyncState)
            {
                var args = new object[1];
                args[0] = message;

                var result = BeginInvoke("BodyMember", args, callback, asyncState);
                return result;
            }

            public Message EndBodyMember(IAsyncResult result)
            {
                return (Message)EndInvoke("BodyMember", new object[0], result);
            }
        }

        private class BodyMemberBodyWriter : BodyWriter
        {
            private readonly string _message;

            public BodyMemberBodyWriter(string message) : base(true)
            {
                _message = message;
            }

            protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
            {
                writer.WriteStartElement("BodyMemberRequestMessage");
                writer.WriteStartElement("BodyMemberRequest");
                writer.WriteString(_message);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }

        private static class BodyMemberBodyReader
        {
            public static string Read(Message message)
            {
#if OPENSILVER
                XDocument document;
                using (MemoryStream memoryStream = new MemoryStream())
                using (XmlDictionaryWriter xmlDictionaryWriter =
                       XmlDictionaryWriter.CreateTextWriter(memoryStream, Encoding.UTF8, false))
                using (StreamReader streamReader = new StreamReader(memoryStream))
                {
                    message.WriteBodyContents(xmlDictionaryWriter);
                    xmlDictionaryWriter.Flush();
                    memoryStream.Position = 0;
                    string bodyContents = streamReader.ReadToEnd();

                    document = XDocument.Parse(bodyContents);
                }
#else
                XDocument document = XDocument.Parse(message.ToString());
#endif
                XElement responseElement = document.Descendants("BodyMemberResponse").First();
                return responseElement.Value;
            }
        }
    }
}
