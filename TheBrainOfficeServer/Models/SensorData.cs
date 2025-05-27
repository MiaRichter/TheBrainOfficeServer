namespace TheBrainOfficeServer.Models;

public class SensorData
{
    public float Temperature { get; set; }
    public float Humidity { get; set; }
    public string Rfid { get; set; }
    public int Servo { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ComponentType { get; set; }
    public string Location { get; set; }
    public string ComponentId { get; set; }
    public string PortName { get; set; }
    public DateTime ReceivedAt { get; set; }
}