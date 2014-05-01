/*
 *	Copyright (C) 2007-2014 ARGUS TV
 *	http://www.argus-tv.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using ArgusTV.DataContracts;
using ArgusTV.UI.Process;
using ArgusTV.ServiceProxy;

namespace ArgusTV.UI.Console.Panels
{
    public partial class SettingsPanel : ContentPanel
    {
        private Recording _sampleRecording;

        public SettingsPanel()
        {
            InitializeComponent();
            _sampleRecording = Utility.CreateSampleRecording();
        }

        public override string Title
        {
            get { return "Settings"; }
        }

        private int _preRecordSeconds;
        private int _postRecordSeconds;
        private KeepUntilMode _keepUntilMode;
        private int? _keepUntilValue;
        private GuideSource _preferredSource;
        private bool _combineConsecutiveRecordings;
        private bool _autoCombineConsecutiveRecordings;
        private bool _combineRecordingsOnlyOnSameChannel;
        private bool _swapRecorderdTunerPriority;
        private DateTime _timePickerReferenceDate = new DateTime(2100, 1, 1, 0, 0, 0, DateTimeKind.Local);
        private int _allowedTimeFrame = 1 * 3600;
        private bool _includeBetaVersionsInUpdateCheck;
        private bool _createThumbnails;
        private bool _alwaysCreate4trFiles;
 
        private void SettingsPanel_Load(object sender, EventArgs e)
        {
            MainForm.SetMenuMode(MainMenuMode.SaveCancel);            

            // Schedules
            _preRecDateTimePicker.MaxDate = _timePickerReferenceDate.AddSeconds(_allowedTimeFrame);
            _preRecDateTimePicker.MinDate = _timePickerReferenceDate;
            _postRecDateTimePicker.MaxDate = _timePickerReferenceDate.AddSeconds(_allowedTimeFrame);
            _postRecDateTimePicker.MinDate = _timePickerReferenceDate;

            ReloadSettingsUI();
        }

        private void ReloadSettingsUI()
        {
            _preRecordSeconds = Utility.SetDateTimePickerValue(MainForm, _preRecDateTimePicker, ConfigurationKey.Scheduler.PreRecordsSeconds);
            _postRecordSeconds = Utility.SetDateTimePickerValue(MainForm, _postRecDateTimePicker, ConfigurationKey.Scheduler.PostRecordsSeconds);

            _keepUntilMode = KeepUntilMode.UntilSpaceIsNeeded;
            string keepUntilMode = MainForm.ConfigurationProxy.GetStringValue(ConfigurationModule.Scheduler, ConfigurationKey.Scheduler.DefaultKeepUntilMode);
            if (!String.IsNullOrEmpty(keepUntilMode))
            {
                _keepUntilMode = (KeepUntilMode)Enum.Parse(typeof(KeepUntilMode), keepUntilMode);
            }
            _keepUntilValue = MainForm.ConfigurationProxy.GetIntValue(ConfigurationModule.Scheduler, ConfigurationKey.Scheduler.DefaultKeepUntilValue);
            _keepUntilControl.SetKeepUntil(_keepUntilMode, _keepUntilValue);

            // TV-Guide
            string preferredSourceString = MainForm.ConfigurationProxy.GetStringValue(ConfigurationModule.Scheduler, ConfigurationKey.Scheduler.PreferredGuideSource);
            _preferredSource = GuideSource.Other;
            if (!String.IsNullOrEmpty(preferredSourceString))
            {
                _preferredSource = (GuideSource)Enum.Parse(typeof(GuideSource), preferredSourceString);
            }
            _sourceComboBox.SelectedIndex = (_preferredSource == GuideSource.XmlTv) ? 0 : 1;

            // Recording conflicts
            _combineConsecutiveRecordings = GetBooleanSetting(ConfigurationKey.Scheduler.CombineConsecutiveRecordings);
            _combineConsecutiveRecordingsCheckBox.Checked = _combineConsecutiveRecordings;
            _autoCombineConsecutiveRecordings = GetBooleanSetting(ConfigurationKey.Scheduler.AutoCombineConsecutiveRecordings);
            _autoCombineConsecutiveRecordingsCheckBox.Checked = _autoCombineConsecutiveRecordings;
            _combineRecordingsOnlyOnSameChannel = GetBooleanSetting(ConfigurationKey.Scheduler.CombineRecordingsOnlyOnSameChannel);
            _combineRecordingsOnlyOnSameChannelCheckBox.Checked = _combineRecordingsOnlyOnSameChannel;
            _swapRecorderdTunerPriority = GetBooleanSetting(ConfigurationKey.Scheduler.SwapRecorderTunerPriorityForRecordings);
            _swapRecorderdTunerPriorityCheckBox.Checked = _swapRecorderdTunerPriority;

            // eMail server
            _smtpServerTextBox.Text = MainForm.ConfigurationProxy.GetStringValue(
                ConfigurationModule.Scheduler, ConfigurationKey.Scheduler.SmtpServer);
            _smtpPortNumericUpDown.Value = MainForm.ConfigurationProxy.GetIntValue(
                ConfigurationModule.Scheduler, ConfigurationKey.Scheduler.SmtpPort).Value;
            _smtpUserNameTextBox.Text = MainForm.ConfigurationProxy.GetStringValue(
                ConfigurationModule.Scheduler, ConfigurationKey.Scheduler.SmtpUserName);
            _smtpPasswordTextBox.Text = MainForm.ConfigurationProxy.GetStringValue(
                ConfigurationModule.Scheduler, ConfigurationKey.Scheduler.SmtpPassword);
            _smtpUseSslCheckBox.Checked = GetBooleanSetting(ConfigurationKey.Scheduler.SmtpEnableSsl);
            _adminEmailsTextBox.Text = MainForm.ConfigurationProxy.GetStringValue(
                ConfigurationModule.Scheduler, ConfigurationKey.Scheduler.AdministratorEmail);
            _fromEmailTextBox.Text = MainForm.ConfigurationProxy.GetStringValue(
                ConfigurationModule.Scheduler, ConfigurationKey.Scheduler.EmailSender);

            // IMBot settings            
            _msnUserNameTextBox.Text = MainForm.ConfigurationProxy.GetStringValue(ConfigurationModule.Messenger, ConfigurationKey.Messenger.MsnAccount);
            _msnPasswordTextBox.Text = MainForm.ConfigurationProxy.GetStringValue(ConfigurationModule.Messenger, ConfigurationKey.Messenger.MsnPassword);
            int? minutesBeforeAlert = MainForm.ConfigurationProxy.GetIntValue(ConfigurationModule.Messenger, ConfigurationKey.Messenger.MinutesBeforeAlert);
            if (minutesBeforeAlert.HasValue)
            {
                _msnMinutesBeforeAlert.Value = minutesBeforeAlert.Value;
            }
            _msnAddressesTextBox.Text = MainForm.ConfigurationProxy.GetStringValue(ConfigurationModule.Messenger, ConfigurationKey.Messenger.MsnContactList);

            // ARGUS TV Scheduler
            _serverTextBox.Text = ProxyFactory.ServerSettings.ServiceUrlPrefix;
            bool? includeBetaVersionsInUpdateCheck =
                MainForm.ConfigurationProxy.GetBooleanValue(ConfigurationModule.Scheduler, ConfigurationKey.Scheduler.IncludeBetaVersionsInUpdateCheck);
            _includeBetaVersionsInUpdateCheck = includeBetaVersionsInUpdateCheck.HasValue && includeBetaVersionsInUpdateCheck.Value;
            _checkBetaVersionsCheckBox.Checked = _includeBetaVersionsInUpdateCheck;

            // Disk space settings
            int minimumFreeDiskSpaceInMB = MainForm.ConfigurationProxy.GetIntValue(ConfigurationModule.Scheduler, ConfigurationKey.Scheduler.MinimumFreeDiskSpaceInMB).Value;
            if (minimumFreeDiskSpaceInMB >= _minFreeDiskSpaceNumericUpDown.Minimum && minimumFreeDiskSpaceInMB <= _minFreeDiskSpaceNumericUpDown.Maximum)
            {
                _minFreeDiskSpaceNumericUpDown.Value = minimumFreeDiskSpaceInMB;
            }
            else
            {
                _minFreeDiskSpaceNumericUpDown.Minimum = minimumFreeDiskSpaceInMB;
                _minFreeDiskSpaceNumericUpDown.Value = minimumFreeDiskSpaceInMB;
            }
            _freeDiskSpaceNumericUpDown.Minimum = _minFreeDiskSpaceNumericUpDown.Value;

            int freeDiskSpaceInMB = MainForm.ConfigurationProxy.GetIntValue(ConfigurationModule.Scheduler, ConfigurationKey.Scheduler.FreeDiskSpaceInMB).Value;
            if (freeDiskSpaceInMB >= _freeDiskSpaceNumericUpDown.Minimum && freeDiskSpaceInMB <= _freeDiskSpaceNumericUpDown.Maximum)
            {
                _freeDiskSpaceNumericUpDown.Value = freeDiskSpaceInMB;
            }
            else
            {
                _freeDiskSpaceNumericUpDown.Value = _freeDiskSpaceNumericUpDown.Minimum;
            }

            this._minFreeDiskSpaceNumericUpDown.ValueChanged += new System.EventHandler(this._minFreeDiskSpaceNumericUpDown_ValueChanged);

            int wakeupMinutes = MainForm.ConfigurationProxy.GetIntValue(ConfigurationModule.Scheduler, ConfigurationKey.Scheduler.WakeupBeforeEventMinutes).Value;
            _wakeupMinutesNumericUpDown.Value = Math.Max(wakeupMinutes, _wakeupMinutesNumericUpDown.Minimum);

            _createThumbnails = GetBooleanSetting(ConfigurationKey.Scheduler.CreateVideoThumbnails);
            _createThumbnailsCheckBox.Checked = _createThumbnails;
            _alwaysCreate4trFiles = GetBooleanSetting(ConfigurationKey.Scheduler.AlwaysCreateMetadataFiles);
            _alwaysCreate4trFilesCheckBox.Checked = _alwaysCreate4trFiles;

            EnableButtons();
        }

        private bool GetBooleanSetting(string key)
        {
            bool result = false;
            bool? value = MainForm.ConfigurationProxy.GetBooleanValue(ConfigurationModule.Scheduler, key);
            if (value.HasValue)
            {
                result = value.Value;
            }
            return result;
        }

        private void EnableButtons()
        {
            _autoCombineConsecutiveRecordingsCheckBox.Enabled = !_combineConsecutiveRecordingsCheckBox.Checked;
            _combineRecordingsOnlyOnSameChannelCheckBox.Enabled = _combineConsecutiveRecordingsCheckBox.Checked
                || _autoCombineConsecutiveRecordingsCheckBox.Checked;
        }

        public override void OnClosed()
        {
            MainForm.SetMenuMode(MainMenuMode.Normal);
        }

        public override void OnSave()
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                // Schedules
                int preRecordSeconds = (int)_preRecDateTimePicker.Value.TimeOfDay.TotalSeconds;
                if (preRecordSeconds != _preRecordSeconds)
                {
                    MainForm.ConfigurationProxy.SetIntValue(ConfigurationModule.Scheduler, ConfigurationKey.Scheduler.PreRecordsSeconds, preRecordSeconds);
                }
                int postRecordSeconds = (int)_postRecDateTimePicker.Value.TimeOfDay.TotalSeconds;
                if (postRecordSeconds != _postRecordSeconds)
                {
                    MainForm.ConfigurationProxy.SetIntValue(ConfigurationModule.Scheduler, ConfigurationKey.Scheduler.PostRecordsSeconds, postRecordSeconds);
                }
                if (_keepUntilControl.KeepUntilMode != _keepUntilMode)
                {
                    MainForm.ConfigurationProxy.SetStringValue(ConfigurationModule.Scheduler, ConfigurationKey.Scheduler.DefaultKeepUntilMode, _keepUntilControl.KeepUntilMode.ToString());                
                }
                if (_keepUntilControl.KeepUntilValue != _keepUntilValue)
                {
                    MainForm.ConfigurationProxy.SetIntValue(ConfigurationModule.Scheduler, ConfigurationKey.Scheduler.DefaultKeepUntilValue, _keepUntilControl.KeepUntilValue);
                }

                // TV-Guide
                GuideSource preferredSource = (_sourceComboBox.SelectedIndex == 0) ? GuideSource.XmlTv : GuideSource.DvbEpg;
                if (preferredSource != _preferredSource)
                {
                    MainForm.ConfigurationProxy.SetStringValue(ConfigurationModule.Scheduler, ConfigurationKey.Scheduler.PreferredGuideSource, preferredSource.ToString());
                }

                // Recording conflicts
                if (_combineConsecutiveRecordingsCheckBox.Checked != _combineConsecutiveRecordings)
                {
                    MainForm.ConfigurationProxy.SetBooleanValue(ConfigurationModule.Scheduler, ConfigurationKey.Scheduler.CombineConsecutiveRecordings, _combineConsecutiveRecordingsCheckBox.Checked);
                }
                if (_autoCombineConsecutiveRecordingsCheckBox.Checked != _autoCombineConsecutiveRecordings)
                {
                    MainForm.ConfigurationProxy.SetBooleanValue(ConfigurationModule.Scheduler,
                        ConfigurationKey.Scheduler.AutoCombineConsecutiveRecordings, _autoCombineConsecutiveRecordingsCheckBox.Checked);
                }
                if (_combineRecordingsOnlyOnSameChannelCheckBox.Checked != _combineRecordingsOnlyOnSameChannel)
                {
                    MainForm.ConfigurationProxy.SetBooleanValue(ConfigurationModule.Scheduler,
                        ConfigurationKey.Scheduler.CombineRecordingsOnlyOnSameChannel, _combineRecordingsOnlyOnSameChannelCheckBox.Checked);
                }
                if (_swapRecorderdTunerPriorityCheckBox.Checked != _swapRecorderdTunerPriority)
                {
                    MainForm.ConfigurationProxy.SetBooleanValue(ConfigurationModule.Scheduler,
                        ConfigurationKey.Scheduler.SwapRecorderTunerPriorityForRecordings, _swapRecorderdTunerPriorityCheckBox.Checked);
                }

                if (_createThumbnailsCheckBox.Checked != _createThumbnails)
                {
                    MainForm.ConfigurationProxy.SetBooleanValue(ConfigurationModule.Scheduler,
                        ConfigurationKey.Scheduler.CreateVideoThumbnails, _createThumbnailsCheckBox.Checked);
                }
                if (_alwaysCreate4trFilesCheckBox.Checked != _alwaysCreate4trFiles)
                {
                    MainForm.ConfigurationProxy.SetBooleanValue(ConfigurationModule.Scheduler,
                        ConfigurationKey.Scheduler.AlwaysCreateMetadataFiles, _alwaysCreate4trFilesCheckBox.Checked);
                }

                if (_checkBetaVersionsCheckBox.Checked != _includeBetaVersionsInUpdateCheck)
                {
                    MainForm.ConfigurationProxy.SetBooleanValue(ConfigurationModule.Scheduler, ConfigurationKey.Scheduler.IncludeBetaVersionsInUpdateCheck,
                        _checkBetaVersionsCheckBox.Checked);
                    MainForm.ConfigurationProxy.SetStringValue(ConfigurationModule.Scheduler, ConfigurationKey.Scheduler.NextUpdateCheck, null);
                }

                // eMail server
                SaveSmtpSettings();

                // IMBot settings 
                SaveIMBotSettings();

                // Disk space settings
                MainForm.ConfigurationProxy.SetIntValue(
                    ConfigurationModule.Scheduler, ConfigurationKey.Scheduler.MinimumFreeDiskSpaceInMB, (int)_minFreeDiskSpaceNumericUpDown.Value);
                MainForm.ConfigurationProxy.SetIntValue(
                    ConfigurationModule.Scheduler, ConfigurationKey.Scheduler.FreeDiskSpaceInMB, (int)_freeDiskSpaceNumericUpDown.Value);

                MainForm.ConfigurationProxy.SetIntValue(
                    ConfigurationModule.Scheduler, ConfigurationKey.Scheduler.WakeupBeforeEventMinutes, (int)_wakeupMinutesNumericUpDown.Value);

                ClosePanel();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void SaveSmtpSettings()
        {
            MainForm.ConfigurationProxy.SetStringValue(
                ConfigurationModule.Scheduler, ConfigurationKey.Scheduler.SmtpServer, _smtpServerTextBox.Text.Trim());
            MainForm.ConfigurationProxy.SetIntValue(
                ConfigurationModule.Scheduler, ConfigurationKey.Scheduler.SmtpPort, (int)_smtpPortNumericUpDown.Value);
            MainForm.ConfigurationProxy.SetStringValue(
                ConfigurationModule.Scheduler, ConfigurationKey.Scheduler.SmtpUserName, _smtpUserNameTextBox.Text.Trim());
            MainForm.ConfigurationProxy.SetStringValue(
                ConfigurationModule.Scheduler, ConfigurationKey.Scheduler.SmtpPassword, _smtpPasswordTextBox.Text.Trim());
            MainForm.ConfigurationProxy.SetBooleanValue(ConfigurationModule.Scheduler,
                ConfigurationKey.Scheduler.SmtpEnableSsl, _smtpUseSslCheckBox.Checked);
            MainForm.ConfigurationProxy.SetStringValue(
                ConfigurationModule.Scheduler, ConfigurationKey.Scheduler.AdministratorEmail, _adminEmailsTextBox.Text.Trim());
            MainForm.ConfigurationProxy.SetStringValue(
                ConfigurationModule.Scheduler, ConfigurationKey.Scheduler.EmailSender, _fromEmailTextBox.Text.Trim());
        }

        private void SaveIMBotSettings()
        {
            MainForm.ConfigurationProxy.SetStringValue(ConfigurationModule.Messenger, ConfigurationKey.Messenger.MsnAccount, _msnUserNameTextBox.Text.Trim());
            MainForm.ConfigurationProxy.SetStringValue(ConfigurationModule.Messenger, ConfigurationKey.Messenger.MsnPassword, _msnPasswordTextBox.Text.Trim());
            MainForm.ConfigurationProxy.SetIntValue(ConfigurationModule.Messenger, ConfigurationKey.Messenger.MinutesBeforeAlert, (int)_msnMinutesBeforeAlert.Value);
            MainForm.ConfigurationProxy.SetStringValue(ConfigurationModule.Messenger, ConfigurationKey.Messenger.MsnContactList, _msnAddressesTextBox.Text.Trim());
        }

        private void _deleteGuideDataButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Are you sure you want to" + Environment.NewLine
                    + "delete all guide programs?", "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                MainForm.GuideProxy.DeleteAllPrograms();
            }
        }

        private void _reconnectButton_Click(object sender, EventArgs e)
        {
            if (MainForm.ConnectToArgusTVService(null, true))
            {
                ReloadSettingsUI();
            }
        }

        private void _combineConsecutiveRecordingsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            EnableButtons();
        }

        private void _autoCombineConsecutiveRecordingsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            EnableButtons();
        }

        private void _sendTestMailButton_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(_smtpServerTextBox.Text.Trim())
                || String.IsNullOrEmpty(_adminEmailsTextBox.Text.Trim()))
            {
                MessageBox.Show(this, "You must set both the SMTP server " + Environment.NewLine
                    + "and administrator e-mail settings", _sendTestMailButton.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (MessageBox.Show(this, "Save the SMTP settings and" + Environment.NewLine
                    + "send a test mail?", _sendTestMailButton.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    SaveSmtpSettings();
                    new LogServiceProxy().SendTestMail();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void _minFreeDiskSpaceNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            _freeDiskSpaceNumericUpDown.Minimum = _minFreeDiskSpaceNumericUpDown.Value;
        }
    }
}
