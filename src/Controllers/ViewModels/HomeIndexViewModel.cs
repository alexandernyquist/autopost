using System.Collections.Generic;
using Autopost.Models;

namespace Autopost.Controllers.ViewModels
{
	public class HomeIndexViewModel
	{
		public IEnumerable<Feed> Feeds { get; }
		
		 public HomeIndexViewModel(IEnumerable<Feed> feeds)
		 {
			 Feeds = feeds;
		 }
	}
}