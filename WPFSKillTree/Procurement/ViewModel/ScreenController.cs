using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using POEApi.Model;
using Procurement.View;
using POESKillTree;
using Procurement.Controls;

namespace Procurement.ViewModel
{
    public class ScreenController
    {
        private static Grid inventory;
        private static Dictionary<string, IView> screens = new Dictionary<string, IView>();

        public double HeaderHeight { get; set; }
        public double FooterHeight { get; set; }
        public bool ButtonsVisible { get; set; }
        public bool FullMode { get; set; }

        public static ScreenController Instance = null;

        public static void Create(Grid layout)
        {
            Instance = new ScreenController(layout);
        }

        private ScreenController(Grid layout)
        {
            inventory = layout;
            initLogin();
        }

        private void execute(object obj)
        {
            LoadView(screens[obj.ToString()]);
        }

        private void initScreens()
        {
            screens.Clear();
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal,
            new Action(() =>
            {
                screens.Add("Inventory", new InventoryView());
            }));
        }

        private void initLogin()
        {
            var loginView = new LoginView();
            var loginVM = (loginView.DataContext as LoginWindowViewModel);
            loginVM.OnLoginCompleted += new LoginWindowViewModel.LoginCompleted(loginCompleted);
            LoadView(loginView);
        }

        void loginCompleted()
        {
            initScreens();
            LoadView(screens.First().Value);
        }

        private static void showMenuButtons()
        {
            //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            //new Action(() =>
            //{
            //    mainView.Buttons.Visibility = Visibility.Visible;
            //}));
        }

        public void LoadView(IView view)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                new Action(() =>
                {
                    inventory.Children.Clear();
                    inventory.Children.Add(view as UserControl);
                }));
        }
    }
}