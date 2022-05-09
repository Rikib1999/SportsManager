using Microsoft.Win32;
using System.IO;
using System.Windows.Media.Imaging;

namespace SportsManager.Others
{
    /// <summary>
    /// Static class for methods serving for image handling.
    /// </summary>
    public static class ImageHandler
    {
        /// <summary>
        /// Opens up file dialog window for selecting path for the image.
        /// </summary>
        /// <returns>Absolute path of the selected image. Null if unsuccessful.</returns>
        public static string SelectImagePath()
        {
            OpenFileDialog open = new();
            open.DefaultExt = ".png";
            open.Filter = "Pictures (*.jpg;*.png)|*.jpg;*.png";

            if (open.ShowDialog() == true)
            {
                return open.FileName;
            }

            return null;
        }

        /// <summary>
        /// Loads image from path to a bitmap.
        /// </summary>
        /// <param name="path">Absolute path to the image.</param>
        /// <returns>Bitmap containing the image.</returns>
        public static BitmapImage ImageToBitmap(string path)
        {
            MemoryStream ms = new();
            byte[] arrbytFileContent = File.ReadAllBytes(path);
            ms.Write(arrbytFileContent, 0, arrbytFileContent.Length);
            ms.Position = 0;

            BitmapImage bitmap = new();
            bitmap.BeginInit();
            bitmap.StreamSource = ms;
            bitmap.EndInit();
            return bitmap;
        }
    }

    /// <summary>
    /// Interface representing that object has an image assigned to it.
    /// </summary>
    public interface IHasImage
    {
        /// <summary>
        /// Path to the image.
        /// </summary>
        public string ImagePath { get; set; }
    }
}