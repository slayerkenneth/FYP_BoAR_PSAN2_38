using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CharacterList : ScriptableObject
{
    public CharacterSelection[] character;

    public int charCount
    {
        get
        {
            return character.Length;
        }
    }
    public CharacterSelection GetCharacter(int index)
    {
        return character[index];
    }
}
