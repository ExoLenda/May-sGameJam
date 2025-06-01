using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField]private AudioSource audiosource;
    // Start is called before the first frame update


    // Update is called once per frame
    public void PlaySound(AudioClip _sound) {
        audiosource.PlayOneShot(_sound);
    }

    public void StopSound() {
        audiosource.Stop();
    }
}
