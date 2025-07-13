using System.Data.SQLite;
using System.Text.RegularExpressions;
using ManaxLibrary.DTOs.Serie;
using ManaxLibrary.DTOs.User;
using ManaxServer.Models;
using ManaxServer.Models.Chapter;
using ManaxServer.Models.Library;
using ManaxServer.Models.Read;
using ManaxServer.Models.Serie;
using ManaxServer.Models.User;
using Microsoft.Extensions.Logging;

namespace KomgaMigrationTool
{
    public class MigrationTool(string sourceDbPath, ManaxContext destinationContext, ILogger<MigrationTool> logger)
    {
        private readonly string _sourceConnectionString = $"Data Source={sourceDbPath};Version=3;";
        private readonly Dictionary<string, long> _userIdMapping = new();
        private readonly Dictionary<string, long> _libraryIdMapping = new();
        private readonly Dictionary<string, long> _serieIdMapping = new();
        private readonly Dictionary<string, long> _bookToChapterIdMapping = new();
        // Dictionnaire pour suivre les noms d'utilisateurs utilisés pour éviter les doublons
        private readonly HashSet<string> _usedUsernames = [];

        public void MigrateData()
        {

            logger.LogInformation("Starting data migration...");
            
            // Migrate users first as they are referenced by other entities
            MigrateUsers();
            
            // Migrate libraries and their dependencies
            MigrateLibraries();
            
            // Migrate series which depend on libraries
            MigrateSeries();
            
            // Migrate books to chapters which depend on series
            MigrateBooks();
            
            // Migrate read progress which depends on users and chapters
            MigrateReadProgress();
            
            logger.LogInformation("Data migration completed successfully.");
        }

        private void MigrateUsers()
        {
            logger.LogInformation("Migrating users...");
            
            using SQLiteConnection connection = new(_sourceConnectionString);
            connection.Open();
            
            using SQLiteCommand command = new("SELECT * FROM \"USER\"", connection);
            using SQLiteDataReader? reader = command.ExecuteReader();
            
            List<User> userList = [];
            
            while (reader.Read())
            {
                string sourceId = reader.GetValue(reader.GetOrdinal("ID")).ToString();
                string username = GetStringOrDefault(reader, "EMAIL");
                string passwordHash = GetStringOrDefault(reader, "PASSWORD");
                bool isAdmin = GetIntOrDefault(reader, "IS_ADMIN") == 1;
                
                // Si username est null ou vide, utiliser l'ID comme base
                if (string.IsNullOrWhiteSpace(username))
                {
                    username = $"user_{sourceId}";
                }
                
                // Assurer l'unicité du nom d'utilisateur
                string uniqueUsername = EnsureUniqueUsername(username);
                
                User user = new()
                {
                    Username = uniqueUsername,
                    PasswordHash = passwordHash,
                    Role = isAdmin ? UserRole.Admin : UserRole.User
                };
                
                userList.Add(user);
                destinationContext.Users.Add(user);
                
                // Enregistrer chaque utilisateur individuellement pour éviter les problèmes de contraintes
                try 
                {
                    destinationContext.SaveChanges();
                    _userIdMapping[sourceId] = user.Id;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Failed to save user {uniqueUsername} from source ID {sourceId}");
                }
            }
            
            logger.LogInformation($"Migrated {userList.Count} users.");
        }
        
        // Méthode pour assurer l'unicité des noms d'utilisateur
        private string EnsureUniqueUsername(string baseUsername)
        {
            if (!_usedUsernames.Contains(baseUsername))
            {
                _usedUsernames.Add(baseUsername);
                return baseUsername;
            }
            
            int counter = 1;
            string candidateUsername;
            do 
            {
                candidateUsername = $"{baseUsername}_{counter}";
                counter++;
            } while (_usedUsernames.Contains(candidateUsername));
            
            _usedUsernames.Add(candidateUsername);
            return candidateUsername;
        }
        
