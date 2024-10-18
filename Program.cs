using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;




namespace Homework4
{
    class Program
    {
        private static string connectionString = "server=localhost;user=root;database= Records;port=3306;password=welcome1";

        static void Main(string[] args)
        {
            try
            {
                EnsureTableExists();
                bool keepRunning = true;
                while (keepRunning)
                {
                    Console.WriteLine("Choose an operation");
                    Console.WriteLine("1. Create a record");
                    Console.WriteLine("2. Read records");
                    Console.WriteLine("3. Update a record");
                    Console.WriteLine("4. Delete a record");
                    Console.WriteLine("5. Exit");
                    var choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "1":
                            CreateRecord();
                            break;
                        case "2":
                            ReadRecords();
                            break;
                        case "3":
                            UpdateRecord();
                            break;
                        case "4":
                            DeleteRecord();
                            break;
                        case "5":
                            keepRunning = false;
                            break;
                        default:
                            Console.WriteLine("Invalid choice, Try again");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static void CreateRecord()
        {
            string name = GetValidInput("Enter name: ", ValidateName);
            string email = GetValidInput("Enter email: ", ValidateEmail);
            string phone = GetValidInput("Enter phone number: ", ValidatePhone);
            string address = GetValidInput("Enter address: ", ValidateAddress);

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO USER (name, email, phone, address, created_at) VALUES(@name, @email, @phone, @address, NOW())";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@phone", phone);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@address", address);
                cmd.ExecuteNonQuery();
                Console.WriteLine("Record inserted successfully.");
            }
        }

        static void ReadRecords()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM USER";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Console.WriteLine($"ID: {reader["id"]}, Name: {reader["name"]}, Email: {reader["email"]}, Phone: {reader["phone"]}, Address: {reader["address"]}, Created At: {reader["created_at"]}");
                }
            }
        }

        static void UpdateRecord()
        {
            Console.Write("Enter the ID of the record to update: ");
            int id;
            if (!int.TryParse(Console.ReadLine(), out id))
            {
                Console.WriteLine("Invalid ID.");
                return;
            }

            Console.WriteLine("What would you like to update?");
            Console.WriteLine("1. Name");
            Console.WriteLine("2. Email");
            Console.WriteLine("3. Phone");
            Console.WriteLine("4. Address");
            var choice = Console.ReadLine();
            string columnToUpdate = null;
            string newValue = null;

            switch (choice)
            {
                case "1":
                    columnToUpdate = "name";
                    newValue = GetValidInput("Enter new name: ", ValidateName);
                    break;
                case "2":
                    columnToUpdate = "email";
                    newValue = GetValidInput("Enter new email: ", ValidateEmail);
                    break;
                case "3":
                    columnToUpdate = "phone";
                    newValue = GetValidInput("Enter new phone: ", ValidatePhone);
                    break;
                case "4":
                    columnToUpdate = "address";
                    newValue = GetValidInput("Enter new address: ", ValidateAddress);
                    break;
                default:
                    Console.WriteLine("Invalid choice.");
                    return;
            }

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = $"UPDATE USER SET {columnToUpdate} = @newValue WHERE id = @id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@newValue", newValue);
                cmd.Parameters.AddWithValue("@id", id);
                int result = cmd.ExecuteNonQuery();

                if (result > 0)
                {
                    Console.WriteLine("Record updated successfully.");
                }
                else
                {
                    Console.WriteLine("Record not found.");
                }
            }
        }

        static void DeleteRecord()
        {
            Console.Write("Enter the ID of the record to delete: ");
            int id;
            if (!int.TryParse(Console.ReadLine(), out id))
            {
                Console.WriteLine("Invalid ID.");
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "DELETE FROM USER WHERE id = @id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);
                int result = cmd.ExecuteNonQuery();

                if (result > 0)
                {
                    Console.WriteLine("Record deleted successfully.");
                }
                else
                {
                    Console.WriteLine("Record not found.");
                }
            }
        }

        static void EnsureTableExists()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS `USER` (
                    `id` INT AUTO_INCREMENT PRIMARY KEY,
                    `name` VARCHAR(100) NOT NULL,
                    `email` VARCHAR(100) NOT NULL,
                    `phone` VARCHAR(15) NOT NULL,
                    `address` VARCHAR(200) NOT NULL,
                    `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );";
                MySqlCommand cmd = new MySqlCommand(createTableQuery, conn);
                cmd.ExecuteNonQuery();
                Console.WriteLine("Table 'USER' is ensured to exist.");
            }
        }

        // Validation Methods
        static bool ValidateName(string name) => !string.IsNullOrEmpty(name);

        static bool ValidateEmail(string email)
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        static bool ValidatePhone(string phone)
        {
            return phone.All(char.IsDigit) && phone.Length >= 10;
        }

        static bool ValidateAddress(string address) => !string.IsNullOrEmpty(address);

        static string GetValidInput(string prompt, Func<string, bool> validate)
        {
            string input;
            do
            {
                Console.Write(prompt);
                input = Console.ReadLine();
                if (!validate(input))
                {
                    Console.WriteLine("Invalid input. Please try again.");
                }
            }
            while (!validate(input));

            return input;
        }
    }
}
