using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MDPlus.Core.Mermaid
{
    /// <summary>
    /// Native WPF vector renderer for Mermaid flowcharts.
    /// Transforms parsed flowchart ASTs and computed layout geometries into
    /// crisp, theme-aware WPF vector UI elements (Canvas, Border, Path, Ellipse, TextBlock)
    /// wrapped inside a MermaidScrollViewer with copy toolbar and CodeBlockTag serialization.
    /// </summary>
    public static class MermaidFlowchartRenderer
    {
        /// <summary>
        /// Renders a MermaidFlowchartGraph into a FlowDocument BlockUIContainer.
        /// </summary>
        public static BlockUIContainer Render(MermaidFlowchartGraph graph, ThemePalette palette, string rawCode)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            if (palette == null) throw new ArgumentNullException(nameof(palette));

            var layout = MermaidLayoutEngine.Layout(graph);
            return Render(layout, palette, rawCode ?? graph.RawSource ?? string.Empty);
        }

        /// <summary>
        /// Renders a computed MermaidLayoutResult into a FlowDocument BlockUIContainer.
        /// </summary>
        public static BlockUIContainer Render(MermaidLayoutResult layout, ThemePalette palette, string rawCode)
        {
            if (layout == null) throw new ArgumentNullException(nameof(layout));
            if (palette == null) throw new ArgumentNullException(nameof(palette));

            var visual = CreateFlowchartVisual(layout, palette, rawCode);

            var buic = new BlockUIContainer(visual)
            {
                Tag = new CodeBlockTag
                {
                    Language = "mermaid",
                    Code = rawCode ?? string.Empty
                }
            };

            return buic;
        }

        /// <summary>
        /// Overload accepting graph and palette, using graph.RawSource as raw code.
        /// </summary>
        public static BlockUIContainer Render(MermaidFlowchartGraph graph, ThemePalette palette)
        {
            return Render(graph, palette, graph?.RawSource ?? string.Empty);
        }

        /// <summary>
        /// Constructs the root vector visual element hierarchy for the flowchart.
        /// </summary>
        public static UIElement CreateFlowchartVisual(MermaidLayoutResult layout, ThemePalette palette, string rawCode)
        {
            var outerBorder = new Border
            {
                Background = palette.CodeBg,
                BorderBrush = palette.CodeBorder ?? palette.Border,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Margin = new Thickness(0, 8, 0, 16)
            };

            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // 1. Header Bar with MERMAID badge and Copy button
            var headerGrid = new Grid
            {
                Background = palette.IsDark
                    ? new SolidColorBrush(Color.FromRgb(25, 30, 36))
                    : new SolidColorBrush(Color.FromRgb(240, 243, 246)),
                Height = 28
            };

            var badgeText = new TextBlock
            {
                Text = "MERMAID",
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = palette.MutedFg,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(12, 0, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            headerGrid.Children.Add(badgeText);

            var copyBtn = new Button
            {
                Content = "Copy",
                FontSize = 11,
                Padding = new Thickness(8, 2, 8, 2),
                Margin = new Thickness(0, 0, 8, 0),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Cursor = System.Windows.Input.Cursors.Hand,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(1),
                BorderBrush = palette.Border,
                Foreground = palette.MutedFg
            };

            string sourceCopy = rawCode ?? string.Empty;
            copyBtn.Click += (s, e) =>
            {
                if (ClipboardHelper.SetText(sourceCopy))
                {
                    copyBtn.Content = "Copied!";
                    var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
                    timer.Tick += (ts, te) =>
                    {
                        copyBtn.Content = "Copy";
                        timer.Stop();
                    };
                    timer.Start();
                }
            };
            headerGrid.Children.Add(copyBtn);
            Grid.SetRow(headerGrid, 0);
            mainGrid.Children.Add(headerGrid);

            // Collapsed text block containing raw code for fallback serialization
            var hiddenCodeTb = new TextBlock
            {
                Text = sourceCopy,
                Visibility = Visibility.Collapsed
            };
            mainGrid.Children.Add(hiddenCodeTb);

            // 2. Vector Canvas inside MermaidScrollViewer
            var canvas = new Canvas
            {
                Width = Math.Max(120.0, layout.TotalWidth),
                Height = Math.Max(80.0, layout.TotalHeight),
                Background = Brushes.Transparent
            };

            // Render Edges (Connectors)
            RenderEdges(canvas, layout, palette);

            // Render Nodes
            RenderNodes(canvas, layout, palette);

            // Render Edge Labels (pill badges)
            RenderEdgeLabels(canvas, layout, palette);

            var scrollViewer = new MermaidScrollViewer
            {
                Content = canvas,
                HorizontalAlignment = HorizontalAlignment.Center,
                Padding = new Thickness(16)
            };

            Grid.SetRow(scrollViewer, 1);
            mainGrid.Children.Add(scrollViewer);

            outerBorder.Child = mainGrid;
            return outerBorder;
        }

        private static void RenderEdges(Canvas canvas, MermaidLayoutResult layout, ThemePalette palette)
        {
            var solidGeo = new StreamGeometry();
            var dashedGeo = new StreamGeometry();
            var thickGeo = new StreamGeometry();
            var arrowGeo = new StreamGeometry();

            using (var sCtx = solidGeo.Open())
            using (var dCtx = dashedGeo.Open())
            using (var tCtx = thickGeo.Open())
            using (var aCtx = arrowGeo.Open())
            {
                foreach (var route in layout.Edges)
                {
                    var ctx = route.Edge.Stroke switch
                    {
                        MermaidStrokeStyle.Dotted => dCtx,
                        MermaidStrokeStyle.Thick => tCtx,
                        _ => sCtx
                    };

                    DrawConnectorSpline(ctx, route, layout.Orientation);

                    if (route.Edge.Arrow == MermaidArrowHead.Arrow)
                    {
                        DrawArrowhead(aCtx, route.EndPoint, route.ArrowheadAngle);
                    }
                }
            }

            solidGeo.Freeze();
            dashedGeo.Freeze();
            thickGeo.Freeze();
            arrowGeo.Freeze();

            var edgeBrush = palette.MutedFg;

            // 1. Solid connectors
            var solidPath = new Path
            {
                Data = solidGeo,
                Stroke = edgeBrush,
                StrokeThickness = 1.5,
                Fill = null
            };
            canvas.Children.Add(solidPath);

            // 2. Dotted / Dashed connectors
            var dashedPath = new Path
            {
                Data = dashedGeo,
                Stroke = edgeBrush,
                StrokeThickness = 1.5,
                StrokeDashArray = new DoubleCollection { 3, 3 },
                Fill = null
            };
            canvas.Children.Add(dashedPath);

            // 3. Thick connectors
            var thickPath = new Path
            {
                Data = thickGeo,
                Stroke = edgeBrush,
                StrokeThickness = 2.5,
                Fill = null
            };
            canvas.Children.Add(thickPath);

            // 4. Arrowheads (filled)
            var arrowPath = new Path
            {
                Data = arrowGeo,
                Fill = edgeBrush,
                Stroke = null
            };
            canvas.Children.Add(arrowPath);
        }

        private static void DrawConnectorSpline(StreamGeometryContext ctx, EdgeLayoutRoute route, MermaidOrientation orientation)
        {
            var p0 = route.StartPoint;
            var p3 = route.EndPoint;

            ctx.BeginFigure(p0, false, false);

            if (route.IsFeedbackEdge)
            {
                // Orthogonal bypass corridor
                if (route.Waypoints.Count >= 2)
                {
                    ctx.LineTo(route.Waypoints[0], true, false);
                    ctx.LineTo(route.Waypoints[1], true, false);
                }
                ctx.LineTo(p3, true, false);
            }
            else if (route.Waypoints.Count > 0)
            {
                // Chained Bezier segments through dummy node waypoints
                Point prev = p0;
                foreach (var wp in route.Waypoints)
                {
                    bool isHoriz = (orientation == MermaidOrientation.LeftToRight || orientation == MermaidOrientation.RightToLeft);
                    Point c1, c2;
                    if (!isHoriz)
                    {
                        double dy = wp.Y - prev.Y;
                        c1 = new Point(prev.X, prev.Y + dy / 2.0);
                        c2 = new Point(wp.X, wp.Y - dy / 2.0);
                    }
                    else
                    {
                        double dx = wp.X - prev.X;
                        c1 = new Point(prev.X + dx / 2.0, prev.Y);
                        c2 = new Point(wp.X - dx / 2.0, wp.Y);
                    }
                    ctx.BezierTo(c1, c2, wp, true, false);
                    prev = wp;
                }

                // Final segment to p3
                {
                    bool isHoriz = (orientation == MermaidOrientation.LeftToRight || orientation == MermaidOrientation.RightToLeft);
                    Point c1, c2;
                    if (!isHoriz)
                    {
                        double dy = p3.Y - prev.Y;
                        c1 = new Point(prev.X, prev.Y + dy / 2.0);
                        c2 = new Point(p3.X, p3.Y - dy / 2.0);
                    }
                    else
                    {
                        double dx = p3.X - prev.X;
                        c1 = new Point(prev.X + dx / 2.0, prev.Y);
                        c2 = new Point(p3.X - dx / 2.0, p3.Y);
                    }
                    ctx.BezierTo(c1, c2, p3, true, false);
                }
            }
            else
            {
                // Direct cubic Bezier
                ctx.BezierTo(route.ControlPoint1, route.ControlPoint2, p3, true, false);
            }
        }

        private static void DrawArrowhead(StreamGeometryContext ctx, Point tip, double angle)
        {
            double len = 9.0;
            double halfW = 4.5;
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);

            var barb1 = new Point(tip.X - len * cos + halfW * (-sin), tip.Y - len * sin + halfW * cos);
            var barb2 = new Point(tip.X - len * cos - halfW * (-sin), tip.Y - len * sin - halfW * cos);

            ctx.BeginFigure(tip, true, true);
            ctx.LineTo(barb1, true, false);
            ctx.LineTo(barb2, true, false);
        }

        private static void RenderNodes(Canvas canvas, MermaidLayoutResult layout, ThemePalette palette)
        {
            var nodeCardBg = palette.IsDark
                ? new SolidColorBrush(Color.FromRgb(33, 38, 45))
                : new SolidColorBrush(Color.FromRgb(255, 255, 255));
            nodeCardBg.Freeze();

            foreach (var kvp in layout.Nodes)
            {
                var nb = kvp.Value;
                var element = CreateNodeVisual(nb, palette, nodeCardBg);
                Canvas.SetLeft(element, nb.X);
                Canvas.SetTop(element, nb.Y);
                canvas.Children.Add(element);
            }
        }

        private static UIElement CreateNodeVisual(NodeLayoutBounds nb, ThemePalette palette, Brush cardBg)
        {
            var textBlock = new TextBlock
            {
                Text = nb.Node.Text,
                FontSize = 13,
                FontWeight = FontWeights.Medium,
                Foreground = palette.EditorFg,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };

            switch (nb.Node.Shape)
            {
                case MermaidNodeShape.Stadium:
                    return new Border
                    {
                        Width = nb.Width,
                        Height = nb.Height,
                        Background = cardBg,
                        BorderBrush = palette.Accent,
                        BorderThickness = new Thickness(1.5),
                        CornerRadius = new CornerRadius(nb.Height / 2.0),
                        Child = textBlock
                    };

                case MermaidNodeShape.RoundedRectangle:
                    return new Border
                    {
                        Width = nb.Width,
                        Height = nb.Height,
                        Background = cardBg,
                        BorderBrush = palette.Accent,
                        BorderThickness = new Thickness(1.5),
                        CornerRadius = new CornerRadius(8),
                        Child = textBlock
                    };

                case MermaidNodeShape.Circle:
                    var circleGrid = new Grid { Width = nb.Width, Height = nb.Height };
                    circleGrid.Children.Add(new Ellipse
                    {
                        Fill = cardBg,
                        Stroke = palette.Accent,
                        StrokeThickness = 1.5,
                        Width = nb.Width,
                        Height = nb.Height
                    });
                    circleGrid.Children.Add(textBlock);
                    return circleGrid;

                case MermaidNodeShape.Diamond:
                    var diamondCanvas = new Canvas { Width = nb.Width, Height = nb.Height };
                    var geo = new PathGeometry();
                    var fig = new PathFigure { StartPoint = new Point(nb.Width / 2.0, 0), IsClosed = true, IsFilled = true };
                    fig.Segments.Add(new LineSegment(new Point(nb.Width, nb.Height / 2.0), true));
                    fig.Segments.Add(new LineSegment(new Point(nb.Width / 2.0, nb.Height), true));
                    fig.Segments.Add(new LineSegment(new Point(0, nb.Height / 2.0), true));
                    geo.Figures.Add(fig);
                    geo.Freeze();

                    diamondCanvas.Children.Add(new Path
                    {
                        Data = geo,
                        Fill = cardBg,
                        Stroke = palette.Accent,
                        StrokeThickness = 1.5
                    });

                    double textWidth = Math.Max(20.0, nb.Width * 0.7);
                    textBlock.Width = textWidth;
                    Canvas.SetLeft(textBlock, (nb.Width - textWidth) / 2.0);
                    Canvas.SetTop(textBlock, (nb.Height - Math.Max(20.0, nb.Height * 0.5)) / 2.0);
                    diamondCanvas.Children.Add(textBlock);
                    return diamondCanvas;

                default: // Rectangle
                    return new Border
                    {
                        Width = nb.Width,
                        Height = nb.Height,
                        Background = cardBg,
                        BorderBrush = palette.Accent,
                        BorderThickness = new Thickness(1.5),
                        CornerRadius = new CornerRadius(3),
                        Child = textBlock
                    };
            }
        }

        private static void RenderEdgeLabels(Canvas canvas, MermaidLayoutResult layout, ThemePalette palette)
        {
            foreach (var route in layout.Edges)
            {
                if (route.LabelPosition.HasValue && !string.IsNullOrEmpty(route.Edge.Label))
                {
                    var pill = new Border
                    {
                        Background = palette.CodeBg,
                        BorderBrush = palette.Border,
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(4),
                        Padding = new Thickness(6, 2, 6, 2),
                        Child = new TextBlock
                        {
                            Text = route.Edge.Label,
                            FontSize = 11,
                            Foreground = palette.EditorFg
                        }
                    };

                    Canvas.SetLeft(pill, route.LabelPosition.Value.X - route.LabelSize.Width / 2.0);
                    Canvas.SetTop(pill, route.LabelPosition.Value.Y - route.LabelSize.Height / 2.0);
                    canvas.Children.Add(pill);
                }
            }
        }
    }
}
