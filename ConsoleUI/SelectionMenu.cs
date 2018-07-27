using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;

namespace ConsoleUI
{
  public class SelectionMenu
  {
    private void SetCursorPosition(int x, int y)
    {
      System.Console.SetCursorPosition(x, y);
    }

    public string[] MenuItems { get; set; }
    public int Columns { get; }
    public int Rows { get; }
    public int CursorPosition { get; private set; }
    public int CellLength { get; }
    public string Title { get; }
    public ConsoleColor CurrentItemColor { get; }

    public SelectionMenu(
      string title,
      string[] menuItems,
      int columns,
      int cellLength,
      ConsoleColor currentItemColor = ConsoleColor.DarkGray
    )
    {
      this.Title = title;
      this.MenuItems = menuItems;
      this.Columns = columns;
      this.Rows = this.MenuItems.Length / columns +
                  (this.MenuItems.Length % columns == 0 ? 0 : 1);
      this.CellLength = cellLength;
      this.CurrentItemColor = currentItemColor;
    }

    private void DrawTable(int x, int y)
    {
      var fullRow = "-".Multiply(this.CellLength);
      var partialRow = " ".Multiply(this.CellLength);
      fullRow += "----";
      fullRow = fullRow.Multiply(this.Columns);
      partialRow += "   |";
      partialRow = partialRow.Multiply(this.Columns);
      partialRow = "|" + partialRow.Substring(1, partialRow.Length - 1);
      partialRow += "\r\n";
      var table = $"{fullRow}\r\n{partialRow}";
      table = table.Multiply(this.Rows);
      table += fullRow;
      this.SetCursorPosition(x, y);
      Console.Write(table);
    }

    private void DrawItems(int offsetX, int offsetY)
    {
      var cursorX = System.Console.CursorLeft;
      var cursorY = System.Console.CursorTop;
      for (int i = 0; i < this.MenuItems.Length; i++)
      {
        var x = offsetX - 1 + (i % this.Columns) * (this.CellLength + 2) + (i % this.Columns + 1) * 2;
        var y = offsetY + 1 + (i / this.Columns) * 2;
        if (i % this.Columns == 0)
        {
          x++;
        }

        this.SetCursorPosition(x, y);
        var menuItemcontent = this.MenuItems[i];
        if (menuItemcontent.Length > this.CellLength)
        {
          menuItemcontent = menuItemcontent.Substring(0, this.CellLength);
        }

        Console.Write(menuItemcontent);
      }

      this.SetCursorPosition(cursorX, cursorY);
    }

    private void DrawCursorItem(int offsetX, int offsetY)
    {
      var curX = System.Console.CursorLeft;
      var curY = System.Console.CursorTop;
      var x = offsetX + 2 + this.CellLength * (this.CursorPosition % this.Columns) +
              (this.CursorPosition % this.Columns) * 4;
      var y = offsetY + 1 + 2 * (this.CursorPosition / this.Columns);
      if (this.CursorPosition % this.Columns > 0)
      {
        x--;
      }

      var content = this.MenuItems[CursorPosition];
      if (content.Length > 8)
      {
        content = content.Substring(0, this.CellLength - 2);
      }

      this.SetCursorPosition(x, y);
      var oldForegroundColor = System.Console.ForegroundColor;
      System.Console.ForegroundColor = this.CurrentItemColor;
      Console.Write("> " + content);
      System.Console.ForegroundColor = oldForegroundColor;
      this.SetCursorPosition(curX, curY);
    }

    public void DrawMenu(int offsetY = 0, int offsetX = 0)
    {
      var curX = System.Console.CursorLeft;
      var curY = System.Console.CursorTop;
      this.SetCursorPosition(offsetX + curX, offsetY + curY);
      Console.WriteLine($"<{this.Title}>");
      this.DrawTable(offsetX + curX, offsetY + curY + 1);
      this.DrawItems(offsetX + curX, offsetY + curY + 1);
      this.DrawCursorItem(offsetX + curX, offsetY + curY + 1);
      this.SetCursorPosition(curX, curY);
    }

    public void MoveCursor(ECursorDirection dir)
    {
      switch (dir)
      {
        case ECursorDirection.Up:
          if (this.CursorPosition >= this.Columns)
          {
            this.CursorPosition -= this.Columns;
          }

          break;

        case ECursorDirection.Down:
          if (this.CursorPosition / this.Columns < this.Rows - 1
              && this.CursorPosition + this.Columns < this.MenuItems.Length)
          {
            this.CursorPosition += this.Columns;
          }

          break;

        case ECursorDirection.Right:
          if (this.CursorPosition % this.Columns < this.Columns - 1
              && this.CursorPosition < this.MenuItems.Length - 1)
          {
            this.CursorPosition++;
          }

          break;

        case ECursorDirection.Left:
          if (this.CursorPosition % this.Columns > 0)
          {
            this.CursorPosition--;
          }

          break;
      }
    }

    public void MoveCursor(ConsoleKey key)
    {
      switch (key)
      {
        case ConsoleKey.UpArrow:
          this.MoveCursor(ECursorDirection.Up);
          break;
        case ConsoleKey.DownArrow:
          this.MoveCursor(ECursorDirection.Down);
          break;
        case ConsoleKey.LeftArrow:
          this.MoveCursor(ECursorDirection.Left);
          break;
        case ConsoleKey.RightArrow:
          this.MoveCursor(ECursorDirection.Right);
          break;
      }
    }

    public void Clear(int offsetY, int offsetX)
    {
      var cursorX = Console.CursorLeft;
      var cursorY = Console.CursorTop;
      var clearString = " ".Multiply(System.Console.WindowWidth - offsetX);
      int rowsToClear = (this.Rows - 1) * 4 + 5;
      for (int i = 0; i < rowsToClear; i++)
      {
        this.SetCursorPosition(cursorX + offsetX, cursorY + offsetY + i);
        Console.Write(clearString);
      }
      this.SetCursorPosition(cursorX, cursorY);
    }

    /**
     * Draws the configured menu and returns the index of the selected item,
     * or null if the menu has been canceled.
     */
    public int? GetMenuResult(int offsetY = 0, int offsetX = 0)
    {
      // var cursorVisiblility = System.Console.CursorVisible;
      // System.Console.CursorVisible = false;
      do
      {
        this.DrawMenu(offsetY, offsetX);
        var key = System.Console.ReadKey().Key;
        switch (key)
        {
          case ConsoleKey.C:
            // System.Console.CursorVisible = cursorVisiblility;
            this.Clear(offsetX, offsetY);
            return null;
          case ConsoleKey.Enter:
            // System.Console.CursorVisible = cursorVisiblility;
            this.Clear(offsetX, offsetY);
            return this.CursorPosition;
        }

        this.MoveCursor(key);
      } while (true);
    }
  }

  public static class StringExtension
  {
    public static string Multiply(this string str, int amount)
    {
      string result = str;
      for (int i = 1; i < amount; i++)
      {
        result += str;
      }

      return result;
    }
  }
}