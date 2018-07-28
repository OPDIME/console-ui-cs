using System;
using ConsoleUI;

namespace OPDIME.ConsoleUI
{
  public class SelectionMenu
  {
    private void SetCursorPosition(int x, int y)
    {
      Console.SetCursorPosition(x, y);
    }

    public string[] MenuItems { get; }
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
      Title = title;
      MenuItems = menuItems;
      Columns = columns;
      Rows = MenuItems.Length / columns +
                  (MenuItems.Length % columns == 0 ? 0 : 1);
      CellLength = cellLength;
      CurrentItemColor = currentItemColor;
    }

    private void DrawTable(int x, int y)
    {
      // prepare text row to divide content
      var dividerRow = "-".Multiply(CellLength);
      // prepare empty content rows
      var contentRow = " ".Multiply(CellLength);
      // add 4 left over dashes due to the space
      // between the cell content and the cell border
      dividerRow += "----";
      // multiply the divider row length by the column count
      // to divide the whole row, rather than one cell
      dividerRow = dividerRow.Multiply(Columns);

      // add 3 spaces and a vertical divider for the content row divider
      contentRow += "   |";
      // multiply by column count, so it fits for the whole row
      contentRow = contentRow.Multiply(Columns);
      contentRow = $"|{contentRow.Substring(1)}\r\n";
      var table = $"{dividerRow}\r\n{contentRow}";
      table = table.Multiply(Rows);
      table += $"\\{dividerRow.Substring(1, dividerRow.Length - 2)}/";
      table = $"/{dividerRow.Substring(2)}\\{table.Substring(dividerRow.Length)}";
      SetCursorPosition(x, y);
      Console.Write(table);
    }

    private void DrawItems(int offsetX, int offsetY)
    {
      // store the old cursor position on the console
      var cursorX = Console.CursorLeft;
      var cursorY = Console.CursorTop;
      // draw all items
      for (int i = 0; i < MenuItems.Length; i++)
      {
        // calculate
        var x = offsetX - 1 + (i % Columns) * (CellLength + 2) + (i % Columns + 1) * 2;
        var y = offsetY + 1 + (i / Columns) * 2;
        if (i % Columns == 0)
        {
          x++;
        }

        SetCursorPosition(x, y);
        var menuItemcontent = MenuItems[i];
        if (menuItemcontent.Length > CellLength)
        {
          menuItemcontent = menuItemcontent.Substring(0, CellLength);
        }

        Console.Write(menuItemcontent);
      }

      SetCursorPosition(cursorX, cursorY);
    }

    private void DrawCursorItem(int offsetX, int offsetY)
    {
      var curX = Console.CursorLeft;
      var curY = Console.CursorTop;
      var x = offsetX + 2 + CellLength * (CursorPosition % Columns) +
              (CursorPosition % Columns) * 4;
      var y = offsetY + 1 + 2 * (CursorPosition / Columns);
      if (CursorPosition % Columns > 0)
      {
        x--;
      }

      var content = MenuItems[CursorPosition];
      if (content.Length > 8)
      {
        content = content.Substring(0, CellLength - 2);
      }

      SetCursorPosition(x, y);
      var oldForegroundColor = Console.ForegroundColor;
      Console.ForegroundColor = CurrentItemColor;
      Console.Write("> " + content);
      Console.ForegroundColor = oldForegroundColor;
      SetCursorPosition(curX, curY);
    }

    private void DrawMenu(int offsetX = 0, int offsetY = 0)
    {
      var curX = Console.CursorLeft;
      var curY = Console.CursorTop;
      SetCursorPosition(offsetX + curX, offsetY + curY);
      Console.WriteLine($"<{Title}>");
      DrawTable(offsetX + curX, offsetY + curY + 1);
      DrawItems(offsetX + curX, offsetY + curY + 1);
      DrawCursorItem(offsetX + curX, offsetY + curY + 1);
      SetCursorPosition(curX, curY);
    }

    private void MoveCursor(ECursorDirection dir)
    {
      switch (dir)
      {
        case ECursorDirection.Up:
          if (CursorPosition >= Columns)
          {
            CursorPosition -= Columns;
          }

          break;

        case ECursorDirection.Down:
          if (CursorPosition / Columns < Rows - 1
              && CursorPosition + Columns < MenuItems.Length)
          {
            CursorPosition += Columns;
          }

          break;

        case ECursorDirection.Right:
          if (CursorPosition % Columns < Columns - 1
              && CursorPosition < MenuItems.Length - 1)
          {
            CursorPosition++;
          }

          break;

        case ECursorDirection.Left:
          if (CursorPosition % Columns > 0)
          {
            CursorPosition--;
          }

          break;
      }
    }

    private void MoveCursor(ConsoleKey key)
    {
      switch (key)
      {
        case ConsoleKey.UpArrow:
          MoveCursor(ECursorDirection.Up);
          break;
        case ConsoleKey.DownArrow:
          MoveCursor(ECursorDirection.Down);
          break;
        case ConsoleKey.LeftArrow:
          MoveCursor(ECursorDirection.Left);
          break;
        case ConsoleKey.RightArrow:
          MoveCursor(ECursorDirection.Right);
          break;
      }
    }

    private void Clear(int offsetX, int offsetY)
    {
      var cursorX = Console.CursorLeft;
      var cursorY = Console.CursorTop;
      var clearString = " ".Multiply(Console.WindowWidth - offsetX);
      int rowsToClear = (Rows - 1) * 4 + 5;
      for (int i = 0; i < rowsToClear; i++)
      {
        SetCursorPosition(cursorX + offsetX, cursorY + offsetY + i);
        Console.Write(clearString);
      }
      SetCursorPosition(cursorX, cursorY);
    }

    /**
     * Draws the configured menu and returns the index of the selected item,
     * or null if the menu has been canceled.
     */
    public int? GetMenuResult(int offsetX = 0, int offsetY = 0)
    {
      // var cursorVisiblility = System.Console.CursorVisible;
      // System.Console.CursorVisible = false;
      do
      {
        DrawMenu(offsetX, offsetY);
        var key = Console.ReadKey().Key;
        // reset the cursor position to its original place
        int left = Console.CursorLeft;
        if (left > 0) {
          --left;
        }
        SetCursorPosition(left, Console.CursorTop);
        switch (key)
        {
          case ConsoleKey.C:
            // System.Console.CursorVisible = cursorVisiblility;
            Clear(offsetX, offsetY);
            return null;
          case ConsoleKey.Enter:
            // System.Console.CursorVisible = cursorVisiblility;
            Clear(offsetX, offsetY);
            return CursorPosition;
        }

        MoveCursor(key);
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