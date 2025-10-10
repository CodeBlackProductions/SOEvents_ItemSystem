using ItemSystem.SubModules;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ItemSystem.Editor
{
    public static class ItemEditor_DynamicEnumSystem
    {
        public static void UpdateDynamicEnum()
        {
            IEnumerable<ScriptableObject> stats = ItemEditor_AssetLoader.LoadAssetsByTypeReference(typeof(SO_Stat));
            string generatedEnum = "public enum EUserStats {}";
            List<string> enumEntries = new List<string>();

            foreach (SO_Stat stat in stats)
            {
                if (enumEntries.Contains(stat.TargetUserStat))
                {
                    continue;
                }
                enumEntries.Add(stat.TargetUserStat);
                generatedEnum = generatedEnum.Insert(generatedEnum.Length - 1, $"{stat.TargetUserStat},");
            }

            generatedEnum = generatedEnum.Replace(",}", "}");

            StreamWriter writer = new StreamWriter("Assets/ItemSystem/Sub Modules/StatModule/EUserStats.cs", false);
            writer.Write(generatedEnum);
            writer.Close();
        }
    }
}