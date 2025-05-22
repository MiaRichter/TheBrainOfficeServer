public class DhtReading
{
    public double TemperatureC { get; set; }
    public double TemperatureF => 32 + (TemperatureC / 0.5556);
    public double Humidity { get; set; }
    public double HeatIndexC { get; set; }
    public double DewPointC { get; set; }
    public bool IsSuccessful { get; set; }
    public string ErrorMessage { get; set; }
}