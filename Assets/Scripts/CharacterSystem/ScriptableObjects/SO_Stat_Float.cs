using UnityEngine;

[CreateAssetMenu(fileName = "NewStat", menuName = "CharacterSystem/Stat/Float")]
public class SO_Stat_Float : SO_Stat
{
    [SerializeField] private float m_Value = 0;
}