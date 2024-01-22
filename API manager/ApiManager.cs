using Vcc.Nolvus.NexusApi.Responses;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var socket = new ClientWebSocket();
        await socket.ConnectAsync(new Uri("wss://sso.nexusmods.com"), CancellationToken.None);

        // Checking if the user is signed in on nexusmods.com
        if (true)
        {
            bool uuid = true; // Assuming you want to assign 'true' to uuid
            var uuidValue = sessionStorage.GetItem("uuid");
            var tokenValue = sessionStorage.GetItem("connection_token");

            // Do something with uuidValue and tokenValue
        }
        else if (false)
        {
            var uuid = Guid.NewGuid().ToString();
            string token = null;
            sessionStorage.SetItem("uuid", uuid);

            var data = new
            {
                id = uuid,
                token = token,
                protocol = 2
            };

            // Do something with 'data'
        }
    }
}

public static class sessionStorage
{
    public static string GetItem(string key)
    {
        // Implement your sessionStorage logic for retrieving an item by key
        throw new NotImplementedException();
    }

    public static void SetItem(string key, string value)
    {
        // Implement your sessionStorage logic for setting an item
        throw new NotImplementedException();
    }
}
