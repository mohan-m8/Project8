using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Project8.Client;

var builder = new ConfigurationBuilder()
    .AddUserSecrets<Program>();

var configuration = builder.Build();

string apiDeploymentName = configuration["Azure:ModelName"] ?? throw new InvalidOperationException("Azure:ModelName is not set in the configuration.");
string projectConnectionString = configuration["AiAgentService"] ?? throw new InvalidOperationException("ConnectionStrings:AiAgentService is not set in the configuration.");

AIProjectClient projectClient = new(projectConnectionString, new DefaultAzureCredential());

 await using AIAgent aIAgent = new AIAgentInstructionsRef(projectClient, apiDeploymentName);
 await aIAgent.RunAsync();
 await aIAgent.DisposeAsync();