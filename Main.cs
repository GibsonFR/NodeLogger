using static NodeLogger.MainConstants;
using static NodeLogger.MainUtility;

namespace NodeLogger
{
    /// <summary>
    /// Stores constant values used throughout the application, such as update frequencies for client and server.
    /// </summary>
    public class MainConstants
    {
        public const float CLIENT_UPDATE_FREQUENCY = 1f;
        public const float SERVER_UPDATE_FREQUENCY = 5f;
    }

    /// <summary>
    /// A MonoBehaviour class that triggers the client and server updates using coroutines.
    /// </summary>
    public class MainManager : MonoBehaviour
    {
        /// <summary>
        /// Called when the script instance is being loaded.
        /// Starts the coroutines for both client and server updates.
        /// </summary>
        void Awake()
        {
            // Start the coroutine to handle client updates every second
            StartCoroutine(UpdateClientCoroutine().WrapToIl2Cpp());

            // Start the coroutine to handle server updates (if applicable)
            StartCoroutine(UpdateServerCoroutine().WrapToIl2Cpp());
        }
    }

    /// <summary>
    /// Utility class containing methods for client and server updates, including coroutines.
    /// </summary>
    public class MainUtility
    {
        /// <summary>
        /// Coroutine that handles client updates at regular intervals.
        /// </summary>
        public static IEnumerator UpdateClientCoroutine()
        {
            while (true)
            {
                // Perform client update
                BasicUpdateClient();

                // Wait for 1 second before the next update
                yield return new WaitForSeconds(CLIENT_UPDATE_FREQUENCY);
            }
        }

        /// <summary>
        /// Coroutine that handles server updates at regular intervals.
        /// </summary>
        public static IEnumerator UpdateServerCoroutine()
        {
            while (true)
            {
                // Perform server update
                MainUpdateServer();

                // Wait for 5 seconds before the next server update (adjust time as needed)
                yield return new WaitForSeconds(SERVER_UPDATE_FREQUENCY);
            }
        }

        /// <summary>
        /// Updates the server-related data by retrieving the current game mode ID and map ID.
        /// </summary>
        public static void MainUpdateServer()
        {
            modeId = GetModeId();
            mapId = GetMapId();
        }

        /// <summary>
        /// Updates data related to the client, specifically the Rigidbody and GameObject if available.
        /// Only updates if the client has a Rigidbody (i.e., is alive).
        /// </summary>
        public static void BasicUpdateClient()
        {
            clientBody = GetClientBody();
            if (clientBody == null) return;

            clientObject = GetClientObject();
        }
    }
}
