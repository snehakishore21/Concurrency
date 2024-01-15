using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class WebCrawler
{
    public class SolutionDfs
    {
        private readonly HashSet<string> res = new HashSet<string>();
        private readonly object lockObject = new object();

        public IList<string> Crawl(string startUrl, IHtmlParser htmlParser)
        {
            string host = GetUrlHost(startUrl);
            lock (lockObject)
            {
                res.Add(startUrl);
            }
            Dfs(startUrl, host, htmlParser);
            return res.ToList();
        }

        private void Dfs(string startUrl, string host, IHtmlParser htmlParser)
        {
            List<string> urls = htmlParser.GetUrls(startUrl);
            foreach (string url in urls)
            {
                lock (lockObject)
                {
                    if (res.Contains(url) || !GetUrlHost(url).Equals(host))
                    {
                        continue;
                    }
                    res.Add(url);
                }
                Dfs(url, host, htmlParser);
            }
        }

        private string GetUrlHost(string url)
        {
            string[] args = url.Split('/');
            return args[2];
        }
    }

    public class SolutionBfs
    {
        private readonly HashSet<string> res = new HashSet<string>();
        private readonly object lockObject = new object();

        public IList<string> Crawl(string startUrl, IHtmlParser htmlParser)
        {
            string host = GetUrlHost(startUrl);
            Queue<string> queue = new Queue<string>();
            lock (lockObject)
            {
                res.Add(startUrl);
            }
            queue.Enqueue(startUrl);
            while (queue.Count > 0)
            {
                string url = queue.Dequeue();
                List<string> urls = htmlParser.GetUrls(url);
                foreach (string u in urls)
                {
                    lock (lockObject)
                    {
                        if (res.Contains(u) || !GetUrlHost(u).Equals(host))
                        {
                            continue;
                        }
                        res.Add(u);
                    }
                    queue.Enqueue(u);
                }
            }
            return res.ToList();
        }

        private string GetUrlHost(string url)
        {
            string[] args = url.Split('/');
            return args[2];
        }
    }

    public interface IHtmlParser
    {
        List<string> GetUrls(string str);
    }
}
