using System;
using System.Net;

public class MiniWebClient : WebClient {

    public int Timeout { get; set; }

    public MiniWebClient() : this(10) { }

    public MiniWebClient(int t)
    {
        Timeout = t;
    }

    protected override WebRequest GetWebRequest(Uri address)
    {
        var request = base.GetWebRequest(address);
        if (request != null)
        {
            request.Timeout = Timeout * 1000;
        }
        return request;
    }
}
