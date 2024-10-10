using static NodeLogger.NodeLoggingConstants;
using static NodeLogger.NodeLoggingUtility;

namespace NodeLogger
{
    /// <summary>
    /// Holds all constants used for logging messages and node size.
    /// </summary>
    public class NodeLoggingConstants
    {
        public const string PAUSED_LOGGING_MESSAGE = "Logging positions paused...";
        public const string UNPAUSED_LOGGING_MESSAGE = "Logging positions unpaused...";
        public const string STARTED_LOGGING_MESSAGE = "Started logging valid positions...";
        public const string STOPPED_LOGGING_MESSAGE = "Stopped logging valid positions. Saving to file...";
        public const string SET_CORNER_A_MESSAGE = "Corner A set. Please set Corner B.";
        public const string SET_CORNER_B_MESSAGE = "Corner B set. Logging surface nodes...";
        public const string SURFACE_LOGGED_MESSAGE = "Surface nodes logged successfully.";
        public const string NO_NODES_TO_REMOVE_MESSAGE = "No nodes to remove.";
        public const string NODES_REMOVED_MESSAGE = "Last logged nodes removed.";
        public const string PRACTICE_MODE_DETECTED_MESSAGE = "Practice mod detected, NodeLogging enabled!";
        public const string LOADING_NODES_MESSAGE = "Starting node map loading...";
        public const string FINISHED_LOADING_NODES_MESSAGE = "Finished loading nodes. Total loaded: ";
        public const string LOADED_NODES_PROGRESS_MESSAGE = "Loaded {0} nodes...";
        public const string NO_FILE_FOUND_MESSAGE = "File not found: {0}";
        public const string NODE_NAME = "Node";
        public const string INTERACT_LAYER_NAME = "Interact";
        public const float NODE_SIZE = 1f;
    }

    /// <summary>
    /// Handles the logging process, user interactions and controls the flow of logic.
    /// </summary>
    public class NodeLoggingManager : MonoBehaviour
    {
        private bool isLogging = false;  // Determines if the system is currently logging positions
        private bool pause = false;      // Pauses logging if true
        private bool init = false;       // Flag to check if the logging has been initialized
        private bool isPractice = false; // Flag to check if the gamemode is Practice
        private readonly bool isLoading = false; // Flag to check if loading of nodes from file occure
        private string nodeMapFile;
        private Vector3Int? cornerA = null;
        private Vector3Int? cornerB = null;
        private readonly Dictionary<Vector3Int, bool> loggedNodes = [];
        private readonly List<GameObject> validationMarkers = [];
        private readonly Stack<List<Vector3Int>> loggedNodeHistory = [];

        void Awake()
        {
            isPractice = GetModeId() == 13;
        }

