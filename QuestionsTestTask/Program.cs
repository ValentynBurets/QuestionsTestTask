using QuestionsTestTask;
class Program
{
    static async Task Main(string[] args)
    {
        var qaService = new QuestionAnsweringService();

        Console.WriteLine("Loading documents and computing embeddings...");
        await qaService.LoadAndComputeEmbeddings();

        Console.WriteLine("Enter a question:");
        string question = Console.ReadLine();

        await qaService.GetAnswer(question);
    }
}
