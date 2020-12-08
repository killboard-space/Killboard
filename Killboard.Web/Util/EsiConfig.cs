
namespace Killboard.Web.Util
{
    public class EsiConfig
    {
        public EsiConfig()
        {
            EsiUrl = "https://esi.evetech.net/";
            DataSource = DataSource.Tranquility;
        }

        public string EsiUrl { get; set; }
        public DataSource DataSource { get; set; }
        public string ClientId { get; set; }
        public string SecretKey { get; set; }
        public string CallbackUrl { get; set; }
        public string UserAgent { get; set; }
    }
}
