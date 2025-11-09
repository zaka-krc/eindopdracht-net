using SuntoryManagementSystem.Models;
using System;
using System.Windows;
using System.Windows.Input;

namespace SuntoryManagementSystem
{
    public partial class UserInfoPopup : Window
    {
        private bool _isClosing = false;

        public UserInfoPopup(ApplicationUser user, string roles)
        {
            InitializeComponent();

            // Set user info
            txtFullName.Text = user.FullName ?? "Gast";
            txtRole.Text = roles ?? "Geen rol";
            txtEmail.Text = user.Email ?? "Geen email";
            txtDepartment.Text = user.Department ?? "Geen afdeling";
            txtLastLogin.Text = user.LastLoginDate?.ToString("dd-MM-yyyy HH:mm") ?? "Nooit";

            // Allow dragging
            MouseLeftButtonDown += (s, e) => 
            {
                try
                {
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        DragMove();
                    }
                }
                catch
                {
                    // Ignore any exceptions during drag
                }
            };
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            SafeClose();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            // Auto-close when clicking outside
            SafeClose();
        }

        private void SafeClose()
        {
            if (_isClosing)
                return;

            try
            {
                _isClosing = true;
                Close();
            }
            catch (InvalidOperationException)
            {
                // Window is already closing or closed, ignore
            }
        }

        // Position popup near the button that opened it
        public void ShowNearElement(FrameworkElement element)
        {
            Point position = element.PointToScreen(new Point(0, element.ActualHeight));
            
            // Adjust position to keep popup on screen
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            Left = position.X - (Width - element.ActualWidth);
            Top = position.Y + 5;

            // Keep popup on screen
            if (Left + Width > screenWidth)
                Left = screenWidth - Width - 10;
            
            if (Top + Height > screenHeight)
                Top = position.Y - Height - 5;

            Show();
        }
    }
}
