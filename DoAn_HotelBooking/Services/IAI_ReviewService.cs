using DoAn_HotelBooking.Models;

namespace DoAn_HotelBooking.Services
{
    public interface IAI_ReviewService
    {
        Task<AI_ReviewViewModel> AnalyzeReviewsAsync(List<string> reviews);
    }
}
