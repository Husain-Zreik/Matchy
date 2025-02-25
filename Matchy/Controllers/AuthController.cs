using System;
using System.Text;
using Matchy.Models;
using System.Web.Mvc;
using Matchy.Database;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Diagnostics;
using Dapper;

namespace Matchy.Controllers
{
    public class AuthController : Controller
    {
        private SqlConnection _connection;

        public AuthController()
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

        // GET: Auth/Signup
        public ActionResult Signup()
        {
            return View();
        }

        // POST: Auth/Signup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Signup(User model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Debugging logs
                    Debug.WriteLine($"Signup attempt for Username: {model.Username}");

                    // Check if passwords match
                    if (model.Password != model.ConfirmPassword)
                    {
                        ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
                        return View(model);
                    }

                    OpenConnection(); // Open the connection

                    // Check if username already exists
                    var existingUser = _connection.QueryFirstOrDefault<User>("SELECT * FROM Users WHERE Username = @Username", new { model.Username });
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("Username", "This username is already taken.");
                        return View(model);
                    }

                    // Hash the password before storing it
                    string hashedPassword = HashPassword(model.Password);

                    // Insert new user
                    string sql = "INSERT INTO Users (Username, Password, Age, CreatedAt) VALUES (@Username, @Password, @Age, @CreatedAt)";
                    _connection.Execute(sql, new
                    {
                        model.Username,
                        Password = hashedPassword, // Use hashed password
                        model.Age,
                        CreatedAt = DateTime.Now
                    });

                    Debug.WriteLine($"User {model.Username} registered successfully.");
                    return RedirectToAction("Login", "Auth");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Signup error: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while registering. Please try again.");
                }
                finally
                {
                    CloseConnection(); // Ensure the connection is closed after the operation
                }
            }

            return View(model);
        }

        // Method to hash the password securely
        private static string HashPassword(string password)
        {
            try
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                    byte[] hashBytes = sha256.ComputeHash(passwordBytes);
                    return Convert.ToBase64String(hashBytes);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error while hashing password: {ex.Message}");
                throw new Exception("Error while hashing password", ex);
            }
        }

        // GET: Auth/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Auth/Login
        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            try
            {
                Debug.WriteLine("Attempting login.");

                User user = ValidateLoginFromDatabase(username, password);
                if (user != null)
                {
                    Session["UserId"] = user.Id;
                    Session["Username"] = user.Username;

                    Debug.WriteLine($"User {username} logged in at {DateTime.Now}.");

                    return RedirectToAction("Index", "Home");
                }

                Debug.WriteLine($"Failed login attempt for {username} at {DateTime.Now}.");
                ModelState.AddModelError("", "Invalid username or password.");
                return View();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred while logging in the user {username}: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while logging in. Please try again.");
                return View();
            }
        }

        // Helper method to validate login from the database
        private User ValidateLoginFromDatabase(string username, string password)
        {
            try
            {
                OpenConnection(); // Open the connection

                string query = "SELECT * FROM Users WHERE Username = @Username";
                var user = _connection.QueryFirstOrDefault<User>(query, new { Username = username });

                if (user != null && HashPassword(password) == user.Password)
                {
                    return user;
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error while validating user login for {username}: {ex.Message}");
                throw new Exception("Error while validating user login", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        // GET: Auth/Logout
        public ActionResult Logout()
        {
            try
            {
                Session.Clear();

                Debug.WriteLine($"User {Session["Username"]} logged out at {DateTime.Now}.");

                return RedirectToAction("Login", "Auth");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred during logout: {ex.Message}");
                return RedirectToAction("Login", "Auth");
            }
        }
    }
}
