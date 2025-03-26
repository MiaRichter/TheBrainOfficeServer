namespace TheBrainOfficeServer.Models
{
    public class InitializeModel
    {
        public int Id { get; set; }
        public string? Component_Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Component_Type { get; set; }
        public string? Location { get; set; }
        public string? Created_At { get; set; }
        public string? Updated_At { get; set; }
        public bool Is_Active { get; set; }
    }
}
