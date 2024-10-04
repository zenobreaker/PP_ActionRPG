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
    [SerializeField] private ParticleSystem particleSystem;


    public event Action OnCutsceneBegin;
    public event Action OnBossSpawn;
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
        for (int i = 0; i < playables.Length; i++)
        {
            playableDataTable.Add(playables[i].name, playables[i].asset);
        }
    }

    private void Start()
    {
        //TODO: 아 ... 이거 맞나..
        BossStageManager.Instance.OnAppearBoss += SetAnimationTarget;
        playableDirector.played += OnBeginTimeLinePlay;
        playableDirector.stopped += OnBossAppaerEnd;
    }


    // Playable 데이터 세팅 
    public void SetPlayableData(string name, bool bPlay = false)
    {
        if (playableDataTable.TryGetValue(name, out PlayableAsset asset))
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

        // 트랙 중 애니메이션 트랙을 찾아 거기서 할당
        foreach (var track in emptyAnimationTracks)
        {
            if (track is AnimationTrack)
            {
                playableDirector.SetGenericBinding(track, target);
                return;
            }
        }

    }

    /// <summary>
    /// 애니메이션 트랙에서 애니메이터가 없는 트랙을 찾아 반환
    /// </summary>
    /// <param name="timelineAsset"></param>
    /// <returns></returns>
    private IEnumerable<AnimationTrack> FindEmptyAnimationTracksInTimeline(TimelineAsset timelineAsset)
    {
        // 타임라인의 모든 트랙을 확인
        foreach (var track in timelineAsset.GetOutputTracks())
        {
            // 트랙이 AnimationTrack인지 확인
            if (track is AnimationTrack animationTrack)
            {
                // 애니메이션 클립이 없는지 확인
                //if (!animationTrack.GetClips().Any())
                if (playableDirector.GetGenericBinding(animationTrack) == null)
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

        OnBossSpawn?.Invoke();
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
        if (director == playableDirector)
        {
            var currentPlayable = playableDirector.playableAsset;
            if (currentPlayable.name != "Boss_Intro_Dragon")
                return;

            OnCutSceneEnd?.Invoke();

            // 보스엔드캠이 켜져 있다며 여기서 꺼준다. 
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
        if (bmcp)
            bmcp.m_NoiseProfile = noise;
    }

    public void End_EndBoss()
    {
        cameraArm.SetActive(true);

        PlayParticles();
        if (bmcp == null)
            return;
        //bossEndlistener.m_ReactionSettings.m_SecondaryNoise = null;
        bmcp.m_NoiseProfile = null;
    }

    public void PlayParticles()
    {
        if (particleSystem != null && !particleSystem.isPlaying)
        {
            particleSystem.Play();
        }

    }
}
