using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lutebot_4
{
    public class Instrument
    {
        public string Name { get; set; }
        public int LowestNote { get; set; }
        public int NoteCount { get; set; }

        public static List<Instrument> Prefabs { get; private set; } = new List<Instrument>();
        private static string baseDirectory = $@"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}Config";
        private static string path = $"{baseDirectory}{Path.DirectorySeparatorChar}Instruments.json";

        static Instrument()
        {
            if (!File.Exists(path))
            {
                Prefabs.Add(new Instrument() { LowestNote = 0, NoteCount = 25, Name = "Mordhau Lute" });
                Prefabs.Add(new Instrument() { LowestNote = 0, NoteCount = 49, Name = "Mordhau Modded Lute" });
                Prefabs.Add(new Instrument() { LowestNote = 45, NoteCount = 32, Name = "Rust Trumpet" });
                Prefabs.Add(new Instrument() { LowestNote = 21, NoteCount = 88, Name = "Rust Piano" });
                Prefabs.Add(new Instrument() { LowestNote = 40, NoteCount = 30, Name = "Rust Can Guitar" });
                Prefabs.Add(new Instrument() { LowestNote = 40, NoteCount = 27, Name = "Rust Acoustic Guitar" });
                Prefabs.Add(new Instrument() { LowestNote = 72, NoteCount = 25, Name = "Rust Xylobones" });
                Prefabs.Add(new Instrument() { LowestNote = 28, NoteCount = 21, Name = "Rust Bass" });
                Prefabs.Add(new Instrument() { LowestNote = 36, NoteCount = 11, Name = "Rust Sousaphone" });
                Prefabs.Add(new Instrument() { LowestNote = 72, NoteCount = 12, Name = "Rust Flute" });
                Prefabs.Add(new Instrument() { LowestNote = 48, NoteCount = 14, Name = "Rust Drums (Bad)" });
                Write();
            }
            else
                Read();
        }

        public static void Read()
        {
            JsonSerializer serializer = new JsonSerializer();
            using (StreamReader stream = new StreamReader(path))
            {
                Prefabs = (List<Instrument>)serializer.Deserialize(stream, typeof(List<Instrument>));
            }
        }

        public static void Write()
        {
            if (!Directory.Exists(baseDirectory))
                Directory.CreateDirectory(baseDirectory);
            JsonSerializer serializer = new JsonSerializer();
            using (StreamWriter stream = new StreamWriter(path, false))
            {
                serializer.Serialize(stream, Prefabs);
            }
        }
    }
}
