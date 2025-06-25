using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Net.NetworkInformation;

public static class Utils
{
    //在这里设定立绘,场景,音乐等文件的路径
    public static class ResoucePaths
    {
        public static string PortraitPath = Path.Combine(Application.streamingAssetsPath, "HGF", "Texture2D", "Portrait");
        public static string CharacterIniPath = Path.Combine(Application.streamingAssetsPath, "HGF", "CharacterInfo.ini");
        public static string SavePath = Path.Combine(Application.persistentDataPath, "savegame.sav");
        public static string BackgroundPath = Path.Combine(Application.streamingAssetsPath, "HGF", "Texture2D", "BackgroundImage");
        public static string VoicePath = Path.Combine(Application.streamingAssetsPath, "HGF", "Audio");
    }
    public static string GetWritePath()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        return Application.streamingAssetsPath;
#elif UNITY_IOS || UNITY_ANDROID
            return  Application.persistentDataPath;
#endif
    }
    public static Sprite LoadTextureByIO(string Path)
    {
        FileStream fs = new FileStream(Path, FileMode.Open, FileAccess.Read);
        fs.Seek(0, SeekOrigin.Begin);//游标的操作，可有可无
        byte[] bytes = new byte[fs.Length];//生命字节，用来存储读取到的图片字节
        try
        {
            fs.Read(bytes, 0, bytes.Length);//开始读取，这里最好用trycatch语句，防止读取失败报错

        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
        fs.Close();//切记关闭

        int width = 2048;//图片的宽（这里两个参数可以提到方法参数中）
        int height = 2048;//图片的高（这里说个题外话，pico相关的开发，这里不能大于4k×4k不然会显示异常，当时开发pico的时候应为这个问题找了大半天原因，因为美术给的图是6000*3600，导致出现切几张图后就黑屏了。。。
        Texture2D texture = new Texture2D(width, height);
        if (texture.LoadImage(bytes))
        {
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));//将生成的texture2d返回，到这里就得到了外部的图片，可以使用了
        }
        else
        {
            return null;
        }
    }
}
public class GameConfig
{
    private Hashtable keyPairs = new Hashtable();
    private string iniFilePath;
    private struct SectionPair
    {
        public string Section;
        public string Key;
    }
    /// <summary>
    /// 在给定的路径上打开INI文件并枚举IniParser中的值。
    /// </summary>
    /// <param name="iniPath">Full path to INI file.</param>
    public GameConfig(string iniPath)
    {
        TextReader iniFile = null;
        string strLine = null;
        string currentRoot = null;
        string[] keyPair = null;
        iniFilePath = iniPath;
        if (File.Exists(iniPath))
        {
            try
            {
                iniFile = new StreamReader(iniPath);
                strLine = iniFile.ReadLine();
                while (strLine != null)
                {
                    strLine = strLine.Trim();
                    if (strLine != "")
                    {
                        if (strLine.StartsWith("[") && strLine.EndsWith("]"))
                        {
                            currentRoot = strLine.Substring(1, strLine.Length - 2);
                        }
                        else
                        {
                            keyPair = strLine.Split(new char[] { '=' }, 2);
                            SectionPair sectionPair;
                            String value = null;
                            if (currentRoot == null)
                                currentRoot = "ROOT";
                            sectionPair.Section = currentRoot;
                            sectionPair.Key = keyPair[0];
                            if (keyPair.Length > 1)
                                value = keyPair[1];
                            keyPairs.Add(sectionPair, value);
                        }
                    }
                    strLine = iniFile.ReadLine();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (iniFile != null)
                    iniFile.Close();
            }
        }
        else
        {
            Save();
        }
    }

    /// <summary>
    /// 返回给定section的值，key对。
    /// </summary>
    /// <param name="sectionName">Section name</param>
    /// <param name="settingName">Key name</param>
    public string GetValue(string sectionName, string settingName)
    {
        SectionPair sectionPair;
        sectionPair.Section = sectionName;
        sectionPair.Key = settingName;
        return (string)keyPairs[sectionPair];
    }
    /// <summary>
    /// 列出给定的Section的所有行
    /// </summary>
    /// <param name="sectionName">Section to enum.</param>
    public string[] EnumSection(string sectionName)
    {
        ArrayList tmpArray = new ArrayList();
        foreach (SectionPair pair in keyPairs.Keys)
        {
            if (pair.Section == sectionName)
                tmpArray.Add(pair.Key);
        }
        return (string[])tmpArray.ToArray(typeof(string));
    }
    /// <summary>
    /// 向要保存的节添加或替换Value。
    /// </summary>
    /// <param name="sectionName">Section to add under.</param>
    /// <param name="settingName">Key name to add.</param>
    /// <param name="settingValue">Value of key.</param>
    public void SetValue(string sectionName, string settingName, string settingValue)
    {
        SectionPair sectionPair;
        sectionPair.Section = sectionName;
        sectionPair.Key = settingName;
        if (keyPairs.ContainsKey(sectionPair))
            keyPairs.Remove(sectionPair);
        keyPairs.Add(sectionPair, settingValue);
        Save();

    }
    /// <summary>
    /// 删除设置
    /// </summary>
    /// <param name="sectionName">指定Section</param>
    /// <param name="settingName">添加的Key</param>
    public void Delete(string sectionName, string settingName)
    {
        SectionPair sectionPair;
        sectionPair.Section = sectionName;
        sectionPair.Key = settingName;
        if (keyPairs.ContainsKey(sectionPair))
            keyPairs.Remove(sectionPair);
        Save();
    }
    /// <summary>
    /// 保存到新文件。
    /// </summary>
    /// <param name="newFilePath">新的文件路径。</param>
    public void SaveSettings(string newFilePath)
    {
        ArrayList sections = new ArrayList();
        string tmpValue = "";
        string strToSave = "";
        foreach (SectionPair sectionPair in keyPairs.Keys)
        {
            if (!sections.Contains(sectionPair.Section))
                sections.Add(sectionPair.Section);
        }
        foreach (string section in sections)
        {
            strToSave += ("[" + section + "]\r\n");
            foreach (SectionPair sectionPair in keyPairs.Keys)
            {
                if (sectionPair.Section == section)
                {
                    tmpValue = (string)keyPairs[sectionPair];
                    if (tmpValue != null)
                        tmpValue = "=" + tmpValue;
                    strToSave += (sectionPair.Key + tmpValue + "\r\n");
                }
            }
            strToSave += "\r\n";
        }
        try
        {
            TextWriter tw = new StreamWriter(newFilePath);
            tw.Write(strToSave);
            tw.Close();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    /// <summary>
    /// 将设置保存回ini文件。
    /// </summary>
    public void Save()
    {
        SaveSettings(iniFilePath);
    }

    public static string GetValue(string Path, string SectionName, string settingName)
    {
        var _ = new GameConfig(Path);
        return _.GetValue(SectionName, settingName);
    }
}

