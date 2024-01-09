using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using IdentityModel.Client;

namespace restclient1
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            /*************/
            /*** LOGIN ***/
            /*************/

            string accessToken = "";

            using (HttpClient httpClient = new HttpClient())
            {
                // Keycloak configuration
                var keycloakUrl = "http://localhost:8080/auth/realms/password-strength-meter";
                var clientId = "password-strength-client";
                var clientSecret = "H0gUDdl6nlTYVxnOTJxOOacRn8vADqKL";
                var username = "admin";
                var password = "P455w0rd!";

                // IdentityModel client configuration
                var client = new HttpClient();
                var disco = await client.GetDiscoveryDocumentAsync(keycloakUrl);

                if (disco.IsError)
                {
                    Console.WriteLine($"Error while discovering Keycloak: {disco.Error}");
                    return;
                }

                // Request a token using Resource Owner Password Credentials (ROPC) grant
                var tokenResponse = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
                {
                    Address = disco.TokenEndpoint,
                    ClientId = clientId,
                    ClientSecret = clientSecret,
                    UserName = username,
                    Password = password
                });

                if (tokenResponse.IsError)
                {
                    Console.WriteLine($"Error while requesting token: {tokenResponse.Error}");
                    return;
                }

                accessToken = tokenResponse.AccessToken.ToString();
                Console.WriteLine($"Access Token: {tokenResponse.AccessToken}");
            }

            Console.WriteLine("Press anykey!");
            Console.ReadLine();

            /********************/
            /*** POST REQUEST ***/
            /********************/

            // API endpoint and Bearer token
            var apiUrl = "http://localhost:8080/auth/admin/realms/password-strength-meter/users/79d3e06e-0758-4945-a1af-bad8a6cece7a/reset-password";
            var bearerToken = accessToken;

            // Data to be sent in the POST request (replace with your actual data)
            var postData = "{\"type\": \"password\", \"value\": \"test\", \"temporary\": false}";
            var content = new StringContent(postData, Encoding.UTF8, "application/json");

            using (var httpClient = new HttpClient())
            {
                try
                {
                    // Set Bearer token in the Authorization header
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

                    // Send POST request
                    HttpResponseMessage response = await httpClient.PutAsync(apiUrl, content);

                    // Check if the request was successful (status code 200-299)
                    if (response.IsSuccessStatusCode)
                    {
                        // Read the content of the response
                        string responseBody = await response.Content.ReadAsStringAsync();

                        // Display the content of the response
                        Console.WriteLine("Response from server:");
                        Console.WriteLine(responseBody);
                    }
                    else
                    {
                        string errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                        Console.WriteLine($"Error Content: {errorContent}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }

            Console.WriteLine("Hello, World!");
        }
    }
}
