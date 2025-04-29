using Azure;
using Azure.AI.Projects;
using System.ClientModel;
using System.Net.Http.Json;
using System.Text.Json;
using Newtonsoft.Json;

namespace Project8.Client;

public abstract class AIAgent(AIProjectClient client, string modelName) : IAsyncDisposable
{
    protected static readonly string SharedPath = Path.Combine(Environment.CurrentDirectory);
    //protected readonly SalesData SalesData = new(SharedPath);
    protected AIProjectClient Client { get; } = client;
    protected string ModelName { get; } = modelName;
    protected AgentsClient? agentClient;
    protected Agent? agent;
    protected AgentThread? thread;
    protected abstract string InstructionsFileName { get; }
    private readonly JsonSerializerOptions options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    const int maxCompletionTokens = 4096;
    const int maxPromptTokens = 10240;
    const float temperature = 0.1f;
    const float topP = 0.1f;
    private bool disposeAgent = true;
    private string? username;
    private string? ingredients;
    public virtual IEnumerable<ToolDefinition> IntialiseAgentTools() => [];

    private IEnumerable<ToolDefinition> InitialiseTools() => [
        ..IntialiseAgentTools()
    ];

    public async Task RunAsync()
    {
        await Console.Out.WriteLineAsync("Creating agent...");
        agentClient = Client.GetAgentsClient();

        await InitialiseAgentAsync(agentClient);

        IEnumerable<ToolDefinition> tools = InitialiseTools();
        ToolResources? toolResources = InitialiseToolResources();

        string instructions = await CreateInstructionsAsync();

        agent = await agentClient.CreateAgentAsync(
            model: ModelName,
            name: "Project-8",
            instructions: instructions,
            tools: tools,
            temperature: temperature,
            toolResources: toolResources
        );

        await Console.Out.WriteLineAsync($"Agent created with ID: {agent.Id}");

        await Console.Out.WriteLineAsync("Creating thread...");
        thread = await agentClient.CreateThreadAsync();
        await Console.Out.WriteLineAsync($"Thread created with ID: {thread.Id}");
        Utils.LogGreen("Welcome to Project 8! \nPlease enter your name :");
        while (true)
        {
            username = await Console.In.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(username))
            {
                Utils.LogBlue("Please enter a valid name.");
                continue;
            }
            else
                break;
        }

        
        var jsonData = File.ReadAllText(Path.Combine(SharedPath, "userprofile.json"));
        var data = JsonConvert.DeserializeObject<List<Person>>(jsonData);
        var person = data?.FirstOrDefault(p => p.Name == username);

