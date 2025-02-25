using System;
using System.Web.Mvc;
using Matchy.Database;
using System.Data.SqlClient;

namespace Memory_Game.Controllers
{
    public class RateController : Controller
    {
        // POST: Rate/AddRate
        [HttpPost]
        public ActionResult AddRate(int userId, int rating)
        {
            var result = AddRateToDatabase(userId, rating);
            if (result)
            {
                return RedirectToAction("Index", "Home"); // Redirect after successful rating
            }
            else
            {
                ModelState.AddModelError("", "Error while adding the rating.");
                return View();
            }
        }

        // Helper method to handle database interaction
        private bool AddRateToDatabase(int userId, int rating)
        {
            var connection = DatabaseConnection.GetConnection();
            var query = "INSERT INTO Rate (UserId, Rating) VALUES (@UserId, @Rating)";
            var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@Rating", rating);
            connection.Open();

            var result = command.ExecuteNonQuery() > 0;

            connection.Close();
            return result;
        }

        // GET: Rate/AverageRating
        public ActionResult AverageRating()
        {
            var avgRating = GetAverageRatingFromDatabase();
            return View(avgRating);
        }

        // Helper method to get average rating
        private double GetAverageRatingFromDatabase()
        {
            var connection = DatabaseConnection.GetConnection();
            var query = "SELECT AVG(Rating) AS AverageRating FROM Rate";
            var command = new SqlCommand(query, connection);

            connection.Open();
            var result = Convert.ToDouble(command.ExecuteScalar() ?? 0);

            connection.Close();
            return result;
        }
    }
}
