using System.Text.Json;
using Microsoft.Extensions.Configuration;
using RestSharp;
using RestSharp.Authenticators;

namespace ZendeskExport
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, false)
                .AddUserSecrets<Program>();

            var configuration = builder.Build();

            var zendeskOptions = configuration.GetSection("Zendesk").Get<ZendeskOptions>();
            var list = await GetZendeskArticles(zendeskOptions);

            WriteArticlesJsonFile(list);

            WriteArticlesAsIndividualHtmlFiles(list);

            WriteArticlesAsIndividualMarkdownFiles(list);
        }

        private static async Task<List<ZendeskArticle>> GetZendeskArticles(ZendeskOptions zendeskOptions)
        {
            var options = new RestClientOptions($"https://{zendeskOptions.Subdomain}/api/v2")
            {
                Authenticator = new HttpBasicAuthenticator($"{zendeskOptions.Username}/token", zendeskOptions.ApiToken)
            };

            var client = new RestClient(options);

            var request = new RestRequest("/help_center/articles", Method.Get);

            var response = await client.ExecuteGetAsync<ZendeskArticleResponse>(request);

            Console.WriteLine($"Total articles: {response.Data.Count}");

            var list = new List<ZendeskArticle>();
            list.AddRange(response.Data.Articles);

            while (response.Data.Next_Page != null)
            {
                Console.WriteLine($"Fetching next page: {response.Data.Next_Page}");
                request = new RestRequest(response.Data.Next_Page, Method.Get);
                response = await client.ExecuteGetAsync<ZendeskArticleResponse>(request);
                list.AddRange(response.Data.Articles);
            }

            Console.WriteLine($"Fetched articles: {list.Count}");
            return list;
        }

        private static void WriteArticlesJsonFile(List<ZendeskArticle> articles)
        {
            Directory.CreateDirectory("Articles");

            var json = JsonSerializer.Serialize(new { Articles = articles });
            File.WriteAllText("Articles\\articles.json", json);

            Console.WriteLine($"Created articles Json file.");
        }

        private static void WriteArticlesAsIndividualHtmlFiles(List<ZendeskArticle> articles)
        {
            Directory.CreateDirectory("Articles\\Html");

            foreach (var article in articles)
            {
                if (article.Body == null) continue;
                File.WriteAllText($"Articles\\Html\\{article.Id}.html", article.Body);
            }
            
            Console.WriteLine($"Created articles Html files.");
        }

        private static void WriteArticlesAsIndividualMarkdownFiles(List<ZendeskArticle> articles)
        {
            Directory.CreateDirectory("Articles\\Md");
            
            var converter = new ReverseMarkdown.Converter();

            foreach (var article in articles)
            {
                if (article.Body == null) continue;
                var markdown = converter.Convert(article.Body);
                File.WriteAllText($"Articles\\Md\\{article.Id}.md", markdown);
            }

            Console.WriteLine($"Created articles Markdown files.");
        }
    }
}
