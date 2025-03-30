using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TrainTray_food_order_booking_system.Models
{
    public class TrainStatusResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("train_name")]
        public string TrainName { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("updated_time")]
        public string UpdatedTime { get; set; }

        [JsonPropertyName("data")]
        public List<TrainStationInfo> Stations { get; set; }
    }

    public class TrainStationInfo
    {
        [JsonPropertyName("is_current_station")]
        public bool IsCurrentStation { get; set; }

        [JsonPropertyName("station_name")]
        public string StationName { get; set; }

        [JsonPropertyName("distance")]
        public string Distance { get; set; }

        [JsonPropertyName("timing")]
        public string Timing { get; set; }

        [JsonPropertyName("delay")]
        public string Delay { get; set; }

        [JsonPropertyName("platform")]
        public string Platform { get; set; }

        [JsonPropertyName("halt")]
        public string Halt { get; set; }
    }
}
