using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Autopost.Controllers.ViewModels;
using Autopost.Models;
using Autopost.Services;

namespace Autopost.Controllers
{
    public class HomeController : Controller
	{
        private FeedService _feedService;
        private RssFeedClient _rssFeedClient;

        public HomeController(FeedService feedService, RssFeedClient rssFeedClient)
		{
			_feedService = feedService;
			_rssFeedClient = rssFeedClient;
		}
		
		public IActionResult AddFeed(string feedUrl)
		{
			if(!_feedService.AddFeed(Startup.User, feedUrl))
			{
				return Content("A feed with this URL already exists.");
			} else {
				return RedirectToAction("Index", "Home");	
			}
		}
		
		public IActionResult AddTarget(string feedUrl, string targetName)
		{
			_feedService.AddTargetToFeed(Startup.User, feedUrl, targetName);
			return RedirectToAction("Index","Home");
		}
		
		public IActionResult Index()
		{
			var feeds = _feedService.GetFeeds(Startup.User);
			return View(new HomeIndexViewModel(feeds));
		}
		
		public async Task<IActionResult> PublishLatest(string feedUrl)
		{
			var feeds = _feedService.GetFeeds(Startup.User);
			if(feeds == null || !feeds.Any())
			{
				return HttpNotFound();
			}
			
			var feed = feeds.FirstOrDefault(x => x.FeedUrl == feedUrl);
			if(feed == null || feed.Targets == null || !feed.Targets.Any())
			{
				return HttpNotFound();
			}
			
			var updated = false;
			var rssFeedEntries = await _rssFeedClient.GetEntries(feedUrl);
			feed.LastFetch = DateTime.UtcNow;
			
			if(rssFeedEntries.Any())
			{
				// Todo: Order by publish date
				var latest = rssFeedEntries.First();
				if(feed.Published != null && !feed.Published.Contains(latest.Link))
				{
					foreach(var target in feed.Targets)
					{
						await PublishPost(latest, target);
						updated = true;	
					}
					
					feed.Published.Add(latest.Link);
				}
			}
			
			if(updated)
			{
				_feedService.StoreFeeds(Startup.User, feeds);
			}
			
			return RedirectToAction("Index", "Home");
		}

        private async Task<bool> PublishPost(RssFeedEntry post, TargetConfig target)
        {
            // Todo: Assume Facebook for now. Introduce a TargetFactory and an ITarget interface.
			var accessToken = target.AccessToken;
			
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new System.Exception("No access token");
            }

            using(var client = new HttpClient())
            {
				try
				{
					var url = $"https://graph.facebook.com/v2.5/{target.AccountId}/feed?link={post.Link}&access_token={accessToken}";
					var response = await client.PostAsync(url, new StringContent(string.Empty));
					var body = await response.Content.ReadAsStringAsync();
					System.Console.WriteLine(body);
					return true;
				}
				catch(Exception ex)
				{
					System.Console.WriteLine("err: " + ex.Message);
					return false;
				}
            }
        }
		
		public async Task<IActionResult> Test()
		{
			var entries = _rssFeedClient.GetEntries("http://nyqui.st/feed/");
			return Json(entries);
		}
    }
}