using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhibliPlanner
{
    static public class DiscordHelper
    {
        static public string DiscordWebhookURL = "https://discord.com/api/webhooks/826578597598199869/HTgq6g-b4v2CbjyzbyRFkSQTXTP_dqoJVQXg7jIsNE37G1_kkHDbjTxFtuy9ffFaW7gQ";

        static public string username = "Leonardo Da Vinki";
        static public string profileURL = "https://res.cloudinary.com/dk-find-out/image/upload/q_80,w_1920,f_auto/A-Getty-148277064_oysal9.jpg";

        public static void SendToWebHook(string msg)
        {
            HttpHelper.Post(DiscordWebhookURL, new NameValueCollection()
            {
                {"username",username },
                {"avatar_url",profileURL },
                {"content",msg }
            });
        }
    }
}
