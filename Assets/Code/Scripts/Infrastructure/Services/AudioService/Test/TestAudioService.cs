using System;
using System.IO;
using com.cyborgAssets.inspectorButtonPro;
using Core.Infrastructure.Service;
using UnityEngine;
using Zenject;

public class TestAudioService : MonoBehaviour
{
    public string[] _testString;
    public string _path;

    [ProButton]
    private void GetPath()
    {
        string fullPath = Application.dataPath + '/' + _path;
        Debug.Log(fullPath);
        AudioTypeTest test = Enum.Parse<AudioTypeTest>("A");
        Debug.Log(test.ToString());
    }
    [ProButton]
    private void Generate()
    {
        string fullPath = Application.dataPath + '/' + _path;
        using (StreamWriter streamWriter = File.CreateText(fullPath))
        {
            streamWriter.Write(
                "//This code is generated!\n\n" +
                "namespace Core.Infrastructure.Service\n" +
                "{\n" +
                "\tpublic enum AudioTypeTest\n" +
                "\t{\n"
            );

            for(int i = 0; i < _testString.Length - 1; i++)
                streamWriter.Write($"\t\t{_testString[i]} = {i},\n");

            streamWriter.Write(
                $"\t\t{_testString[_testString.Length - 1]} = {_testString.Length - 1}\n" +
                $"\t}}\n" +
                $"}}\n"
            );
        }
    }

}
