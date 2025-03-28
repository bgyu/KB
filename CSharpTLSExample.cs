/* Part 1: Generate Certificates
/certs
  ├── ca.pem           (Root CA certificate)
  ├── ca.key           (Root CA private key)
  ├── server.pem       (Server certificate)
  ├── server.key       (Server private key)
  ├── server.pfx       (Server cert + key in PFX)
  ├── client.pem       (Client certificate)
  ├── client.key       (Client private key)
  ├── client.pfx       (Client cert + key in PFX)

  Generate commands (Run in terminal)
  # 1. Create Root CA
openssl genrsa -out ca.key 2048
openssl req -x509 -new -nodes -key ca.key -sha256 -days 1024 -out ca.pem -subj "/CN=MyRootCA"

# 2. Create Server certificate
openssl genrsa -out server.key 2048
openssl req -new -key server.key -out server.csr -subj "/CN=localhost"
openssl x509 -req -in server.csr -CA ca.pem -CAkey ca.key -CAcreateserial -out server.pem -days 500 -sha256
openssl pkcs12 -export -out server.pfx -inkey server.key -in server.pem -certfile ca.pem -password pass:123456

# 3. Create Client certificate
openssl genrsa -out client.key 2048
openssl req -new -key client.key -out client.csr -subj "/CN=MyClient"
openssl x509 -req -in client.csr -CA ca.pem -CAkey ca.key -CAcreateserial -out client.pem -days 500 -sha256
openssl pkcs12 -export -out client.pfx -inkey client.key -in client.pem -certfile ca.pem -password pass:123456
*/

/*
 * Simple Mutual TLS Demo
 * Folder structure:
 * /certs
 *   - ca.pem, ca.key
 *   - server.pem, server.key, server.pfx
 *   - client.pem, client.key, client.pfx
 */

// ================= SERVER ===================
// Program.cs (ASP.NET Core Minimal API Server)
// dotnet run --project Server
using Microsoft.AspNetCore.Server.Kestrel.Https;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureHttpsDefaults(httpsOptions =>
    {
        httpsOptions.ServerCertificate = new X509Certificate2("certs/server.pfx", "123456");
        httpsOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
        httpsOptions.ClientCertificateValidation = (cert, chain, errors) =>
        {
            Console.WriteLine($"Client Certificate: {cert.Subject}");
            return cert.Subject.Contains("MyClient");
        };
    });
});

var app = builder.Build();

app.MapGet("/", () => "🎉 Hello, authenticated client!");

app.Run();

// ================= CLIENT ===================
// dotnet run --project Client
// Program.cs (Console Client)
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;

var handler = new HttpClientHandler();
handler.ClientCertificates.Add(new X509Certificate2("certs/client.pfx", "123456"));
handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, errors) =>
{
    return cert.Subject.Contains("localhost");
};

using var client = new HttpClient(handler);

try
{
    var response = await client.GetAsync("https://localhost:5001");
    var content = await response.Content.ReadAsStringAsync();
    Console.WriteLine(content);
}
catch (Exception ex)
{
    Console.WriteLine($"❗️ Error: {ex.Message}");
}


