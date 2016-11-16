using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using System.Net;
using System.Threading;
using System.IO;
using System.Threading.Tasks;


namespace Tretton37WebDownloader
{
    class ScriptParser :  ParserBase, IParser
    {
        private const string SCRIPT_XPATH = "//script/@src";

        private List<string> _scriptLinks = new List<string>();

        public List<string> ResourceLinks
        {
            get { return _scriptLinks; }
            set { _scriptLinks = value; }
        }


        public ScriptParser()
        {

        }

        public void Parse(WebPage page)
        {
            HtmlNodeCollection scriptfiles = null;
            _doc.LoadHtml(page.HtmlContent);
              try
            {
                //Hmm..I originally had SelectNodes(..).ToList(); But I kept getting a null reference exception since at least on element returned in the collection was null.
                 scriptfiles = _doc.DocumentNode.SelectNodes(SCRIPT_XPATH); //Get all of the script files with the src attribute. 

                 if (scriptfiles != null)
                 {
                     Parallel.ForEach(scriptfiles, (script) =>
                     {
                         if (script != null)
                         {
                             var src = script.Attributes[WebCrawler.SRC_HTML_ATTRIBUTE].Value;

                             if(src != null & src != string.Empty)
                             if (!src.Contains(WebCrawler.CDN_IDENTIFIER)) // No need to bother with CDN links. 
                             {
                                 var path = new Uri(_baseUri, src).AbsoluteUri;
                                 if (!_scriptLinks.Contains(path))
                                 {
                                     _scriptLinks.Add(path);
                                 }
                             }
                         }                          
                     });
                 }
              
                  //Just to show my progress.
                //foreach (var item in scriptfiles) //Loop through each script file 
                //{
                //    var src = item.Attributes[WebCrawler.SRC_HTML_ATTRIBUTE].Value;
                //    if (!src.Contains(WebCrawler.CDN_IDENTIFIER)) // No need to bother with CDN links. 
                //    {
                //        var path = new Uri(_baseUri, src).AbsoluteUri;
                //        if (!_scriptLinks.Contains(path))
                //        {
                //            _scriptLinks.Add(path);
                //        }
                //    }
                //}
            }
            catch (Exception e)
            {
               // Console.WriteLine("Error parsing script elements on page: {0}.", page.PageUrl);
            }

          
        }

    }
}
