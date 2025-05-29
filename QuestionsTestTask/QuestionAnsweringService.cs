using Azure;
using Azure.AI.OpenAI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Net;
using System.Text.Json;

namespace QuestionsTestTask
{
    class QuestionAnsweringService
    {
        private Dictionary<string, float[]> _embeddings;
        private List<string> _documents;

        public async Task LoadAndComputeEmbeddings()
        {
            _documents = LoadDocuments();
            _embeddings = await ComputeEmbeddings(_documents);
        }

        public async Task GetAnswer(string question)
        {
            float[] questionEmbedding = await ComputeEmbedding(question);
            string bestMatch = FindMostRelevantDocument(questionEmbedding, _embeddings);
            await GetAnswerFromAzure(question, bestMatch);
        }

        private List<string> LoadDocuments()
        {
            if (!Directory.Exists(Config.FolderPath))
            {
                Console.WriteLine($"Warning: The folder '{Config.FolderPath}' does not exist.");
                return new List<string>();
            }
            return Directory.GetFiles(Config.FolderPath, "*.txt").Select(File.ReadAllText).ToList();
        }

        private async Task<Dictionary<string, float[]>> ComputeEmbeddings(List<string> documents)
        {
            return documents.ToDictionary(doc => doc, doc => new float[768]);
        }

        private async Task<float[]> ComputeEmbedding(string text)
        {
            return new float[768];
        }

        private string FindMostRelevantDocument(float[] queryEmbedding, Dictionary<string, float[]> embeddings)
        {
            return embeddings.Keys.FirstOrDefault();
        }

        private async Task GetAnswerFromAzure(string question, string context)
        {
            AzureKeyCredential credential = new AzureKeyCredential(Config.ApiKey);

            // Initialize the AzureOpenAIClient
            AzureOpenAIClient azureClient = new(new Uri(Config.AzureOpenAiEndpoint), credential);

            ChatClient chatClient = azureClient.GetChatClient("gpt-35-turbo");

            // Create a list of chat messages
            var messages = new List<ChatMessage>
            {
                question
            };

            // Create chat completion options

            var chatCompletionOptions = new ChatCompletionOptions
            {
                Temperature = (float)0.7,
                MaxOutputTokenCount = 800,

                TopP = (float)0.95,
                FrequencyPenalty = (float)0,
                PresencePenalty = (float)0
            };

            try
            {
                // Create the chat completion request
                ChatCompletion completion = await chatClient.CompleteChatAsync(messages, chatCompletionOptions);

                // Print the response
                if (completion != null)
                {
                    Console.WriteLine(JsonSerializer.Serialize(completion, new JsonSerializerOptions() { WriteIndented = true }));
                }
                else
                {
                    Console.WriteLine("No response received.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
