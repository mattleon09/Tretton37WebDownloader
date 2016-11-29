using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.IO.Compression;



namespace Tretton37WebDownloader
{
        public class WebCrawler
    {
        public const string TRETTON37_ROOT_FILESYSTEM_PATH = @"C:\tretton37\";
        public const string CDN_IDENTIFIER = "cdn";
        public const string SRC_HTML_ATTRIBUTE = "src";
        public const char FORWARD_SLASH_CHAR = '/';
        public const string FORWARD_SLASH_STR = "/";
        public const string HTML_EXTENSION = ".html";
        public const string HTTP_PREFIX = "http://";
        public const string HTTPS_PREFIX = "https://";
        public const string TELEPHONE_LINKS = "tel:";
        public const string MAILTO_LINKS = "mailto:";
        public const string DOT_EXTENSION_SEPARATOR = ".";
        public const string HREF_HTML_ATTRIBUTE = "href";
        public const string REL_HTML_ATTRIBUTE = "rel";
        public const string CSS_URL_PROPERTY = "url";
        public const string MAIN_PAGE_NAME = "index.html";
        public const string STYLESHEET_REL_ATTR_VALUE = "stylesheet";
        public const char POUND_SYMBOL_CHAR = '#'; //This could very well be part of libraries like Knockout or React. But I have never seen it before. Ex who-we-are#{another name here}


       // private static List<WebPage> pages = new List<WebPage>(); //Use this to track the parsed sites. 
        private static List<string> crawledPages = new List<string>();//Use this to track the parsed sites. 
        public static string startingWebUrl;


        public static List<string> globalDownloadedList = new List<string>(); //Use this to track the already downloaded resources. 


        public WebCrawler(string url)
        {
            if (url != null && url != string.Empty)         
                startingWebUrl = url; //Set the  initial web url property (root page).  
           
                   
        }

        /// <summary>
        /// Call ParsePage for the initial home page.
        /// </summary>
        public void TraverseTretton37sSite() // I could have just as easily used a parameter instead of the startingWebUrl property.
        {   
            Console.WriteLine("Beginning traversal...");
            ParsePage(startingWebUrl); //
            Console.WriteLine("Done traversing the site...");
        }


        /// <summary>
        /// This is where most of the action occurs. 
        /// It gets the html content of the current page and starts searching for all the internal links. While ignoring external ones. 
        /// Then calls a function to parse the resources on the current page. 
        /// Finally, it starts crawling the found links and goes down the ninja hole. 
        /// </summary>
        /// <param name="urlPath"></param>
        private void ParsePage(string urlPath)
        {

            if (!PageHasBeenCrawled(urlPath)) //Check if the page has already been parsed. 
            {
                if (!crawledPages.Contains(urlPath)) //Keep track of the pages that are already being iterated over. To prevent repeated actions. Irony. 
                    crawledPages.Add(urlPath);

                Console.WriteLine("Begin parsing page {0} \n", urlPath);
              
                //This should be wrapped in a try catch to catch any exceptions that occur within the GetWebString method. 
                string urlContent = GetWebString(urlPath); //Get the html contents of the current page/url.


                //Note: I got rid of the WebPage object. I thought it unnecessary due to everything I need like the "urlContent" and "urlPath" being centralized. 
           
                if(urlContent != null) {
                    AnchorParser linkParser = new AnchorParser();//Instantiate a new AnchorParser object. 
                    linkParser.Parse(urlContent); //Call Parse to capture all of the internal links on the current page. 


                    ParseResourceFilesAndLinks(urlContent); 
                   

                    if (linkParser.ResourceLinks.Count > 0)
                    {
                        //Get a copy of the resource/internal links of the current page. 
                        var copyGlobal = linkParser.ResourceLinks.ToList(); 
                        var diff = linkParser.ResourceLinks.Except(crawledPages).ToList();//Get the links that haven't been parsed/crawled yet. 
                        if (diff.Count > 0)
                        {       
                            Console.WriteLine("Crawling all of the internal links found on the page: {0}... \n", urlPath);
                            //Concurrently crawl all of the internal links on this current page, recursively calling ParsePage in the process.
                            Parallel.ForEach(diff, (currentInternalLink) =>
                            {
                                if (currentInternalLink != null && currentInternalLink != string.Empty)
                                    ParsePage(currentInternalLink);                   
                            });
                        }
                    
                    }

                      
                }
            }

        }

        /// <summary>
        /// Concurrently run the three resource parse and download functions.
        /// I separated the three "parse" code snippets into different methods to be able to run the three steps concurrently.
        /// </summary>
        /// <param name="wPage"></param>
        private void ParseResourceFilesAndLinks(string htmlContent)
        {
            if (htmlContent != null )
            {         
                //Concurrently execute all of three of the resource methods that parse and download the files. 
                Parallel.Invoke(() => ParseAndDownloadImages(htmlContent),//,  //Get the images
                 () => ParseAndDownloadStyleFiles(htmlContent), //Get the style files and fonts
                 () => ParseAndDowloadScriptFiles(htmlContent));   //Get the script files
              
            }      
        }

