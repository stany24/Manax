using System.Data.SQLite;

namespace ECSDatabase;

public static class Database
{
    internal static SQLiteConnection Connection { get; private set; }
    public static void Connect(string storagePath)
    {
        string dbPath = Path.Combine(storagePath,"Manax.sqlite");
        string connectionString = $"Data Source={dbPath};Version=3;";
        try
        {
            Connection = new SQLiteConnection(connectionString);
            Connection.Open();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error connecting to database: {e.Message}");
        }
    }

    public static void Check()
    {
        string sql = "CREATE TABLE IF NOT EXIST entities (id INTEGER PRIMARY KEY, type TEXT)";
        using (SQLiteCommand command = new SQLiteCommand(sql, Connection))
        {
            object? result = command.ExecuteScalar();
            if (result == null)
            {
                Console.WriteLine("Table 'entities' does not exist. Creating...");
            }
        }
    }
}