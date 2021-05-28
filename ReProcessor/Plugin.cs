﻿using IPA;
using IPA.Config.Stores;
using IPA.Loader;
using ReProcessor.Installers;
using SiraUtil;
using SiraUtil.Attributes;
using SiraUtil.Zenject;
using Conf = IPA.Config.Config;
using IPALogger = IPA.Logging.Logger;

namespace ReProcessor
{
    [Plugin(RuntimeOptions.DynamicInit), Slog]
    public class Plugin
    {
        //internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }
        public static Config Config { get; internal set; }

        [Init]
        public Plugin(Conf conf, Zenjector zenjector, IPALogger logger, PluginMetadata metadata)
        {
            Config config = conf.Generated<Config>();
            zenjector.On<PCAppInit>().Pseudo(Container =>
            {
                Container.BindLoggerAsSiraLogger(logger);
                Container.BindInstance(config).AsSingle();
                Container.BindInstance(new UBinder<Plugin, PluginMetadata>(metadata));
            });
            Config = config;
            //Instance = this;
            Log = logger;
            //zenjector.OnApp<MyMainInstaller>().WithParameters(10); // Use Zenject's installer parameter system!
            zenjector.OnMenu<MenuSettingsInstaller>();
            //zenjector.OnMenu<MenuInstaller>();
            zenjector.OnGame<GameplayInstaller>();

            // Specify the scene name or contract or installer!
            //zenjector.On("Menu").Register<Installers.GameplayInstaller>();
        }

        [OnStart]
        public void OnApplicationStart()
        {
            Log.Debug("OnApplicationStart");
            //new GameObject("ReProcessorController").AddComponent<ReProcessorController>();

        }

        [OnExit]
        public void OnApplicationQuit()
        {
            Log.Debug("OnApplicationQuit");

        }
    }
}
