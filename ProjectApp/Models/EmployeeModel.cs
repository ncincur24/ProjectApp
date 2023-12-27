using Newtonsoft.Json;

namespace ProjectApp.Models
{
    public class EmployeeModel
    {
        [JsonProperty("EmployeeName")]
        public string EmployeeName { get; set; }

        [JsonProperty("StarTimeUtc")]
        public DateTime StarTimeUtc { get; set; }

        [JsonProperty("EndTimeUtc")]
        public DateTime EndTimeUtc { get; set; }
        public double TotalHours => (EndTimeUtc - StarTimeUtc).TotalHours;
    }
}
