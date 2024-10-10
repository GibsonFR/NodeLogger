namespace NodeLogger
{
    /// <summary>
    /// This class stores global variables to improve code readability and separation of concerns, primarily for use in Plugin.cs.
    /// </summary>
    public class Variables
    {
        //folder
        public static string assemblyFolderPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        public static string defaultFolderPath = assemblyFolderPath + "\\";
        public static string mainFolderPath = defaultFolderPath + @"NodeLogger\";

        //file
        public static string logFilePath = mainFolderPath + "log.txt";
        public static string nodeMapFolderPath = mainFolderPath + @"nodeMap\";

        //Rigidbody
        public static Rigidbody clientBody;

        //GameObject
        public static GameObject clientObject;

        // int
        public static int modeId, mapId;
    }
}
