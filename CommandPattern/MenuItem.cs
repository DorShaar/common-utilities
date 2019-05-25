using System.Windows.Input;

namespace CommandPattern
{
    public class MenuItem
    {
        public ICommand Command { get; set; }
        public string Text { get; set; }

        public virtual void Selected(object parameter = null)
        {
            if (Command != null)
                Command.Execute(parameter);
        }
    }
}
