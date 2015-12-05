using System.Collections.Generic;
using System.Linq;
using Autopost.Models;

namespace Autopost.Services
{
    public class FeedService
	{
        private IKeyValueStore _keyValueStore;

        public FeedService(IKeyValueStore keyValueStore)
		{
			_keyValueStore = keyValueStore;
		}
		
		public bool AddFeed(string userName, string feedUrl)
		{
			var feeds = GetFeeds(userName);
			
			// Check that we do not have a feed with this url already
			if(feeds.Any(x => x.FeedUrl == feedUrl)) {
				return false;
			}
			
			feeds.Add(new Feed(feedUrl));
			StoreFeeds(userName, feeds);
			return true;
		}
		
		public bool AddTargetToFeed(string userName, string feedUrl, string targetName)
		{
			var feeds = GetFeeds(userName);
			if(feeds == null || !feeds.Any())
			{
				return false;
			}
			
			var feed = feeds.FirstOrDefault(x => x.FeedUrl == feedUrl);
			if(feed == null)
			{
				return false;	
			}
			
			if(feed.AddTarget(targetName))
			{
				StoreFeeds(userName, feeds);
				return true;
			}
			
			return false;
		}

        public List<Feed> GetFeeds(string userName)
        {
            return _keyValueStore.Get<List<Feed>>(UserKey(userName))
				?? new List<Feed>();
        }
		
		public void StoreFeeds(string userName, IEnumerable<Feed> feeds)
		{
			_keyValueStore.Set(UserKey(userName), feeds);
		}
		
		private string UserKey(string userName) =>
			$"{userName}.feeds";
    }
}