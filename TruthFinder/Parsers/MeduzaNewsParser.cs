using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;

namespace TruthFinder.Parsers;

public class MeduzaNewsParser : INewsParser
{
    public string GetTextByUrl(string url)
    {
        string result = string.Empty;
        try
        {
            HttpClient client = new HttpClient();
            string html = string.Empty;

            var task = Task.Run(async () => html = await client.GetStringAsync(url));
            task.Wait();
            
            
            HtmlAgilityPack.HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            var articleNodes = doc.DocumentNode.SelectNodes("//div[@class='GeneralMaterial-module-article']//p/text()");

            foreach (var node in articleNodes)
            {
                result += node.InnerText + " ";
            }

            result = HttpUtility.HtmlDecode(result);
        }
        catch (Exception ex)
        {
            
        }

        return result;
    }

    public bool IsUrlRight(string url)
    {
        return url.Contains("meduza.io");
    }
}