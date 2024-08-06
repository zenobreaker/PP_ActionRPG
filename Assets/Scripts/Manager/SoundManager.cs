using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.AI;

[System.Serializable]
public class Sound
{
    public string soundName;
    public AudioClip clip;
}

public class SoundManager : MonoBehaviour
{
    private  static SoundManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        Awake_InitSFXTable();
    }

    public static SoundManager Instance { get { return instance; } }

    
    ///////////////////////////////////////////////////////////////////////////

    [Header("���� ���")]
    [SerializeField] Sound[] bgmSounds = null;
    [SerializeField] Sound[] sfxSounds = null;

    [Header("BGM �÷��̾�")]
    public AudioSource bgmPlayer = null;

    [Header("ȿ���� �÷��̾�")]
    public AudioSource[] sfxPlayers = null;

    public float sfxVolume;
    public float bgmVolume;


    private Dictionary<string, AudioClip> sfxSoundTable = new Dictionary<string, AudioClip>();

    private void Awake_InitSFXTable()
    {
        foreach(Sound sound in sfxSounds )
        {
            sfxSoundTable.Add(sound.soundName, sound.clip);
        }
    }

    private void Start()
    {
        PlayRandomBGM();
    }

    private void Update()
    {
        if (!bgmPlayer.isPlaying)
        {
            int random = Random.Range(0, bgmSounds.Length - 1);

            bgmPlayer.clip = bgmSounds[random].clip;
            bgmPlayer.Play();
            bgmVolume = bgmPlayer.volume;
        }
    }

    public void PlayRandomBGM()
    {
        if (bgmSounds.Length <= 0)
            return; 

        int random = Random.Range(0, bgmSounds.Length - 1); // ����Ÿ���� MAX�� ������ �Ǽ� Ÿ���� MAX�� ����
        bgmPlayer.clip = bgmSounds[random].clip;
        bgmPlayer.Play();
    }

    public void PlaySFX(string soundName)
    {
        if (string.IsNullOrEmpty(soundName))
            return;

        if (sfxSoundTable == null)
            return;

        if(sfxSoundTable.TryGetValue(soundName, out AudioClip clip))
        {
            AudioSource audioSource = GetNotPlayingAudioSource();
            if (audioSource == null)
            {
                Debug.Log($"Auido Source All Playing");
                return;
            }
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    private AudioSource GetNotPlayingAudioSource()
    {
        foreach(AudioSource audioSource in sfxPlayers)
        {
            if (audioSource.isPlaying)
                continue;

            return audioSource;
        }

        return null; 
    }


    private void Old_Code(string _soundName)
    {
        // ȿ���� �÷��̾ �������� ������Ҹ� �־��ָ� �������� ����� ����������. (������ ���� ���� �Ҹ� �����̰� ũ��)
        for (int i = 0; i < sfxSounds.Length; i++)
        {
            if (_soundName == sfxSounds[i].soundName)
            {
                for (int x = 0; x < sfxPlayers.Length; x++)
                {
                    if (!sfxPlayers[x].isPlaying)
                    {
                        sfxPlayers[x].clip = sfxSounds[i].clip;
                        sfxPlayers[x].Play();
                        sfxVolume = sfxPlayers[x].volume;
                        //Debug.Log("�÷�����" + _soundName);
                        return;
                    }
                }
                Debug.Log("��� ȿ���� �÷��̾ ��� ���Դϴ�!");
                return;
            }
        }
        Debug.Log("��ϵ� ȿ������ �����ϴ�.");
    }
}
