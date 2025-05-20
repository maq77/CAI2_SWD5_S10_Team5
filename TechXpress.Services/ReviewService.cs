using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Data.Model;
using TechXpress.Data.Repositories.Base;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;

namespace TechXpress.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReviewService> _logger;
        public ReviewService(IUnitOfWork unitOfWork, ILogger<ReviewService> logger)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> AddReviewAsync(ReviewDTO review)
        {
            try
            {
                _logger.LogInformation("Received productId: {ProductId}", review.ProductId);
                if (review.ProductId == 0)
                {
                    _logger.LogError("Invalid productId received.");
                    return false;
                }
                _logger.LogInformation("Received UserId: {UserId}", review.UserId);
                if (string.IsNullOrEmpty(review.UserId))
                {
                    _logger.LogError("Invalid UserId received.");
                    return false;
                }
                _logger.LogInformation("Received Rating: {Rating}", review.Rating);
                if (review.Rating == 0)
                {
                    _logger.LogError("Invalid Rating received.");
                    return false;
                }
                _logger.LogInformation("Received Comment: {Comment}", review.Comment);
                /*if (string.IsNullOrEmpty(review.Comment))
                {
                    _logger.LogError("Invalid Comment received.");
                    return false;
                }*/
                _logger.LogInformation($"Adding review Rating: {review.Rating} for productId: {review.ProductId} with customerId: {review.UserId} that has commented with {review.Comment} .");
                var reviewEntity = new Review
                {
                    ProductId = review.ProductId,
                    UserId = review.UserId,
                    Rating = review.Rating,
                    Comment = review.Comment
                };
                await _unitOfWork.Reviews.Add(reviewEntity, log=>Console.WriteLine(log));
                _logger.LogInformation($"Review added successfully for productId: {review.ProductId} with customerId: {review.UserId}.");
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding review");
                return false;
            }
        }

        public async Task<bool> DeleteReviewAsync(int reviewId)
        {
            try
            {
                _logger.LogInformation("Deleting review with ID: {ReviewId}", reviewId);
                var review = await _unitOfWork.Reviews.GetById(reviewId);
                if (review == null)
                {
                    _logger.LogError("Review with ID {ReviewId} not found.", reviewId);
                    return false;
                }
                _logger.LogInformation("Review with ID: {ReviewId} found. Proceeding to delete.", reviewId);
                await _unitOfWork.Reviews.Delete(reviewId, log => Console.WriteLine(log));
                _logger.LogInformation("Review with ID: {ReviewId} deleted successfully.", reviewId);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch(Exception ex) 
            {
                _logger.LogError(ex, "Error deleting review");
                return false;
            }
        }

        public async Task<double> GetAverageRatingByProductIdAsync(int productId)
        {
            try
            {
                _logger.LogInformation("Getting average rating for product ID: {ProductId}", productId);
                var reviews = await _unitOfWork.Reviews.GetAll(r => r.ProductId == productId);

                if (reviews == null || !reviews.Any())
                {
                    _logger.LogInformation("No reviews found for product ID: {ProductId}", productId);
                    return 0;
                }

                double averageRating = reviews.Average(r => r.Rating);
                _logger.LogInformation("Average rating for product ID {ProductId} is {AverageRating}", productId, averageRating);

                return averageRating;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting average rating for product ID: {ProductId}", productId);
                return 0;
            }
        }

        public async Task<ReviewDTO?> GetReviewByIdAsync(int reviewId)
        {
            try
            {
                _logger.LogInformation("Getting review by ID: {ReviewId}", reviewId);
                var review = await _unitOfWork.Reviews.GetById(reviewId);

                if (review == null)
                {
                    _logger.LogWarning("Review with ID {ReviewId} not found", reviewId);
                    return null;
                }

                _logger.LogInformation("Review with ID {ReviewId} found successfully", reviewId);
                return new ReviewDTO
                {
                    Id = review.Id,
                    ProductId = review.ProductId,
                    UserId = review.UserId,
                    Rating = review.Rating,
                    Comment = review.Comment,
                    Date = review.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting review by ID: {ReviewId}", reviewId);
                return null;
            }
        }

        public async Task<List<ReviewDTO>> GetReviewsByProductIdAsync(int productId)
        {
            try
            {
                _logger.LogInformation("Getting reviews for product ID: {ProductId}", productId);
                var reviews = await _unitOfWork.Reviews.GetAll_includes(r => r.ProductId == productId, includes: new[] { "User" });

                if (reviews == null || !reviews.Any())
                {
                    _logger.LogInformation("No reviews found for product ID: {ProductId}", productId);
                    return new List<ReviewDTO>();
                }

                var reviewDTOs = reviews.Select(r => new ReviewDTO
                {
                    Id = r.Id,
                    ProductId = r.ProductId,
                    UserId = r.UserId,
                    UserName = r.User?.UserName ?? "Anonymous",
                    Rating = r.Rating,
                    Comment = r.Comment,
                    Date = r.CreatedAt
                }).ToList();

                _logger.LogInformation("Found {Count} reviews for product ID: {ProductId}", reviewDTOs.Count, productId);
                return reviewDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reviews for product ID: {ProductId}", productId);
                return new List<ReviewDTO>();
            }
        }

        public async Task<List<ReviewDTO>> GetReviewsByUserIdAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Getting reviews for user ID: {UserId}", userId);
                var reviews = await _unitOfWork.Reviews.GetAll_includes(r => r.UserId == userId, includes: new[] { "Product" });

                if (reviews == null || !reviews.Any())
                {
                    _logger.LogInformation("No reviews found for user ID: {UserId}", userId);
                    return new List<ReviewDTO>();
                }

                var reviewDTOs = reviews.Select(r => new ReviewDTO
                {
                    Id = r.Id,
                    ProductId = r.ProductId,
                    ProductName = r.Product?.Name ?? "Unknown Product",
                    UserId = r.UserId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    Date = r.CreatedAt
                }).ToList();

                _logger.LogInformation("Found {Count} reviews for user ID: {UserId}", reviewDTOs.Count(), userId);
                return reviewDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reviews for user ID: {UserId}", userId);
                return new List<ReviewDTO>();
            }
        }

        public async Task<bool> UpdateReviewAsync(ReviewDTO review)
        {
            try
            {
                _logger.LogInformation("Updating review ID: {ReviewId}", review.Id);
                var existingReview = await _unitOfWork.Reviews.GetById(review.Id);

                if (existingReview == null)
                {
                    _logger.LogError("Review with ID {ReviewId} not found for update", review.Id);
                    return false;
                }

                existingReview.Rating = review.Rating;
                existingReview.Comment = review.Comment;
                //existingReview.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Reviews.Update(existingReview, log => Console.WriteLine(log));
                _logger.LogInformation("Review with ID {ReviewId} updated successfully", review.Id);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating review ID: {ReviewId}", review.Id);
                return false;
            }
        }
    }
}
