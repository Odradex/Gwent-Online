using UnityEngine;
using UnityEditor;
using System.IO;
using System;

#if (UNITY_EDITOR)
public class GenerateCards : MonoBehaviour
{
    [MenuItem("Assets/Import Cards")]
    public static void Import()
    {
        string line;
        CardObject asset;
        StreamReader reader = new StreamReader(@"C:\Users\Oleg\Desktop\cards.txt");
        while((line = reader.ReadLine()) != null)
        {
            asset = ScriptableObject.CreateInstance<CardObject>();
            try
            {
                string[] cardData = line.Split('|');
                asset.FullImage = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/Cards/Специальные карты/{cardData[1]}");
                asset.SmallImage = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/Cards/Специальные карты/{cardData[1].Replace(".png","_small.png")}");
                asset.name = cardData[1].Replace(".png", "");
                // asset.basePower = Convert.ToInt32(cardData[2].Replace("hero", "").Split('_')[0]);
                // asset.ability = GetAbility(cardData[3]);
                // asset.row = GetRow(cardData[4]);
                asset.type = CardObject.Type.Special;
                // asset.isHero = cardData[2].Contains("hero");
            }
            catch (System.Exception e)
            {
                Debug.Log("Import error:" + e.Message);
                reader.Close();
                return;
            }

            AssetDatabase.CreateAsset(asset, $"Assets/Resources/Card Objects/Special/{asset.name}.asset");
            AssetDatabase.SaveAssets();
        }
        reader.Close();
        EditorUtility.FocusProjectWindow();
    }

    private static CardObject.Row GetRow(string row)
    {
        switch (row)
        {
            case "close" : return CardObject.Row.Close;
            case "ranged" : return CardObject.Row.Ranged;
            case "siege" : return CardObject.Row.Siege;
            case "agile" : return CardObject.Row.Agile;
            default: throw new Exception("Card row not found");
        }
    }

    private static CardObject.Ability GetAbility(string ability)
    {
        switch (ability)
        {
            case "tightBond" : return CardObject.Ability.TightBond;
            case "spy" : return CardObject.Ability.Spy;
            case "medic" : return CardObject.Ability.Medic;
            case "muster" : return CardObject.Ability.Muster;
            case "horn" : return CardObject.Ability.Horn;
            case "moraleBoost" : return CardObject.Ability.MoraleBoost;
            default: return CardObject.Ability.None;
        }
    }
}
#endif