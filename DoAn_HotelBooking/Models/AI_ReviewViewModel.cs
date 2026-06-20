namespace DoAn_HotelBooking.Models
{
    public class AI_ReviewViewModel
    {
        public double TiLeYeuThich { get; set; }
        public List<string> UuDiem { get; set; } = new List<string>();
        public List<string> GoiY { get; set; } = new();
        public List<string> NhuocDiem { get; set; } = new List<string>();
    }
}
