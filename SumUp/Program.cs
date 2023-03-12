using NewsAPI;
using NewsAPI.Models;
using NewsAPI.Constants;
using System;
using OpenAI.GPT3.Managers;
using OpenAI.GPT3;
using OpenAI.GPT3.ObjectModels.RequestModels;
using OpenAI.GPT3.ObjectModels;

namespace MyApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateArticleSummary("Metaverse");
        }

        private static void CreateArticleSummary(string topic)
        {
            var articles = GetNewsArticles(topic, DateTime.UtcNow.AddDays(-7));
            Summarize(articles).Wait();
        }

        /// <summary>
        /// Uses News API to retrieve articles on the specific topic from the last specific date.
        /// </summary>
        private static List<Article> GetNewsArticles(string topic, DateTime fromDate)
        {
            // Create News API service
            var newsApiClient = new NewsApiClient("e183b0598e204980969039dc8aff222d");
            var articlesResponse = newsApiClient.GetEverything(new EverythingRequest
            {
                Q = topic,
                SortBy = SortBys.Popularity,
                Language = Languages.EN,
                From = fromDate
            });
            if (articlesResponse.Status == Statuses.Ok)
            {
                return articlesResponse.Articles.Take(7).ToList();
            }
            else
                return new List<Article>();
        }

        /// <summary>
        /// Uses ChatGPT to summarize all the specified articles.
        /// </summary>
        private static async Task Summarize(List<Article> content)
        {
            // Create Open AI service
            var openAiService = new OpenAIService(new OpenAiOptions()
            {
                ApiKey = "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"
            });

            Console.WriteLine("\n\nAttempting to summarize articles...\n\n");

            // Create prompt
            string prompt = String.Format("Please write an essay that covers the important points for the following content in the JSON list: {0}",
                    Newtonsoft.Json.JsonConvert.SerializeObject(content.Select(x => x.Content).ToList()));

            Console.WriteLine(prompt);

            // Create completion result
            var completionResult = await openAiService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest()
            {
                Messages = new List<ChatMessage>
                {
                    ChatMessage.FromUser(prompt)
                },
                Model = Models.ChatGpt3_5Turbo0301,
                MaxTokens = 1000
            });

            
            // Output summary if successful
            if (completionResult.Successful)
            {
                Console.WriteLine("\n\nSummarization completed.\n\n");
                Console.WriteLine(completionResult.Choices.First().Message.Content);
            }
            else
            {
                // Output error if unsuccessful
                if (completionResult.Error == null)
                {
                    throw new Exception("Unknown Error");
                }
                Console.WriteLine($"{completionResult.Error.Code}: {completionResult.Error.Message}");
            }
        }
    }
}