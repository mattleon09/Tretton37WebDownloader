
using System.Collections.Generic;
namespace Tretton37WebDownloader
{

    /// <summary>
    /// My first time actually using an interface and I truly understand the usefulness. Why haven't I used this more? 
    /// </summary>
    interface IParser
    {

     
        List<string> ResourceLinks {get; set;} //The main link/url collection for each "Parser" class. 
      
        /// <summary>
        /// I define Parse here and have each of the "Parser" classes implement their own functionality. 
        /// 
        /// </summary>
        /// <param name="page"></param>
        void Parse(WebPage page);

    }
}
