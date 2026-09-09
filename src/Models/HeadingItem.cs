using System.Collections.Generic;
using System.Windows;

namespace MDPlus.Models
{
    public class HeadingItem
    {
        public int Level { get; set; } = 1;
        public string Text { get; set; } = string.Empty;
        public string Anchor { get; set; } = string.Empty;
        public Thickness IndentMargin => new Thickness((Level - 1) * 14, 2, 4, 2);
        public List<HeadingItem> Children { get; set; } = new List<HeadingItem>();
    }
}
