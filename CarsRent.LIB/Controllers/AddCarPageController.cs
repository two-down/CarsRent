﻿using System.Windows.Controls;
using CarsRent.LIB.DataBase;
using CarsRent.LIB.Model;
using CarsRent.LIB.Validation;

namespace CarsRent.LIB.Controllers;

public class AddCarPageController : BaseAddEntityController
{
    private Car? _car;

    private readonly Dictionary<string, WheelsType> _wheelsType;
    private readonly Dictionary<string, Status> _status;

    public AddCarPageController(Car? car)
    {
        _wheelsType = new Dictionary<string, WheelsType>
        {
            { "летние", WheelsType.Summer },
            { "зимние", WheelsType.Winter }
        };

        _status = new Dictionary<string, Status>
        {
            { "готова", Status.Ready },
            { "в аренде", Status.OnLease },
            { "в ремонте", Status.UnderRepair }
        };

        _car = car;
    }

    public void CreateComboBoxesValues(ref ComboBox wheelsType, ref ComboBox carStatus)
    {
        wheelsType.ItemsSource = _wheelsType.Keys;
        wheelsType.SelectedIndex = 0;
        carStatus.ItemsSource = _status.Keys;
        carStatus.SelectedIndex = 0;
    }

    public ValueTask<bool> UpdateOwnersItemsSourceAsync(string searchText, int startPoint, int count, ref ListBox listBox)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            listBox.ItemsSource = HumanCommands.FindAndSelectOwnersAsync(searchText, startPoint, count).AsTask().Result;
            return new ValueTask<bool>(true);
        }
        
        if (_car == null || _car.OwnerId.HasValue == false)
        {
            listBox.ItemsSource = HumanCommands.SelectOwnersGroupAsync(startPoint, count).AsTask().Result;
            return new ValueTask<bool>(true);
        }

        var selectedOwner = BaseCommands<Owner>.SelectByIdAsync(_car.OwnerId).AsTask().Result;
        var selectedHuman = BaseCommands<Human>.SelectByIdAsync(selectedOwner.HumanId).AsTask().Result;

        var humansList = HumanCommands.SelectOwnersGroupAsync(startPoint, count)
            .AsTask().Result.Where(human => human.Id != selectedHuman.Id).Take(2).ToList();
        humansList.Add(selectedHuman);

        listBox.ItemsSource = humansList;
        listBox.SelectedItem = selectedHuman;

        return new ValueTask<bool>(true);
    }

    public override string AddEditEntity(UIElementCollection collection, Dictionary<string, string> valuesRelDict)
    {
        var valuesDict = new Dictionary<string, string>(valuesRelDict);

        if (int.TryParse(valuesDict["price"], out var price) == false)
        {
            return "В поле стоимость автомобиля введено не число.";
        }

        if (int.TryParse(valuesDict["year"], out var year) == false)
        {
            return "В поле год выпуска автомобиля введено не число.";
        }

        if (int.TryParse(valuesDict["displacement"], out var displacement) == false)
        {
            return "В поле рабочий объем двигателя автомобиля введено не число.";
        }

        if (DateTime.TryParse(valuesDict["issuingDate"], out var issuingDate) == false)
        {
            return "В поле дата выдачи паспорта автомобиля введена не дата.";
        }
        
        if (int.TryParse(valuesDict["owner"], out var humanId) == false)
        {
            return "Вы не выбрали арендодателя.";
        }
        
        var owner = BaseCommands<Owner>.SelectAllAsync().AsTask().Result
            .Where(owner => owner.HumanId == humanId).FirstOrDefault();

        _car ??= new Car();

        _car.Brand = valuesDict["brand"];
        _car.Model = valuesDict["model"];
        _car.PassportNumber = valuesDict["passportNumber"];
        _car.VIN = valuesDict["vin"];
        _car.BodyNumber = valuesDict["bodyNumber"];
        _car.Color = valuesDict["color"];
        _car.EngineNumber = valuesDict["engineNumber"];
        _car.RegistrationNumber = valuesDict["registrationNumber"];
        _car.Price = price;
        _car.Year = year;
        _car.EngineDisplacement = displacement;
        _car.PassportIssuingDate = issuingDate;
        _car.WheelsType = _wheelsType[valuesDict["wheelsType"]];
        _car.CarStatus = _status[valuesDict["status"]];
        _car.OwnerId = owner.Id;

        var validationResults = ModelValidation.Validate(_car);
            
        if (validationResults.Any())
        {
            return validationResults.First().ErrorMessage;
        }

        base.SaveItemInDb(_car);
        
        return string.Empty;
    }
}