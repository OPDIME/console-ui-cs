using System;
using OPDIME.ConsoleUI;

namespace ConsoleUI
{
  class Program
  {
    static void Main(string[] args)
    {
      Console.ForegroundColor = ConsoleColor.Cyan;
      var items = new[] { "Hello", "World", "my", "name", "is", "Jörn" };
      var m = new SelectionMenu("test", items, 4, 20);
      var result = m.GetMenuResult();
      Console.WriteLine("result:" + (result ?? -1).ToString());
    }
  }
}
