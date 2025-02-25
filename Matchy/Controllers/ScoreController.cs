using System;
using Matchy.Models;
using Matchy.Database;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Memory_Game.Controllers
{
    public class ScoreController : Controller
    {
        // Configure JSON serialization settings for dates
        private JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            DateFormatString = "yyyy-MM-ddTHH:mm:ss",
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        // Helper method to return JSON with custom settings
        private JsonResult CustomJson(object data, JsonRequestBehavior behavior = JsonRequestBehavior.DenyGet)
        {
            return new JsonResult
            {
                Data = data,
                JsonRequestBehavior = behavior,
                ContentType = "application/json",
                ContentEncoding = System.Text.Encoding.UTF8,
                MaxJsonLength = int.MaxValue,
                RecursionLimit = 10
            };
        }

        // POST: Score/AddScore
        [HttpPost]
        public ActionResult AddScore(int scoreValue, int level)
        {
            // Retrieve UserId from session
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Auth"); // Redirect if not logged in
            }

            int userId = (int)Session["UserId"];
            var result = AddScoreToDatabase(userId, scoreValue, level);

            if (result)
            {
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, message = "Error while adding the score." });
            }
        }

        private bool AddScoreToDatabase(int userId, int scoreValue, int level)
        {
            var connection = DatabaseConnection.GetConnection();
            var query = "INSERT INTO Scores (UserId, Score, Level) VALUES (@UserId, @Score, @Level)";
            var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@Score", scoreValue);
            command.Parameters.AddWithValue("@Level", level);
            connection.Open();

            var result = command.ExecuteNonQuery() > 0;

            connection.Close();
            return result;
        }

        // GET: Score/AllScores
        [HttpGet]
        public JsonResult AllScores()
        {
            // Retrieve UserId from session
            if (Session["UserId"] == null)
            {
                return Json(new { error = "Not logged in" }, JsonRequestBehavior.AllowGet);
            }

            int userId = (int)Session["UserId"];
            var scores = GetAllScoresByUser(userId);

            // Get username for display
            string username = GetUsernameById(userId);

            var scoreList = new List<object>();
            foreach (var score in scores)
            {
                scoreList.Add(new
                {
                    username = username,
                    level = score.Level,
                    scoreValue = score.ScoreValue,
                    achievedAt = score.AchievedAt.ToString("yyyy-MM-ddTHH:mm:ss")
                });
            }

            return Json(scoreList, JsonRequestBehavior.AllowGet);
        }

        private List<Score> GetAllScoresByUser(int userId)
        {
            var scores = new List<Score>();

            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    var query = "SELECT * FROM Scores WHERE UserId = @UserId ORDER BY AchievedAt DESC";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);
                        connection.Open();

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                scores.Add(new Score
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                                    ScoreValue = reader.GetInt32(reader.GetOrdinal("Score")),
                                    Level = reader.GetInt32(reader.GetOrdinal("Level")),
                                    AchievedAt = reader.GetDateTime(reader.GetOrdinal("AchievedAt"))
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error
                System.Diagnostics.Debug.WriteLine("Error fetching scores: " + ex.Message);
                // Optionally, return an empty list if there's an error
                scores = new List<Score>();
            }

            return scores;
        }

        private string GetUsernameById(int userId)
        {
            string username = "Unknown";

            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    var query = "SELECT Username FROM Users WHERE Id = @UserId";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);
                        connection.Open();
                        var result = command.ExecuteScalar();
                        if (result != null)
                        {
                            username = result.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error fetching username: " + ex.Message);
            }

            return username;
        }

        // GET: Score/HighScores
        [HttpGet]
        public JsonResult HighScores()
        {
            var highScores = GetHighScoresForAllUsers();

            var scoreList = new List<object>();
            foreach (var score in highScores)
            {
                // Get username for each user
                string username = GetUsernameById(score.UserId);

                scoreList.Add(new
                {
                    username = username,
                    level = score.Level,
                    scoreValue = score.ScoreValue,
                    achievedAt = score.AchievedAt.ToString("yyyy-MM-ddTHH:mm:ss")
                });
            }

            return Json(scoreList, JsonRequestBehavior.AllowGet);
        }

        private List<Score> GetHighScoresForAllUsers()
        {
            var highScores = new List<Score>();

            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    var query = @"
                WITH RankedScores AS (
                    SELECT 
                        s.UserId,
                        s.Score AS HighScore,
                        s.Level,
                        s.AchievedAt,
                        ROW_NUMBER() OVER (
                            PARTITION BY s.UserId 
                            ORDER BY s.Level DESC, s.Score DESC, s.AchievedAt DESC
                        ) AS Rank
                    FROM Scores s
                )
                SELECT UserId, HighScore, Level, AchievedAt
                FROM RankedScores
                WHERE Rank = 1
                ORDER BY HighScore DESC, Level DESC;";

                    using (var command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                highScores.Add(new Score
                                {
                                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                                    ScoreValue = reader.GetInt32(reader.GetOrdinal("HighScore")),
                                    Level = reader.GetInt32(reader.GetOrdinal("Level")),
                                    AchievedAt = reader.GetDateTime(reader.GetOrdinal("AchievedAt"))
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error fetching high scores: " + ex.Message);
                highScores = new List<Score>();
            }

            return highScores;
        }

        // POST: Score/ResetScores
        [HttpPost]
        public JsonResult ResetScores()
        {
            // Retrieve UserId from session
            if (Session["UserId"] == null)
            {
                return Json(new { success = false, message = "Not logged in" });
            }

            var result = ResetAllScoresInDatabase();
            if (result)
            {
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, message = "Error while resetting scores." });
            }
        }

        private bool ResetAllScoresInDatabase()
        {
            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    var query = "DELETE FROM Scores";
                    using (var command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error resetting scores: " + ex.Message);
                return false;
            }
        }
    }
}