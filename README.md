# Project8

az login

dotnet user-secrets set "AiAgentService" "<Project connection string>" --project "<CSPROJ file path>"

dotnet user-secrets set "Azure:ModelName" "gpt-4o-mini" --project "<CSPROJ file path>"

dotnet add package Azure.AI.Projects

dotnet add /workspaces/Project8/Project8.csproj package Azure.AI.Projects --prerelease

 git config --global user.name "Your Name"

git config --global user.email "you@example.com"