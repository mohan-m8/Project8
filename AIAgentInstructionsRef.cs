using Azure.AI.Projects;

namespace Project8.Client;

public class AIAgentInstructionsRef(AIProjectClient client, string modelName)
    : AIAgent(client, modelName)
{    
    protected override string InstructionsFileName => "function_calling.txt";
}
