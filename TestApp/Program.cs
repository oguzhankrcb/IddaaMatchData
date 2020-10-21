using IddaaSimuService;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TestApp
{
    class Program
    {

        public static MatchData GetMatchData(string iddaaKod)
        {
            try
            {
                using (WebClient wc = new WebClient())
                {

                    wc.Encoding = Encoding.UTF8;
                    wc.Headers.Add("Referer", "http://arsiv.mackolik.com/Iddaa-Programi");
                    wc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.75 Safari/537.36");
                    wc.Headers.Add("DNT", "1");
                    wc.Headers.Add("X-Requested-With", "XMLHttpRequest");
                    wc.Headers.Add("Accept-Language", "tr-TR,tr;q=0.9,en-US;q=0.8,en;q=0.7");
                    wc.Headers.Add("Accept", "text/plain, */*; q=0.01");
                    string response = wc.DownloadString("http://arsiv.mackolik.com/AjaxHandlers/IddaaHandler.aspx?command=morebets&mac=" + iddaaKod + "&type=ByLeague");

                    MatchData matchData = new MatchData();

                    matchData = JsonConvert.DeserializeObject<MatchData>(response);

                    return matchData;
                }
            }
            catch
            {
                return null;
            }
        }


        static void Main(string[] args)
        {
            MatchData matchData = GetMatchData("3424241");

            Console.WriteLine("Match : " + matchData.Match + "\r\n");

            foreach (Market market in matchData.Event.Markets)
            {
                Console.WriteLine("----------------------" + market.Name + "----------------------\r\n");

                foreach (Outcome outcome in market.Outcomes)
                {
                    Console.WriteLine(outcome.OutcomeName + " - " + outcome.Odd + "\r\n");
                }
            }

            Console.Read();

        }
    }
}
