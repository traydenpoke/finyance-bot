using HtmlAgilityPack;
using System.Globalization;
using System.Net;

namespace FinyanceApp.Services
{
    public class GoogleFinanceService
    {
        private readonly HttpClient _http;

        public GoogleFinanceService()
        {
            _http = new HttpClient();
            _http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
                                                           "AppleWebKit/537.36 (KHTML, like Gecko) " +
                                                           "Chrome/115.0 Safari/537.36");
        }

        // Return price of an asset - currently only supports Canadian-listed assets (TSE & CAD)
        public async Task<float?> GetPriceAsync(string symbol, string type)
        {
            var suffix = type == "stock" ? ":TSE" : "-CAD";
            var url = $"https://www.google.com/finance/quote/{symbol}{suffix}";

            var res = await _http.GetAsync(url);
            if (!res.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to fetch: {url} - Status {res.StatusCode}");
                return null;
            }

            var html = await res.Content.ReadAsStringAsync();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var priceNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'YMlKec') and contains(@class,'fxKbKc')]");

            if (priceNode != null)
            {
                var raw = WebUtility.HtmlDecode(priceNode.InnerText).Trim().Replace("$", "");
                if (float.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
                {
                    return price;
                }
            }

            return null;
        }
    }
}
