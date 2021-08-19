using CsvHelper.Configuration.Attributes;

namespace Sessionizing.Domain
{
    public class PageView
    {
        [Index(0)]
        public string visitor { get; set; }
        [Index(1)]
        public string mainUrl { get; set; }
        [Index(2)]
        public string url { get; set; }
        [Index(3)]
        public long timestamp{ get; set; }

        public PageView(string visitor,string mainUrl, string url, long timestamp)
        {
            this.visitor = visitor;
            this.url = url;
            this.mainUrl = mainUrl;
            this.timestamp = timestamp;
        }
        
        
    }
}