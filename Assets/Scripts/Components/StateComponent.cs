using System;
using UnityEngine;


public class StateComponent : MonoBehaviour
{
    public enum StateType 
    {
        Idle = 0, Equip, Action, Evade, Damaged, Dead, Airborne,
    }
    private StateType type = StateType.Idle;
    public StateType Type { get => type; }

    public event Action<StateType, StateType> OnStateTypeChanged;
    public event Action<StateType> OnStateTypeChanging; // ���� ���¿� ���� �̺�Ʈ ��

   
    public bool IdleMode { get => type == StateType.Idle; }
    public bool EquipMode { get => type == StateType.Equip; }
    public bool ActionMode { get => type == StateType.Action; }
    public bool EvadeMode { get => type == StateType.Evade; }
    public bool DamagedMode { get => type == StateType.Damaged; }
    public bool DeadMode { get => type == StateType.Dead; }
    public bool AirborneMode { get=>type == StateType.Airborne; }

    public void SetIdleMode() => ChangeType(StateType.Idle);
    public void SetEquipMode() => ChangeType(StateType.Equip);
    public void SetActionMode() => ChangeType(StateType.Action);
    public void SetEvadeMode() => ChangeType(StateType.Evade);
    public void SetDamagedMode() => ChangeType(StateType.Damaged);
    public void SetDeadMode() => ChangeType(StateType.Dead);
    public void SetAirborneMode() =>ChangeType(StateType.Airborne);


    private enum ConditionType
    {
        None, Down, Max,
    }

    private ConditionType myCondition;

    public bool NoneCondition { get => myCondition == ConditionType.None; }
    public bool DownCondition { get => myCondition == ConditionType.Down; }
    public void SetNoneConditon() { myCondition = ConditionType.None; }
    public void SetDownCondition() { myCondition = ConditionType.Down; }


    private void ChangeType(StateType type)
    {
        if (this.type == type)
            return;

        StateType prevType = this.type; // ���� ���� ���� 
        this.type = type; // Ÿ�� ��ü 

        OnStateTypeChanging?.Invoke(prevType);
        OnStateTypeChanged?.Invoke(prevType, type);  // �̺�Ʈ�� �ִٸ� ��
    }

}
