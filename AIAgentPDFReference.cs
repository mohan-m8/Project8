using Azure.AI.Projects;

namespace Project8.Client;

public class AIAgentPDFReference(AIProjectClient client, string modelName) : AIAgent(client, modelName)
{
    protected override string InstructionsFileName => "file_search.txt";

    private VectorStore? vectorStore;

    public override IEnumerable<ToolDefinition> IntialiseAgentTools() =>
        [new FileSearchToolDefinition()];

    protected override async Task InitialiseAgentAsync(AgentsClient agentClient)
    {
        string datasheet = Path.Combine(SharedPath, "Recipe PDF", "Recipe-Book.pdf");
        Utils.LogPurple($"Uploading file: {datasheet}");

        AgentFile file = await agentClient.UploadFileAsync(
            filePath: datasheet,
            purpose: AgentFilePurpose.Agents
        );

        Utils.LogPurple($"File uploaded: {file.Id}");

        vectorStore = await agentClient.CreateVectorStoreAsync(
            fileIds: [file.Id],
            name: "Recipes Information Vector Store"
        );

        Utils.LogPurple($"Vector store created: {vectorStore.Id}");
    }

    protected override ToolResources? InitialiseToolResources()
    {
        if (vectorStore is null)
        {
            throw new InvalidOperationException("Vector store must be created before initialising tool resources.");
        }

        return new ToolResources
        {
            FileSearch = new FileSearchToolResource([vectorStore.Id], null)
        };
    }
}
