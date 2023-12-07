using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Azure.WPF.Framework;
using Azure.ImagingSystem;
using Azure.LaserScanner.ViewModel;
using Azure.Configuration.Settings;
using Azure.LaserScanner.View;

namespace Azure.LaserScanner
{
    public class SettingsViewModel : ViewModelBase
    {
        #region Fields

        private ICommand _changePageCommand;

        private IPageViewModel _currentPageViewModel;
        private List<IPageViewModel> _pageViewModels;

        #endregion

        public SettingsViewModel()
        {
            // Add available pages
            //
            // Add 'General' tab
            PageViewModels.Add(new GeneralSettingsViewModel());
            // Add 'Advanced' tab
            PageViewModels.Add(new AdvancedSettingsViewModel());

            // Set starting page
            if (PageViewModels != null && PageViewModels.Count > 0)
            {
                CurrentPageViewModel = PageViewModels[0];
                CurrentPageViewModel.IsSelected = true;
            }
        }

        #region Properties / Commands

        public ICommand ChangePageCommand
        {
            get
            {
                if (_changePageCommand == null)
                {
                    _changePageCommand = new RelayCommand(
                        p => ChangeViewModel((IPageViewModel)p),
                        p => p is IPageViewModel);
                }

                return _changePageCommand;
            }
        }

        public List<IPageViewModel> PageViewModels
        {
            get
            {
                if (_pageViewModels == null)
                    _pageViewModels = new List<IPageViewModel>();

                return _pageViewModels;
            }
        }

        public IPageViewModel CurrentPageViewModel
        {
            get
            {
                return _currentPageViewModel;
            }
            set
            {
                if (_currentPageViewModel != value)
                {
                    _currentPageViewModel = value;
                    RaisePropertyChanged("CurrentPageViewModel");
                }
            }
        }

        #endregion

        #region Methods

        private void ChangeViewModel(IPageViewModel viewModel)
        {
            if (!PageViewModels.Contains(viewModel))
                PageViewModels.Add(viewModel);

            if (_currentPageViewModel != viewModel)
            {
                if (!Workspace.This.HasSecurePassword &&
                    !viewModel.Name.Equals("GENERAL", StringComparison.OrdinalIgnoreCase))
                {
                    // Prompt for password
                    string strSecureXmlPath = System.IO.Path.Combine(Workspace.This.AppDataPath, "Secure.xml");

                    // Get the current password for the 'Settings' page
                    string passwordHash = SecureSettings.GetPassword(strSecureXmlPath);

                    // Don't prompt for a password if current password is blank/empty
                    if (!string.IsNullOrEmpty(passwordHash))
                    {
                        PasswordPrompt passwordPrompt = new PasswordPrompt(strSecureXmlPath);
                        passwordPrompt.ShowDialog();
                        if (passwordPrompt.DialogResult != true)
                        {
                            _currentPageViewModel.IsSelected = true;    // Keep the current page selected.
                            return;
                        }
                    }

                    // Don't prompt for password until the next session.
                    Workspace.This.HasSecurePassword = true;
                }

                CurrentPageViewModel = PageViewModels
                    .FirstOrDefault(vm => vm == viewModel);
            }
        }

        #endregion
    }
}
