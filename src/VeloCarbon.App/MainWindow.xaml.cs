using System.Windows;

namespace VeloCarbon.App;

public partial class MainWindow : Window
{
    public MainWindow() { InitializeComponent(); }
    private void Exit_Click(object sender, RoutedEventArgs e) => Close();
    private void AddActivity_Click(object sender, RoutedEventArgs e) => MainTabs.SelectedIndex = 1;
}