        void Update()
        {
            if (!isPractice) return;

            // Toggle pause logging state when 'P' key is pressed, and ensure loading isn't in progress
            if (Input.GetKeyDown(KeyCode.P) && !isLoading)
            {
                pause = !pause;
                // Display appropriate message depending on pause state
                ForceMessage(pause ? PAUSED_LOGGING_MESSAGE : UNPAUSED_LOGGING_MESSAGE);
            }

            // Load nodes from file asynchronously when 'L' key is pressed, if no loading is in progress
            if (Input.GetKeyDown(KeyCode.L) && !isLoading)
            {
                pause = true;
                isLogging = true;
                nodeMapFile = $"{nodeMapFolderPath}{mapId}.txt";
                StartCoroutine(LoadNodesAsync(loggedNodes, validationMarkers).WrapToIl2Cpp());
            }

            // Remove selected nodes when 'F' key is pressed, and ensure no loading is in progress
            if (Input.GetKeyDown(KeyCode.F) && !isLoading)
            {
                ObjectOutliner[] selectedNodes = FindObjectsOfType<ObjectOutliner>();
                RemoveNodes(selectedNodes, loggedNodes, validationMarkers);
            }

            // Toggle logging on and off when 'T' key is pressed, if no loading is in progress
            if (Input.GetKeyDown(KeyCode.T) && !isLoading)
            {
                isLogging = !isLogging;

                if (isLogging)
                {
                    // If this is the first time logging, initialize the logging file
                    if (!init)
                    {
                        nodeMapFile = $"{nodeMapFolderPath}{mapId}.txt";
                        init = true;
                    }

                    // Display message indicating logging has started
                    ForceMessage(STARTED_LOGGING_MESSAGE);
                }
                else
                {
                    // Display message indicating logging has stopped and save the logged positions
                    ForceMessage(STOPPED_LOGGING_MESSAGE);
                    SaveLoggedPositions(nodeMapFile, loggedNodes);
                }
            }

            // Set corners A and B when paused and 'C' key is pressed, if no loading is in progress
            if (pause && Input.GetKeyDown(KeyCode.C) && !isLoading)
            {
                SetCorner(clientBody.transform.position, ref cornerA, ref cornerB, loggedNodes, loggedNodeHistory, validationMarkers);
            }

            // Remove the last logged nodes when paused and 'R' key is pressed, if no loading is in progress
            if (pause && Input.GetKeyDown(KeyCode.R) && !isLoading)
            {
                RemoveLastLoggedNodes(loggedNodeHistory, loggedNodes, validationMarkers, ref cornerA, ref cornerB);
            }

            // Log the player's position when logging is active and not paused
            if (isLogging && !pause)
            {
                LogPlayerPosition(clientBody.transform.position, loggedNodes, loggedNodeHistory, validationMarkers);
            }

        }
    }

    /// <summary>
    /// Utility class that provides the functionality for logging, setting corners, creating markers, etc.
    /// </summary>
    public class NodeLoggingUtility
    {
        /// <summary>
        /// Logs the player's position if the node is new, adds it to the history, and creates a validation marker.
        /// </summary>
        public static void LogPlayerPosition(Vector3 playerPosition, Dictionary<Vector3Int, bool> loggedNodes, Stack<List<Vector3Int>> loggedNodeHistory, List<GameObject> validationMarkers)
        {
            Vector3Int nodePosition = WorldToGrid(playerPosition, NODE_SIZE);

            // If the node is not yet logged, add it and create a validation marker
            if (AddNodeIfNotExists(nodePosition, loggedNodes))
            {
                loggedNodeHistory.Push([nodePosition]);
                CreateValidationMarker(nodePosition, Color.green, NODE_SIZE, validationMarkers);
            }
        }

        /// <summary>
        /// Adds the node to the loggedNodes dictionary if it does not already exist.
        /// </summary>
        private static bool AddNodeIfNotExists(Vector3Int nodePosition, Dictionary<Vector3Int, bool> loggedNodes)
        {
            // Only add the node if it doesn't already exist
            if (!loggedNodes.ContainsKey(nodePosition))
            {
                loggedNodes[nodePosition] = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets the corners A and B based on the player's position and logs all surface nodes between them.
        /// </summary>
        public static void SetCorner(Vector3 playerPosition, ref Vector3Int? cornerA, ref Vector3Int? cornerB, Dictionary<Vector3Int, bool> loggedNodes, Stack<List<Vector3Int>> loggedNodeHistory, List<GameObject> validationMarkers)
        {
            Vector3Int nodePosition = WorldToGrid(playerPosition, NODE_SIZE);

            if (cornerA == null)
            {
                cornerA = nodePosition;
                CreateValidationMarker(nodePosition, Color.red, NODE_SIZE, validationMarkers);
                ForceMessage(SET_CORNER_A_MESSAGE);
            }
            else if (cornerB == null)
            {
                cornerB = nodePosition;
                CreateValidationMarker(nodePosition, Color.red, NODE_SIZE, validationMarkers);
                ForceMessage(SET_CORNER_B_MESSAGE);
                LogSurfaceNodes(cornerA.Value, cornerB.Value, loggedNodes, loggedNodeHistory, validationMarkers, NODE_SIZE);
                cornerA = null;
                cornerB = null;
            }
        }

        /// <summary>
        /// Logs all the nodes within the surface defined by corner A and corner B.
        /// </summary>
        public static void LogSurfaceNodes(Vector3Int cornerA, Vector3Int cornerB, Dictionary<Vector3Int, bool> loggedNodes, Stack<List<Vector3Int>> loggedNodeHistory, List<GameObject> validationMarkers, float nodeSize)
        {
            int minX = Mathf.Min(cornerA.x, cornerB.x);
            int maxX = Mathf.Max(cornerA.x, cornerB.x);
            int minY = Mathf.Min(cornerA.y, cornerB.y);
            int maxY = Mathf.Max(cornerA.y, cornerB.y);
            int minZ = Mathf.Min(cornerA.z, cornerB.z);
            int maxZ = Mathf.Max(cornerA.z, cornerB.z);

            List<Vector3Int> currentSurfaceNodes = [];

            // Log all nodes in the surface defined by cornerA and cornerB
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    for (int z = minZ; z <= maxZ; z++)
                    {
                        Vector3Int nodePosition = new Vector3Int(x, y, z);

                        if (!loggedNodes.ContainsKey(nodePosition))
                        {
                            loggedNodes[nodePosition] = true;
                            currentSurfaceNodes.Add(nodePosition);
                            CreateValidationMarker(nodePosition, Color.green, nodeSize, validationMarkers);
                        }
                    }
                }
            }

            loggedNodeHistory.Push(currentSurfaceNodes);
            ForceMessage(SURFACE_LOGGED_MESSAGE);
        }

