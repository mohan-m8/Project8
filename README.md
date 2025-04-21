# Project8

az login

dotnet user-secrets set "AiAgentService" "<Project connection string>" --project "<CSPROJ file path>"

dotnet user-secrets set "Azure:ModelName" "gpt-4o-mini" --project "<CSPROJ file path>"