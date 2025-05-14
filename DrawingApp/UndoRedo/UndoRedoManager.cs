using System;
using System.Collections.Generic;

namespace DrawingApp
{
    public class UndoRedoManager
    {
        private readonly List<Shape> shapes;
        private readonly Stack<Shape> undoStack = new Stack<Shape>();
        private readonly Stack<Shape> redoStack = new Stack<Shape>();

        public UndoRedoManager(List<Shape> shapes)
        {
            this.shapes = shapes ?? throw new ArgumentNullException(nameof(shapes));
        }

        public void AddShape(Shape shape)
        {
            if (shape == null) throw new ArgumentNullException(nameof(shape));
            
            undoStack.Push(shape);
            redoStack.Clear();
        }

        public void Undo()
        {
            if (undoStack.Count > 0)
            {
                Shape shape = undoStack.Pop();
                shapes.Remove(shape);
                redoStack.Push(shape);
            }
        }

   
        public void Redo()
        {
            if (redoStack.Count > 0)
            {
                Shape shape = redoStack.Pop();
                shapes.Add(shape);
                undoStack.Push(shape);
            }
        }

        public void Reset()
        {
            undoStack.Clear();
            redoStack.Clear();
        }
    }
}