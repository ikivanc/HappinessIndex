using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace HappinessIndexUWP
{
    class EventHubHandler
    {

        public async void ehpush(String payload)
        {
            DeviceClient tryDev = DeviceClient.CreateFromConnectionString("HostName=happyhub.azure-devices.net;DeviceId=bathingelephant;SharedAccessKey=5fyLxnXPS47vyHKamvkcNFZVsdvguMzsQJbpflN578A=");
            // string txttt = "içeriktxt";


            try
            {
                // var serialisedString = JsonConvert.SerializeObject(payload);
                //var content = new Message(Encoding.UTF8.GetBytes(serialisedString)); 
                var content = new Message(Encoding.UTF8.GetBytes(payload));

                await tryDev.SendEventAsync(content);

            }
            catch (Exception ex)
            {

            }
        }
    }
}
