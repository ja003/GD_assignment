using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Settings", menuName = "Minecraft/Settings")]
public class Settings : ScriptableObject
{
    public int LoadDistance = 16;
    public bool enableThreading;
}