        if (person == null)
        {
            Utils.LogBlue($"No user profile found for {username}. Please create a profile.");
            return;
        }
        Console.WriteLine($"Welcome {person.Name}! Would you like to provide some ingredients for your meal? (yes/no)");
        string? response = await Console.In.ReadLineAsync();
        if (response != null){
            if(response.Equals("yes", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.WriteLine($"Please enter your ingredients (comma separated):");
                ingredients = await Console.In.ReadLineAsync();
            }
        }
        
        while (true)
        {
            await Console.Out.WriteLineAsync();
            
            Utils.LogGreen("Enter your query (type 'exit' or 'save' to quit):");
            string? promptquestion = await Console.In.ReadLineAsync();
            string? prompt =$" Do not generate content summaries or data that hasn’t been explicitly provided.\n"+
                            $"Question : {promptquestion} form the Recepies information vector store \n\n" +
                            $"I like : {person.Likes} \n" +
                            $"I dont Like : {person.Dislikes} \n" +
                            $"Allergies : {person.Allergies} \n" +
                            $"Favorites : {person.Favorites} \n";
            if (ingredients != null)
                prompt += $"Ingredients : {ingredients} \n";
            if (prompt is null)
            {
                continue;
            }

            if (prompt.Equals("exit", StringComparison.InvariantCultureIgnoreCase))
            {
                break;
            }

            if (prompt.Equals("save", StringComparison.InvariantCultureIgnoreCase))
            {
                Utils.LogGreen($"Saving thread with ID: {thread.Id} for agent ID: {agent.Id}. You can view this in AI Foundry at https://ai.azure.com.");
                disposeAgent = false;
                continue;
            }

            _ = await agentClient.CreateMessageAsync(
                threadId: thread.Id,
                role: MessageRole.User,
                content: prompt
            );

            /*AsyncCollectionResult<StreamingUpdate> streamingUpdate = agentClient.CreateRunStreamingAsync(
                threadId: thread.Id,
                assistantId: agent.Id,
                maxCompletionTokens: maxCompletionTokens,
                maxPromptTokens: maxPromptTokens,
                temperature: temperature,
                topP: topP
            );

            await foreach (StreamingUpdate update in streamingUpdate)
            {
                await HandleStreamingUpdateAsync(update);
            }*/
            var aiclient = client.GetAgentsClient();
            Response<ThreadRun> runResponse = await aiclient.CreateRunAsync(
                    thread.Id,
                    agent.Id);
            ThreadRun run = runResponse.Value;

            // Poll until the run reaches a terminal status
            do
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                runResponse = await aiclient.GetRunAsync(thread.Id, runResponse.Value.Id);
            }
            while (runResponse.Value.Status == RunStatus.Queued
                || runResponse.Value.Status == RunStatus.InProgress);

            Response<PageableList<ThreadMessage>> messagesResponse = await aiclient.GetMessagesAsync(thread.Id,runResponse.Value.Id,1);
            IReadOnlyList<ThreadMessage> messages = messagesResponse.Value.Data;

            // Display messages
            foreach (ThreadMessage threadMessage in messages)
            {
                //Console.Write($"{threadMessage.CreatedAt:yyyy-MM-dd HH:mm:ss} - {threadMessage.Role,10}: ");
                foreach (MessageContent contentItem in threadMessage.ContentItems)
                {
                    if (contentItem is MessageTextContent textItem)
                    {
                        Console.Write(textItem.Text);
                    }
                }
                Console.WriteLine();
            }
        }
    }

    protected virtual ToolResources? InitialiseToolResources() => null;

    protected virtual Task<string> CreateInstructionsAsync()
    {
        string instructionsFile = Path.Combine(SharedPath, "instructions", InstructionsFileName);

        if (!File.Exists(instructionsFile))
        {
            throw new FileNotFoundException("Instructions file not found.", instructionsFile);
        }

        string instructions = File.ReadAllText(instructionsFile);

        return Task.FromResult(instructions);
    }

    protected virtual Task InitialiseAgentAsync(AgentsClient agentClient) => Task.CompletedTask;

    private async Task HandleStreamingUpdateAsync(StreamingUpdate update)
    {
        switch (update.UpdateKind)
        {
            case StreamingUpdateReason.RunRequiresAction:
                // The run requires an action from the application, such as a tool output submission.
                // This is where the application can handle the action.
                RequiredActionUpdate requiredActionUpdate = (RequiredActionUpdate)update;
                await Console.Out.WriteAsync("HandleActionAsync(requiredActionUpdate)");;
                break;

            case StreamingUpdateReason.MessageUpdated:
                // The agent has a response to the user, potentially requiring some user input
                // or further action. This comes as a stream of message content updates.
                MessageContentUpdate messageContentUpdate = (MessageContentUpdate)update;
                await Console.Out.WriteAsync(messageContentUpdate.Text);
                break;

            case StreamingUpdateReason.MessageCompleted:
                MessageStatusUpdate messageStatusUpdate = (MessageStatusUpdate)update;
                ThreadMessage tm = messageStatusUpdate.Value;

                var contentItems = tm.ContentItems;

                foreach (MessageContent contentItem in contentItems)
                {
                    if (contentItem is MessageImageFileContent imageContent)
                    {
                        await DownloadImageFileContentAsync(imageContent);
                    }
                }
                break;

            case StreamingUpdateReason.RunCompleted:
                // The run is complete, so we can print a new line.
                await Console.Out.WriteLineAsync();
                break;

            case StreamingUpdateReason.RunFailed:
                // The run failed, so we can print the error message.
                RunUpdate runFailedUpdate = (RunUpdate)update;

                if (runFailedUpdate.Value.LastError.Code == "rate_limit_exceeded")
                {
                    await Console.Out.WriteLineAsync(runFailedUpdate.Value.LastError.Message);
                    break;
                }

                await Console.Out.WriteLineAsync($"Error: {runFailedUpdate.Value.LastError.Message} (code: {runFailedUpdate.Value.LastError.Code})");
                break;
            /*default: 
                await Console.Out.WriteLineAsync($"Unknown update type: {update}");
                break;*/
        }
    }

    private async Task DownloadImageFileContentAsync(MessageImageFileContent imageContent)
    {
        if (agentClient is null)
        {
            return;
        }

        Utils.LogGreen($"Getting file with ID: {imageContent.FileId}");

        BinaryData fileContent = await agentClient.GetFileContentAsync(imageContent.FileId);
        string directory = Path.Combine(SharedPath, "files");
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string filePath = Path.Combine(directory, imageContent.FileId + ".png");
        await File.WriteAllBytesAsync(filePath, fileContent.ToArray());

        Utils.LogGreen($"File save to {Path.GetFullPath(filePath)}");
    }


    public class Person
    {
        public required string Name { get; set; }
        public string? Likes { get; set; }
        public string? Dislikes { get; set; }
        public string? Allergies { get; set; }
        public string? Favorites { get; set; }
    }

    public async ValueTask DisposeAsync()
    {
        if (!disposeAgent)
        {
            return;
        }

        if (agentClient is not null)
        {
            try{
                if (thread is not null)
                {
                    await agentClient.DeleteThreadAsync(thread.Id);
                }

                if (agent is not null)
                {
                    await agentClient.DeleteAgentAsync(agent.Id);
                }
            }
            catch (Azure.RequestFailedException ex)
            {
                Utils.LogBlue($"Error disposing agent client: {ex.Message}");
            }
            
        }
    }

}