using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Web.Mvc;
using Dapper;
using Matchy.Database;

namespace Matchy.Controllers
{
    public class GameController : Controller
    {
        private readonly SqlConnection _connection;

        public GameController()
        {
            _connection = DatabaseConnection.GetConnection();
        }

        // Use the connection object directly for all database interactions
        private void OpenConnection()
        {
            if (_connection.State != System.Data.ConnectionState.Open)
            {
                _connection.Open();
            }
        }

        private void CloseConnection()
        {
            if (_connection.State == System.Data.ConnectionState.Open)
            {
                _connection.Close();
            }
        }

        // GET: Game
        public ActionResult Index()
        {
            // Retrieve the username from the session
            string playerName = Session["Username"]?.ToString();

            // Ensure playerName is not null or empty
            if (string.IsNullOrEmpty(playerName))
            {
                return RedirectToAction("Login", "Auth"); // Redirect to login if the player name is missing
            }

            // Pass player name to the view
            ViewBag.PlayerName = playerName;

            return View();
        }

        // Save score endpoint with try-catch and return response to client
        [HttpPost]
        public ActionResult SaveScore(int score, int level)
        {
            // Retrieve the player name from session
            string playerName = Session["Username"]?.ToString();

            // Ensure playerName is not null or empty
            if (string.IsNullOrEmpty(playerName))
            {
                return Json(new { success = false, message = "Player name is missing." });
            }

            try
            {
                Debug.WriteLine("Save Score Method Called");

                using (var connection = _connection)
                {
                    // Get user ID
                    var userId = connection.QuerySingleOrDefault<int>(
                        "SELECT Id FROM Users WHERE Username = @Username",
                        new { Username = playerName });

                    if (userId == 0)
                    {
                        Debug.WriteLine($"Player {playerName} not found in the database.");
                        return Json(new { success = false, message = "Player not found." });
                    }

                    // Insert score
                    var rowsAffected = connection.Execute(
                        "INSERT INTO Scores (UserId, Score, Level) VALUES (@UserId, @Score, @Level)",
                        new { UserId = userId, Score = score, Level = level });

                    if (rowsAffected > 0)
                    {
                        Debug.WriteLine($"Score saved successfully for user {playerName} (ID: {userId}).");
                        return Json(new { success = true, message = "Score saved successfully." });
                    }
                    else
                    {
                        Debug.WriteLine($"Failed to save score for user {playerName} (ID: {userId}).");
                        return Json(new { success = false, message = "Failed to save score." });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving score to the database: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while saving the score." });
            }
        }

        // Save rating endpoint with try-catch and return response to client
        [HttpPost]
        public ActionResult SaveRating(int rating)
        {
            // Retrieve the player name from session
            string playerName = Session["Username"]?.ToString();

            // Ensure playerName is not null or empty
            if (string.IsNullOrEmpty(playerName))
            {
                return Json(new { success = false, message = "Player name is missing." });
            }

            try
            {
                Debug.WriteLine("Save Rating Method Called");

                using (var connection = _connection)
                {
                    // Get user ID
                    var userId = connection.QuerySingleOrDefault<int>(
                        "SELECT Id FROM Users WHERE Username = @Username",
                        new { Username = playerName });

                    if (userId == 0)
                    {
                        Debug.WriteLine($"Player {playerName} not found in the database.");
                        return Json(new { success = false, message = "Player not found." });
                    }

                    // Insert rating
                    var rowsAffected = connection.Execute(
                        "INSERT INTO Rate (UserId, Rating) VALUES (@UserId, @Rating)",
                        new { UserId = userId, Rating = rating });

                    if (rowsAffected > 0)
                    {
                        Debug.WriteLine($"Rating {rating} saved successfully for user {playerName} (ID: {userId}).");
                        return Json(new { success = true, message = "Rating saved successfully." });
                    }
                    else
                    {
                        Debug.WriteLine($"Failed to save rating for user {playerName} (ID: {userId}).");
                        return Json(new { success = false, message = "Failed to save rating." });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving rating to the database: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while saving the rating." });
            }
        }
    }
}
