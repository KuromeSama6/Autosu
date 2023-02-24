using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using System.Xml.Serialization;

namespace Autosu.Utils {
    public static class SerializationUtil {
        public static bool doNotSave = false;

        public static bool Save(string path, object saveData) {
            if (doNotSave) return true;

            BinaryFormatter bf = GetBinaryFormatter();

            string dir = string.Join("\\", path.Split("\\")[0..(path.Split("\\").Length - 1)]);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            string fileName = $"{path}";

            FileStream file = File.Create(fileName);
            bf.Serialize(file, saveData);
            file.Close();

            return true;
        }

        public static T Load<T>(string path) {
            if (doNotSave) return default(T);

            BinaryFormatter bf = GetBinaryFormatter();
            if (!File.Exists(path)) return default(T);

            FileStream file = File.Open(path, FileMode.Open);
            try {
                T ret = (T) bf.Deserialize(file);
                file.Close();

                return ret;

            } catch {
                file.Close();
                return default(T);

            }

        }

        public static T LoadOrNew<T>(string relPath) where T : new() {
            T ret = Load<T>(relPath);
            if (ret == null) ret = new T();

            return ret;
        }

        public static byte[] Serialize(object obj) {
            BinaryFormatter bf = GetBinaryFormatter();
            var ms = new MemoryStream();
            bf.Serialize(ms, obj);
            var ret = ms.ToArray();
            ms.Close();
            return ret;
        }

        public static T Deserialize<T>(byte[] bytes) {
            var ms = new MemoryStream();
            var bf = GetBinaryFormatter();
            ms.Write(bytes, 0, bytes.Length);
            ms.Seek(0, SeekOrigin.Begin);
            var ret = (T) bf.Deserialize(ms);
            ms.Close();
            return ret;
        }

        public static BinaryFormatter GetBinaryFormatter() {
            return new BinaryFormatter();
        }

    }
    

}