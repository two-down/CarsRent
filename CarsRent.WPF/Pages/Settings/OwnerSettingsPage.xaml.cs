﻿using CarsRent.LIB.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CarsRent.LIB.DataBase;
using CarsRent.LIB.Model;
using CarsRent.WPF.ViewControllers;

namespace CarsRent.WPF.Pages.Settings
{
    public partial class OwnerSettingsPage : Page
    {
        private int? _ownerId;
        
        public OwnerSettingsPage()
        {
            InitializeComponent();
            UpdateOwners();
        }

        private async void UpdateOwners()
        {
            var list = await OwnersSettings.GetOwners(TbxSearchOwner.Text, 0, 10);
            LbxOwner.ItemsSource = list;
        }

        private void tbxSearchOwner_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateOwners();
        }

        private void ButtonAddEdit_OnClick(object sender, RoutedEventArgs e)
        {
            var controller = new FillingRenterFieldsController();
            var fields = new Dictionary<string, string>(controller.CreateValuesRelationDict(Panel.Children));

            var error = OwnersSettings.AddOwner(fields, _ownerId);

            if (string.IsNullOrWhiteSpace(error))
            {
                LblAddError.Content = string.Empty;
                LblAddDone.Content = "Арендодатель успешно добавлен.";
                UpdateOwners();
                return;
            }

            LblAddDone.Content = string.Empty;
            LblAddError.Content = error;

            _ownerId = null;
        }

        private void ButtonDelete_OnClick(object sender, RoutedEventArgs e)
        {
            var error = OwnersSettings.DeleteOwner(LbxOwner.SelectedItem);
            
            if (string.IsNullOrWhiteSpace(error))
            {
                UpdateOwners();
                LblChooseError.Content = string.Empty;
                LblChooseDone.Content = "Арендодатель успешно удален.";
                return;
            }

            LblChooseDone.Content = string.Empty;
            LblChooseError.Content = error;
        }

        private void ButtonModify_OnClick(object sender, RoutedEventArgs e)
        {
            if (LbxOwner.SelectedItem is not Human human)
            {
                return;
            }

            var controller = new FillingRenterFieldsController();
            var valuesRelDict = controller.CreateValuesRelationDict(human);
            var collection = Panel.Children;
            
            controller.FillFields(ref collection, valuesRelDict);

            var owner = BaseCommands<Owner>.SelectAllAsync().AsTask().Result
                .Where(owner => owner.HumanId == human.Id).FirstOrDefault();

            _ownerId = owner.Id;
        }
    }
}