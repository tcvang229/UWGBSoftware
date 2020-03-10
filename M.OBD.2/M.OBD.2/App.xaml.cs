using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
//using M.OBD.2.Services;
//using M.OBD.2.Views;

namespace M.OBD._2
{
    public partial class App : Application
    {
        //string to hold the location of the sql-lite database
        //init to empty
        public static string Database = string.Empty;

        public App()
        {
            InitializeComponent();


            //setup navigation for the rest of the project
            //make the MainPage the first page that opens
            MainPage = new NavigationPage(new MainPage());
        }

        public App(string database)
        {
            InitializeComponent();

            MainPage = new NavigationPage(new MainPage());

            Database = database;
        }


        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
