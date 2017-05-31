﻿using System;
using System.Linq;
using System.Reflection;

using BizHawk.Emulation.Common;
using BizHawk.Client.Common;

namespace BizHawk.Client.EmuHawk
{
	// Loads and Injects Dependencies into Config dialogs
	// Currently the notion of a config dialog is anything that inherits Form
	// Currently only supports Model dialogs, Modeless requires extra logic for focusing
	// Currently only supports parameterless constructors, dependenies should be declared and injected
	public class ConfigManager
	{
		private readonly MainForm _mainForm;
		private readonly Config _config;

		public ConfigManager(MainForm mainForm, Config config)
		{
			_mainForm = mainForm;
			_config = config;
		}

		public bool IsAvailable<T>()
			where T : ConfigForm
		{
			return IsAvailable(typeof(T));
		}

		private bool IsAvailable(Type t)
		{
			if (!ServiceInjector.IsAvailable(_mainForm.Emulator.ServiceProvider, t))
			{
				return false;
			}

			var tool = Assembly
				.GetExecutingAssembly()
				.GetTypes()
				.FirstOrDefault(type => type == t);

			if (tool == null) // This isn't a tool, must not be available
			{
				return false;
			}

			return true;
		}

		public T ShowDialog<T>()
			where T : ConfigForm
		{
			T newDialog = Activator.CreateInstance<T>();

			var result = ServiceInjector.UpdateServices(_mainForm.Emulator.ServiceProvider, newDialog);

			if (!result)
			{
				throw new InvalidOperationException("Current core can not provide all the required dependencies");
			}

			newDialog.Owner = _mainForm;
			newDialog.MainForm = _mainForm;
			newDialog.Config = _config;

			newDialog.ShowDialog();

			return newDialog;
		}
	}
}