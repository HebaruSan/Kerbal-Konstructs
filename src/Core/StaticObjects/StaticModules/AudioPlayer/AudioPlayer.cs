using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalKonstructs.Core;
using UnityEngine;


namespace KerbalKonstructs
{
    public class AudioPlayer : StaticModule
    {

        public string audioClip;
        public double minDistance = 1;
        public double maxDistance = 500;
        public bool loop = true;
        public float volume = 1;
        AudioSource audioPlayer = null;

        public void Start()
        {
            AudioClip soundFile = GameDatabase.Instance.GetAudioClip(audioClip);

            if (soundFile == null)
            {
                Log.UserError("No audiofile found at: " + audioClip);
                return;
            }

            double scale = InstanceUtil.GetStaticInstanceForGameObject(gameObject).ModelScale;


            audioPlayer = gameObject.AddComponent<AudioSource>();
            audioPlayer.clip = soundFile;
            audioPlayer.minDistance = (float) (minDistance * scale);
            audioPlayer.maxDistance = (float) (maxDistance * scale);
            audioPlayer.loop = loop;
            audioPlayer.volume = volume * KerbalKonstructs.soundMasterVolume;
            audioPlayer.playOnAwake = true;
            audioPlayer.spatialBlend = 1f;
            audioPlayer.rolloffMode = AudioRolloffMode.Linear;
            audioPlayer.Play();
        }

        public override void StaticObjectUpdate()
        {
            if (audioPlayer != null)
            {
                double scale = InstanceUtil.GetStaticInstanceForGameObject(gameObject).ModelScale;
                audioPlayer.minDistance = (float) (minDistance * scale);
                audioPlayer.maxDistance = (float) (maxDistance * scale);
            }
        }
    }
}
