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
        // ���ϴ� �̺�Ʈ ������ ���⿡ �߰��մϴ�.
        // ��: EventManager.TriggerEvent(eventName);
    }

}
