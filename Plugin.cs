﻿using IPA;
using IPA.Config.Stores;
using ServerBrowser.Installers;
using SiraUtil.Web.SiraSync;
using SiraUtil.Zenject;
using IPALogger = IPA.Logging.Logger;

namespace ServerBrowser
{
    [Plugin(RuntimeOptions.DynamicInit)]
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Plugin
    {
        // ReSharper disable once MemberCanBePrivate.Global
        internal static BssbConfig Config { get; private set; } = null!;
        
        private IPALogger _log = null!;

        [Init]
        public void Init(IPALogger logger, Zenjector zenjector, IPA.Config.Config config)
        {
            _log = logger;
            
            Config = config.Generated<BssbConfig>();

            zenjector.UseMetadataBinder<Plugin>();
            zenjector.UseLogger(logger);
            zenjector.UseHttpService();
            zenjector.UseSiraSync(SiraSyncServiceType.GitHub, "roydejong", "BeatSaberServerBrowser");
            
            zenjector.Install<BssbAppInstaller>(Location.App);
            zenjector.Install<BssbMenuInstaller>(Location.Menu);
        }

        [OnEnable]
        public void OnEnable()
        {
        }

        [OnDisable]
        public void OnDisable()
        {
        }
    }
}