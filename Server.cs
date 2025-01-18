using System.Net;
using System.Net.Sockets;
using System.Text;
using codecrafters_http_server.src;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
TcpListener server = null;

try
{
    server = new TcpListener(IPAddress.Any, 4221);
    server.Start();

    while (true)
    {
        if (server.Pending())
        {
            var socket = server.AcceptSocket(); // wait for client
            Task.Run(() => ProcessRequest(socket));
        }
    }

}
catch (SocketException e)
{
    Console.WriteLine("SocketException: {0}", e);
}
finally
{
    server?.Stop();
}


void ProcessRequest(Socket socket)
{
    var request = MyHTTPRequest.ParseRequest(socket);
    if (request is not null)
    {
        Console.WriteLine($"Http Method: {request.Method}");
        Console.WriteLine($"Http Target: {request.Target}");
        Console.WriteLine($"Http Version: {request.Version}");
    }

    string responseString = string.Empty;

    if (request is not null)
    {
        switch (request.Method)
        {
            case "GET":
                responseString = ProcessGet(request);
                break;
            case "POST":
                responseString = ProcessPost(request);
                break;
        }
    }
    else
    {
        responseString = "HTTP/1.1 404 Not Found\r\n\r\n";
    }

    Byte[] responseBytes = Encoding.ASCII.GetBytes(responseString);

    socket.Send(responseBytes);

    socket.Close();
}

string ProcessPost(MyHTTPRequest request)
{
    string responseString = string.Empty;

    if (request.Target?.ToLower().StartsWith("/files/") ?? false)
    {
        var argv = Environment.GetCommandLineArgs();
        string filePath = GetFilePath(argv);

        var fileName = $"{filePath}{request.Target.Substring(7)}";

        int contentLength = int.Parse(request.Headers?["Content-Length"] ?? "0");
        string contentType = request.Headers?["Content-Type"] ?? "";

        string? body = request.Body;

        if (contentLength != body?.Length)
        {
            responseString = "HTTP/1.1 400 Bad Request\r\n\r\n";
        }
        else
        {
            File.WriteAllText(fileName, body);
            responseString = "HTTP/1.1 201 Created\r\n\r\n";
        }
    }
    else
    {
        responseString = "HTTP/1.1 404 Not Found\r\n\r\n";
    }
    return responseString;
}

string ProcessGet(MyHTTPRequest request)
{
    string responseString = string.Empty;
    if (request.Target == "/")
    {
        responseString = "HTTP/1.1 200 OK\r\n\r\n";
    }
    else if (request.Target?.ToLower().StartsWith("/echo/") ?? false)
    {
        var target = request.Target;
        string content = target.Substring(6);

        responseString = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {content.Length}\r\n\r\n{content}";
    }
    else if (request.Target?.ToLower().StartsWith("/user-agent") ?? false)
    {
        string userAgentValue = request.Headers?["User-Agent"] ?? "";
        responseString = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {userAgentValue.Length}\r\n\r\n{userAgentValue}";
    }
    else if (request.Target?.ToLower().StartsWith("/files/") ?? false)
    {
        var argv = Environment.GetCommandLineArgs();
        string filePath = GetFilePath(argv);

        var fileName = $"{filePath}{request.Target.Substring(7)}";

        if (File.Exists(fileName))
        {
            var content = File.ReadAllText(fileName);

            responseString = $"HTTP/1.1 200 OK\r\nContent-Type: application/octet-stream\r\nContent-Length: {content.Length}\r\n\r\n{content}";
        }
        else
        {
            responseString = "HTTP/1.1 404 Not Found\r\n\r\n";
        }
    }
    else
    {
        responseString = "HTTP/1.1 404 Not Found\r\n\r\n";
    }
    return responseString;
}

string GetFilePath(string[] argv)
{
    string filePath = string.Empty;

    if (argv is not null)
    {
        for (int i = 0; i < argv.Length; i++)
        {
            if (i > 0 && argv[i - 1] == "--directory")
            {
                filePath = argv[i];
                break;
            }
        }
    }

    return filePath;
}

