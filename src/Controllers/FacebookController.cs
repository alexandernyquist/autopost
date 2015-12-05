using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.OptionsModel;
using Newtonsoft.Json.Linq;
using Autopost.Models;
using Autopost.Options;
using Autopost.Services;

namespace Autopost.Controllers
{
    public class FacebookController : Controller
	{
		private FeedService _feedService;
		
		private FacebookOptions _options;
		
        public FacebookController(FeedService feedService, IOptions<FacebookOptions> optionsAccessor)
		{
			_feedService = feedService;
			_options = optionsAccessor.Value;
		}
		
		public IActionResult Test()
		{
			return Content(_options.AppId);
		}
		
		public async Task<IActionResult> ValidateAccessToken(string target)
		{
			// Find stored access token
			var feeds = _feedService.GetFeeds(Startup.User.ToLower());
			
			if(feeds != null)
			{
				foreach(var feed in feeds)
				{
					if(feed.Targets != null)
					{
						foreach(var targetConfig in feed.Targets)
						{
							if(targetConfig.Name == "Facebook" && !string.IsNullOrEmpty(targetConfig.AccessToken))
							{
								// Validate
								using(var client = new HttpClient())
								{
									var url = $"https://graph.facebook.com/v2.5/me?access_token={targetConfig.AccessToken}";
									
									try
									{
										var response = await client.GetStringAsync(url);
										
										var jobject = JObject.Parse(response);
										if(jobject["error"] == null)
										{
											targetConfig.AccountId = jobject["id"].Value<string>();
											_feedService.StoreFeeds(Startup.User.ToLower(), feeds);
											
											// Valid
											return Json(new
											{
												success = true,
												valid = true
											});
										}	
									}
									catch(HttpRequestException ex) when((ex.Message ?? "").Contains("(Bad Request)"))
									{
										// TODO: How to properly get the status code?
										
										return Json(new
										{
											success = true,
											valid = false
										});	
									}
								}	
							}
						}	
					}	
				}
			}
			
			return Json(new
			{
				success = true,
				valid = false
			});
		}
		
		public IActionResult BeginAuthenticate(string target)
		{
			return Redirect(GetRedirectUrl());
		}
		
		public async Task<IActionResult> Authenticate(string code)
        {
            var url =
                $"https://graph.facebook.com/v2.3/oauth/access_token?client_id={_options.AppId}&client_secret={_options.AppSecret}&redirect_uri={_options.RedirectUrl}&code={code}";

            using (var client = new HttpClient())
            {
                var response = await client.GetStringAsync(url);

                // Get access token
                var jobject = JObject.Parse(response);
                var accessToken = jobject["access_token"].Value<string>();

				// Update target
				var updated = false;
				var feeds = _feedService.GetFeeds(Startup.User.ToLower());
				foreach(var feed in feeds)
				{
					if(feed.Targets == null)
					{
						continue;
					}
					
					foreach(var target in feed.Targets)
					{
						if(target.Name == "Facebook")
						{
							target.AccessToken = accessToken;	
							updated = true;
						}
					}
				}
				
				if(updated)
				{
					_feedService.StoreFeeds(Startup.User.ToLower(), feeds);
				}

                return RedirectToAction("Index", "Home");
            }
        }
		
		private string GetRedirectUrl() =>
			$"https://www.facebook.com/dialog/oauth?client_id={_options.AppId}&scope=read_insights,manage_pages,user_posts,publish_actions&redirect_uri={_options.RedirectUrl}";
	}
}