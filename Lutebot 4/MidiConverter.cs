using NAudio.Midi;

using System;
using System.Collections.Generic;
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
        public Dictionary<int, string> InstrumentNamesByChannel { get; private set; }
        public Dictionary<int, string> TrackNames { get; private set; }


        public void Load(string path)
        {
            LoadedMidi = new MidiFile(path, false);
            // Parse the midi for info; we want to start by getting a list of all the channels

            InstrumentNamesByChannel = new Dictionary<int, string>();
            TrackNames = new Dictionary<int, string>();
            for (int i = 0; i < LoadedMidi.Tracks; i++)
            {
                TrackNames[i] = "Untitled";
                var e = LoadedMidi.Events[i];
                foreach (var ie in e)
                {
                    if(ie is TextEvent)
                    {
                        var me = ie as TextEvent;
                        if (me.MetaEventType == MetaEventType.SequenceTrackName)
                        {
                            TrackNames[i] = me.Text;
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

                        InstrumentNamesByChannel[ie.Channel] = instrumentName;
                    }
                }
            }
        }
    }

    public class MidiTrack
    {
        public string TrackName { get; set; }
        public int Track { get; set; }

        public MidiTrack(string trackname, int track)
        {
            TrackName = trackname;
            Track = track;
        }
    }

    public class MidiChannel
    {
        public string InstrumentName { get; set; }
        public int Channel { get; set; }

        public MidiChannel(string instrument, int channel)
        {
            InstrumentName = instrument;
            Channel = channel;
        }
    }
}
