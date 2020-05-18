﻿using System.Collections.Generic;
using System.IO;
using FreeMote.Plugins;

namespace FreeMote.Psb.Types
{
    class SoundArchiveType : IPsbType
    {
        public const string VoiceResourceKey = "voice";
        public PsbType PsbType => PsbType.SoundArchive;
        public bool IsThisType(PSB psb)
        {
            return psb.TypeId == "sound_archive";
        }

        public List<T> CollectResources<T>(PSB psb, bool deDuplication = true) where T : IResourceMetadata
        {
            List<T> resourceList = psb.Resources == null
                ? new List<T>()
                : new List<T>(psb.Resources.Count);

            if (psb.Objects[VoiceResourceKey] is PsbDictionary voiceDic)
            {
                var context = FreeMount.CreateContext();
                foreach (var voice in voiceDic)
                {
                    if (voice.Value is PsbDictionary voiceValue)
                    {
                        resourceList.Add((T)(IResourceMetadata)GenerateAudioMetadata(psb, voice.Key, voiceValue, context));
                    }
                }
            }
            else
            {
                return resourceList;
            }

            //Set Spec
            resourceList.ForEach(r => r.Spec = psb.Platform);
            //resourceList.Sort((md1, md2) => (int)(md1.Index - md2.Index));

            return resourceList;
        }


        internal AudioMetadata GenerateAudioMetadata(PSB psb, string name, PsbDictionary voice, FreeMountContext context)
        {
            var md = new AudioMetadata();
            md.Name = name;

            if (voice["file"] is PsbString fileStr)
            {
                md.FileString = fileStr;
            }

            if (voice["loopStr"] is PsbString loopStr)
            {
                md.LoopStr = loopStr;
            }

            if (voice["loop"] is PsbNumber loopNum)
            {
                md.Loop = loopNum.IntValue;
            }

            if (voice["quality"] is PsbNumber qualityNum)
            {
                md.Quality = qualityNum.IntValue;
            }

            if (voice["type"] is PsbNumber typeNum)
            {
                md.Type = typeNum.IntValue;
            }

            if (voice["device"] is PsbNumber deviceNum)
            {
                md.Device = deviceNum.IntValue;
            }

            if (voice["channelList"] is PsbList channelList)
            {
                foreach (var channel in channelList)
                {
                    if (channel is PsbDictionary channelDic)
                    {
                        if (context.TryGetArchData(psb, channelDic, out var archData))
                        {
                            md.ChannelList.Add(archData);
                        }
                    }
                }
            }

            return md;
        }

        public void Link(PSB psb, FreeMountContext context, IList<string> resPaths, string baseDir = null,
            PsbLinkOrderBy order = PsbLinkOrderBy.Convention)
        {
            
        }

        public void Link(PSB psb, FreeMountContext context, IDictionary<string, string> resPaths, string baseDir = null)
        {
            
        }

        public void UnlinkToFile(PSB psb, FreeMountContext context, string name, string dirPath, bool outputUnlinkedPsb = true,
            PsbLinkOrderBy order = PsbLinkOrderBy.Name)
        {
        }

        public Dictionary<string, string> OutputResources(PSB psb, FreeMountContext context, string name, string dirPath,
            PsbExtractOption extractOption = PsbExtractOption.Original)
        {
            var resources = psb.CollectResources<AudioMetadata>();
            Dictionary<string, string> resDictionary = new Dictionary<string, string>();

            if (extractOption == PsbExtractOption.Original)
            {
                for (int i = 0; i < psb.Resources.Count; i++)
                {
                    var relativePath = psb.Resources[i].Index == null ? $"#{i}.bin" : $"{psb.Resources[i].Index}.bin";

                    File.WriteAllBytes(
                        Path.Combine(dirPath, relativePath),
                        psb.Resources[i].Data);
                    resDictionary.Add(Path.GetFileNameWithoutExtension(relativePath), $"{name}/{relativePath}");
                }
            }
            else
            {
                foreach (var resource in resources)
                {
                    if (resource.ChannelList.Count == 1)
                    {
                        var relativePath = resource.GetFileName(resource.ChannelList[0].WaveExtension);
                        var bts = resource.ChannelList[0].TryToWave(context);
                        if (bts != null)
                        {
                            File.WriteAllBytes(Path.Combine(dirPath, relativePath), bts);
                            resDictionary.Add(resource.Name, $"{name}/{relativePath}");
                        }
                    }
                    else if (resource.ChannelList.Count > 1)
                    {
                        for (var j = 0; j < resource.ChannelList.Count; j++)
                        {
                            var waveChannel = resource.ChannelList[j];
                            var relativePath = resource.GetFileName($"-{j}{waveChannel.WaveExtension}");
                            var bts = waveChannel.TryToWave(context);
                            if (bts != null)
                            {
                                File.WriteAllBytes(Path.Combine(dirPath, relativePath), bts);
                                resDictionary.Add(resource.Name, $"{name}/{relativePath}");
                            }
                        }
                    }
                }
            }

            return resDictionary;
        }
    }
}