            /// <summary>
            /// Function that gets the HTML content from the passed in URL if possible. 
            /// Then, saves the page as an html file on the disk. 
            /// Finally, it returns that HTML data.
            /// </summary>
            /// <param name="Url"></param>
            /// <returns></returns>
        private string GetWebString(string Url)
        {

            Thread.Sleep(500);// Not sure why I had this. 
           // string filePath = string.Empty; //No longer used anymore
            string pageHtmlName = string.Empty;
            string strResponse = string.Empty;
          
            Uri uri = new Uri(Url); //Get a uri object from the url. 
            if (uri != null) 
            {

                //Get the last segment which is usually the page name. I wonder if I could take care of this in a better way.
                var currentPage = uri.Segments.Last().Replace("/", ""); 

                //If the current page is not empty, then try to use the name and add a .html extension to it. 
                if (currentPage != string.Empty)
                    if (Url.Contains(HTML_EXTENSION)) //If the page name already contains .html then just use that. 
                        pageHtmlName = currentPage;
                    else
                        pageHtmlName = currentPage + HTML_EXTENSION; //else, add it on.
                else
                    pageHtmlName = MAIN_PAGE_NAME; //If it is empty, then it must be the main url/page. 
                try
                {

                    HttpWebRequest wrWebRequest = WebRequest.Create(Url) as HttpWebRequest;
                    HttpWebResponse wrWebResponse = (HttpWebResponse)wrWebRequest.GetResponse();
                    StreamReader srResponse;

                    using (srResponse = new StreamReader(wrWebResponse.GetResponseStream()))
                    {
                        //Get the contents of the page as string. I thought about using the async methods. But this code will most likley be running on a separate thread anyway. 
                        //I didn't think it would be wise to use async/await on something that wasn't holding up some sort of UI thread. 
                        strResponse = srResponse.ReadToEnd();

                        //Save the page as a html file on disk on a new thread.
                        //I did this because the main work wasn't already done. 
                        //I figured I could let any filesystem interaction run on another thread while the rest of the code continues on. 
                        new Thread(() =>
                              DownloadHtmlPage(TRETTON37_ROOT_FILESYSTEM_PATH + pageHtmlName, strResponse)  
                             
                        ).Start();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occured while trying to get data from {0}. Exception:  {1}", Url, e.Message);
                }
            }
          
          
            return strResponse;
        }


        /// <summary>
        /// Determin if the passed in string is a web page or a resource path.
        /// </summary>
        /// <param name="anchorElm"></param>
        /// <returns></returns>
        public static bool IsWebPage(string anchorElm)
        {
            if (anchorElm.IndexOf("javascript:") == 0)
                return false;
            string urlAssetExt = anchorElm.Substring(anchorElm.LastIndexOf(DOT_EXTENSION_SEPARATOR) + 1, anchorElm.Length - anchorElm.LastIndexOf(DOT_EXTENSION_SEPARATOR) - 1);
            switch (urlAssetExt)
            {
                case "jpg":
                case "png":
                case "ico":
                case "css":
                case "woff":
                case "eot":
                    return false;
            }
            if (urlAssetExt.Contains("css")) //I looked for this because the "css" indicator might not be the "extension" in the string. Ex. 'css?f34793c9-17e6-439f-8809-7009f2b25de5' in  main.css?f34793c9-17e6-439f-8809-7009f2b25de5"
                return false; //I wonder if I could do this a better way. 

            return true;
        }

        /// <summary>
        /// Determine if the passed in url belongs to a page that has already been crawled through. 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static bool PageHasBeenCrawled(string url)
        {
            foreach (string pageUrl in crawledPages)
            {
                if (pageUrl == url) //If the current iteration's url matches one in the pages collection, then the page has already been parsed. 
                    return true;
            }

            return false;//Page hasn't been parsed yet. 
        }

        /// <summary>
        /// Download the file from the  passed in resource url. 
        /// </summary>
        /// <param name="resourceUrl"></param>
        public static void DownloadResourceFile(string resourceUrl)
        {
         
            //Ignore cdn links. 
            if (resourceUrl != null &&resourceUrl != string.Empty && !resourceUrl.Contains(CDN_IDENTIFIER))
            {
                string path = WebCrawler.TRETTON37_ROOT_FILESYSTEM_PATH;

                //Get each folder/dir and sub-dur from the resource url.
                Uri uri = new Uri(resourceUrl);
                var seg = uri.Segments;

                var domainname = uri.GetComponents(UriComponents.Host, UriFormat.Unescaped);

                //Make sure that the url is within the same domain as the root site. Otherwise, don't bother with downloading it locally. 
               if (domainname == new Uri(startingWebUrl).GetComponents(UriComponents.Host,UriFormat.Unescaped))
               {
                   //Build the path. 
                   for (int i = 0; i < seg.Length; i++)
                   {
                       string segment = seg[i];
                       segment = segment.Replace(FORWARD_SLASH_STR, @"\");
                       if (segment != string.Empty)
                       {
                           path = path + segment;
                       }
                   }

                   if (path != string.Empty)
                   {
                       //Create the dir if it does not exist. 
                       var folderPath = Path.GetDirectoryName(path);
                       if (!Directory.Exists(folderPath))
                           Directory.CreateDirectory(folderPath);


                       //Delete the file if it exists. This allow us to get a fresh copy from the site. 
                       if (!File.Exists(path)) { 
                          // File.Delete(path);

                       //Download the file 
                           using (var wc = new WebClient())
                           {
                               wc.Proxy = WebRequest.DefaultWebProxy;
                               byte[] data = wc.DownloadData(resourceUrl);
                               File.WriteAllBytes(path, data);
                           }
                       Thread.Sleep(10);
                        }
                   }
               }        
            }
        }

     

        public static void DownloadHtmlPage(string path, string response)
        {
            //If the file does not exist, then save it to disk. 
            if (!File.Exists(path))
                File.WriteAllText(path, response);
       
             // File.Delete(TRETTON37_ROOT_FILESYSTEM_PATH + pageName);
                
        }

        /// <summary>
        /// Parse all of the '<script>' elements that have a .js file in them.  
        /// </summary>
        /// <param name="wPage"></param>
        private void ParseAndDowloadScriptFiles(string htmlContent)
        {
              ScriptParser scriptParser = new ScriptParser();
              scriptParser.Parse(htmlContent);
           

            if (scriptParser.ResourceLinks.Count > 0)
            {

                //Only download resources that haven't been downloaded yet. 
             var diff = RemainingResources(globalDownloadedList, scriptParser.ResourceLinks); //scriptParser.ResourceLinks.Except(copyGlobal).ToList();
                if (diff.Count > 0)
                {
                    new Thread(() =>
                    scriptParser.DownloadResources(diff)
                    ).Start();
                }
               
                
            }
          
        }

        /// <summary>
        /// Parse the css files on the page along with the urls links within the css code. Then download the associated resources. 
        /// </summary>
        /// <param name="wPage"></param>
        private void ParseAndDownloadStyleFiles(string htmlContent)
        {
            StyleParser cssParser = new StyleParser();
            cssParser.Parse(htmlContent);
         
            if (cssParser.ResourceLinks.Count > 0)
            {  //Only download resources that haven't been downloaded yet. 
                var diff = RemainingResources(cssParser.ResourceLinks, cssParser.InnerStyleLinks);
                if (diff.Count > 0)
                {
                    new Thread(() =>
                                 cssParser.DownloadResources(diff)
                           ).Start();

                }
             //cssParser.DownloadResources(cssParser.ResourceLinks);          
             //cssParser.DownloadResources(cssParser.InnerStyleLinks);        
            }

            if (cssParser.InnerStyleLinks.Count > 0)
            {  //Only download resources that haven't been downloaded yet. 
              
                var diff = RemainingResources(globalDownloadedList,cssParser.InnerStyleLinks);
                if (diff.Count() > 0)
                {
                    new Thread(() =>

                        cssParser.DownloadResources(diff)
                        
                   ).Start();


                }
                               
            }
        }

        public static List<string> RemainingResources(List<string> downloadedList, List<string> resourceList) {
             var copyGlobal = downloadedList.ToList();
             var diff = resourceList.Except(copyGlobal).ToList();
             return diff;
        }

        /// <summary>
        /// Parse the img elements on the page and download the image files locally. 
        /// </summary>
        /// <param name="wPage"></param>
        private void ParseAndDownloadImages(string htmlContent)
        {
           
            ImageParser imageParser = new ImageParser();
            imageParser.Parse(htmlContent);
          

            if (imageParser.ResourceLinks.Count > 0)
            {  //Only download resources that haven't been downloaded yet. 

                var diff = RemainingResources(globalDownloadedList, imageParser.ResourceLinks);//imageParser.ResourceLinks.Except(copyGlobal).ToList();

                if (diff.Count > 0)
                {
                    new Thread(() =>

                       imageParser.DownloadResources(diff)

                  ).Start();
                }
               // imageParser.DownloadResources(imageParser.ResourceLinks);
              
            }      
        }          
    }
}
