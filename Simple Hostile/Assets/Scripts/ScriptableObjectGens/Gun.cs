using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Kawaiisun.SimpleHostile
{
    [CreateAssetMenu(fileName = "New Gun", menuName = "Gun")]
    public class Gun : ScriptableObject
    {
        public string name;
        public int damage;
        public int ammo;
        public int burst; // 0 semi | 1 auto | 2+ burst fire
        public int pellets;
        public int clipsize;
        public float firerate;
        public float bloom;
        public float recoil;
        public float kickback;
        public float aimSpeed;
        public float reload;
        [Range(0, 1)] public float mainFOV;
        [Range(0, 1)] public float weaponFOV;
        public AudioClip gunshotSound;
        public float pitchRandomization;
        public float shotVolume;
        public GameObject prefab;
        public GameObject display;
        public bool recovery;

        private int stash; //current ammo
        private int clip; //current clip

        public void Initialize ()
        {
            stash = ammo;
            clip = clipsize;
        }

        public bool FireBullet ()
        {
            if (clip > 0)
            {
                clip -= 1;
                return true;
            }
            else return false;
        }

        public void Reload ()
        {
            stash += clip;
            clip = Mathf.Min(clipsize, stash);
            stash -= clip;
        }

        public int GetStash() { return stash; }
        public int GetClip() { return clip; }
    }
}
