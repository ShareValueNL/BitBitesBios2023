using System.Threading.Tasks;
using System.Web.Http;
using Firebase.Database;
using Tweetinvi;
using Tweetinvi.Models;

namespace WebApi.Controllers
{
    public class TwitterController : ApiController
    {
        private const string ConsumerKey = "CONSUMER_KEY";
        private const string ConsumerSecret = "CONSUMER_SECRET";
        private const string AccessToken = "ACCESS_TOKEN";
        private const string AccessTokenSecret = "ACCESS_TOKEN_SECRET";

        public FirebaseClient FirebaseClient { get; set; }

        public ITwitterClient TwitterClient { get; set; }

        public TwitterController()
        {
            TwitterClient = new TwitterClient(new TwitterCredentials(ConsumerKey, ConsumerSecret, AccessToken, AccessTokenSecret));
            FirebaseClient = new FirebaseClient("https://nodejs-express-demo.firebaseio.com/");
        }

        [HttpGet]
        [Route("twitter/{username}")]
        public async Task<IHttpActionResult> GetTwitterInfo(string username)
        {
            try
            {
                var searchParameters = new SearchTweetsParameters(username)
                {
                    MaximumNumberOfResults = 1
                };
                var searchResult = await TwitterClient.Search.SearchTweetsAsync(searchParameters);
                var userProfile = searchResult[0].CreatedBy;

                var twitterSavedObj = new
                {
                    followers = userProfile.FollowersCount,
                    screen_name = userProfile.ScreenName,
                    logo = userProfile.ProfileImageUrl,
                    banner = userProfile.ProfileBannerURL,
                    profile_link_color = userProfile.ProfileLinkColor,
                    profile_text_color = userProfile.ProfileTextColor
                };

                await FirebaseClient.Child("twitter").Child(username).PutAsync(twitterSavedObj);

                return Ok(new[] { new { data = twitterSavedObj } });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}