        /// <summary>
        /// Creates a validation marker at the specified node position.
        /// </summary>
        public static void CreateValidationMarker(Vector3Int nodePosition, Color color, float nodeSize, List<GameObject> validationMarkers)
        {
            Vector3 worldPosition = GridToWorld(nodePosition, nodeSize);
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = NODE_NAME;
            marker.layer = LayerMask.NameToLayer(INTERACT_LAYER_NAME);
            marker.transform.position = worldPosition;
            marker.transform.localScale = new Vector3(nodeSize, nodeSize, nodeSize);
            marker.GetComponent<Renderer>().material.color = color;

            validationMarkers.Add(marker);
        }

        /// <summary>
        /// Removes the last logged nodes and their corresponding validation markers.
        /// </summary>
        public static void RemoveLastLoggedNodes(Stack<List<Vector3Int>> loggedNodeHistory, Dictionary<Vector3Int, bool> loggedNodes, List<GameObject> validationMarkers, ref Vector3Int? cornerA, ref Vector3Int? cornerB)
        {
            if (loggedNodeHistory.Count > 0)
            {
                List<Vector3Int> lastNodes = loggedNodeHistory.Pop();

                foreach (Vector3Int node in lastNodes)
                {
                    if (loggedNodes.ContainsKey(node))
                    {
                        loggedNodes.Remove(node);
                    }

                    GameObject marker = validationMarkers.Find(m => WorldToGrid(m.transform.position, NODE_SIZE) == node);
                    if (marker != null)
                    {
                        validationMarkers.Remove(marker);
                        GameObject.Destroy(marker);
                    }
                }

                ForceMessage(NODES_REMOVED_MESSAGE);
            }
            else
            {
                ForceMessage(NO_NODES_TO_REMOVE_MESSAGE);
            }

            // Reset corners after removal
            cornerA = null;
            cornerB = null;
        }


        /// <summary>
        /// Removes the specified nodes from the logged nodes and destroys their validation markers.
        /// </summary>
        public static void RemoveNodes(ObjectOutliner[] selectedNodes, Dictionary<Vector3Int, bool> loggedNodes, List<GameObject> validationMarkers)
        {
            foreach (var node in selectedNodes)
            {
                Vector3Int nodePosition = WorldToGrid(node.transform.position, NODE_SIZE);
                ForceMessage($"Removing node in {nodePosition}");

                if (loggedNodes.ContainsKey(nodePosition))
                {
                    loggedNodes.Remove(nodePosition);
                }

                GameObject marker = validationMarkers.Find(m => WorldToGrid(m.transform.position, NODE_SIZE) == nodePosition);
                if (marker != null)
                {
                    validationMarkers.Remove(marker);
                    GameObject.Destroy(marker);
                }
            }
        }

