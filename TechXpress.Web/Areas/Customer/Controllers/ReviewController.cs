using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.Security.Claims;
using TechXpress.Data.Model;
using TechXpress.Services;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;

namespace TechXpress.Web.Areas.Customer.Controllers
{
    [Authorize(Policy = "Auth")]
    [Area("Customer")]
    public class ReviewController : Controller
    {
        private readonly IReviewService _reviewService;
        private readonly UserManager<User> _userManager;

        public ReviewController(IReviewService reviewService, UserManager<User> userManager)
        {
            _reviewService = reviewService;
            _userManager = userManager;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction(nameof(Login), "Account", new { area = "" });

            var reviews = await _reviewService.GetReviewsByUserIdAsync(userId);

            return View(reviews);
        }

        [HttpPost]
        public async Task<IActionResult> AddReview(ReviewDTO reviewDto)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid review data.";
                return RedirectToAction("Details", "Product", new { id = reviewDto.ProductId });
            }

            // If user is authenticated, get their ID
            var user = await _userManager.GetUserAsync(User);
            if (user!=null)
            {
                reviewDto.UserId = user.Id;

                // If user didn't provide a name, use their username
                if (string.IsNullOrEmpty(reviewDto.UserName))
                {
                    reviewDto.UserName = user?.UserName ?? "Anonymous";
                }
            }
            else
            {
                // For anonymous users, create a temporary ID
                reviewDto.UserId = "anonymous-" + Guid.NewGuid().ToString();

                // If no name was provided, use "Anonymous"
                if (string.IsNullOrEmpty(reviewDto.UserName))
                {
                    reviewDto.UserName = "Anonymous";
                }
            }

            reviewDto.Date = DateTime.UtcNow;

            var result = await _reviewService.AddReviewAsync(reviewDto);

            if (result)
            {
                TempData["Success"] = "Your review has been added successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to add your review. Please try again.";
            }

            return RedirectToAction("Details", "Product", new { area = "", id = reviewDto.ProductId, showReviews = true });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteReview(int id, int productId)
        {
            var review = await _reviewService.GetReviewByIdAsync(id);

            if (review == null)
            {
                TempData["Error"] = "Review not found.";
                return RedirectToAction("Details", "Product", new { id = productId });
            }

            // Check if the current user is the owner of the review
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (review.UserId != userId && !User.IsInRole("Admin"))
            {
                TempData["Error"] = "You are not authorized to delete this review.";
                return RedirectToAction("Details", "Product", new { id = productId });
            }

            var result = await _reviewService.DeleteReviewAsync(id);

            if (result)
            {
                TempData["Success"] = "Your review has been deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to delete your review. Please try again.";
            }

            return RedirectToAction("Details", "Product", new { area="", id = productId, showReviews = true });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateReview(ReviewDTO reviewDto)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid review data.";
                return RedirectToAction("Details", "Product", new { id = reviewDto.ProductId });
            }

            var existingReview = await _reviewService.GetReviewByIdAsync(reviewDto.Id);

            if (existingReview == null)
            {
                TempData["Error"] = "Review not found.";
                return RedirectToAction("Details", "Product", new { id = reviewDto.ProductId });
            }

            // Check if the current user is the owner of the review
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (existingReview.UserId != userId && !User.IsInRole("Admin"))
            {
                TempData["Error"] = "You are not authorized to update this review.";
                return RedirectToAction("Details", "Product", new { id = reviewDto.ProductId });
            }

            // Preserve the original user ID and product ID
            reviewDto.UserId = existingReview.UserId;
            reviewDto.ProductId = existingReview.ProductId;

            var result = await _reviewService.UpdateReviewAsync(reviewDto);

            if (result)
            {
                TempData["Success"] = "Your review has been updated successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to update your review. Please try again.";
            }

            return RedirectToAction("Details", "Product", new { area="", id = reviewDto.ProductId, showReviews = true });
        }
    }
}
