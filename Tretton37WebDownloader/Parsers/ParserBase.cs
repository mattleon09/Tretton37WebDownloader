using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net;
using System.Linq;

namespace Tretton37WebDownloader
{
    abstract class ParserBase
    {

        public HtmlDocument _doc = new HtmlDocument();
        public Uri _baseUri = new Uri(WebCrawler.startingWebUrl);


        /// <summary>
        /// Default functionality for the DownloadResources method. 
        /// </summary>
        /// <param name="ResourceUrls"></param>
       public void DownloadResources(List<string> ResourceUrls)
        {
          
           Parallel.ForEach(ResourceUrls ,(currentResourceUrl) => {
               try
               {
                  
                   WebCrawler.DownloadResourceFile(currentResourceUrl);
               }
               catch (Exception e)
               {
                   Console.WriteLine("Error downloading resource: {0} \n", currentResourceUrl);
               }
    
           });

           var diff = ResourceUrls.Except(WebCrawler.globalDownloadedList).ToList();
           if (diff.Count > 0)
               WebCrawler.globalDownloadedList.AddRange(diff);
        }
    }
}
