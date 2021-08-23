using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sessionizing.Domain;

namespace Sessionizing
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //CreateHostBuilder(args).Build().Run();
            IReader reader = new CSVReader("/Users/aviv.amsellem/Downloads/");
            
            // list of IEnumerable - contains all PageViews of each of the csv files
            List<IEnumerator<PageView>> allPageViewsListsIterators = reader.ReadData();


            List<long> timeStampsList = new List<long>();
            
            for (int i = 0; i < allPageViewsListsIterators.Count; i++)
            {
                if (allPageViewsListsIterators[i].MoveNext())
                {
                    timeStampsList.Add(allPageViewsListsIterators[i].Current.timestamp);
                    allPageViewsListsIterators[i].MoveNext();
                }
                else
                {
                    allPageViewsListsIterators.RemoveAt(i);
                }
            }

            Dictionary<string, Tuple<Dictionary<string, Session>, int, List<long>>> urlSessionDictionary =
                new Dictionary<string, Tuple<Dictionary<string, Session>, int, List<long>>>();

            Dictionary<string, HashSet<string>> userIdUniqueURLVisits = new Dictionary<string, HashSet<string>>();
            
            

            while (allPageViewsListsIterators.Count != 0)
            {
                long minTimeStamp = timeStampsList.Min();
                int indexOfMinTimeStamp = timeStampsList.IndexOf(minTimeStamp);

                PageView currentPageView = allPageViewsListsIterators[indexOfMinTimeStamp].Current;

                string visitorID = currentPageView.visitor;
                
                if (!userIdUniqueURLVisits.ContainsKey(visitorID))
                {
                    userIdUniqueURLVisits[visitorID] = new HashSet<string>();
                }
                userIdUniqueURLVisits[visitorID].Add(currentPageView.mainUrl);
                
                //check of the current url exist in the dictionary
                if (urlSessionDictionary.ContainsKey(currentPageView.mainUrl))
                {
                    //check if the current visitor already have a session for the current url
                    if (urlSessionDictionary[currentPageView.mainUrl].Item1.ContainsKey(currentPageView.visitor))
                    {
                        long userLastSessionTimeStamp = urlSessionDictionary[currentPageView.mainUrl]
                            .Item1[currentPageView.visitor].endTime;

                        //check if the current page view belong to the last session (no longer than 30 minute).
                        if (currentPageView.timestamp - userLastSessionTimeStamp <= (30 * 60))
                        {
                            //update the last time stamp of the current session.
                            urlSessionDictionary[currentPageView.mainUrl]
                                .Item1[currentPageView.visitor].endTime = currentPageView.timestamp;
                        }
                        else
                        {
                            // get the start session
                            long userStartSessionTimeStamp = urlSessionDictionary[currentPageView.mainUrl]
                                .Item1[currentPageView.visitor].startTime;
                            
                            // add the session time to the sessions length list
                            List<long> sessionsLengthList = urlSessionDictionary[currentPageView.mainUrl].Item3;
                            sessionsLengthList.Add(userLastSessionTimeStamp - userStartSessionTimeStamp);
                            
                            //set the new session by the current page view.
                            Dictionary<string, Session> visitorSessionDictionary = urlSessionDictionary[currentPageView.mainUrl].Item1;
                            visitorSessionDictionary[currentPageView.visitor] = new Session(currentPageView.timestamp);
                            
                            // increment the counter of the sessions amount
                            int sessionCounter = urlSessionDictionary[currentPageView.mainUrl].Item2 + 1;
                            
                            urlSessionDictionary[currentPageView.mainUrl] = Tuple.Create(visitorSessionDictionary, sessionCounter, sessionsLengthList);
                        }
                    }
                    else // the current visitor page view its first appearance
                    {
                        // set to the user a new session
                        Dictionary<string, Session> visitorSessionDictionary = urlSessionDictionary[currentPageView.mainUrl].Item1;
                        visitorSessionDictionary[currentPageView.visitor] = new Session(currentPageView.timestamp);
                        
                        // get  the session counter and increment it
                        int sessionCounter = urlSessionDictionary[currentPageView.mainUrl].Item2 + 1;
                        
                        List<long> sessionsLengthList = urlSessionDictionary[currentPageView.mainUrl].Item3;
                        
                        urlSessionDictionary[currentPageView.mainUrl] = Tuple.Create(visitorSessionDictionary, sessionCounter, sessionsLengthList);
                    }
                }
                else //url not exist yet in the data structure -> set first session timestamp, set sessions counter to 1, initial new list for sessions lengths
                {
                    Dictionary<string, Session> visitorSessionDictionary = new Dictionary<string, Session>();
                    visitorSessionDictionary[currentPageView.visitor] = new Session(currentPageView.timestamp);
                    
                    urlSessionDictionary[currentPageView.mainUrl] = Tuple.Create(visitorSessionDictionary, 1, new List<long>());
                }

                
                if (!allPageViewsListsIterators[indexOfMinTimeStamp].MoveNext())
                {
                    allPageViewsListsIterators.RemoveAt(indexOfMinTimeStamp);
                    timeStampsList.RemoveAt(indexOfMinTimeStamp);
                }
                else
                {
                    timeStampsList[indexOfMinTimeStamp] =
                        allPageViewsListsIterators[indexOfMinTimeStamp].Current.timestamp;
                }
            }
            
            Console.WriteLine($"user id unique websites: {userIdUniqueURLVisits["visitor_9747"].Count}");
        }

        public static bool isGotNext(List<List<PageView>> allPageViewsLists, int[] IndexArray, long[] timeStampsArray)
        {
            for (int i = 0; i < allPageViewsLists.Count; i++)
            {
                if (IndexArray[i] < allPageViewsLists[i].Count)
                {
                    return true;
                }
                else if (IndexArray[i] == allPageViewsLists[i].Count)
                {
                    timeStampsArray[i] = long.MaxValue;
                }
            }

            return false;
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}