using System.Drawing;
using System.Linq;

namespace TextAdventures.Quest.EditorControls
{
    internal static class XamlIconRenderer
    {
        internal static Bitmap RenderXaml(string name, int size)
        {
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            var resourceName = asm.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith("." + name + ".xaml", System.StringComparison.OrdinalIgnoreCase));
            if (resourceName == null) return null;
            using (var stream = asm.GetManifestResourceStream(resourceName))
            {
                var visual = System.Windows.Markup.XamlReader.Load(stream) as System.Windows.FrameworkElement;
                if (visual == null) return null;
                visual.Width = size;
                visual.Height = size;
                visual.Measure(new System.Windows.Size(size, size));
                visual.Arrange(new System.Windows.Rect(0, 0, size, size));
                var rtb = new System.Windows.Media.Imaging.RenderTargetBitmap(size, size, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
                rtb.Render(visual);
                rtb.Freeze();
                var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
                encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(rtb));
                using (var ms = new System.IO.MemoryStream())
                {
                    encoder.Save(ms);
                    ms.Position = 0;
                    using (var raw = new Bitmap(ms))
                        return new Bitmap(raw);
                }
            }
        }
    }
}
