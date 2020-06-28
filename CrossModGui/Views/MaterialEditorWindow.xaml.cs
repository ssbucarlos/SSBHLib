﻿using CrossModGui.ViewModels;
using System.Windows;

namespace CrossModGui.Views
{
    /// <summary>
    /// Interaction logic for MaterialEditorWindow.xaml
    /// </summary>
    public partial class MaterialEditorWindow : Window
    {
        private readonly MaterialEditorWindowViewModel viewModel;
        public MaterialEditorWindow(MaterialEditorWindowViewModel viewModel)
        {
            this.viewModel = viewModel;
            DataContext = this.viewModel;
            InitializeComponent();
        }
    }
}