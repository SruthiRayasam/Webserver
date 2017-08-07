using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebServer;

namespace WebServerTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod_FetchTwitterFeed()
        {
            var searchText = string.Empty;
            var _twitterFeedReader = new TwitterFeedReader();
            var feedText = _twitterFeedReader.FetchTwitterFeed(searchText);
            Assert.IsTrue(feedText.Length > 0);
        }

        [TestMethod]
        public void TestMethod_FetchTwitterFeed_With_SearchText()
        {
            var searchText = "OR @salesforce";
            var _twitterFeedReader = new TwitterFeedReader();
            var feedText = _twitterFeedReader.FetchTwitterFeed(searchText);
            Assert.IsTrue(feedText.Length > 0);
        }
    }
}
