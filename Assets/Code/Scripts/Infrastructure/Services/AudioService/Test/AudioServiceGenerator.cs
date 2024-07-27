using System;
using System.IO;
using com.cyborgAssets.inspectorButtonPro;
using Core.Infrastructure.Service;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioServiceGenerator")]
public class AudioServiceGenerator : ScriptableObject
{
    [SerializeField] private string _namespace = "Core.Infrastructure.Service";
    [Header("Audio Types")]
    [SerializeField] private string[] _audioFileTypes;
    [SerializeField] private string _audioFileTypeEnumPath;
    [Header("Key Settings")]
    [SerializeField] private AudioFileCell[] _audioFileCells;
    [SerializeField] private string _audioClipKeyEnumPath;

    private const string AUDIO_FILE_TYPE_ENUM_NAME = "AudioFileTypeTest";
    private const string AUDIO_CLIP_KEY_ENUM_NAME = "AudioClipKey";

    [Serializable]
    private struct AudioFileCell
    {
        public AudioFileTypeTest AudioFileType;
        public AudioFile File;
    }

    [ProButton]
    private void GenerateAudioFileTypeEnum()
    {
        string fullPath = Application.dataPath + '/' + _audioFileTypeEnumPath;
        using (StreamWriter streamWriter = File.CreateText(fullPath))
        {
            streamWriter.Write(
                "//This code is generated!\n\n" +
                $"namespace {_namespace}\n" +
                "{\n" +
                $"\tpublic enum {AUDIO_FILE_TYPE_ENUM_NAME}\n" +
                "\t{\n"
            );

            for (int i = 0; i < _audioFileTypes.Length - 1; i++)
                streamWriter.Write($"\t\t{_audioFileTypes[i]} = {i},\n");

            string lastType = _audioFileTypes[_audioFileTypes.Length - 1];
            streamWriter.Write(
                $"\t\t{lastType} = {_audioFileTypes.Length - 1}\n" +
                $"\t}}\n" +
                $"}}\n"
            );
        }
    }
    [ProButton]
    private void GenerateAudioClipKeyEnum()
    {
        string fullPath = Application.dataPath + '/' + _audioClipKeyEnumPath;
        using (StreamWriter streamWriter = File.CreateText(fullPath))
        {
            streamWriter.Write(
                "//This code is generated!\n\n" +
                $"namespace {_namespace}\n" +
                "{\n" +
                $"\tpublic enum {AUDIO_CLIP_KEY_ENUM_NAME}\n" +
                "\t{\n"
            );

            int numberOfEnum = 0;
            for (int i = 0; i < _audioFileCells.Length - 1; i++)
            {
                for (int j = 0; j < _audioFileCells[i].File.AudioClips.Length; j++)
                {
                    AudioFile.ClipKey clipKey = _audioFileCells[i].File.AudioClips[j];
                    streamWriter.Write($"\t\t{_audioFileCells[i].AudioFileType}_{clipKey.Key} = {numberOfEnum},\n");
                    numberOfEnum++;
                }

            }
            AudioFileCell lastAudioFileCell = _audioFileCells[_audioFileCells.Length - 1];
            for (int j = 0; j < lastAudioFileCell.File.AudioClips.Length - 1; j++)
            {
                AudioFile.ClipKey clipKey = lastAudioFileCell.File.AudioClips[j];
                streamWriter.Write($"\t\t{lastAudioFileCell.AudioFileType}_{clipKey.Key} = {numberOfEnum},\n");
                numberOfEnum++;
            }

            AudioFile.ClipKey lastClipKey = lastAudioFileCell.File.AudioClips[lastAudioFileCell.File.AudioClips.Length - 1];
            streamWriter.Write(
                $"\t\t{lastAudioFileCell.AudioFileType}_{lastClipKey.Key} = {numberOfEnum}\n" +
                $"\t}}\n" +
                $"}}\n"
            );
            streamWriter.Close();
        }
    }
}
