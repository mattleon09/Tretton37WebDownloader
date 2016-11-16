using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Threading;



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


        private static List<WebPage> pages = new List<WebPage>(); //Use this to track the parsed sites. 
        public static string startingWebUrl;


        public WebCrawler(string url)
        {
            if (url != null && url != string.Empty)
            {
                startingWebUrl = url; //Set the web url property.  
            }
                   
        }

        /// <summary>
        /// Call ParsePage for the initial home page.
        /// </summary>
        public void TraverseTretton37sSite()
        {   
            Console.WriteLine("Beginning traversal...");
            ParsePage(startingWebUrl);
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

            if (!PageHasBeenCrawled(urlPath))
            {
                Console.WriteLine("Begin parsing page {0} \n", urlPath);
              
                string urlContent = GetWebString(urlPath);
               
                WebPage wPage = new WebPage();
                wPage.HtmlContent = urlContent;
                wPage.PageUrl = urlPath;

                pages.Add(wPage);


                if(wPage.HtmlContent != null) {

                
                    AnchorParser linkParser = new AnchorParser();
                    linkParser.Parse(wPage);

                    ParseResourceFilesAndLinks(wPage);

                    if (linkParser.ResourceLinks.Count > 0)
                    {
                        Console.WriteLine("Crawling all of the internal links found on the page: {0}... \n", urlPath);                
                        Parallel.ForEach(linkParser.ResourceLinks, (currentInternalLink) =>
                        {

                            if (currentInternalLink != null && currentInternalLink != string.Empty)
                                ParsePage(currentInternalLink);       
                        });
                    }

                      
                }
            }

        }

        /// <summary>
        /// Concurrently run the three resource parse and download functions.
        /// I separated the three "parse" code snippets into different methods to be able to run the three steps concurrently.
        /// </summary>
        /// <param name="wPage"></param>
        private void ParseResourceFilesAndLinks(WebPage wPage)
        {
            if (wPage != null && wPage.PageUrl != null)
            {         
                    Parallel.Invoke(() => ParseAndDownloadImages(wPage),//,  //Get the images
                 () => ParseAndDownloadStyleFiles(wPage), //Get the style files and fonts
                 () => ParseAndDowloadScriptFiles(wPage));   //Get the script files
              
            }      
        }

        private string GetWebString(string Url)
        {

            Thread.Sleep(1000);
            string filePath = string.Empty;
            string pageName = string.Empty;
            if (Url.IndexOf(HTTP_PREFIX) > -1 || Url.IndexOf(HTTPS_PREFIX) > -1)
                if (Url != WebCrawler.startingWebUrl)
                    if (Url.Contains(HTML_EXTENSION))
                    {
                        pageName = Url.Substring(Url.LastIndexOf(FORWARD_SLASH_STR) + 1, Url.Length - Url.LastIndexOf(FORWARD_SLASH_STR) - 1);//Url.
                    }
                    else
                    {
                        pageName = Url.Substring(Url.LastIndexOf(FORWARD_SLASH_STR) + 1, Url.Length - Url.LastIndexOf(FORWARD_SLASH_STR) - 1) + HTML_EXTENSION;//Url.
                    }
                else
                    pageName = MAIN_PAGE_NAME;

            else
            {
                pageName = Url.Replace("\"", string.Empty);
                pageName = pageName.Replace(FORWARD_SLASH_STR, string.Empty);
                if (!(pageName.Split('.').Count() > 1))
                    pageName = pageName + HTML_EXTENSION;
                Url = startingWebUrl + Url;
            }

            if (pageName == HTML_EXTENSION)
                pageName = MAIN_PAGE_NAME;

            foreach (WebPage page in pages)
            {
                if (page.PageUrl == Url)
                    return null;
            }

            string strResponse = string.Empty;

            try
            {
                HttpWebRequest wrWebRequest = WebRequest.Create(Url) as HttpWebRequest;
                HttpWebResponse wrWebResponse = (HttpWebResponse)wrWebRequest.GetResponse(); 
                StreamReader srResponse;

                using (srResponse = new StreamReader(wrWebResponse.GetResponseStream()))
                {
                    strResponse = srResponse.ReadToEnd(); //srResponse.ReadToEndAsync();

                    if (!File.Exists(TRETTON37_ROOT_FILESYSTEM_PATH + pageName))
                    {
                        File.WriteAllText(TRETTON37_ROOT_FILESYSTEM_PATH + pageName, strResponse);
                    }
                    // File.Delete(TRETTON37_ROOT_FILESYSTEM_PATH + pageName);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occured while trying to get data from {0}", Url, e.Message);
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
            foreach (WebPage page in pages)
            {
                if (page.PageUrl == url)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Download the file from the  passed in resource url. 
        /// </summary>
        /// <param name="resourceUrl"></param>
        public static void DownloadFile(string resourceUrl)
        {
            //Ignore cdn links. 
            if (resourceUrl != string.Empty && !resourceUrl.Contains(CDN_IDENTIFIER))
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

                       //Download the file using WebClient
                       WebClient wc = new WebClient();
                       wc.DownloadFile(resourceUrl, path);
                       Thread.Sleep(1000);
                        }
                   }
               }        
            }
        }

        /// <summary>
        /// Parse all of the '<script>' elements that have a .js file in them.  
        /// </summary>
        /// <param name="wPage"></param>
        private void ParseAndDowloadScriptFiles(WebPage wPage)
        {
          //  Console.WriteLine("Parsing all of the script elements with 'JS' files paths...");
            ScriptParser scriptParser = new ScriptParser();
            scriptParser.Parse(wPage);
           

            if (scriptParser.ResourceLinks.Count > 0)
            {
                if (wPage.PageUrl.Contains("flickr"))
                {
                    Console.WriteLine("NO");
                }
              //  Console.WriteLine("Downloading script files for page {0}", wPage.PageUrl);
                scriptParser.DownloadResources(scriptParser.ResourceLinks);
                
            }
        }

        /// <summary>
        /// Parse the css files on the page along with the urls links within the css code. Then download the associated resources. 
        /// </summary>
        /// <param name="wPage"></param>
        private void ParseAndDownloadStyleFiles(WebPage wPage)
        {
         //   Console.WriteLine("Parsing all of the css style links...");
            StyleParser cssParser = new StyleParser();
             cssParser.Parse(wPage);
           

            if (cssParser.ResourceLinks.Count > 0)
            {
           //  Console.WriteLine("Downloading css files for page {0}", wPage.PageUrl);
             cssParser.DownloadResources(cssParser.ResourceLinks);
            cssParser.DownloadResources(cssParser.InnerStyleLinks);
          
            }

        
        }

        /// <summary>
        /// Parse the img elements on the page and download the image files locally. 
        /// </summary>
        /// <param name="wPage"></param>
        private void ParseAndDownloadImages(WebPage wPage)
        {
         //   Console.WriteLine("Parsing all of the image elements for their source urls...");
            ImageParser imageParser = new ImageParser();
            imageParser.Parse(wPage);
          

            if (imageParser.ResourceLinks.Count > 0)
            {
           //     Console.WriteLine("Downloading image files for page {0}", wPage.PageUrl);
                imageParser.DownloadResources(imageParser.ResourceLinks);
              
            }
        }
    }
}
