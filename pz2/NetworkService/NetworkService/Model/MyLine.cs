using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NetworkService.Model
{
    public class MyLine
    {
        private const int row_num = 4;
        private const int col_num = 4;

        public int Ind1 { get; set; }
        public int Ind2 { get; set; }
        public Line Line { get; set; }

        public MyLine()
        {

        }

        public MyLine(int ind1, int ind2)
        {
            Ind1 = ind1;
            Ind2 = ind2;

            UpdateLine();

        }

        public void UpdateLine()
        {
            int y1 = Ind1 / 4;
            int x1 = Ind1 % 4;
            int y2 = Ind2 / 4;
            int x2 = Ind2 % 4;

            Line = new Line();

            Line.Stroke = System.Windows.Media.Brushes.Black;
            Line.SnapsToDevicePixels = true;
            Line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);

            Line.X1 = 50 + x1 * 100;
            Line.X2 = 50 + x2 * 100;
            Line.Y1 = 50 + y1 * 100;
            Line.Y2 = 50 + y2 * 100;

            Line.StrokeThickness = 3;
        }

    }
}
