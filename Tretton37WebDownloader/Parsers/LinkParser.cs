using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using System.Threading.Tasks;
using System.Threading;

namespace Tretton37WebDownloader
{
    class AnchorParser : ParserBase,IParser
    {


        //  private const string STR_LINK_REGEX = "href=\"[a-zA-Z./:&\\d_-]+\"";
        private const string LINK_XPATH = "//a/@href";


        private List<string> _internalUrls = new List<string>();

        public List<string> ResourceLinks
        {
            get { return _internalUrls; }
            set { _internalUrls = value; }
        }


        public AnchorParser()
        {
           
        }

        public void Parse(string htmlContent)
        {

            HtmlNodeCollection linkPaths = null;

            _doc.LoadHtml(htmlContent);
            try
            {
                linkPaths = _doc.DocumentNode.SelectNodes(LINK_XPATH); //Select all of the anchor tags with the 'href' attribute. 
              //  Console.WriteLine("Link paths found!. Verifying and collecting...");

                if (linkPaths != null) //If there are some...
                {
                    //Iterate through linkPaths using Parrallel programming.
                    //This is to try and concurrently grab  the urls within the href attributes. 
                    Parallel.ForEach(linkPaths, (item) =>   
                    {
                        if (item != null)
                        {
                            var href = item.Attributes[WebCrawler.HREF_HTML_ATTRIBUTE].Value; //Get the contents within the 'href' attribute. 

                            if (href != null && href != string.Empty)
                            {
                                var path = new Uri(_baseUri, href).AbsoluteUri;
                                //I did this to avoid duplicate/similar url paths. Ex. "/join" and "/join/"
                                if (path.Count(x => x == WebCrawler.FORWARD_SLASH_CHAR) >= 4) 
                                {
                                    var last = path.LastIndexOf(WebCrawler.FORWARD_SLASH_CHAR);
                                    path = path.Substring(0, last);
                                }
                                if (!path.Contains(WebCrawler.MAILTO_LINKS) && !path.Contains(WebCrawler.TELEPHONE_LINKS)) //Leave out email and phone links
                                {
                                    if (!path.Contains(WebCrawler.POUND_SYMBOL_CHAR)) //Avoid sub-sections of pages
                                    {

                                        // Console.WriteLine("Eliminating external link...");
                                        if (!IsExternalUrl(path) && path != WebCrawler.startingWebUrl)
                                        {
                                            //    Console.WriteLine("Verifying that link is a web page...");
                                            if (WebCrawler.IsWebPage(path))
                                            {
                                                if (!_internalUrls.Contains(path))
                                                    _internalUrls.Add(path);
                                            }
                                        }
                                    }
                                }
                            }
                      
                        }
                    });
                }

               

                //foreach (var item in linkPaths)
                //{
                //    if (item != null)
                //    {
                //        var href = item.Attributes[WebCrawler.HREF_HTML_ATTRIBUTE].Value;
                //        var path = new Uri(_baseUri, href).AbsoluteUri;


                //        if (path.Count(x => x == WebCrawler.FORWARD_SLASH_CHAR) >= 4)
                //        {
                //            var last = path.LastIndexOf(WebCrawler.FORWARD_SLASH_CHAR);
                //            path = path.Substring(0, last);
                //        }
                //        if (!path.Contains(WebCrawler.MAILTO_LINKS) && !path.Contains(WebCrawler.TELEPHONE_LINKS))
                //        {


                //            if (!path.Contains(WebCrawler.POUND_SYMBOL_CHAR)) //Avoid sub-sections of pages
                //        {

                //           // Console.WriteLine("Eliminating external link...");
                //            if (!IsExternalUrl(path) && path != WebCrawler.startingWebUrl)
                //            {
                //            //    Console.WriteLine("Verifying that link is a web page...");
                //                if (WebCrawler.IsWebPage(path))
                //                {
                //                    if (!_internalUrls.Contains(path))
                //                        _internalUrls.Add(path);
                //                }
                //            }
                //        }
                //    }
                //    }
                //}
              //  links.Remove(rootUrl + "/");
            }
            catch (Exception e)
            {
                //Console.WriteLine("Error parsing links on page: {0}", page.PageUrl);
            }

            
     }

        private bool IsExternalUrl(string url)
        {
            ///var domainname = new Uri(url).GetComponents(UriComponents.Host, UriFormat.Unescaped);
            if (url.IndexOf("tretton37.com") > -1)
                return false;
            else if ((url.Length >= 7) && (url.IndexOf("http://") > -1 || url.IndexOf("www") > -1 || url.IndexOf("https://") > -1))
                return true;
            else
                return false;
        }



        //Not implemented here 
        public void DownloadResources(List<string> resources)
        {

        }
    }
}
