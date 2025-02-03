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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KTruckGui.Assets
{
    /// <summary>
    /// Interaction logic for ReloadButton.xaml
    /// </summary>
    public partial class ReloadButton : System.Windows.Controls.UserControl
    {
        public event EventHandler ReloadClicked;

        public ReloadButton()
        {
            InitializeComponent();
        }

        private void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            ReloadClicked?.Invoke(this, EventArgs.Empty); // Raise an event when the button is clicked
        }
    }
}
