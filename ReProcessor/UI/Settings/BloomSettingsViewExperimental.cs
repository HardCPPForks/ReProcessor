﻿using System;
using System.Collections.Generic;
using System.Linq;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
//using RuntimeUnityEditor.BSIPA4;
using UnityEngine.Serialization;
using Zenject;
using ReProcessor.Files;
using static ReProcessor.Config;
using static ReProcessor.PresetExtensions;

namespace ReProcessor.UI
{
    [ViewDefinition("ReProcessor.UI.Views.BloomSettingsViewExperimental.bsml")]
    [HotReload(RelativePathToLayout = @"..\Views\BloomSettingsViewExperimental.bsml")]
    internal abstract class BloomSettingsView2 : BSMLAutomaticViewController
    {
        internal static BloomSettingsView2 Instance;
        private static Preset tmpPreset;

        public abstract List<CameraSetting> GetSettings();
        public abstract List<CameraSetting> GetDefaults();

        [UIComponent("setting-list")]
        internal CustomCellListTableData SettingList;
        [UIValue("settings")]
        internal List<object> settingsList = new List<object>();

        public class EffectListObject
        {
            internal CameraSetting setting;
            

            [UIValue("label")] private string Label = "";
            [UIValue("increment")] private float Increment = 0.05f;
            [UIValue("min")] private float Min = Plugin.Config.MinAmountIncrease;
            [UIValue("max")] private float Max = Plugin.Config.MaxAmountIncrease;
            [UIValue("num")]
            internal bool IsNumber
            {
                get => setting.ValueType.Equals(valueType.Decimal) || setting.ValueType.Equals(valueType.Integer);
            }
            [UIValue("int")] private bool integer
            {
                get => setting.ValueType.Equals(valueType.Integer);
            }
            [UIValue("enum")]
            internal bool IsDropdown
            {
                get => setting.ValueType.Equals(valueType.Enumerator);
            }
            [UIValue("dropdown-options")] private List<object> passes = Defaults.Passes;

            [UIValue("dropdown-value")]
            private string DropdownValue
            {
                get => setting.Value.ToString();
                set
                {
                    Instance.NotifyPropertyChanged();
                    setting.Value = value;
                    
                    Managers.MenuCoreManager.MainCamAccess().SetCameraSetting(setting);
                }
            }
            [UIValue("slider-value")]
            private float SliderValue
            {
                get
                {
                    if (setting.ValueType.Equals(valueType.Decimal))
                        return (float)setting.Value;
                    if (setting.ValueType.Equals(valueType.Integer))
                        return (Int32)setting.Value;
                    else
                        return 0;
                }
                set
                {
                    Instance.NotifyPropertyChanged();
                    setting.Value = value;
                    Managers.MenuCoreManager.MainCamAccess().SetCameraSetting(setting);
                }
            }

            [UIAction("decrease")]
            private void DecreaseVal()
            {
                SliderValue -= Increment;
                Instance.SettingList.tableView.ReloadData();
            }
            [UIAction("increase")]
            private void IncreaseVal()
            {
                SliderValue += Increment;
                Instance.SettingList.tableView.ReloadData();

            }
            public EffectListObject(CameraSetting camSetting)
            {
                Plugin.Log.Notice($"{camSetting.FriendlyName} has a value of {camSetting.Value} (type of {camSetting.Value.GetType().ToString()})");

                this.setting = camSetting;
                this.Label = setting.FriendlyName;
                if (camSetting.ValueType.Equals(valueType.Decimal))
                {
                    this.SliderValue = (float.Parse(camSetting.Value.ToString()));
                    //this.DropdownValue = "";
                }
                if (camSetting.ValueType.Equals(valueType.Integer))
                {
                    this.SliderValue = (Int32.Parse(camSetting.Value.ToString()));
                    //this.DropdownValue = "";
                }
                if (camSetting.ValueType.Equals(valueType.Enumerator))
                {
                    this.DropdownValue = camSetting.Value.ToString();
                    //this.SliderValue = 0f;
                }
            }
        }

        [UIAction("#post-parse")]
        internal void PostParse()
        {
            Plugin.preset = Load(Plugin.PresetName);
            Instance = this;
            SettingList.data.Clear();
            Fill(GetSettings());
        }

        internal static void Fill(List<CameraSetting> preset)
        {
            Instance.SettingList.data.Clear();
            Plugin.Log.Notice($"setting list currently has {preset.Count()} Settings");
            foreach (var setting in preset)
                Instance.SettingList.data.Add(new EffectListObject(setting));
            Instance.NotifyPropertyChanged();
            Instance.SettingList.tableView.ReloadData();
        }

        [UIAction("revert")]
        internal void Revert()
        {
            GetSettings().Clear();
            foreach (var a in GetDefaults())
                GetSettings().Add(a);
            
            Managers.MenuCoreManager.MainCamAccess().ApplySettings(GetSettings());
            Instance.NotifyPropertyChanged();
            Fill(GetSettings());
            Plugin.preset.Save();
            Plugin.preset = Load(Plugin.PresetName);
        }

        [UIAction("cancel-button")]
        internal void Cancel()
        {
            Plugin.preset = Load(Plugin.PresetName);
            Managers.MenuCoreManager.MainCamAccess().ApplySettings(GetSettings());
            rSettingsFlowCoordinator.SwitchMiddleView();
            Instance.NotifyPropertyChanged();
        }
        [UIAction("apply-button")]
        internal void Apply()
        {
            GetSettings().Clear();
            foreach(var setting in settingsList)
            {
                EffectListObject e = (EffectListObject)setting;
                GetSettings().Add(e.setting);
            }
            Plugin.preset.Save();
            Instance.NotifyPropertyChanged();
            SettingList.tableView.ReloadData();
        }

    }
}