        private void MigrateLibraries()
        {
            logger.LogInformation("Migrating libraries...");
            
            using SQLiteConnection connection = new(_sourceConnectionString);
            connection.Open();
            
            using SQLiteCommand command = new("SELECT * FROM LIBRARY", connection);
            using SQLiteDataReader? reader = command.ExecuteReader();
            
            List<Library> libraryList = [];
            
            while (reader.Read())
            {
                string name = GetStringOrDefault(reader, "NAME");
                GetIntOrDefault(reader, "TYPE");
                string folderPath = GetStringOrDefault(reader, "ROOT");
                
                Library library = new()
                {
                    Name = name,
                    Path = folderPath
                };
                
                libraryList.Add(library);
                destinationContext.Libraries.Add(library);
            }
            
            destinationContext.SaveChanges();
            
            // Map source IDs to destination IDs
            for (int i = 0; i < libraryList.Count; i++)
            {
                using SQLiteCommand sourceIdCommand = new($"SELECT ID FROM LIBRARY LIMIT 1 OFFSET {i}", connection);
                string sourceId = Convert.ToString(sourceIdCommand.ExecuteScalar());
                _libraryIdMapping[sourceId] = libraryList[i].Id;
            }
            
            logger.LogInformation($"Migrated {libraryList.Count} libraries.");
        }
        
        private void MigrateSeries()
        {
            logger.LogInformation("Migrating series...");
            
            using SQLiteConnection connection = new(_sourceConnectionString);
            connection.Open();
            
            using SQLiteCommand command = new("SELECT * FROM SERIES", connection);
            using SQLiteDataReader? reader = command.ExecuteReader();
            
            List<Serie> seriesList = [];
            
            while (reader.Read())
            {
                string sourceId = GetStringOrDefault(reader, "ID");
                string name = GetStringOrDefault(reader, "NAME");
                string folderPath = GetStringOrDefault(reader, "URL");
                string sourceLibraryId = GetStringOrDefault(reader, "LIBRARY_ID");
                
                // Skip if library doesn't exist in mapping
                if (!_libraryIdMapping.TryGetValue(sourceLibraryId, out long libraryId))
                {
                    logger.LogWarning($"Skipping series {sourceId} because library {sourceLibraryId} was not migrated.");
                    continue;
                }
                
                Serie serie = new()
                {
                    Title = name,
                    LibraryId = libraryId,
                    Path = folderPath,
                    Status = Status.Ongoing
                };
                
                seriesList.Add(serie);
                destinationContext.Series.Add(serie);
            }
            
            destinationContext.SaveChanges();
            
            // Map source IDs to destination IDs
            for (int i = 0; i < seriesList.Count; i++)
            {
                using SQLiteCommand sourceIdCommand = new($"SELECT ID FROM SERIES LIMIT 1 OFFSET {i}", connection);
                if (sourceIdCommand.ExecuteScalar() == null) continue;
                string sourceId = Convert.ToString(sourceIdCommand.ExecuteScalar());
                _serieIdMapping[sourceId] = seriesList[i].Id;
            }
            
            logger.LogInformation($"Migrated {seriesList.Count} series.");
        }
        
        private void MigrateBooks()
        {
            logger.LogInformation("Migrating books to chapters...");
            
            using SQLiteConnection connection = new(_sourceConnectionString);
            connection.Open();
            
            using SQLiteCommand command = new("SELECT * FROM BOOK", connection);
            using SQLiteDataReader? reader = command.ExecuteReader();
            
            List<Chapter> chaptersList = [];
            
            while (reader.Read())
            {
                string sourceId = GetStringOrDefault(reader, "ID");
                string fileName = GetStringOrDefault(reader, "NAME");
                string path = GetStringOrDefault(reader, "URL");
                string sourceSeriesId = GetStringOrDefault(reader, "SERIES_ID");
                float number = ExtractChapterNumber(fileName);
                
                // Skip if series doesn't exist in mapping
                if (!_serieIdMapping.TryGetValue(sourceSeriesId, out long serieId))
                {
                    logger.LogWarning($"Skipping book {sourceId} because series {sourceSeriesId} was not migrated.");
                    continue;
                }
                
                Chapter chapter = new()
                {
                    FileName = fileName,
                    Number = Convert.ToInt32(number),
                    SerieId = serieId,
                    Path = path
                };
                
                chaptersList.Add(chapter);
                destinationContext.Chapters.Add(chapter);
            }
            
            destinationContext.SaveChanges();
            
            // Map source IDs to destination IDs
            for (int i = 0; i < chaptersList.Count; i++)
            {
                using SQLiteCommand sourceIdCommand = new($"SELECT ID FROM BOOK LIMIT 1 OFFSET {i}", connection);
                if (sourceIdCommand.ExecuteScalar() == null) continue;
                string sourceId = Convert.ToString(sourceIdCommand.ExecuteScalar());
                _bookToChapterIdMapping[sourceId] = chaptersList[i].Id;
            }
            
            logger.LogInformation($"Migrated {chaptersList.Count} books to chapters.");
        }
        
