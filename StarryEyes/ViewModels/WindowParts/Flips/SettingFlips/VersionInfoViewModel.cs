﻿using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Livet;
using StarryEyes.Models.Subsystems;
using StarryEyes.Settings;

namespace StarryEyes.ViewModels.WindowParts.Flips.SettingFlips
{
    public class VersionInfoViewModel : ViewModel
    {
        public const string Twitter = "http://twitter.com/";

        public VersionInfoViewModel()
        {
            // when update is available, callback this.
            AutoUpdateService.UpdateStateChanged += () => this._isUpdateAvailable = true;
            _contributors = ViewModelHelperRx.CreateReadOnlyDispatcherCollectionRx(
                ContributionService.Contributors, c => new ContributorViewModel(c),
                DispatcherHolder.Dispatcher);
            this.CompositeDisposable.Add(
                _contributors.ListenCollectionChanged()
                             .Subscribe(_ => RaisePropertyChanged(() => IsDonated)));
        }

        [UsedImplicitly]
        public void CheckUpdate()
        {
            Task.Run(() => this.CheckUpdates());
        }

        #region Open links

        public void OpenOfficial()
        {
            BrowserHelper.Open(App.OfficialUrl);
        }

        public void OpenKarno()
        {
            BrowserHelper.Open(Twitter + "karno");
        }

        public void OpenKriletan()
        {
            BrowserHelper.Open(Twitter + "kriletan");
        }

        public void OpenLicense()
        {
            BrowserHelper.Open(App.LicenseUrl);
        }

        public void OpenDonation()
        {
            BrowserHelper.Open(App.DonationUrl);
        }

        #endregion

        #region Contributors

        private readonly ReadOnlyDispatcherCollectionRx<ContributorViewModel> _contributors;
        public ReadOnlyDispatcherCollectionRx<ContributorViewModel> Contributors
        {
            get { return this._contributors; }
        }

        public bool IsDonated
        {
            get
            {
                var users = ContributionService.Contributors.Select(c => c.ScreenName).ToArray();
                return Setting.Accounts.Collection.Any(c => users.Contains(c.UnreliableScreenName));
            }
        }

        #endregion

        #region Versioning

        public string Version
        {
            get { return App.FormattedVersion; }
        }

        private bool _isChecking;
        private bool _isUpdateAvailable;

        public bool IsChecking
        {
            get { return this._isChecking; }
        }

        public bool IsUpdateIsNotExisted
        {
            get { return !this._isChecking && !this._isUpdateAvailable; }
        }

        public bool IsUpdateExisted
        {
            get { return !this._isChecking && this._isUpdateAvailable; }
        }

        public async void CheckUpdates()
        {
            this._isChecking = true;
            this.RefreshCheckState();
            this._isUpdateAvailable = await AutoUpdateService.CheckPrepareUpdate(App.Version);
            this._isChecking = false;
            this.RefreshCheckState();
        }

        private void RefreshCheckState()
        {
            this.RaisePropertyChanged(() => this.IsChecking);
            this.RaisePropertyChanged(() => this.IsUpdateExisted);
            this.RaisePropertyChanged(() => this.IsUpdateIsNotExisted);
        }

        public void StartUpdate()
        {
            AutoUpdateService.StartUpdate(App.Version);
        }

        #endregion
    }

    public class ContributorViewModel : ViewModel
    {
        private readonly string _name;
        private readonly string _screenName;

        public string Name
        {
            get { return this._name; }
        }

        public string ScreenName
        {
            get { return this._screenName; }
        }

        public bool IsLinkOpenable
        {
            get { return this._screenName != null; }
        }

        public ContributorViewModel(Contributor contributor)
        {
            this._name = contributor.Name;
            this._screenName = contributor.ScreenName;
        }

        public void OpenUserTwitter()
        {
            BrowserHelper.Open(VersionInfoViewModel.Twitter + this._screenName);
        }
    }
}
