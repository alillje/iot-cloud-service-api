namespace iot_cloud_service_api.Interfaces
{
    public interface ITempData
    {
        double Temperature { get; set; }

        double Humidity { get; set; }

        DateTime Timestamp { get; set; }
    }
}
