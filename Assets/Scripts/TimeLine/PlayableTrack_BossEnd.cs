using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UIElements;

public class PlayableTrack_BossEnd : PlayableBehaviour
{
    public string eventName;
    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        Debug.Log($"Event triggered: {eventName}");
        // 원하는 이벤트 로직을 여기에 추가합니다.
        // 예: EventManager.TriggerEvent(eventName);
    }

}
