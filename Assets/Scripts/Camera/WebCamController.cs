using UnityEngine;
using UnityEngine.UI;
using System.Collections;

#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

namespace BusquedaTesoro.Camera
{
    /// <summary>
    /// Inicializa la cámara trasera del dispositivo y la muestra en un RawImage a pantalla completa.
    /// Maneja permisos en Android y corrección de rotación/aspecto.
    /// </summary>
    public class WebCamController : MonoBehaviour
    {
        [Header("Referencias UI")]
        [SerializeField] private RawImage cameraDisplay;

        [Header("Configuración")]
        [SerializeField] private int requestedWidth = 1280;
        [SerializeField] private int requestedHeight = 720;
        [SerializeField] private int requestedFPS = 30;

        private WebCamTexture webCamTexture;
        private bool isCameraInitialized = false;

        // Propiedad pública para que QRScannerController acceda a la textura
        public WebCamTexture CameraTexture => webCamTexture;
        public bool IsCameraReady => isCameraInitialized && webCamTexture != null && webCamTexture.isPlaying;

        private void Start()
        {
            StartCoroutine(InitializeCameraRoutine());
        }

        private IEnumerator InitializeCameraRoutine()
        {
            // --- Paso 1: Solicitar permisos en Android ---
#if PLATFORM_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Permission.RequestUserPermission(Permission.Camera);

                // Esperar hasta que el usuario responda al diálogo de permisos
                float timeout = 30f;
                float elapsed = 0f;
                while (!Permission.HasUserAuthorizedPermission(Permission.Camera) && elapsed < timeout)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
                {
                    Debug.LogError("[WebCamController] Permiso de cámara denegado por el usuario.");
                    yield break;
                }
            }
#endif

            // --- Paso 2: Esperar a que los dispositivos de cámara estén disponibles ---
            // En algunos dispositivos tarda un frame en detectar las cámaras
            yield return new WaitForEndOfFrame();

            if (WebCamTexture.devices.Length == 0)
            {
                Debug.LogError("[WebCamController] No se encontraron cámaras en el dispositivo.");
                yield break;
            }

            // --- Paso 3: Buscar la cámara trasera ---
            string backCameraName = null;
            for (int i = 0; i < WebCamTexture.devices.Length; i++)
            {
                if (!WebCamTexture.devices[i].isFrontFacing)
                {
                    backCameraName = WebCamTexture.devices[i].name;
                    break;
                }
            }

            // Si no hay cámara trasera, usar la primera disponible (útil para testing en PC)
            if (string.IsNullOrEmpty(backCameraName))
            {
                backCameraName = WebCamTexture.devices[0].name;
                Debug.LogWarning("[WebCamController] No se encontró cámara trasera. Usando: " + backCameraName);
            }

            // --- Paso 4: Crear e iniciar la WebCamTexture ---
            webCamTexture = new WebCamTexture(backCameraName, requestedWidth, requestedHeight, requestedFPS);
            cameraDisplay.texture = webCamTexture;
            webCamTexture.Play();

            // Esperar a que la cámara realmente arranque (puede tardar unos frames)
            int maxWaitFrames = 300; // ~5 segundos a 60fps
            int framesWaited = 0;
            while (!webCamTexture.didUpdateThisFrame && framesWaited < maxWaitFrames)
            {
                framesWaited++;
                yield return null;
            }

            if (webCamTexture.isPlaying)
            {
                isCameraInitialized = true;
                Debug.Log($"[WebCamController] Cámara iniciada: {backCameraName} ({webCamTexture.width}x{webCamTexture.height})");
                AdjustDisplayRotationAndAspect();
            }
            else
            {
                Debug.LogError("[WebCamController] La cámara no pudo iniciar.");
            }
        }

        /// <summary>
        /// Corrige la rotación y el aspecto del RawImage para que el feed
        /// se vea correctamente en orientación Portrait.
        /// </summary>
        private void AdjustDisplayRotationAndAspect()
        {
            if (webCamTexture == null) return;

            int rotation = webCamTexture.videoRotationAngle;
            bool mirrored = webCamTexture.videoVerticallyMirrored;
            bool isRotated = (rotation == 90 || rotation == 270);

            float screenW = Screen.width;
            float screenH = Screen.height;
            float texW = webCamTexture.width;
            float texH = webCamTexture.height;

            // Aplicar rotación
            cameraDisplay.rectTransform.localEulerAngles = new Vector3(0, 0, -rotation);

            // --- Corrección de aspecto + cobertura completa ---
            //
            // El RawImage está en stretch (= tamaño de pantalla: screenW x screenH).
            // La textura (texW x texH) se mapea a ese rect, lo que estira la imagen
            // si las proporciones no coinciden.
            //
            // Paso 1: Deshacer el estiramiento escalando cada eje por (texDim / screenDim).
            //         Esto hace que cada píxel de textura ocupe el mismo espacio en ambos ejes.
            //
            // Paso 2: Después de la rotación, las dimensiones efectivas de la textura
            //         en pantalla cambian. Calculamos el factor de cobertura para que
            //         la imagen cubra toda la pantalla (el excedente se recorta).

            // Dimensiones efectivas de la textura en relación a la pantalla después de rotar
            float effectiveTexW = isRotated ? texH : texW;
            float effectiveTexH = isRotated ? texW : texH;

            // Factor de cobertura: escalar hasta que ambos ejes cubran la pantalla
            float coverScale = Mathf.Max(screenW / effectiveTexW, screenH / effectiveTexH);

            // Escala final por eje: deshace el estiramiento y aplica cobertura
            float scaleX = (texW / screenW) * coverScale;
            float scaleY = (texH / screenH) * coverScale;

            // Aplicar espejo si la cámara lo requiere
            if (mirrored) scaleY = -scaleY;

            cameraDisplay.rectTransform.localScale = new Vector3(scaleX, scaleY, 1f);
        }

        private void OnDestroy()
        {
            if (webCamTexture != null && webCamTexture.isPlaying)
            {
                webCamTexture.Stop();
            }
        }

        private void OnApplicationPause(bool pause)
        {
            // Pausar/reanudar cámara cuando la app pierde/recupera foco
            if (webCamTexture == null) return;

            if (pause)
            {
                webCamTexture.Stop();
            }
            else if (isCameraInitialized)
            {
                webCamTexture.Play();
                AdjustDisplayRotationAndAspect();
            }
        }
    }
}