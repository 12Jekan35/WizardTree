using System.DirectoryServices;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using WizardTree.Models;
using WizardTree.Presenter;

namespace WizardTree.Views
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainPresenter _presenter;

        public MainWindow()
        {
            InitializeComponent();
            _presenter = new MainPresenter(this);
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await _presenter.InitializeFileSystemAsync();
        }

        private void SortOptionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SortOptionComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                if (Enum.TryParse(selectedItem.Tag.ToString(), out Presenter.SortOption sortOption))
                {
                    _presenter.SortItems(sortOption);
                }
            }
        }
    }
}
