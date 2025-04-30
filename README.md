# Project8

az login

dotnet user-secrets set "AiAgentService" "<Project connection string>" --project "<CSPROJ file path>"

dotnet user-secrets set "Azure:ModelName" "gpt-4o-mini" --project "<CSPROJ file path>"

dotnet add package Azure.AI.Projects

dotnet add /workspaces/Project8/Project8.csproj package Azure.AI.Projects --prerelease

 git config --global user.name "Your Name"

git config --global user.email "you@example.com"






# 🍽️ Personalized Recipe Assistant using Azure AI

## 🚀 Project Overview

This is a C# Console Application that uses **Azure OpenAI** to generate **personalized recipes** based on user-specific preferences. Instead of offering generic culinary suggestions, the app tailors every recipe strictly around the **user’s likes**, **dislikes**, **allergies**, and **favorites**, ensuring safe, enjoyable, and custom meal plans.

---

## 🧠 How It Works

1. ✅ User enters their name.
2. 📄 The app looks for a file named `instructions_<username>.txt`.
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
