namespace NodeLogger
{
    /// <summary>
    /// Utility class that provides various helper functions related to logging, file handling, and messaging.
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Sends a force message in the chat, visible only to the client.
        /// </summary>
        public static void ForceMessage(string message)
        {
            ChatBox.Instance.ForceMessage(message);
        }

        /// <summary>
        /// Writes a line to a specified file.
        /// </summary>
        public static void Log(string path, string line)
        {
            // Use StreamWriter to open the file and append a new line
            using StreamWriter writer = new(path, true);
            writer.WriteLine(line); // Write the new line
        }

        /// <summary>
        /// Creates a folder if it does not exist.
        /// </summary>
        public static void CreateFolder(string path, string logPath)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception ex)
            {
                Log(logPath, "Error [CreateFolder]: " + ex.Message);
            }
        }

        /// <summary>
        /// Creates a file if it does not exist.
        /// </summary>
        public static void CreateFile(string path, string logPath)
        {
            try
            {
                if (!File.Exists(path))
                {
                    using StreamWriter sw = File.CreateText(path);
                    sw.WriteLine("");
                }
            }
            catch (Exception ex)
            {
                Log(logPath, "Error [CreateFile]: " + ex.Message);
            }
        }

        /// <summary>
        /// Resets a file by overwriting its contents, essentially clearing it.
        /// </summary>
        public static void ResetFile(string path, string logPath)
        {
            try
            {
                if (File.Exists(path))
                {
                    using StreamWriter sw = new(path, false); // Opens file for overwriting
                }
            }
            catch (Exception ex)
            {
                Log(logPath, "Error [ResetFile]: " + ex.Message);
            }
        }
    }

    /// <summary>
    /// Provides various functions related to retrieving client-specific data.
    /// </summary>
    public class ClientData
    {
        /// <summary>
        /// Returns the GameObject representing the client player.
        /// </summary>
        public static GameObject GetClientObject()
        {
            return GameObject.Find("/Player");
        }

        /// <summary>
        /// Returns the Rigidbody component of the client player, if available.
        /// </summary>
        public static Rigidbody GetClientBody()
        {
            return GetClientObject()?.GetComponent<Rigidbody>();
        }
    }

    public class ServerData
    {
        /// <summary>
        /// Returns the ID of the currently active game mode.
        /// </summary>
        /// <returns>Integer representing the ID of the current game mode.</returns>
        public static int GetModeId()
        {
            return LobbyManager.Instance.gameMode.id;
        }

        /// <summary>
        /// Returns the ID of the currently active map.
        /// </summary>
        /// <returns>Integer representing the ID of the current map.</returns>
        public static int GetMapId()
        {
            return LobbyManager.Instance.map.id;
        }
    }
}
