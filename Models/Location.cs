namespace OfficeManagerWPF.Models
{
    public class Location
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Manager { get; set; }
        public string PhoneNumber { get; set; }
        public string Notes { get; set; }
        public bool IsActive { get; set; }
    }
}
