using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using System.Threading.Tasks;
using System.Threading;


namespace Tretton37WebDownloader
{
    class ImageParser :ParserBase, IParser
    {
    

        private const string IMAGE_XPATH = "//img/@src";

        private List<string> _imageLinks = new List<string>();

        public List<string> ResourceLinks
        {
            get { return _imageLinks; }
            set { _imageLinks = value; }
        }

        public void Parse(WebPage page)
        {

            HtmlNodeCollection imageFiles = null;
            _doc.LoadHtml(page.HtmlContent);
         
            try
            {
                //Hmm..I originally had SelectNodes(..).ToList(); But I kept getting a null reference exception since at least on element returned in the collection was null.
                imageFiles = _doc.DocumentNode.SelectNodes(IMAGE_XPATH); 
                if (imageFiles != null)
                {
                    Parallel.ForEach(imageFiles, (imageFile) =>
                    {
                        if (imageFile != null)
                        {
                            var src = imageFile.Attributes[WebCrawler.SRC_HTML_ATTRIBUTE].Value;

                            if (src != null && src != string.Empty)
                            {
                                var path = new Uri(_baseUri, src).AbsoluteUri;
                                if (!_imageLinks.Contains(path))
                                {
                                    _imageLinks.Add(path);
                                }
                            }                   
                        }                      
                    });
                }
               
              //  Console.WriteLine("Images found! Collecting...");
                //foreach (var item in imageFiles)
                //{
                //    var src = item.Attributes[WebCrawler.SRC_HTML_ATTRIBUTE].Value;
                //    var path = new Uri(_baseUri, src).AbsoluteUri;
                //    if (!_imageLinks.Contains(path))
                //    {
                //        _imageLinks.Add(path);
                //    }
                //}
            }
            catch (System.ArgumentNullException e)
            {
                Console.WriteLine(e.Message);
               // Console.WriteLine("Error parsing image elements on page: {0}. Possibly none exist.", page.PageUrl);
            }
          
        }
    }
}
