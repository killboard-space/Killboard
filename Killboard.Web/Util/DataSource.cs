using System.Runtime.Serialization;

namespace Killboard.Web.Util
{
    public enum DataSource
    {
        [EnumMember(Value = "singularity")] /**/ Singularity,
        [EnumMember(Value = "tranquility")] /**/ Tranquility,
        [EnumMember(Value = "serenity")]    /**/ Serenity
    }
}
