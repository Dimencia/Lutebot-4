using NAudio.Midi;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lutebot_4
{
    public partial class MainForm : Form
    {
        private MidiConverter converter = new MidiConverter(Instrument.Prefabs.First());

        public MainForm()
        {
            InitializeComponent();
            // Fill the instrumentComboBox
            instrumentComboBox.DataSource = Instrument.Prefabs;
            instrumentComboBox.DisplayMember = "Name";
            instrumentComboBox.SelectedIndex = 0;

            trackAlignmentPanel.Paint += TrackAlignmentPanel_Paint;
        }

        private void TrackAlignmentPanel_Paint(object sender, PaintEventArgs e)
        {
            converter.DrawTrackAlignmentImage(trackAlignmentPanel.Width, trackAlignmentPanel.Height, e.Graphics);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                MultiPartPanel.Visible = true;
                this.Height += MultiPartPanel.Height;
            }
            else
            {
                MultiPartPanel.Visible = false;
                this.Height -= MultiPartPanel.Height;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var result = openMidiDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                LoadMidi(openMidiDialog.FileName);
            }
        }

        private void LoadMidi(string path)
        {
            converter.Load(path);
            // TODO: Parse out our custom info from the end of it
            labelMidiName.Text = Path.GetFileNameWithoutExtension(path).Replace("_"," ");
            // TODO: Resize this text to auto-fit

            // Setup the track and channel lists
            channelListBox.Items.Clear();
            int i = 0;
            foreach(var track in converter.MidiTracks)
            {
                channelListBox.Items.Add(track);
                channelListBox.SetItemChecked(i++, track.Active);
            }
            channelListBox.DisplayMember = "TrackName";

            i = 0;
            instrumentListBox.Items.Clear();
            foreach (var channel in converter.MidiChannels)
            {
                instrumentListBox.Items.Add(channel);
                instrumentListBox.SetItemChecked(i++, channel.Active);
            }
            instrumentListBox.DisplayMember = "InstrumentName";
        }
    }
}
