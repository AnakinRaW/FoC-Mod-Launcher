﻿using FocLauncherApp.Utilities;

namespace FocLauncherApp
{
    public partial class SplashScreen
    {
        public SplashScreen()
        {
            InitializeComponent();
            MainWindowManager.Instance.Initialize(this);
        }
    }
}
