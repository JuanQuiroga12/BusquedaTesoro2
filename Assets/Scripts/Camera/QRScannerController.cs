using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using System.Collections;
using System.Collections.Generic;
using BusquedaTesoro.Core;

namespace BusquedaTesoro.Camera
{
    /// <summary>
    /// Captura un frame de la cámara al presionar el botón "Escanear",
    /// lo decodifica con ZXing, y envía el resultado al GameStateManager.
    /// No escanea continuamente — solo al presionar el botón.
    /// </summary>
    public class QRScannerController : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private WebCamController webCamController;
        [SerializeField] private GameStateManager gameStateManager;
        [SerializeField] private Button scanButton;

        [Header("Configuración")]
        [Tooltip("Segundos de espera entre escaneos para evitar lecturas duplicadas")]
        [SerializeField] private float scanCooldown = 1.5f;

        [Tooltip("Ancho máximo al que se reduce la imagen para decodificar (mejora detección de cerca)")]
        [SerializeField] private int maxDecodeWidth = 640;

        private float lastScanTime = -999f;
        private bool isProcessing = false;

        private void Start()
        {
            if (scanButton != null)
            {
                scanButton.onClick.AddListener(OnScanButtonPressed);
            }
            else
            {
                Debug.LogError("[QRScannerController] No se asignó el botón de escaneo.");
            }

            // Suscribirse a eventos del GameStateManager para controlar el botón
            if (gameStateManager != null)
            {
                gameStateManager.OnClueCompleted += HandleClueCompleted;
                gameStateManager.OnClueRevealed += HandleClueRevealed;
                gameStateManager.OnGameWon += HandleGameWon;
            }
        }

        /// <summary>
        /// Se ejecuta al presionar el botón "Escanear".
        /// </summary>
        public void OnScanButtonPressed()
        {
            if (Time.time - lastScanTime < scanCooldown)
            {
                return;
            }

            if (!webCamController.IsCameraReady)
            {
                Debug.LogWarning("[QRScannerController] La cámara no está lista.");
                return;
            }

            if (isProcessing)
            {
                return;
            }

            isProcessing = true;
            lastScanTime = Time.time;

            string result = DecodeQRFromCamera();

            if (!string.IsNullOrEmpty(result))
            {
                Debug.Log($"[QRScannerController] QR detectado: '{result}'");
                // Deshabilitar botón ANTES de procesar para evitar avances múltiples
                SetScanButtonEnabled(false);
                gameStateManager.ProcessScannedQR(result);
            }
            else
            {
                Debug.Log("[QRScannerController] No se detectó ningún QR en el frame.");
            }

            isProcessing = false;
        }

        /// <summary>
        /// Lee el frame actual de la WebCamTexture y lo decodifica con ZXing.
        /// Reduce la resolución para mejorar la detección a corta distancia.
        /// </summary>
        private string DecodeQRFromCamera()
        {
            WebCamTexture camTexture = webCamController.CameraTexture;

            if (camTexture == null || !camTexture.isPlaying)
            {
                Debug.LogWarning("[QRScannerController] WebCamTexture no disponible.");
                return null;
            }

            try
            {
                Color32[] pixels = camTexture.GetPixels32();
                int width = camTexture.width;
                int height = camTexture.height;

                // --- Reducir resolución si es necesario ---
                // Cuando el QR está muy cerca, la imagen es demasiado grande
                // y ZXing no la procesa bien. Reducir mejora la detección.
                if (width > maxDecodeWidth)
                {
                    float scale = (float)maxDecodeWidth / width;
                    int newWidth = maxDecodeWidth;
                    int newHeight = Mathf.RoundToInt(height * scale);

                    Color32[] downscaled = DownscalePixels(pixels, width, height, newWidth, newHeight);
                    pixels = downscaled;
                    width = newWidth;
                    height = newHeight;
                }

                // --- Invertir filas (WebCamTexture va de abajo hacia arriba) ---
                Color32[] flippedPixels = new Color32[pixels.Length];
                for (int y = 0; y < height; y++)
                {
                    int srcRow = y * width;
                    int dstRow = (height - 1 - y) * width;
                    System.Array.Copy(pixels, srcRow, flippedPixels, dstRow, width);
                }

                // --- Convertir a byte[] RGBA32 y decodificar ---
                byte[] rgbaBytes = new byte[flippedPixels.Length * 4];
                for (int i = 0; i < flippedPixels.Length; i++)
                {
                    rgbaBytes[i * 4] = flippedPixels[i].r;
                    rgbaBytes[i * 4 + 1] = flippedPixels[i].g;
                    rgbaBytes[i * 4 + 2] = flippedPixels[i].b;
                    rgbaBytes[i * 4 + 3] = flippedPixels[i].a;
                }

                var reader = new BarcodeReaderGeneric();
                reader.Options.TryHarder = true;
                reader.Options.PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE };

                var result = reader.Decode(rgbaBytes, width, height, RGBLuminanceSource.BitmapFormat.RGBA32);

                if (result != null)
                {
                    return result.Text;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[QRScannerController] Error al decodificar: {e.Message}");
            }

            return null;
        }

        /// <summary>
        /// Reduce la resolución de un array de píxeles usando muestreo por vecino más cercano.
        /// </summary>
        private Color32[] DownscalePixels(Color32[] source, int srcWidth, int srcHeight, int dstWidth, int dstHeight)
        {
            Color32[] result = new Color32[dstWidth * dstHeight];
            float xRatio = (float)srcWidth / dstWidth;
            float yRatio = (float)srcHeight / dstHeight;

            for (int y = 0; y < dstHeight; y++)
            {
                int srcY = Mathf.FloorToInt(y * yRatio);
                for (int x = 0; x < dstWidth; x++)
                {
                    int srcX = Mathf.FloorToInt(x * xRatio);
                    result[y * dstWidth + x] = source[srcY * srcWidth + srcX];
                }
            }

            return result;
        }

        // ═══════════════════════════════════════════
        // CONTROL DEL BOTÓN DE ESCANEO
        // ═══════════════════════════════════════════

        /// <summary>
        /// Habilita o deshabilita el botón de escaneo.
        /// </summary>
        public void SetScanButtonEnabled(bool enabled)
        {
            if (scanButton != null)
            {
                scanButton.interactable = enabled;
            }
        }

        // Cuando se completa una pista → deshabilitar escaneo (esperar "Siguiente Pista")
        private void HandleClueCompleted(int index)
        {
            SetScanButtonEnabled(false);
        }

        // Cuando se revela una nueva pista → rehabilitar escaneo
        // Cuando se revela una nueva pista → rehabilitar escaneo con delay
        private void HandleClueRevealed(int index)
        {
            // Dar tiempo para que el jugador aparte el celular del QR anterior
            StartCoroutine(EnableScanButtonAfterDelay(3f));
        }

        private IEnumerator EnableScanButtonAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            SetScanButtonEnabled(true);
        }

        // Cuando se gana el juego → deshabilitar escaneo
        private void HandleGameWon()
        {
            SetScanButtonEnabled(false);
        }

        private void OnDestroy()
        {
            if (scanButton != null)
            {
                scanButton.onClick.RemoveListener(OnScanButtonPressed);
            }

            if (gameStateManager != null)
            {
                gameStateManager.OnClueCompleted -= HandleClueCompleted;
                gameStateManager.OnClueRevealed -= HandleClueRevealed;
                gameStateManager.OnGameWon -= HandleGameWon;
            }
        }
    }
}