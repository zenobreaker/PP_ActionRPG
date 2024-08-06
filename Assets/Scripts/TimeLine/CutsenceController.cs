using Cinemachine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;


[Serializable]
public class PlayableData
{
    public string name;
    public PlayableAsset asset;
}

public class CutsenceController : MonoBehaviour
{
    [SerializeField] private PlayableDirector playableDirector;
    [SerializeField] private PlayableData[] playables;  
    private Dictionary<string, PlayableAsset> playableDataTable;


    private CinemachineBrain brain;
    [SerializeField] private CinemachineVirtualCamera followCam;
    [SerializeField] private CinemachineVirtualCamera bossEndCam;
    [SerializeField] private Cinemachine.NoiseSettings noise; 
    private CinemachineImpulseListener bossEndlistener;
    CinemachineBasicMultiChannelPerlin bmcp;

    [SerializeField] private GameObject cameraArm;

    public event Action OnCutsceneBegin;
    public event Action OnCutSceneEnd;

    private void Awake()
    {
        followCam = GetComponentInChildren<CinemachineVirtualCamera>();
        if (followCam != null)
        {
            followCam.gameObject.SetActive(false);
        }
        
        if (bossEndCam != null)
        {
            bossEndlistener = bossEndCam.GetComponent<CinemachineImpulseListener>();
            
        }

        cameraArm = FindObjectOfType<CameraArm>().gameObject;
        playableDirector = GetComponent<PlayableDirector>();

        playableDataTable = new Dictionary<string, PlayableAsset>();
        for(int i = 0; i < playables.Length; i++) 
        {
            playableDataTable.Add(playables[i].name, playables[i].asset); 
        }
    }

    private void Start()
    {
        //TODO: �� ... �̰� �³�..
        BossStageManager.Instance.OnAppearBoss += SetAnimationTarget;
        playableDirector.played += OnBeginTimeLinePlay;
        playableDirector.stopped += OnBossAppaerEnd;
    }    


    // Playable ������ ���� 
    public void SetPlayableData(string name, bool bPlay = false)
    {
        if(playableDataTable.TryGetValue(name, out PlayableAsset asset)) 
        {
            playableDirector.playableAsset = asset;
            if (bPlay)
                OnPlay();
        }
    }

    public void OnPlay()
    {
        if (playableDirector != null)
        {
          
            playableDirector.Play();
        }
    }

    private void SetAnimationTarget(GameObject target)
    {
        if (playableDirector == null)
            return;
        if (target == null) 
            return;

        TimelineAsset timelineAsset = (TimelineAsset)playableDirector.playableAsset;

        var emptyAnimationTracks = FindEmptyAnimationTracksInTimeline(timelineAsset);

        // Ʈ�� �� �ִϸ��̼� Ʈ���� ã�� �ű⼭ �Ҵ�
        foreach (var track  in emptyAnimationTracks)
        {
            if(track is AnimationTrack)
            {
                playableDirector.SetGenericBinding(track, target);
                return; 
            }
        }

    }

    /// <summary>
    /// �ִϸ��̼� Ʈ������ �ִϸ����Ͱ� ���� Ʈ���� ã�� ��ȯ
    /// </summary>
    /// <param name="timelineAsset"></param>
    /// <returns></returns>
    private IEnumerable<AnimationTrack> FindEmptyAnimationTracksInTimeline(TimelineAsset timelineAsset)
    {
        // Ÿ�Ӷ����� ��� Ʈ���� Ȯ��
        foreach (var track in timelineAsset.GetOutputTracks())
        {
            // Ʈ���� AnimationTrack���� Ȯ��
            if (track is AnimationTrack animationTrack)
            {
                // �ִϸ��̼� Ŭ���� ������ Ȯ��
                //if (!animationTrack.GetClips().Any())
                if(playableDirector.GetGenericBinding(animationTrack) == null)
                {
                    yield return animationTrack;
                }
            }
        }
    }

    public void Begin_FollowCam()
    {
        if (followCam == null)
            return;
        if (cameraArm == null)
            return;
        followCam.gameObject.SetActive(true);
        cameraArm.SetActive(false);
        bossEndCam.gameObject.SetActive(false);
    }

    public void End_FollowCam()
    {
        if (followCam == null)
            return;
        if (cameraArm == null)
            return;
        followCam.gameObject.SetActive(false);
        cameraArm.SetActive(true);
    }

    private void OnBeginTimeLinePlay(PlayableDirector director)
    {
        if (director == playableDirector)
        {
            var currentPlayable = playableDirector.playableAsset;
            if (currentPlayable.name != "Boss_Intro")
                return;

            OnCutsceneBegin?.Invoke();
        }
    }

    private void OnBossAppaerEnd(PlayableDirector director)
    {
        if(director == playableDirector)
        {
            var currentPlayable = playableDirector.playableAsset;
            if (currentPlayable.name != "Boss_Intro")
                return; 
            
            OnCutSceneEnd?.Invoke();
            
            // ��������ķ�� ���� �ִٸ� ���⼭ ���ش�. 
            bossEndCam.gameObject.SetActive(false);
        }
    }


    Vector3 shake = new Vector3(0, 1, 0);

    public void Begin_EndBoss()
    {
        if (bossEndlistener == null)
            return;
        bossEndCam.gameObject.SetActive(true);
        cameraArm.SetActive(false);
        //bossEndlistener.m_ReactionSettings.m_SecondaryNoise = noise;
        bmcp = bossEndCam.AddCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        if(bmcp)
            bmcp.m_NoiseProfile = noise;
    }

    public void End_EndBoss()
    {
        cameraArm.SetActive(true);
        
        if (bmcp == null)
            return;

        //bossEndlistener.m_ReactionSettings.m_SecondaryNoise = null;
        bmcp.m_NoiseProfile = null;
    }
}
