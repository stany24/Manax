namespace Server.Environement
{
    public static class EnvLoader
    {
        public static void Load()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Environement", ".env");
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Env file not found");
                return;
            }

            foreach (string line in File.ReadAllLines(filePath))
            {
                string[] parts = line.Split(
                    '=',
                    StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length != 2)
                    continue;
                Environment.SetEnvironmentVariable(parts[0], parts[1]);
            }

            string? path = Environment.GetEnvironmentVariable(nameof(Variables.STORAGE_PATH));
            if (path == null)
            {
                Environment.SetEnvironmentVariable(nameof(Variables.STORAGE_PATH),AppDomain.CurrentDomain.BaseDirectory);
            }
        }
        
        public static bool IsDebug()
        {
            string? debug = Environment.GetEnvironmentVariable(nameof(Variables.DEBUG));
            return debug != null && debug.ToLower().Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        public static void Print()
        {
            if(!IsDebug()) {return;}
            foreach (Variables key in Enum.GetValues(typeof(Variables)))
            {
                string? value = Environment.GetEnvironmentVariable(key.ToString());
                Console.WriteLine(value != null ? $"{key}: {value}" : $"{key}: not set");
            }
        }
    }
}