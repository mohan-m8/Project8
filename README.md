# Project8

# 🍽️ Personalized Recipe Assistant using Azure AI

## 🚀 Project Overview

This is a C# Console Application that uses **Azure OpenAI** to generate **personalized recipes** based on user-specific preferences. Instead of offering generic culinary suggestions, the app tailors every recipe strictly around the **user’s likes**, **dislikes**, **allergies**, and **favorites**, ensuring safe, enjoyable, and custom meal plans.

### 🔐 **Authentication & Secrets Management**

```bash
az login
```
- Logs you into your Azure account via the CLI. This is needed to manage Azure resources from the command line.

```bash
dotnet user-secrets set "AiAgentService" "" --project ""
```
- Stores a secret (`AiAgentService`) in the *user secrets store* for a .NET project. You should fill in the project path and the value for the secret.
- Example:  
  `dotnet user-secrets set "AiAgentService" "my-api-key" --project "./MyApp/MyApp.csproj"`

```bash
dotnet user-secrets set "Azure:ModelName" "gpt-4o-mini" --project ""
```
- Sets a secret named `Azure:ModelName` to use the GPT-4o Mini model. This is typically used for configuration settings, like choosing which AI model to invoke.

---

### 📦 **Package Management**

```bash
dotnet add package Azure.AI.Projects
```
- Adds the NuGet package `Azure.AI.Projects` to your .NET project. This package may be hypothetical or in preview.

```bash
dotnet add /workspaces/Project8/Project8.csproj package Azure.AI.Projects --prerelease
```
- Same as above but explicitly adds the *pre-release version* of the package to the given project.

---

### 🔧 **Git Configuration**

```bash
git config --global user.name "Your Name"
git config --global user.email "you@example.com"
```
- Configures your Git identity globally (for all projects on your system). Required for making commits and associating them with your identity.

---

Would you like a script or README-style format to include this in your project documentation?

## 🧠 How It Works

1. ✅ User enters their name.
2. 📄 The app looks for a file named `userprofile.json`.
3. 📁 If the file doesn't exist, a default template is created.
4. 🖊️ User manually edits their preferences.
5. 🔁 The app reloads the file on-demand (no restart needed).
6. 🧑‍🍳 Azure AI uses the preferences to generate recipes, avoiding allergens and disliked ingredients.

---

## 🗂️ Example Instruction File

```txt
Name: John
Likes: spicy food, quick meals
Dislikes: mushrooms, olives
Allergies: peanuts
Favorites: paneer, dosa, chickpeas

# Please update your preferences above.
