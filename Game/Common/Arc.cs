using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Game.Common
{
    public class Arc : Shape
    {
        public float ArcStart { get; set; }
        public float ArcEnd { get; set; }
        protected override Geometry DefiningGeometry
        {
            get
            {
                double maxWidth = RenderSize.Width;
                double maxHeight = RenderSize.Height;
                double maxRadius = Math.Min(maxWidth, maxHeight) / 2.0;

                PolarPoint arcStart = new PolarPoint(maxRadius, ArcStart);
                PolarPoint arcFinish = new PolarPoint(maxRadius, ArcEnd);

                StreamGeometry geom = new StreamGeometry();
                using (StreamGeometryContext ctx = geom.Open())
                {
                    ctx.BeginFigure(
                        new Point((maxWidth / 2.0) + arcStart.X,
                                   (maxHeight / 2.0) - arcStart.Y),
                        false,
                        false);
                    ctx.ArcTo(
                        new Point((maxWidth / 2.0) + arcFinish.X,
                                  (maxHeight / 2.0) - arcFinish.Y),
                        new Size(maxRadius, maxRadius),
                        0.0,     // rotationAngle
                        false,   // greater than 180 deg?
                        SweepDirection.Counterclockwise,
                        true,    // isStroked
                        true);
                }

                return geom;
            }
        }
    }
}