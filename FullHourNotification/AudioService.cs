using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace FullHourNotification
{
    /*
     * This Audio code was created by:
     * http://bresleveloper.blogspot.ca/2012/06/c-service-play-sound-with-naudio.html
     * http://naudio.codeplex.com/
     * http://stackoverflow.com/questions/2143439/play-wave-file-from-a-windows-service-c
     *
     */

        public interface IInputFileFormatPlugin
        {
            string Name { get; }
            string Extension { get; }
            WaveStream CreateWaveStream(string fileName);
        }

        [Export(typeof(IInputFileFormatPlugin))]
        class WaveInputFilePlugin : IInputFileFormatPlugin
        {
            public string Name
            { get { return "WAV file"; } }
            public string Extension
            { get { return ".wav"; } }

            public WaveStream CreateWaveStream(string fileName)
            {
                WaveStream readerStream = new WaveFileReader(fileName);
                if (readerStream.WaveFormat.Encoding != WaveFormatEncoding.Pcm
                      && readerStream.WaveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
                {
                    readerStream = WaveFormatConversionStream.CreatePcmStream(readerStream);
                    readerStream = new BlockAlignReductionStream(readerStream);
                }
                return readerStream;
            }
        }

        class AudioService
        {
            public static void Concatenate(string outputFile, IEnumerable<string> sourceFiles)
            {
                byte[] buffer = new byte[1024];
                WaveFileWriter waveFileWriter = null;
                try
                {
                    foreach (string sourceFile in sourceFiles)
                    {
                        using (WaveFileReader reader = new WaveFileReader(sourceFile))
                        {
                            if (waveFileWriter == null)
                                waveFileWriter = new WaveFileWriter(outputFile, reader.WaveFormat);
                            else
                            {
                                if (!reader.WaveFormat.Equals(waveFileWriter.WaveFormat))
                                    throw new InvalidOperationException(
                                             "Can't concatenate WAV Files that don't share the same format");
                            }
                            int read;
                            while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                waveFileWriter.WriteData(buffer, 0, read);
                            }
                        }
                    }
                }
                finally
                {
                    if (waveFileWriter != null)
                        waveFileWriter.Dispose();
                }
            }

            private IWavePlayer waveOut = new WasapiOut(NAudio.CoreAudioApi.AudioClientShareMode.Shared, 300);
            WaveStream fileWaveStream;
            Action<float> setVolumeDelegate;

            [ImportMany(typeof(IInputFileFormatPlugin))]
            public IEnumerable<IInputFileFormatPlugin> InputFileFormats { get; set; }

            void OnPreVolumeMeter(object sender, NAudio.Wave.SampleProviders.StreamVolumeEventArgs e)
            {
                // we know it is stereo
                //w aveformPainter1.AddMax(e.MaxSampleValues[0]);
                //waveformPainter2.AddMax(e.MaxSampleValues[1]);
            }

            public ISampleProvider CreateInputStream(string fileName)
            {
                var plugin = new WaveInputFilePlugin();
                if (plugin == null)
                    throw new InvalidOperationException("Unsupported file extension");
                fileWaveStream = plugin.CreateWaveStream(fileName);
                var waveChannel = new NAudio.Wave.SampleProviders.SampleChannel(fileWaveStream);
                setVolumeDelegate = (vol) => waveChannel.Volume = vol;
                waveChannel.PreVolumeMeter += OnPreVolumeMeter;
                var postVolumeMeter = new MeteringSampleProvider(waveChannel);
                postVolumeMeter.StreamVolume += OnPostVolumeMeter;
                return postVolumeMeter;
            }

            void OnPostVolumeMeter(object sender, StreamVolumeEventArgs e)
            {
                // we know it is stereo
                //volumeMeter1.Amplitude = e.MaxSampleValues[0];
                //volumeMeter2.Amplitude = e.MaxSampleValues[1];
            }

            public void WASAPI(string fileName)
            {
                ISampleProvider sampleProvider = null;
                sampleProvider = CreateInputStream(fileName);
                waveOut.Init(new SampleToWaveProvider(sampleProvider));
                waveOut.Play();
                return;
            }
        
    }

    //==============
}
