using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Autopost.Services
{
    public class RssFeedClient
    {
        public async Task<IEnumerable<RssFeedEntry>> GetEntries(string url)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetStringAsync(url);
                var entries = new List<RssFeedEntry>();

                var doc = XDocument.Parse(response);
                foreach (var item in doc.Descendants("item"))
                {
                    entries.Add(new RssFeedEntry(
                        item.Element("title")?.Value,
                        item.Element("link")?.Value,
                        item.Element("description")?.Value
                    ));
                }

                return entries;
            }
        }
    }

    public class RssFeedEntry
    {
        public string Title { get; }
        public string Link { get; }
        public string Description { get; }

        public RssFeedEntry(string title, string link, string description)
        {
            Title = title;
            Link = link;
            Description = description;
        }
    }
}
