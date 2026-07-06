using System;
using System.Drawing;
using System.Windows.Forms;

namespace PrivacyDots
{
    public class SettingsForm : Form
    {
        readonly AppSettings _live;
        readonly AppSettings _original;
        readonly Action _onChanged;

        TrackBar _sizeBar;
        Label _sizeValue;
        ComboBox _positionBox;
        NumericUpDown _marginBox;
        CheckBox _startupBox;
        bool _accepted;

        public SettingsForm(AppSettings live, Action onChanged)
        {
            _live = live;
            _original = live.Snapshot();
            _onChanged = onChanged;
            BuildUi();
        }

        void BuildUi()
        {
            Text = "Privacy Dots Settings";
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = true;
            ShowInTaskbar = true;
            StartPosition = FormStartPosition.CenterScreen;
            AutoScaleMode = AutoScaleMode.Font;
            Font = new Font("Segoe UI", 9f);
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            try
            {
                Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch
            {
            }

            TableLayoutPanel grid = new TableLayoutPanel();
            grid.AutoSize = true;
            grid.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            grid.ColumnCount = 3;
            grid.Padding = new Padding(12);
            grid.Location = new Point(0, 0);

            // row 0: dot size
            Label sizeLabel = MakeLabel("Dot size:");
            grid.Controls.Add(sizeLabel, 0, 0);

            _sizeBar = new TrackBar();
            _sizeBar.Minimum = AppSettings.MinDotSize;
            _sizeBar.Maximum = AppSettings.MaxDotSize;
            _sizeBar.TickFrequency = 2;
            _sizeBar.Value = _live.DotSize;
            _sizeBar.Width = 220;
            _sizeBar.Anchor = AnchorStyles.Left;
            _sizeBar.ValueChanged += delegate
            {
                _live.DotSize = _sizeBar.Value;
                _sizeValue.Text = _sizeBar.Value + " px";
                NotifyChanged();
            };
            grid.Controls.Add(_sizeBar, 1, 0);

            _sizeValue = MakeLabel(_live.DotSize + " px");
            grid.Controls.Add(_sizeValue, 2, 0);

            // row 1: position
            grid.Controls.Add(MakeLabel("Position:"), 0, 1);

            _positionBox = new ComboBox();
            _positionBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _positionBox.Items.AddRange(new object[]
            {
                "Top left", "Top center", "Top right", "Bottom left", "Bottom right"
            });
            _positionBox.SelectedIndex = (int)_live.Position;
            _positionBox.Width = 180;
            _positionBox.Anchor = AnchorStyles.Left;
            _positionBox.Margin = new Padding(3, 6, 3, 6);
            _positionBox.SelectedIndexChanged += delegate
            {
                _live.Position = (DotPosition)_positionBox.SelectedIndex;
                NotifyChanged();
            };
            grid.Controls.Add(_positionBox, 1, 1);
            grid.SetColumnSpan(_positionBox, 2);

            // row 2: margin
            grid.Controls.Add(MakeLabel("Edge margin:"), 0, 2);

            FlowLayoutPanel marginRow = new FlowLayoutPanel();
            marginRow.AutoSize = true;
            marginRow.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            marginRow.WrapContents = false;
            marginRow.Margin = new Padding(0);
            marginRow.Anchor = AnchorStyles.Left;

            _marginBox = new NumericUpDown();
            _marginBox.Minimum = 0;
            _marginBox.Maximum = AppSettings.MaxMargin;
            _marginBox.Value = _live.Margin;
            _marginBox.Width = 70;
            _marginBox.Margin = new Padding(3, 6, 3, 6);
            _marginBox.ValueChanged += delegate
            {
                _live.Margin = (int)_marginBox.Value;
                NotifyChanged();
            };
            marginRow.Controls.Add(_marginBox);

            Label marginUnit = MakeLabel("px from screen edge");
            marginUnit.Margin = new Padding(3, 10, 3, 6);
            marginRow.Controls.Add(marginUnit);

            grid.Controls.Add(marginRow, 1, 2);
            grid.SetColumnSpan(marginRow, 2);

            // row 3: startup checkbox
            _startupBox = new CheckBox();
            _startupBox.Text = "Start with Windows";
            _startupBox.Checked = AppSettings.GetRunAtStartup();
            _startupBox.AutoSize = true;
            _startupBox.Margin = new Padding(3, 10, 3, 6);
            _startupBox.Anchor = AnchorStyles.Left;
            grid.Controls.Add(_startupBox, 0, 3);
            grid.SetColumnSpan(_startupBox, 3);

            // row 4: buttons, right-aligned
            FlowLayoutPanel buttonRow = new FlowLayoutPanel();
            buttonRow.FlowDirection = FlowDirection.RightToLeft;
            buttonRow.AutoSize = true;
            buttonRow.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            buttonRow.WrapContents = false;
            buttonRow.Margin = new Padding(3, 14, 3, 3);
            buttonRow.Anchor = AnchorStyles.Right;

            Button cancelButton = new Button();
            cancelButton.Text = "Cancel";
            cancelButton.AutoSize = true;
            cancelButton.Padding = new Padding(10, 2, 10, 2);
            cancelButton.Click += delegate { Close(); };
            buttonRow.Controls.Add(cancelButton);

            Button okButton = new Button();
            okButton.Text = "OK";
            okButton.AutoSize = true;
            okButton.Padding = new Padding(14, 2, 14, 2);
            okButton.Click += delegate
            {
                _accepted = true;
                _live.Save();
                AppSettings.SetRunAtStartup(_startupBox.Checked);
                Close();
            };
            buttonRow.Controls.Add(okButton);

            grid.Controls.Add(buttonRow, 0, 4);
            grid.SetColumnSpan(buttonRow, 3);

            Controls.Add(grid);

            AcceptButton = okButton;
            CancelButton = cancelButton;
        }

        static Label MakeLabel(string text)
        {
            Label label = new Label();
            label.Text = text;
            label.AutoSize = true;
            label.Anchor = AnchorStyles.Left;
            label.Margin = new Padding(3, 8, 3, 6);
            return label;
        }

        void NotifyChanged()
        {
            if (_onChanged != null) _onChanged();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (!_accepted)
            {
                _live.CopyFrom(_original);
                NotifyChanged();
            }
            base.OnFormClosed(e);
        }
    }
}
