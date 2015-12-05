using System;
using System.Collections.Generic;
using System.Linq;

namespace Autopost.Models
{
	public class Feed
	{
		public string FeedUrl { get; set; }
				
		public DateTime? LastFetch { get; set; }
				
		public List<TargetConfig> Targets { get; set; }
				
		public HashSet<string> Published { get; set; }
		
		public Feed(string feedUrl)
		{
			FeedUrl = feedUrl;
			Published = new HashSet<string>();
		}
		
		protected Feed()
		{
			
		}

        public bool AddTarget(string name)
        {
            if(Targets == null)
			{
				Targets = new List<TargetConfig>();
			}
			
			if(!Targets.Any(x => x.Name == name))
			{
				Targets.Add(new TargetConfig {
					Name = name
				});
				
				return true;
			}
			
			return false;
        }
    }
}