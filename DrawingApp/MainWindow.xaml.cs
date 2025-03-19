using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DrawingApp
{
    public partial class MainWindow : Window
    {
        private List<Shape> shapes = new List<Shape>();
        private Shape currentShape;
        private Point startPoint;
        private Stack<Shape> undoStack = new Stack<Shape>();
        private Stack<Shape> redoStack = new Stack<Shape>();
        private string currentMode = "";
        private bool isDrawing = false;

        public MainWindow()
        {
            InitializeComponent();
            DrawingCanvas.MouseDown += DrawingCanvas_MouseDown;
            DrawingCanvas.MouseMove += DrawingCanvas_MouseMove;
            DrawingCanvas.MouseUp += DrawingCanvas_MouseUp;
            DrawingCanvas.MouseRightButtonDown += DrawingCanvas_MouseRightButtonDown;
            UndoButton.Click += UndoButton_Click;
            RedoButton.Click += RedoButton_Click;
            ShapeSelector.SelectionChanged += ShapeSelector_SelectionChanged;
            this.KeyDown += MainWindow_KeyDown; // Привязываем событие к окну
        }

        private void ShapeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isDrawing && currentShape != null)
            {
                FinalizeShape();
                isDrawing = false;
            }
        }

        private void DrawingCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point clickPoint = e.GetPosition(DrawingCanvas);

            if (!isDrawing || currentMode == "Polygon" || currentMode == "Polyline")
            {
                startPoint = clickPoint;
                string selectedShape = (ShapeSelector.SelectedItem as ComboBoxItem)?.Content.ToString();

                if (string.IsNullOrEmpty(selectedShape))
                {
                    return;
                }

                currentMode = selectedShape;
                isDrawing = true;

                if (selectedShape == "Polygon" || selectedShape == "Polyline")
                {
                    if (currentShape == null || !IsPolygonOrPolyline(currentShape))
                    {
                        currentShape = ShapeFactory.CreateShape(selectedShape);
                    }
                    (currentShape as dynamic).Points.Add(startPoint);
                }
                else
                {
                    currentShape = ShapeFactory.CreateShape(selectedShape);
                    if (currentShape is LineShape line)
                    {
                        line.Start = startPoint;
                        line.End = startPoint;
                    }
                    else if (currentShape is RectangleShape rect)
                    {
                        rect.TopLeft = startPoint;
                        rect.BottomRight = startPoint;
                    }
                    else if (currentShape is EllipseShape ellipse)
                    {
                        ellipse.TopLeft = startPoint;
                        ellipse.BottomRight = startPoint;
                    }
                }

                SetShapeProperties(currentShape);
                RedrawCanvas();
            }
        }

        private void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing && currentShape != null)
            {
                Point currentPoint = e.GetPosition(DrawingCanvas);

                if (currentMode == "Line" && currentShape is LineShape line)
                {
                    line.End = currentPoint;
                    SetShapeProperties(currentShape);
                    RedrawCanvas();
                }
                else if (currentMode == "Rectangle" && currentShape is RectangleShape rect)
                {
                    rect.BottomRight = currentPoint;
                    SetShapeProperties(currentShape);
                    RedrawCanvas();
                }
                else if (currentMode == "Ellipse" && currentShape is EllipseShape ellipse)
                {
                    ellipse.BottomRight = currentPoint;
                    SetShapeProperties(currentShape);
                    RedrawCanvas();
                }
                else if (currentMode == "Polygon" || currentMode == "Polyline")
                {
                    RedrawCanvasWithPreview(currentPoint);
                }
            }
        }

        private void DrawingCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isDrawing && currentShape != null && (currentShape is LineShape || currentShape is RectangleShape || currentShape is EllipseShape))
            {
                FinalizeShape();
                isDrawing = false;
            }
        }

        private void DrawingCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isDrawing && currentShape != null && IsPolygonOrPolyline(currentShape))
            {
                FinalizeShape();
                isDrawing = false;
            }
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (isDrawing && e.Key == Key.Escape)
            {
                currentShape = null;
                isDrawing = false;
                RedrawCanvas();
                e.Handled = true; // Помечаем событие как обработанное
            }
        }

        private void SetShapeProperties(Shape shape)
        {
            double thickness = ThicknessSlider.Value;
            string strokeColorName = (ColorSelector.SelectedItem as ComboBoxItem)?.Content.ToString();
            string fillColorName = (FillColorSelector.SelectedItem as ComboBoxItem)?.Content.ToString();
            Color strokeColor = GetColorFromName(strokeColorName);
            Color fillColor = fillColorName == "None" ? Colors.Transparent : GetColorFromName(fillColorName);
            shape.SetProperties(thickness, strokeColor, fillColor);
        }

        private void FinalizeShape()
        {
            if (currentShape == null) return;

            SetShapeProperties(currentShape);
            shapes.Add(currentShape);
            undoStack.Push(currentShape);
            redoStack.Clear();
            currentShape = null;
            currentMode = "";
            RedrawCanvas();
        }

        private void RedrawCanvas()
        {
            DrawingCanvas.Children.Clear();
            foreach (var shape in shapes)
            {
                DrawingCanvas.Children.Add(shape.Draw());
            }
            if (currentShape != null)
            {
                DrawingCanvas.Children.Add(currentShape.Draw());
            }
        }

        private void RedrawCanvasWithPreview(Point previewPoint)
        {
            DrawingCanvas.Children.Clear();

            foreach (var shape in shapes)
            {
                DrawingCanvas.Children.Add(shape.Draw());
            }

            if (currentShape != null)
            {
                UIElement currentElement = currentShape.Draw();
                DrawingCanvas.Children.Add(currentElement);

                if (IsPolygonOrPolyline(currentShape) && (currentShape as dynamic).Points.Count > 0)
                {
                    var points = (currentShape as dynamic).Points as List<Point>;
                    Point lastPoint = points[points.Count - 1];

                    Line previewLine = new Line
                    {
                        X1 = lastPoint.X,
                        Y1 = lastPoint.Y,
                        X2 = previewPoint.X,
                        Y2 = previewPoint.Y,
                        Stroke = new SolidColorBrush(GetColorFromName((ColorSelector.SelectedItem as ComboBoxItem)?.Content.ToString())),
                        StrokeThickness = ThicknessSlider.Value,
                        StrokeDashArray = new DoubleCollection { 4, 4 }
                    };
                    DrawingCanvas.Children.Add(previewLine);

                    if (currentShape is PolygonShape && points.Count > 1)
                    {
                        PolygonShape previewPolygon = new PolygonShape();
                        foreach (var point in points)
                        {
                            previewPolygon.Points.Add(point);
                        }
                        previewPolygon.Points.Add(previewPoint);
                        SetShapeProperties(previewPolygon);

                        UIElement previewElement = previewPolygon.Draw();
                        if (previewElement is System.Windows.Shapes.Polygon polygon)
                        {
                            polygon.Opacity = 0.5;
                        }
                        DrawingCanvas.Children.Add(previewElement);
                    }
                }
            }
        }

        private Color GetColorFromName(string colorName)
        {
            switch (colorName)
            {
                case "Red": return Colors.Red;
                case "Blue": return Colors.Blue;
                case "Green": return Colors.Green;
                case "Black": return Colors.Black;
                default: return Colors.Black;
            }
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            if (isDrawing)
            {
                currentShape = null;
                isDrawing = false;
                RedrawCanvas();
                return;
            }

            if (undoStack.Count > 0)
            {
                Shape shape = undoStack.Pop();
                shapes.Remove(shape);
                redoStack.Push(shape);
                RedrawCanvas();
            }
        }

        private void RedoButton_Click(object sender, RoutedEventArgs e)
        {
            if (redoStack.Count > 0)
            {
                Shape shape = redoStack.Pop();
                shapes.Add(shape);
                undoStack.Push(shape);
                RedrawCanvas();
            }
        }

        private bool IsPolygonOrPolyline(Shape shape)
        {
            return shape is PolygonShape || shape is PolylineShape;
        }
    }

    public static class ShapeFactory
    {
        private static readonly Dictionary<string, Func<Shape>> shapeCreators = new Dictionary<string, Func<Shape>>
        {
            { "Line", () => new LineShape() },
            { "Rectangle", () => new RectangleShape() },
            { "Ellipse", () => new EllipseShape() },
            { "Polygon", () => new PolygonShape() },
            { "Polyline", () => new PolylineShape() }
        };

        public static Shape CreateShape(string shapeName)
        {
            if (string.IsNullOrEmpty(shapeName))
            {
                throw new ArgumentException("Название фигуры не может быть пустым или null.");
            }

            if (shapeCreators.TryGetValue(shapeName, out var creator))
            {
                return creator();
            }
            throw new ArgumentException($"Неизвестный тип фигуры: {shapeName}");
        }
    }
}