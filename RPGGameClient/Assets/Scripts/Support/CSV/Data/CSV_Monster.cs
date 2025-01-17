﻿// Generate By @XmlToCSV
using UnityEngine;

namespace RPGGame
{
    public class CSV_Monster : CSVDataBase
    {
        // 怪id
        public int id;

        // 怪名称
        public string name;

        // 怪模型
        public string art;

        // 怪行为
        public string behavior;

        // 移动速度
        public float moveSpeed;

        // 攻击速度
        public float attackSpeed;

        // 旋转速度
        public float rotationSpeed;

        // 范围
        public float range;

        // 物理攻击
        public float attack;

        // 魔法攻击
        public float magicAttack;

        // 物理防御
        public float defense;

        // 魔法防御
        public float magicDefense;

        // 初始生命
        public float hp;

        // 最大生命
        public float hpMax;
    }
}