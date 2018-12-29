using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QEnc
{
    /// <summary>
    /// Interaction logic for StateIcon.xaml
    /// </summary>
    public partial class StateNote : UserControl
    {
        public StateNote()
        {
            InitializeComponent();
            ((Storyboard)Resources["HideIcon"]).Begin();
        }

        public enum States { Unseted, Loaded, Processing, Copy, Unavailable };
        private States currentState;
        public States State
        {
            get
            {
                return currentState;
            }
            set
            {
                currentState = value;
                switch (value)
                {
                    case States.Unseted:
                        ((Storyboard)Resources["HideIcon"]).Begin();
                        break;
                    case States.Loaded:
                        ((Storyboard)Resources["ShowLoadedIcon"]).Begin();
                        break;
                    case States.Processing:
                        ((Storyboard)Resources["ShowProcessingIcon"]).Begin();
                        break;
                    case States.Copy:
                        ((Storyboard)Resources["ShowCopyIcon"]).Begin();
                        break;
                    case States.Unavailable:
                        ((Storyboard)Resources["ShowUnavailableIcon"]).Begin();
                        break;
                }

            }
        }
    }
}
