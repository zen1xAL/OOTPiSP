using System;
using System.Collections.Generic;

namespace DrawingApp
{
    public class UndoRedoManager
    {
        private readonly List<Shape> shapes; // Ссылка на список фигур
        private readonly Stack<Shape> undoStack = new Stack<Shape>();
        private readonly Stack<Shape> redoStack = new Stack<Shape>();

        public UndoRedoManager(List<Shape> shapes)
        {
            this.shapes = shapes ?? throw new ArgumentNullException(nameof(shapes));
        }

        // Добавление фигуры в историю (вызывается после завершения рисования)
        public void AddShape(Shape shape)
        {
            if (shape == null) throw new ArgumentNullException(nameof(shape));
            shapes.Add(shape);
            undoStack.Push(shape);
            redoStack.Clear(); // Очищаем Redo, так как новое действие сбрасывает историю восстановления
        }

        // Отмена последнего действия
        public void Undo()
        {
            if (undoStack.Count > 0)
            {
                Shape shape = undoStack.Pop();
                shapes.Remove(shape);
                redoStack.Push(shape);
            }
        }

        // Восстановление последнего отменённого действия
        public void Redo()
        {
            if (redoStack.Count > 0)
            {
                Shape shape = redoStack.Pop();
                shapes.Add(shape);
                undoStack.Push(shape);
            }
        }

        // Проверка, можно ли выполнить Undo
        public bool CanUndo => undoStack.Count > 0;

        // Проверка, можно ли выполнить Redo
        public bool CanRedo => redoStack.Count > 0;

        // Сброс истории (например, при очистке холста)
        public void Reset()
        {
            undoStack.Clear();
            redoStack.Clear();
        }
    }
}