        private void MigrateReadProgress()
        {
            logger.LogInformation("Migrating read progress...");
            
            using SQLiteConnection connection = new(_sourceConnectionString);
            connection.Open();
            
            using SQLiteCommand command = new("SELECT * FROM READ_PROGRESS", connection);
            using SQLiteDataReader? reader = command.ExecuteReader();
            
            int migratedCount = 0;
            
            while (reader.Read())
            {
                string sourceBookId = GetStringOrDefault(reader, "BOOK_ID");
                string sourceUserId = GetStringOrDefault(reader, "USER_ID");
                DateTime? lastModified = GetDateTimeOrDefault(reader, "LAST_MODIFIED");
                
                // Skip if book or user doesn't exist in mapping
                if (!_bookToChapterIdMapping.TryGetValue(sourceBookId, out long chapterId) ||
                    !_userIdMapping.TryGetValue(sourceUserId, out long userId))
                {
                    continue;
                }
                
                Read read = new()
                {
                    ChapterId = chapterId,
                    UserId = userId,
                    Date = lastModified ?? DateTime.UtcNow,
                };
                
                destinationContext.Reads.Add(read);
                migratedCount++;
                
                // Save in batches to prevent memory issues
                if (migratedCount % 1000 == 0)
                {
                    destinationContext.SaveChanges();
                }
            }
            
            destinationContext.SaveChanges();
            
            logger.LogInformation($"Migrated {migratedCount} read progress records.");
        }
        
        private static float ExtractChapterNumber(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return 0;
                
            // Look for chapter patterns like "Ch.123", "Chapter 123", "c123", etc.
            Match match = Regex.Match(fileName, @"(?:ch(?:apter)?\.?\s*)(\d+(?:\.\d+)?)");
            
            if (match.Success && float.TryParse(match.Groups[1].Value, out float chapterNumber))
            {
                return chapterNumber;
            }
            
            // Try to extract just a number if nothing else works
            match = Regex.Match(fileName, @"(\d+(?:\.\d+)?)");
            
            if (match.Success && float.TryParse(match.Groups[1].Value, out chapterNumber))
            {
                return chapterNumber;
            }
            
            return 0;
        }
        
        #region Helper methods for handling null values from SQLite reader
        
        private static string GetStringOrDefault(SQLiteDataReader reader, string columnName)
        {
            int ordinal;
            try
            {
                ordinal = reader.GetOrdinal(columnName);
            }
            catch (Exception)
            {
                return string.Empty;
            }
            
            return !reader.IsDBNull(ordinal) ? reader.GetString(ordinal) : string.Empty;
        }
        
        private static int GetIntOrDefault(SQLiteDataReader reader, string columnName, int defaultValue = 0)
        {
            int ordinal;
            try
            {
                ordinal = reader.GetOrdinal(columnName);
            }
            catch (Exception)
            {
                return defaultValue;
            }
            
            return !reader.IsDBNull(ordinal) ? reader.GetInt32(ordinal) : defaultValue;
        }
        
        private static long GetInt64OrDefault(SQLiteDataReader reader, string columnName, long defaultValue = 0)
        {
            int ordinal;
            try
            {
                ordinal = reader.GetOrdinal(columnName);
            }
            catch (Exception)
            {
                return defaultValue;
            }
            
            return !reader.IsDBNull(ordinal) ? reader.GetInt64(ordinal) : defaultValue;
        }
        
        private static bool GetBoolOrDefault(SQLiteDataReader reader, string columnName, bool defaultValue = false)
        {
            int ordinal;
            try
            {
                ordinal = reader.GetOrdinal(columnName);
            }
            catch (Exception)
            {
                return defaultValue;
            }
            
            return !reader.IsDBNull(ordinal) && reader.GetBoolean(ordinal);
        }
        
        private static DateTime? GetDateTimeOrDefault(SQLiteDataReader reader, string columnName)
        {
            int ordinal;
            try
            {
                ordinal = reader.GetOrdinal(columnName);
            }
            catch (Exception)
            {
                return null;
            }
            
            if (reader.IsDBNull(ordinal))
                return null;
                
            // SQLite stores dates in various formats, try to handle them
            object value = reader.GetValue(ordinal);
            
            if (value is DateTime dateTime)
                return dateTime;
                
            if (value is string dateString && DateTime.TryParse(dateString, out dateTime))
                return dateTime;
                
            if (value is long ticks)
                return new DateTime(ticks);
                
            return null;
        }
        
        #endregion
    }
}
