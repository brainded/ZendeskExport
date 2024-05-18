using System.Collections.Generic;

public class ZendeskArticle
{
    public long Id { get; set; }

    public string Title { get; set; }

    public string Body { get; set; }

    public string HtmlUrl { get; set; }
}

public class ZendeskArticleResponse
{
    public int Count { get; set; }

    public List<ZendeskArticle> Articles { get; set; }

    public string Next_Page { get; set; }
}