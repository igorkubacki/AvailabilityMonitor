# Availability Monitor

## ℹ️ Newer version
There is a newer, re-created version of the app using Firebase instead of Entity Framework [here](https://github.com/igorkubacki/AvailabilityMonitor-Firebase).

## Table of contents
* [General info](#-general-info)
* [Features](#-features)
* [How it works](#%EF%B8%8F-how-it-works)
* [Technologies](#-technologies)

## 📄 General info
The app was created to monitor product stock and prices and change it on the website I am currently managing. It imports products from 
PrestaShop, then adds info about stock and prices based on the supplier XML file and notifies about any changes.

## ✨ Features

* Importing products from your PrestaShop online store - no need to add them manually.
* Easily browse your products - sort and filter them as you need.
* Meaningful data charts - see the price and quantity for each product in past months.
* Notifications page - a quick insight into latest changes.


## ✔️ How it works

* Import your products from PrestaShop by just entering your shop URL and API key in the configuration tab.
* Enter the URL of the supplier XML file with your products (products will be matched by the product SKU).
* Now you can update supplier info when you want and see all the changes.


## 💻 Technologies

* .NET 6
* [Chart.js](https://www.chartjs.org/)
* Entity Framework Core
* Visual Studio
