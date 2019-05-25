using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace CommandPattern
{
    public class Menu : List<MenuItem>
    {
        public void RegisterCommand(string text, ICommand command)
        {
            MenuItem menuItem = new MenuItem
            {
                Text = text,
                Command = command
            };

            this.Add(menuItem);
        }

        public void Run()
        {
            bool userQuit = false;
            while (!userQuit)
            {
                ShowMenu();
                try
                {
                    userQuit = !GetUserSelection(out int userSelection);
                    if (!userQuit)
                    {
                        //Console.Clear();
                        this[userSelection - 1].Selected();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private bool GetUserSelection(out int userSelection)
        {
            bool userQuit = false;
            string userSelectionStr = Console.ReadLine();
            if (userSelectionStr.ToLower() != "q")
            {
                userSelection = int.Parse(userSelectionStr);
                if (!(userSelection > 0 && userSelection <= Count))
                    throw new ArgumentException(string.Format($"You must select a number between 1-{Count}"));
            }
            else
            {
                userSelection = 0;
                userQuit = true;
            }

            return !userQuit;
        }

        private void ShowMenu()
        {
            Console.WriteLine("Please choose:");

            int itemNum = 1;
            foreach (MenuItem item in this)
            {
                Console.WriteLine($"{itemNum.ToString()}: {item.Text}");
                itemNum++;
            }

            Console.WriteLine(@"
Type your selection number and press 'enter'.
To quit type 'Q' and then 'enter'
");
        }
    }
}
