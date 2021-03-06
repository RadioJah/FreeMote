﻿namespace FreeMote.Psb
{
    /// <summary>
    /// Audio Arch Data
    /// </summary>
    public interface IArchData
    {
        uint Index { get; }
        string Extension { get; }
        string WaveExtension { get; set; }
        PsbAudioFormat Format { get; }
        PsbResource Data { get; }
        PsbDictionary PsbArchData { get; set; }

        /// <summary>
        /// Generate PSB object "archData" value
        /// </summary>
        /// <returns></returns>
        IPsbValue ToPsbArchData();
    }
}
