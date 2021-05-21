using NAudio.Midi;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lutebot_4
{
    /// <summary>
    /// The purpose of this class is to handle the intermediate conversion steps
    /// For example, it holds the loaded midi, and can be modified based on what the user selects
    /// 
    /// </summary>
    public class MidiConverter
    {
        public MidiFile LoadedMidi { get; private set; }
        public Dictionary<int, MidiChannel> MidiChannelMap { get; private set; }
        public Dictionary<int, MidiTrack> MidiTrackMap { get; private set; } 
        public List<MidiChannel> MidiChannels { get; private set; }
        public List<MidiTrack> MidiTracks { get; private set; }
        private Instrument _selectedInstrument;
        public Instrument SelectedInstrument { get { return _selectedInstrument; } set { _selectedInstrument = value; UpdateSelectedInstrument(); } }

        private Random random = new Random();

        public MidiConverter(Instrument defaultInstrument)
        {
            SelectedInstrument = defaultInstrument;
        }

        private void UpdateSelectedInstrument()
        {
            // This is called when SelectedInstrument changes, we might have to modify or update things.
            // Unsure if this is necessary yet, idk why I added it.  I kinda forgot they can pass the fully qualified Instrument
        }

        public void Load(string path)
        {
            LoadedMidi = new MidiFile(path, false);
            // Parse the midi for info; we want to start by getting a list of all the channels
            // We also need to know the highest and lowest note for each Instrument/Channel (screw tracks)
            // And of course, that gives us overall highest/lowest
            MidiChannelMap = new Dictionary<int, MidiChannel>();
            MidiTrackMap = new Dictionary<int, MidiTrack>();
            MidiChannels = new List<MidiChannel>();
            MidiTracks = new List<MidiTrack>();

            for (int i = 0; i < LoadedMidi.Tracks; i++)
            {
                var newTrack = new MidiTrack("Untitled", i);
                MidiTrackMap[i] = newTrack;
                MidiTracks.Add(newTrack);
                var e = LoadedMidi.Events[i];
                foreach (var ie in e)
                {
                    if (ie is TextEvent)
                    {
                        var me = ie as TextEvent;
                        if (me.MetaEventType == MetaEventType.SequenceTrackName)
                        {
                            MidiTrackMap[i].TrackName = me.Text;
                        }
                    }
                    else if (ie is PatchChangeEvent)
                    {
                        // These are the correct events, they happen at the start to set the instrument for a channel
                        // Note that specifically channel 10 is usually interpreted as drums, and the instrument they set on it is arbitrary
                        var patchChange = ie as PatchChangeEvent;
                        string instrumentName = "Drums";
                        if (ie.Channel != 10)
                            instrumentName = PatchChangeEvent.GetPatchName(patchChange.Patch);
                        var channel = new MidiChannel(instrumentName, ie.Channel);
                        MidiChannelMap[ie.Channel] = channel;
                        MidiChannels.Add(channel);
                    }
                    else if (ie is NoteOnEvent)
                    {
                        var noteEvent = ie as NoteOnEvent;
                        MidiChannel channel;
                        if (!MidiChannelMap.ContainsKey(ie.Channel))
                        {
                            // We got a note for a channel that never had an instrument set
                            // Initialize it as piano
                            channel = new MidiChannel("Piano (Default)", ie.Channel);
                            MidiChannelMap[ie.Channel] = channel;
                            MidiChannels.Add(channel);
                        }
                        else
                            channel = MidiChannelMap[ie.Channel];

                        // If the OffEvent is null, that means this is itself an off event... otherwise it links to the one that ends it
                        if (noteEvent.OffEvent != null && noteEvent.Velocity > 0)
                        {
                            if (channel.HighestNote == null || noteEvent.NoteNumber > channel.HighestNote.Number)
                                channel.HighestNote = new MidiNote(noteEvent.NoteNumber);
                            if (channel.LowestNote == null || noteEvent.NoteNumber < channel.LowestNote.Number)
                                channel.LowestNote = new MidiNote(noteEvent.NoteNumber);
                        }
                    }
                }
            }
        }


        


        public void DrawTrackAlignmentImage(int width, int height, Graphics graphics)
        {
            // So, our goal here
            // 1. Draw a grid with clear separations between octaves.  Label the octaves
            // 2. Draw the range of our selected instrument on it
            // 3. Draw the range of each MIDIInstrument onto it
            // 4. Detect click/drag to move them

            // This is going to be called from the panel's Paint event
            // Scrolling will happen outside of this, don't worry about it for now

            // Just like last time, we're going to draw the octaves horizontally - which is a bit awkward cuz that's normally time, but, we don't need time.

            // Alright, well, IDK.  Draw the selected instrument at the top
            float noteWidth = 2;
            float channelHeight = 25;
            float yPadding = 3;

            var selectedInstrumentRect = new RectangleF(width / 2f - noteWidth * (SelectedInstrument.NoteCount / 2f), 0, noteWidth * SelectedInstrument.NoteCount, channelHeight);
            DrawSelectedInstrumentAlignmentGraphic(selectedInstrumentRect, graphics);

            float y = selectedInstrumentRect.Y + selectedInstrumentRect.Height + yPadding;
            // Draw each Channel on this map
            if (MidiChannels != null)
                foreach (var channel in MidiChannels)
                {
                    if (channel.HighestNote != null && channel.LowestNote != null)
                    {
                        var channelRect = new RectangleF(width / 2f - noteWidth * ((channel.HighestNote.Number - channel.LowestNote.Number) / 2f) + (channel.LowestNote.Number - SelectedInstrument.LowestNote)*noteWidth, y, noteWidth * (channel.HighestNote.Number - channel.LowestNote.Number), channelHeight);
                        DrawAlignmentChannel(channel, channelRect, graphics);
                        y += channelHeight + yPadding;
                    }
                }

            // This kind of works.  But it exposes a lot of obvious problems.
            // 1. Horizontal scrolling, can't really happen, and we don't have enough space.  The labels are already bigger than the boxes
            // , and would already be going offscreen if they were in proper positions
            // 2. It's ugly.  Both of these can be kinda solved by adding a small box for each label, like on LB3
            // But really, I don't want them to draw rectangles for these... 
        }

        private void DrawAlignmentChannel(MidiChannel channel, RectangleF rect, Graphics graphics)
        {
            // Unlike previous LuteBot, I don't want the rectangles drawn for these
            // I want the text near the bottom, and above it, just a horizontal line spanning the notes (and labels on either end of that line)
            var brush = new SolidBrush(channel.DrawColor);
            //graphics.FillRectangle(brush, rect.X, rect.Y, rect.Width, rect.Height);
            graphics.DrawLine(new Pen(brush), rect.X, rect.Y, rect.X + rect.Width, rect.Y);

            var stringSize = graphics.MeasureString(channel.InstrumentName, SystemFonts.DefaultFont);
            graphics.DrawString(channel.InstrumentName, SystemFonts.DefaultFont, brush, rect.X + rect.Width/2 - stringSize.Width/2, rect.Y + rect.Height - stringSize.Height);

            // Draw its highest and lowest notes
            var smallerFont = new Font(FontFamily.GenericSansSerif, 7);
            stringSize = graphics.MeasureString(channel.LowestNote.Name, smallerFont);
            
            graphics.DrawString(channel.LowestNote.Name, smallerFont, brush, rect.X - stringSize.Width - 3, rect.Y);
            graphics.DrawString(channel.HighestNote.Name, smallerFont, brush, rect.X + rect.Width + 3, rect.Y);
        }
        private void DrawSelectedInstrumentAlignmentGraphic(RectangleF selectedInstrumentRect, Graphics graphics)
        {
            graphics.FillRectangle(Brushes.DarkGreen, selectedInstrumentRect.X, selectedInstrumentRect.Y, selectedInstrumentRect.Width, selectedInstrumentRect.Height);

            var stringSize = graphics.MeasureString(SelectedInstrument.Name, SystemFonts.DefaultFont);

            graphics.DrawString(SelectedInstrument.Name, SystemFonts.DefaultFont, Brushes.White, selectedInstrumentRect.X + selectedInstrumentRect.Width / 2 - stringSize.Width / 2,
                selectedInstrumentRect.Y + selectedInstrumentRect.Height / 2 - stringSize.Height / 2);

            // Draw its highest and lowest notes
            var smallerFont = new Font(FontFamily.GenericSansSerif, 7);
            string lowestName = MidiNote.GetName(SelectedInstrument.LowestNote);
            stringSize = graphics.MeasureString(lowestName, smallerFont);

            graphics.DrawString(lowestName, smallerFont, Brushes.DarkGreen, selectedInstrumentRect.X - stringSize.Width - 3, selectedInstrumentRect.Y);
            graphics.DrawString(MidiNote.GetName(SelectedInstrument.LowestNote + SelectedInstrument.NoteCount), smallerFont, Brushes.DarkGreen, selectedInstrumentRect.X + selectedInstrumentRect.Width + 3, selectedInstrumentRect.Y);
        }
    }

    public class MidiTrack
    {
        public bool Active { get; set; } = true;
        public string TrackName { get; set; }
        public int Track { get; set; }

        public MidiTrack(string trackname, int track)
        {
            TrackName = trackname;
            Track = track;
        }
    }

    public class MidiNote
    {
        public string Name { get; private set; }
        public int Number { get; private set; }
        public MidiNote(int number)
        {
            Number = number;
            Name = GetName(number);
        }

        private static readonly string[] NoteNames = new string[] { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

        public static string GetName(int noteNumber)
        {
            int octave = noteNumber / 12;
            return String.Format("{0}{1}", NoteNames[noteNumber % 12], octave);
        }
    }

    public class MidiChannel
    {
        private static Random random = new Random();
        public Color DrawColor { get; set; } = Color.FromArgb(random.Next(150), random.Next(150), random.Next(150));
        public bool Active { get; set; } = true;
        public string InstrumentName { get; private set; }
        public int Channel { get; private set; }
        public MidiNote LowestNote { get; set; }
        public MidiNote HighestNote { get; set; }
        /// <summary>
        /// Default 0, the number of octaves to offset this channel (negative values are fine).  Set this when the users adjusts track alignment
        /// </summary>
        public int NoteOffset { get; set; }

        public MidiChannel(string instrument, int channel)
        {
            InstrumentName = instrument;
            Channel = channel;
        }
    }
}
