namespace Utilities.RequestClient.Tests.TestObjects
{
    public class ResultDto
    {
        public Args args { get; set; }
        public Data data { get; set; }
        public Files files { get; set; }
        public Form form { get; set; }
        public Headers headers { get; set; }
        public Json json { get; set; }
        public string url { get; set; }
    }

    public class Args
    {
    }

    public class Data
    {
        public string test { get; set; }
    }

    public class Files
    {
    }

    public class Form
    {
    }

    public class Headers
    {
        public string host { get; set; }
        public string contentlength { get; set; }
        public string accept { get; set; }
        public string acceptencoding { get; set; }
        public string cachecontrol { get; set; }
        public string contenttype { get; set; }
        public string cookie { get; set; }
        public string postmantoken { get; set; }
        public string useragent { get; set; }
        public string xforwardedport { get; set; }
        public string xforwardedproto { get; set; }
    }

    public class Json
    {
        public string test { get; set; }
    }
}
