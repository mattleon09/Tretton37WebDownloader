using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;


namespace Tretton37WebDownloader
{
    class StyleParser : ParserBase, IParser
    {

        private const string SCRIPT_XPATH = "//link/@href";

        private List<string> _styleLinks = new List<string>();
        private List<string> _innerStyleLinks = new List<string>();

        public List<string> InnerStyleLinks
        {
            get { return _innerStyleLinks; }
            set { _innerStyleLinks = value; }
        }

        public List<string> ResourceLinks
        {
            get { return _styleLinks; }
            set { _styleLinks = value; }
        }

        public StyleParser()
        {

        }

        public void Parse(string htmlContent)
        {
            HtmlNodeCollection styleFiles = null;
            _doc.LoadHtml(htmlContent);
            try
            {
                 styleFiles = _doc.DocumentNode.SelectNodes(SCRIPT_XPATH);
                 if (styleFiles != null)
                 {
                     foreach( var styleFile in styleFiles)
                     {
                         if (styleFile != null)
                         {
                             if (styleFile.Attributes[WebCrawler.REL_HTML_ATTRIBUTE] != null && styleFile.Attributes[WebCrawler.REL_HTML_ATTRIBUTE].Value == WebCrawler.STYLESHEET_REL_ATTR_VALUE) //Only get link elements that contain stylesheet references. 
                             {
                                 var href = styleFile.Attributes[WebCrawler.HREF_HTML_ATTRIBUTE].Value;
                                 if (href != null && href != string.Empty)
                                 {
                                     var path = new Uri(_baseUri, href).AbsoluteUri;
                                     if (!_styleLinks.Contains(path) && !WebCrawler.IsWebPage(path))
                                     {
                                         _styleLinks.Add(path);
                                         foreach (var csslink in _styleLinks)
                                         {
                                             if (csslink != null && csslink != string.Empty)
                                             {
                                                 WebClient wc = new WebClient();
                                                 var css = wc.DownloadString(csslink);
                                                 //Find "url" or "import"
                                                 Regex cssUrls = new Regex(@"(url|@import)\((?<char>['""])?(?<url>.*?)\k<char>?\)", RegexOptions.IgnoreCase); //Get all url properties in the css. 

                                                 foreach (Match match in cssUrls.Matches(css))
                                                 {
                                                     if (match != null)
                                                     {
                                                         var extractedUrl = match.Groups[WebCrawler.CSS_URL_PROPERTY].Value;
                                                         if (extractedUrl != null && extractedUrl != string.Empty)
                                                         {
                                                             var innerStylePath = new Uri(_baseUri, extractedUrl).AbsoluteUri;
                                                             if (!_innerStyleLinks.Contains(innerStylePath) && !WebCrawler.IsWebPage(innerStylePath))
                                                             {
                                                                 _innerStyleLinks.Add(innerStylePath);
                                                             }
                                                         }                                                       
                                                     }
                                                    
                                                 }
                                             }
                                         }
                                     }
                                 }                    
                             }
                         }
                      
                         Thread.Sleep(500);
                     };
                 }
               
                //foreach (var item in scriptfiles)
                //{
                //    if (item.Attributes[WebCrawler.REL_HTML_ATTRIBUTE] != null && item.Attributes[WebCrawler.REL_HTML_ATTRIBUTE].Value == WebCrawler.STYLESHEET_REL_ATTR_VALUE) //Only get link elements that contain stylesheet references. 
                //    { 
                //    var href = item.Attributes[WebCrawler.HREF_HTML_ATTRIBUTE].Value;
                //    var path = new Uri(_baseUri, href).AbsoluteUri;
                //    if (!_styleLinks.Contains(path) && !WebCrawler.IsWebPage(path))
                //    {
                //        _styleLinks.Add(path);
                //        foreach (var csslink in _styleLinks)
                //        {
                //            WebClient wc = new WebClient();
                //            var css = wc.DownloadString(csslink);
                //            Regex cssUrls = new Regex(@"(url|@import)\((?<char>['""])?(?<url>.*?)\k<char>?\)", RegexOptions.IgnoreCase); //Get all url properties in the css. 

                //            foreach (Match match in cssUrls.Matches(css))
                //            {
                //                var extractedUrl = match.Groups[WebCrawler.CSS_URL_PROPERTY].Value;
                //                var innerStylePath = new Uri(_baseUri, extractedUrl).AbsoluteUri;
                //                if (!_innerStyleLinks.Contains(innerStylePath) && !WebCrawler.IsWebPage(innerStylePath))
                //                {
                //                    _innerStyleLinks.Add(innerStylePath);
                //                }
                //            }

                //        }
                //    }
                //    }
                //}

               
            }
            catch (Exception e)
            {
               // Console.WriteLine("Error parsing CSS links on page: {0} \n", WebCrawler.startingWebUrl);
            }

          

         
          
           
        }
    }
}
