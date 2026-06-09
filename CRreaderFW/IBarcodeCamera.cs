using System;

namespace WindowsFormsApp1
{
    internal interface IBarcodeCamera : IDisposable
    {
        event Action<BarcodeCapture> BarcodeCaptured;
        void Start();
        void TriggerOnce();
        void Stop();
    }

    internal sealed class BarcodeCapture
    {
        public string Barcode { get; private set; }
        public byte[] ImageBytes { get; private set; }
        public string ImageExtension { get; private set; }

        public BarcodeCapture(string barcode, byte[] imageBytes, string imageExtension)
        {
            Barcode = barcode ?? string.Empty;
            ImageBytes = imageBytes;
            ImageExtension = string.IsNullOrWhiteSpace(imageExtension) ? ".jpg" : imageExtension;
        }
    }
}
