using System.Collections;
using UnityEngine;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using System.IO;

namespace DaggerfallWorkshop
{
    public sealed class MidiOutWrapper
    {
        private static MidiOutWrapper instance = null;
        private static readonly object padlock = new object();

        OutputDevice midiOut = null;
        Playback midiPlayback;
        MidiFile currentMidiFile;
        int deviceId = 0; //TODO

        public static MidiOutWrapper Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new MidiOutWrapper();
                    }
                    return instance;
                }
            }
        }

        public bool IsPlaying
        {
            get
            {
                return midiPlayback != null ? midiPlayback.IsRunning : false;
            }
        }

        public void Dispose()
        {
            if (midiPlayback != null) midiPlayback.Dispose();
            if (midiOut != null) midiOut.Dispose();
        }

        public void Init()
        {
            midiOut?.Dispose();
            midiOut = OutputDevice.GetByIndex(deviceId);
        }

        public void Stop()
        {
            if (midiPlayback != null && midiPlayback.IsRunning)
            {
                midiPlayback.Stop();
                midiPlayback.Dispose();
                midiOut.TurnAllNotesOff();
                midiOut.Dispose();
            }
        }

        public void Pause()
        {
            if (midiPlayback != null)
            {
                midiPlayback.Stop();
            }
        }

        public void Resume()
        {
            if (midiPlayback != null && !midiPlayback.IsRunning)
            {
                midiPlayback.Play();
            }
        }

        public void Play(Stream midiStream)
        {
            Stop();
            currentMidiFile = MidiFile.Read(midiStream);
            midiOut?.Dispose();
            midiPlayback?.Dispose();
            try
            {
                midiOut = OutputDevice.GetByIndex(deviceId);
                /*
                midiPlayback = currentMidiFile.GetPlayback(midiOut, new PlaybackSettings
                {
                    ClockSettings = new MidiClockSettings
                    {
                        CreateTickGeneratorCallback = () => new RegularPrecisionTickGenerator()
                    }

                });*/
                midiPlayback = currentMidiFile.GetPlayback(midiOut);
                midiPlayback.Start();
            }
            catch (MidiDeviceException e)
            {
                // Try again next time
                midiOut?.Dispose();
                midiPlayback?.Dispose();
            }
        }

    }
}