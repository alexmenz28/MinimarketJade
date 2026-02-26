using System.Drawing;
using System.Drawing.Imaging;
using ZXing;
using ZXing.Windows.Compatibility;

namespace MinimarketJade.Web.Helpers;

public static class BarcodeGenerator
{
    public static string GenerarBase64(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return "";

        try
        {
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.CODE_128,
                Options = new ZXing.Common.EncodingOptions
                {
                    Width = 300,
                    Height = 100,
                    Margin = 10
                }
            };

            using var bitmap = writer.Write(texto);
            using var ms = new MemoryStream();

            bitmap.Save(ms, ImageFormat.Png);
            return Convert.ToBase64String(ms.ToArray());
        }
        catch
        {
            return ""; 
        }
    }
}