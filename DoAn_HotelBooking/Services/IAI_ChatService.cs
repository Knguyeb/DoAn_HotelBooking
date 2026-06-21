namespace DoAn_HotelBooking.Services
{
    public interface IAI_ChatService
    {
        Task<string> TuVanKhachHangAsync(string userMessage, string thongTinPhongTrong);
    }
}
