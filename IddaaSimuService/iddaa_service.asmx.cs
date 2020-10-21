using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;

namespace IddaaSimuService
{
    /// <summary>
    /// Summary description for iddaa_service
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class iddaa_service : System.Web.Services.WebService
    {

        iddaasim_dbEntities iddaaDB;
        
        public string GetDate()
        {
            return (DateTime.Now.ToShortDateString().Replace(".", "/"));
        }

        public MatchData GetMatchData(string iddaaKod)
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

        public List<Tuple<string, double>> GetOutcomes(List<Outcome> Outcomes)
        {
            List<Tuple<string, double>> retlist = new List<Tuple<string, double>>();

            foreach (Outcome outcome in Outcomes)
            {
                Tuple<string, double> tuple = new Tuple<string, double>(outcome.OutcomeName, outcome.Odd) ;

                retlist.Add(tuple);

                //Console.WriteLine(outcome.OutcomeName + " - " + outcome.Odd + "\r\n");
            }

            return retlist;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void GetPlayableMatches(string date)
        {
            using (WebClient wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                string json_str = wc.DownloadString("http://goapi.mackolik.com/livedata?date=" + date);

                UnclearedMatches m = new UnclearedMatches();

                m = JsonConvert.DeserializeObject<UnclearedMatches>(json_str);


                ArrayList matchesarraylist = new ArrayList();


                for (int i = 0; i < m.m.Count; i++)
                {
                    matchesarraylist.Add(m.m[i]);
                }

           
                foreach (List<object> obj in matchesarraylist)
                {
                    string MS = obj[6].ToString();
                    string iddaaKodu = obj[0].ToString();
                    string matchcode = obj[14].ToString();
                    string evSahibi = obj[2].ToString();
                    string konukEkip = obj[4].ToString();
                    string startTime = obj[16].ToString();


                    TimeSpan ts = TimeSpan.Parse(startTime);
                    TimeSpan ts2 = DateTime.Now.TimeOfDay;


                    if (MS == "MS" || matchcode == "0")
                    {

                    }
                    else
                    {
                        if (TimeSpan.Compare(ts, ts2) == 1)
                        {
                            MatchData matchData = GetMatchData(iddaaKodu);

                            if (matchData == null)
                                continue;

                            using (iddaaDB = new iddaasim_dbEntities())
                            {
                                long iddaaKodLong = Convert.ToInt64(iddaaKodu);
                                match findmatch = iddaaDB.matches.Where(m => m.iddaakod == iddaaKodLong).FirstOrDefault();

                                if (findmatch == null)
                                {
                                    match newmatch = new match();

                                    newmatch.iddaakod = Convert.ToInt64(iddaaKodu);
                                    newmatch.startDate = DateTime.Parse(date);
                                    newmatch.startTime = ts;
                                    newmatch.evsahibi = evSahibi;
                                    newmatch.konuk = konukEkip;


                                    foreach (Market market in matchData.Event.Markets)
                                    {
                                        Console.WriteLine("----------------------" + market.Name + "----------------------\r\n");

                                        if (market.Name == "Maç Sonucu")
                                        {
                                            List<Tuple<string, double>> getOutcomes = GetOutcomes(market.Outcomes);

                                            foreach(Tuple<string, double> t in getOutcomes)
                                            {
                                                if (t.Item1 == "1")
                                                    newmatch.ms1 = t.Item2;
                                                else if (t.Item1 == "X")
                                                    newmatch.msx = t.Item2;
                                                else if (t.Item1 == "2")
                                                    newmatch.ms2 = t.Item2;
                                            }
                                        }
                                        else if (market.Name == "Çifte Şans")
                                        {
                                            List<Tuple<string, double>> getOutcomes = GetOutcomes(market.Outcomes);

                                            foreach (Tuple<string, double> t in getOutcomes)
                                            {
                                                if (t.Item1 == "1-X")
                                                    newmatch.cifte1x = t.Item2;
                                                else if (t.Item1 == "1-2")
                                                    newmatch.cifte12 = t.Item2;
                                                else if (t.Item1 == "X-2")
                                                    newmatch.cifte2x = t.Item2;
                                            }
                                        }
                                        else if (market.Name == "0,5 Alt/Üst")
                                        {
                                            List<Tuple<string, double>> getOutcomes = GetOutcomes(market.Outcomes);

                                            foreach (Tuple<string, double> t in getOutcomes)
                                            {
                                                if (t.Item1 == "Alt")
                                                    newmatch.alt05 = t.Item2;
                                                else if (t.Item1 == "Üst")
                                                    newmatch.ust05 = t.Item2;
                                            }
                                        }
                                        else if (market.Name == "1,5 Alt/Üst")
                                        {
                                            List<Tuple<string, double>> getOutcomes = GetOutcomes(market.Outcomes);

                                            foreach (Tuple<string, double> t in getOutcomes)
                                            {
                                                if (t.Item1 == "Alt")
                                                    newmatch.alt15 = t.Item2;
                                                else if (t.Item1 == "Üst")
                                                    newmatch.ust15 = t.Item2;
                                            }
                                        }
                                        else if (market.Name == "2,5 Alt/Üst")
                                        {
                                            List<Tuple<string, double>> getOutcomes = GetOutcomes(market.Outcomes);

                                            foreach (Tuple<string, double> t in getOutcomes)
                                            {
                                                if (t.Item1 == "Alt")
                                                    newmatch.alt25 = t.Item2;
                                                else if (t.Item1 == "Üst")
                                                    newmatch.ust25 = t.Item2;
                                            }
                                        }
                                        else if (market.Name == "3,5 Alt/Üst")
                                        {
                                            List<Tuple<string, double>> getOutcomes = GetOutcomes(market.Outcomes);

                                            foreach (Tuple<string, double> t in getOutcomes)
                                            {
                                                if (t.Item1 == "Alt")
                                                    newmatch.alt35 = t.Item2;
                                                else if (t.Item1 == "Üst")
                                                    newmatch.ust35 = t.Item2;
                                            }
                                        }
                                        else if (market.Name == "4,5 Alt/Üst")
                                        {
                                            List<Tuple<string, double>> getOutcomes = GetOutcomes(market.Outcomes);

                                            foreach (Tuple<string, double> t in getOutcomes)
                                            {
                                                if (t.Item1 == "Alt")
                                                    newmatch.alt45 = t.Item2;
                                                else if (t.Item1 == "Üst")
                                                    newmatch.ust45 = t.Item2;
                                            }
                                        }
                                        else if (market.Name == "Karşılıklı Gol")
                                        {
                                            List<Tuple<string, double>> getOutcomes = GetOutcomes(market.Outcomes);

                                            foreach (Tuple<string, double> t in getOutcomes)
                                            {
                                                if (t.Item1 == "Var")
                                                    newmatch.kgvar = t.Item2;
                                                else if (t.Item1 == "Yok")
                                                    newmatch.kgyok = t.Item2;
                                            }
                                        }
                                        else if (market.Name == "Toplam Gol Aralığı")
                                        {
                                            List<Tuple<string, double>> getOutcomes = GetOutcomes(market.Outcomes);

                                            foreach (Tuple<string, double> t in getOutcomes)
                                            {
                                                if (t.Item1 == "0-1 Gol")
                                                    newmatch.tgol01 = t.Item2;
                                                else if (t.Item1 == "2-3 Gol")
                                                    newmatch.tgol23 = t.Item2;
                                                else if (t.Item1 == "4-5 Gol")
                                                    newmatch.tgol45 = t.Item2;
                                                else if (t.Item1 == "6+ Gol")
                                                    newmatch.tgol6arti = t.Item2;
                                            }
                                        }
                                        else if (market.Name == "1. Yarı Sonucu")
                                        {
                                            List<Tuple<string, double>> getOutcomes = GetOutcomes(market.Outcomes);

                                            foreach (Tuple<string, double> t in getOutcomes)
                                            {
                                                if (t.Item1 == "1")
                                                    newmatch.ilkyari1 = t.Item2;
                                                else if (t.Item1 == "X")
                                                    newmatch.ilkyarix = t.Item2;
                                                else if (t.Item1 == "2")
                                                    newmatch.ilkyari2 = t.Item2;
                                            }
                                        }
                                        else if (market.Name == "2. Yarı Sonucu")
                                        {
                                            List<Tuple<string, double>> getOutcomes = GetOutcomes(market.Outcomes);

                                            foreach (Tuple<string, double> t in getOutcomes)
                                            {
                                                if (t.Item1 == "1")
                                                    newmatch.ikinciyari1 = t.Item2;
                                                else if (t.Item1 == "X")
                                                    newmatch.ikinciyarix = t.Item2;
                                                else if (t.Item1 == "2")
                                                    newmatch.ikinciyari2 = t.Item2;
                                            }
                                        }
                                        else if (market.Name == "İlk Korner")
                                        {
                                            List<Tuple<string, double>> getOutcomes = GetOutcomes(market.Outcomes);

                                            foreach (Tuple<string, double> t in getOutcomes)
                                            {
                                                if (t.Item1 == "1")
                                                    newmatch.ilkkorner1 = t.Item2;
                                                else if (t.Item1 == "Olmaz")
                                                    newmatch.ilkkorner0 = t.Item2;
                                                else if (t.Item1 == "2")
                                                    newmatch.ilkkorner2 = t.Item2;
                                            }
                                        }
                                        else if (market.Name == "İlk Gol")
                                        {
                                            List<Tuple<string, double>> getOutcomes = GetOutcomes(market.Outcomes);

                                            foreach (Tuple<string, double> t in getOutcomes)
                                            {
                                                if (t.Item1 == "1")
                                                    newmatch.ilkg1 = t.Item2;
                                                else if (t.Item1 == "Olmaz")
                                                    newmatch.ilkgolmaz = t.Item2;
                                                else if (t.Item1 == "2")
                                                    newmatch.ilkg2 = t.Item2;
                                            }
                                        }
                                        else if (market.Name == "Kırmızı Kart")
                                        {
                                            List<Tuple<string, double>> getOutcomes = GetOutcomes(market.Outcomes);

                                            foreach (Tuple<string, double> t in getOutcomes)
                                            {
                                                if (t.Item1 == "Var")
                                                    newmatch.kirmizikartvar = t.Item2;
                                                else if (t.Item1 == "Yok")
                                                    newmatch.kirmizikartyok = t.Item2;
                                            }
                                        }
                                        else if (market.Name == "Maç Skoru")
                                        {
                                            List<Tuple<string, double>> getOutcomes = GetOutcomes(market.Outcomes);

                                            foreach (Tuple<string, double> t in getOutcomes)
                                            {
                                                if (t.Item1 == "1-0")
                                                    newmatch.mskor10 = t.Item2;
                                                else if (t.Item1 == "2-0")
                                                    newmatch.mskor20 = t.Item2;
                                                else if (t.Item1 == "2-1")
                                                    newmatch.mskor21 = t.Item2;
                                                else if (t.Item1 == "3-0")
                                                    newmatch.mskor30 = t.Item2;
                                                else if (t.Item1 == "3-1")
                                                    newmatch.mskor31 = t.Item2;
                                                else if (t.Item1 == "3-2")
                                                    newmatch.mskor32 = t.Item2;
                                                else if (t.Item1 == "4-0")
                                                    newmatch.mskor40 = t.Item2;
                                                else if (t.Item1 == "4-1")
                                                    newmatch.mskor41 = t.Item2;
                                                else if (t.Item1 == "4-2")
                                                    newmatch.mskor42 = t.Item2;
                                                else if (t.Item1 == "5-0")
                                                    newmatch.mskor50 = t.Item2;
                                                else if (t.Item1 == "6-0")
                                                    newmatch.mskor60 = t.Item2;
                                                else if (t.Item1 == "5-1")
                                                    newmatch.mskor51 = t.Item2;
                                                else if (t.Item1 == "0-0")
                                                    newmatch.mskor00 = t.Item2;
                                                else if (t.Item1 == "1-1")
                                                    newmatch.mskor11 = t.Item2;
                                                else if (t.Item1 == "2-2")
                                                    newmatch.mskor22 = t.Item2;
                                                else if (t.Item1 == "3-3")
                                                    newmatch.mskor33 = t.Item2;
                                                else if (t.Item1 == "0-1")
                                                    newmatch.mskor01 = t.Item2;
                                                else if (t.Item1 == "0-2")
                                                    newmatch.mskor02 = t.Item2;
                                                else if (t.Item1 == "1-2")
                                                    newmatch.mskor12 = t.Item2;
                                                else if (t.Item1 == "0-3")
                                                    newmatch.mskor03 = t.Item2;
                                                else if (t.Item1 == "1-3")
                                                    newmatch.mskor13 = t.Item2;
                                                else if (t.Item1 == "2-3")
                                                    newmatch.mskor23 = t.Item2;
                                                else if (t.Item1 == "0-4")
                                                    newmatch.mskor04 = t.Item2;
                                                else if (t.Item1 == "1-4")
                                                    newmatch.mskor14 = t.Item2;
                                                else if (t.Item1 == "2-4")
                                                    newmatch.mskor24 = t.Item2;
                                                else if (t.Item1 == "0-5")
                                                    newmatch.mskor05 = t.Item2;
                                                else if (t.Item1 == "1-5")
                                                    newmatch.mskor15 = t.Item2;
                                                else if (t.Item1 == "0-6")
                                                    newmatch.mskor06 = t.Item2;
                                                else if (t.Item1 == "Diğer")
                                                    newmatch.mskorothers = t.Item2;
                                            }
                                        }


                                    }


                                    iddaaDB.matches.Add(newmatch);
                                    iddaaDB.SaveChanges();


                                }


                            }
                        }
                        
                    }

                }


            }
                


        }
    }
}
