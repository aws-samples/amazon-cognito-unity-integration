public class FacebookDeviceVerificationCode
{
    public string code;
    public string user_code;
    public string verification_uri;
    public int expires_in;
    public int interval;
}

public class Error
{
    public string message { get; set; }
    public string type { get; set; }
    public int code { get; set; }
    public int error_subcode { get; set; }
    public bool is_transient { get; set; }
    public string error_user_title { get; set; }
    public string error_user_msg { get; set; }
    public string fbtrace_id { get; set; }
}

public class FacebookPollResult
{
    public Error error { get; set; }
    public string access_token { get; set; }
    public int data_access_expiration_time { get; set; }
    public int expires_in { get; set; }
}

public class Data
    {
        public int height { get; set; }
        public bool is_silhouette { get; set; }
        public string url { get; set; }
        public int width { get; set; }
    }

public class Picture
{
    public Data data { get; set; }
}

public class FacebookUserData
{
    public string name { get; set; }
    public Picture picture { get; set; }
    public string email { get; set; }
    public string id { get; set; }
}