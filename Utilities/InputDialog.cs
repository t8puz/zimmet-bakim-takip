using System;
using System.Windows;
using System.Windows.Controls;

namespace Zimmet_Bakim_Takip.Utilities
{
    public class InputDialog : Window
    {
        private System.Windows.Controls.TextBox answerTextBox;
        private System.Windows.Controls.Button okButton;
        private System.Windows.Controls.Button cancelButton;
        
        public string Answer { get; private set; }

        public InputDialog(string title, string question)
        {
            Title = title;
            Width = 400;
            Height = 200;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;
            WindowStyle = WindowStyle.ToolWindow;
            
            // Ana grid
            var grid = new Grid();
            Content = grid;
            
            // Satır tanımları
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            
            // Soru etiketi
            var questionTextBlock = new TextBlock
            {
                Text = question,
                Margin = new Thickness(10),
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(questionTextBlock, 0);
            grid.Children.Add(questionTextBlock);
            
            // Cevap metin kutusu
            answerTextBox = new System.Windows.Controls.TextBox
            {
                Margin = new Thickness(10, 5, 10, 10),
                Height = 25
            };
            Grid.SetRow(answerTextBox, 1);
            grid.Children.Add(answerTextBox);
            
            // Buton paneli
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 0, 10, 10)
            };
            Grid.SetRow(buttonPanel, 2);
            grid.Children.Add(buttonPanel);
            
            // Tamam butonu
            okButton = new System.Windows.Controls.Button
            {
                Content = "Tamam",
                Width = 80,
                Height = 25,
                Margin = new Thickness(0, 0, 10, 0),
                IsDefault = true
            };
            okButton.Click += OkButton_Click;
            buttonPanel.Children.Add(okButton);
            
            // İptal butonu
            cancelButton = new System.Windows.Controls.Button
            {
                Content = "İptal",
                Width = 80,
                Height = 25,
                IsCancel = true
            };
            cancelButton.Click += CancelButton_Click;
            buttonPanel.Children.Add(cancelButton);
            
            // Odağı metin kutusuna ver
            Loaded += (s, e) => answerTextBox.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Answer = answerTextBox.Text;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
} 