        /// <summary>
        /// Saves the logged positions to a file.
        /// </summary>
        public static void SaveLoggedPositions(string nodeMapFile, Dictionary<Vector3Int, bool> loggedNodes)
        {
            using StreamWriter writer = new(nodeMapFile);
            foreach (var node in loggedNodes.Keys)
            {
                writer.WriteLine(node);
            }
        }

        /// <summary>
        /// Converts world coordinates to grid coordinates.
        /// </summary>
        public static Vector3Int WorldToGrid(Vector3 worldPos, float nodeSize)
        {
            return new Vector3Int(
                Mathf.FloorToInt(worldPos.x / nodeSize),
                Mathf.FloorToInt(worldPos.y / nodeSize),
                Mathf.FloorToInt(worldPos.z / nodeSize));
        }

        /// <summary>
        /// Converts grid coordinates to world coordinates.
        /// </summary>
        public static Vector3 GridToWorld(Vector3Int gridPos, float nodeSize)
        {
            return new Vector3(
                gridPos.x * nodeSize + nodeSize / 2,
                gridPos.y * nodeSize + nodeSize / 2,
                gridPos.z * nodeSize + nodeSize / 2);
        }

        /// <summary>
        /// Starts the coroutine to load nodes asynchronously from a file.
        /// </summary>
        public static IEnumerator LoadNodesAsync(Dictionary<Vector3Int, bool> loggedNodes, List<GameObject> validationMarkers)
        {
            yield return LoadNodeMapFromFileAsync($"{nodeMapFolderPath}{mapId}.txt", loggedNodes, validationMarkers);
        }

        /// <summary>
        /// Coroutine for loading nodes from a file asynchronously.
        /// </summary>
        public static IEnumerator LoadNodeMapFromFileAsync(string path, Dictionary<Vector3Int, bool> loggedNodes, List<GameObject> validationMarkers)
        {
            if (!IsValidFilePath(path))
            {
                yield break;
            }

            string[] lines = File.ReadAllLines(path);
            int nodeCount = 0;

            foreach (string line in lines)
            {
                if (ProcessLine(line, loggedNodes, validationMarkers, ref nodeCount))
                {
                    if (nodeCount % 10 == 0)
                    {
                        ForceMessage(string.Format(LOADED_NODES_PROGRESS_MESSAGE, nodeCount));
                        yield return new WaitForSeconds(0.01f); // Yield to prevent freezing
                    }
                }
            }

            ForceMessage($"{FINISHED_LOADING_NODES_MESSAGE} {nodeCount}");
        }

        /// <summary>
        /// Validates if the given file path is valid and file exists.
        /// </summary>
        private static bool IsValidFilePath(string path)
        {
            if (!File.Exists(path))
            {
                ForceMessage(string.Format(NO_FILE_FOUND_MESSAGE, path));
                return false;
            }
            return true;
        }

        /// <summary>
        /// Processes a line from the file and adds the node if valid.
        /// </summary>
        private static bool ProcessLine(string line, Dictionary<Vector3Int, bool> loggedNodes, List<GameObject> validationMarkers, ref int nodeCount)
        {
            if (TryParseVector3Int(line, out Vector3Int nodePosition))
            {
                if (!loggedNodes.ContainsKey(nodePosition))
                {
                    loggedNodes[nodePosition] = true;
                    nodeCount++;
                    CreateValidationMarker(nodePosition, Color.green, NODE_SIZE, validationMarkers);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Attempts to parse a string into a Vector3Int.
        /// </summary>
        private static bool TryParseVector3Int(string value, out Vector3Int result)
        {
            result = Vector3Int.zero;
            string[] parts = value.Trim('(', ')').Split(',');
            if (parts.Length == 3 &&
                int.TryParse(parts[0], out int x) &&
                int.TryParse(parts[1], out int y) &&
                int.TryParse(parts[2], out int z))
            {
                result = new Vector3Int(x, y, z);
                return true;
            }
            return false;
        }
    }
}
