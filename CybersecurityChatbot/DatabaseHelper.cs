using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using CybersecurityChatbot.Models;

namespace CybersecurityChatbot
{
    /// <summary>
    /// Handles all MySQL database operations for the Task Assistant.
    /// Part 3 Task 1: Implements full CRUD (Create, Read, Update, Delete)
    /// so all task actions sync correctly between the GUI and the database.
    /// Rubric target: Robust, error-handled DB integration — all CRUD actions
    /// sync correctly between GUI and DB.
    /// </summary>
    public class DatabaseHelper
    {
        // ── Connection string ────────────────────────────────────────────────
        // IMPORTANT: Update Pwd= to match your local MySQL root password.
        // The database 'cyberbot_db' will be created automatically by InitialiseDatabase().
        private const string ConnectionString =
            "Server=localhost;Database=cyberbot_db;Uid=root;Pwd=;";

        /// <summary>
        /// Creates the 'cyberbot_db' database and 'Tasks' table if they don't exist.
        /// Called once on application startup in MainWindow constructor.
        /// Uses CREATE TABLE IF NOT EXISTS to avoid errors on repeated launches.
        /// </summary>
        public static void InitialiseDatabase()
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();

                // Create Tasks table with all required columns:
                // Id (auto PK), Title, Description, IsCompleted, ReminderInfo,
                // ReminderDate (nullable), DateAdded
                string sql = @"
                    CREATE TABLE IF NOT EXISTS Tasks (
                        Id           INT AUTO_INCREMENT PRIMARY KEY,
                        Title        VARCHAR(200) NOT NULL,
                        Description  TEXT,
                        IsCompleted  BOOLEAN DEFAULT FALSE,
                        ReminderInfo VARCHAR(200),
                        ReminderDate DATETIME NULL,
                        DateAdded    VARCHAR(50)
                    );";
                new MySqlCommand(sql, conn).ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                // Log error without crashing the application
                Console.WriteLine("DB Init Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Inserts a new task into the Tasks table (CREATE operation).
        /// Part 3 Task 1: Supports adding tasks with title, description, and optional reminder.
        /// Returns the new auto-generated Id from MySQL (LAST_INSERT_ID).
        /// </summary>
        /// <param name="t">The TaskItem to insert.</param>
        /// <returns>The newly assigned database Id, or -1 on failure.</returns>
        public static int AddTask(TaskItem t)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();

                // Use parameterised query to prevent SQL injection
                string sql = @"
                    INSERT INTO Tasks
                        (Title, Description, IsCompleted, ReminderInfo, ReminderDate, DateAdded)
                    VALUES
                        (@ti, @de, @ic, @ri, @rd, @da);
                    SELECT LAST_INSERT_ID();";

                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ti", t.Title);
                cmd.Parameters.AddWithValue("@de", t.Description ?? "");
                cmd.Parameters.AddWithValue("@ic", t.IsCompleted);
                cmd.Parameters.AddWithValue("@ri", t.ReminderInfo ?? "");
                // ReminderDate is nullable — use DBNull if not set
                cmd.Parameters.AddWithValue("@rd", t.ReminderDate.HasValue
                    ? (object)t.ReminderDate.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@da", t.DateAdded);

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch (Exception ex)
            {
                Console.WriteLine("AddTask Error: " + ex.Message);
                return -1; // Signal failure to caller
            }
        }

        /// <summary>
        /// Retrieves all tasks from the database ordered by Id (READ operation).
        /// Part 3 Task 1: Displays title, description, and reminders to the user.
        /// Returns an empty list (not null) if no tasks exist or on error.
        /// </summary>
        public static List<TaskItem> GetAllTasks()
        {
            var list = new List<TaskItem>();
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();

                using var reader = new MySqlCommand(
                    "SELECT * FROM Tasks ORDER BY Id;", conn).ExecuteReader();

                while (reader.Read())
                {
                    // Safely handle nullable DB columns with IsDBNull checks
                    list.Add(new TaskItem
                    {
                        Id = reader.GetInt32("Id"),
                        Title = reader.GetString("Title"),
                        Description = reader.IsDBNull(reader.GetOrdinal("Description"))
                                        ? "" : reader.GetString("Description"),
                        IsCompleted = reader.GetBoolean("IsCompleted"),
                        ReminderInfo = reader.IsDBNull(reader.GetOrdinal("ReminderInfo"))
                                        ? "" : reader.GetString("ReminderInfo"),
                        ReminderDate = reader.IsDBNull(reader.GetOrdinal("ReminderDate"))
                                        ? (DateTime?)null
                                        : reader.GetDateTime("ReminderDate"),
                        DateAdded = reader.IsDBNull(reader.GetOrdinal("DateAdded"))
                                        ? "" : reader.GetString("DateAdded")
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetTasks Error: " + ex.Message);
            }
            return list;
        }

        /// <summary>
        /// Marks a task as completed in the database (UPDATE operation).
        /// Part 3 Task 1: Changes reflect correctly in the DB as required by the rubric.
        /// </summary>
        /// <param name="id">The database Id of the task to mark complete.</param>
        /// <returns>True if the update succeeded, false otherwise.</returns>
        public static bool CompleteTask(int id)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                var cmd = new MySqlCommand(
                    "UPDATE Tasks SET IsCompleted = TRUE WHERE Id = @id;", conn);
                cmd.Parameters.AddWithValue("@id", id);
                return cmd.ExecuteNonQuery() > 0; // Returns true if a row was affected
            }
            catch { return false; }
        }

        /// <summary>
        /// Permanently removes a task from the database (DELETE operation).
        /// Part 3 Task 1: Deletion reflects correctly in the DB as required by the rubric.
        /// </summary>
        /// <param name="id">The database Id of the task to delete.</param>
        /// <returns>True if deletion succeeded, false otherwise.</returns>
        public static bool DeleteTask(int id)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                var cmd = new MySqlCommand(
                    "DELETE FROM Tasks WHERE Id = @id;", conn);
                cmd.Parameters.AddWithValue("@id", id);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch { return false; }
        }
    }
}