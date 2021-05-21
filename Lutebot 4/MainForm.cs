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
        private MidiConverter converter = new MidiConverter();

        public MainForm()
        {
            InitializeComponent();
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
            foreach(var kvp in converter.TrackNames)
            {
                channelListBox.Items.Add(kvp);
                channelListBox.SetItemChecked(i++, true);
            }
            channelListBox.DisplayMember = "Value";

            i = 0;
            instrumentListBox.Items.Clear();
            foreach (var kvp in converter.InstrumentNamesByChannel)
            {
                instrumentListBox.Items.Add(kvp);
                instrumentListBox.SetItemChecked(i++, true);
            }
            instrumentListBox.DisplayMember = "Value";
        }
    }
}
