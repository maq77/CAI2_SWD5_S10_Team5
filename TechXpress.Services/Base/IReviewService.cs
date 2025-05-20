using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Services.DTOs;

namespace TechXpress.Services.Base
{
    public interface IReviewService
    {
        Task<bool> AddReviewAsync(ReviewDTO review);
        Task<bool> UpdateReviewAsync(ReviewDTO review);
        Task<bool> DeleteReviewAsync(int reviewId);
        Task<ReviewDTO?> GetReviewByIdAsync(int reviewId);
        Task<List<ReviewDTO>> GetReviewsByProductIdAsync(int productId);
        Task<List<ReviewDTO>> GetReviewsByUserIdAsync(string userId);
        Task<double> GetAverageRatingByProductIdAsync(int productId);
    }
}
