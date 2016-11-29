using System;
using System.Diagnostics;


namespace Tretton37WebDownloader
{
    class Program
    {
        /// <summary>
        /// Thank you Christian and the rest of Tretton37 for allowing me to take part in this assessment. I really appreciate the experience and the challenge of having to work with 
        /// certains components or ideas that I have never worked with or even thought I had. No matter the outcome, I am glad I had this chance which has certainly improved me
        /// as a programmer.  I hope this can show you what I can do and the malleable talent that I have. As well as my eagerness to learn.  
        /// </summary>
        private static WebCrawler webCrawler; 

        /// <summary>
        /// Instantiates a new instance of WebCrawler, calls TraverseTretton37Site(), then tracks the time elapsed.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
          //  string paramURL = args[0]; 
            string str = "https://www.tretton37.com";  //Need to make sure any url passed into the function has http/https;

            Console.WriteLine("By default, the program will save the contents to 'C:\\tretton37\\'");
            Stopwatch stopWatch = new Stopwatch(); //Instantiate a stopwatch object to keep track of the running time. 
            stopWatch.Start();
            webCrawler = new WebCrawler(str); //Instantiate a new instance of WebCrawler. 
            webCrawler.TraverseTretton37sSite(); //Call the  initial method. 

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            Console.WriteLine("Elapsed Time: {0} minutes, {1} seconds \n", ts.Minutes, ts.Seconds);
        }
    }
}
