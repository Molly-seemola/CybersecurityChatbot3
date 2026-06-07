using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using CybersecurityChatbot.Models;

namespace CybersecurityChatbot
{
    public class DatabaseHelper
    {
        // *** CHANGE these to match your MySQL setup ***
        private const string ConnectionString =
            "Server=localhost;Database=cyberbot_db;Uid=root;Pwd=;";

        public static void InitialiseDatabase()
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                string createTable = @"
                    CREATE TABLE IF NOT EXISTS Tasks (
                        Id INT AUTO_INCREMENT PRIMARY KEY,
                        Title VARCHAR(200) NOT NULL,
                        Description TEXT,
                        IsCompleted BOOLEAN DEFAULT FALSE,
                        ReminderInfo VARCHAR(200),
                        ReminderDate DATETIME NULL,
                        DateAdded VARCHAR(50)
                    );";
                using var cmd = new MySqlCommand(createTable, conn);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("DB Init Error: " + ex.Message);
            }
        }

        public static int AddTask(TaskItem task)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                string sql = @"INSERT INTO Tasks (Title, Description, IsCompleted, ReminderInfo, ReminderDate, DateAdded)
                               VALUES (@title, @desc, @done, @reminder, @reminderDate, @added);
                               SELECT LAST_INSERT_ID();";
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@title", task.Title);
                cmd.Parameters.AddWithValue("@desc", task.Description ?? "");
                cmd.Parameters.AddWithValue("@done", task.IsCompleted);
                cmd.Parameters.AddWithValue("@reminder", task.ReminderInfo ?? "");
                cmd.Parameters.AddWithValue("@reminderDate", task.ReminderDate.HasValue ? (object)task.ReminderDate.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@added", task.DateAdded);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch (Exception ex)
            {
                Console.WriteLine("AddTask Error: " + ex.Message);
                return -1;
            }
        }

        public static List<TaskItem> GetAllTasks()
        {
            var tasks = new List<TaskItem>();
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                string sql = "SELECT * FROM Tasks ORDER BY Id;";
                using var cmd = new MySqlCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    tasks.Add(new TaskItem
                    {
                        Id = reader.GetInt32("Id"),
                        Title = reader.GetString("Title"),
                        Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? "" : reader.GetString("Description"),
                        IsCompleted = reader.GetBoolean("IsCompleted"),
                        ReminderInfo = reader.IsDBNull(reader.GetOrdinal("ReminderInfo")) ? "" : reader.GetString("ReminderInfo"),
                        ReminderDate = reader.IsDBNull(reader.GetOrdinal("ReminderDate")) ? (DateTime?)null : reader.GetDateTime("ReminderDate"),
                        DateAdded = reader.IsDBNull(reader.GetOrdinal("DateAdded")) ? "" : reader.GetString("DateAdded")
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetTasks Error: " + ex.Message);
            }
            return tasks;
        }

        public static bool CompleteTask(int id)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                string sql = "UPDATE Tasks SET IsCompleted = TRUE WHERE Id = @id;";
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch { return false; }
        }

        public static bool DeleteTask(int id)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                string sql = "DELETE FROM Tasks WHERE Id = @id;";
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch { return false; }
        }
    }
}
