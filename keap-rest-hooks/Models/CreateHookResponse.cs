namespace keap_rest_hooks.Models
{
    public class CreateHookResponse
    {
        public string eventKey { get; set; }
        public string hookUrl { get; set; }
        public string key { get; set; }
        public string status { get; set; }
    }
}