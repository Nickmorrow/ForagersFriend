using Microsoft.Data.SqlClient;


string connectionString = "Server=LITTLEBEAR\\SQLEXPRESS;Database=Forager;Integrated Security=True;Trusted_Connection=True;TrustServerCertificate=True;";


try
{
    using (SqlConnection connection = new SqlConnection(connectionString))
    {
        connection.Open();
        Console.WriteLine("Connection successful!");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Connection failed: {ex.Message}");
}
