using GMap.NET;
using GMap.NET.WindowsPresentation;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace Find_My_Boef.Model
{
    public class GMapPolygonColor : GMapMarker, IShapable
    {
        public List<PointLatLng> Points { get; set; }
        public Brush Stroke { get; set; }
        public Brush Fill { get; set; }

        public GMapPolygonColor(IEnumerable<PointLatLng> points, Brush stroke, Brush fill) : base(new PointLatLng())
        {
            Points = new List<PointLatLng>(points);
            Stroke = stroke;
            Fill = fill;
        }

        public override void Clear()
        {
            base.Clear();
            Points.Clear();
        }

        public virtual Path CreatePath(List<Point> localPath, bool addBlurEffect)
        {
            StreamGeometry streamGeometry = new();
            using (StreamGeometryContext streamGeometryContext = streamGeometry.Open())
            {
                streamGeometryContext.BeginFigure(localPath[0], isFilled: true, isClosed: true);
                streamGeometryContext.PolyLineTo(localPath, isStroked: true, isSmoothJoin: true);
            }

            streamGeometry.Freeze();
            Path path = new()
            {
                Data = streamGeometry
            };
            if (addBlurEffect)
            {
                BlurEffect blurEffect = new BlurEffect();
                blurEffect.KernelType = KernelType.Gaussian;
                blurEffect.Radius = 3.0;
                blurEffect.RenderingBias = RenderingBias.Performance;
                path.Effect = blurEffect;
            }

            path.Stroke = Stroke;
            path.StrokeThickness = 5.0;
            path.StrokeLineJoin = PenLineJoin.Round;
            path.StrokeStartLineCap = PenLineCap.Triangle;
            path.StrokeEndLineCap = PenLineCap.Square;
            path.Fill = Fill;
            path.IsHitTestVisible = false;

            return path;
        }
    }
}
