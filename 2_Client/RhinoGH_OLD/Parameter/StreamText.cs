using System.Drawing;
using Rhino.Geometry;

namespace GeoGrasshopper
{
    public class StreamText
    {
        private Point3d position;
        private Vector3d normal;
        private Color color;
        private string text;
        private int textSize;

        private static StreamText _default = new StreamText("notSet");

        public StreamText(string text)
        {
            position = new Point3d(0,0,0);
            normal = Vector3d.ZAxis;
            color = Color.Black;
            this.text = text;
            textSize = 14;
        }

        public static StreamText Default
        {
            get { return _default; }
            set { _default = value; }
        }

        public Point3d Position
        {
            get { return position; }
            set { position = value; }
        }

        public Vector3d Normal
        {
            get { return normal; }
            set { normal = value; }
        }

        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        public int TextSize
        {
            get { return textSize; }
            set { textSize = value; }
        }
    }
}