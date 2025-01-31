﻿using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class will proceed character's behaviour
/// </summary>
/// 
public enum EnCharacterAction
{
    None,
    Move,
    Attack,
    Die
}
public class CharacterAction
{
    public Character TargetCharacter { get; private set; }
    public CircleUnit TargetCC { get; private set; }
    public EnCharacterAction Action { get; private set; }
    public void SetAction(EnCharacterAction action, Character targetCharacter, CircleUnit targetCC)
    {
        this.Action = action;
        this.TargetCharacter = targetCharacter;
        this.TargetCC = targetCC;
    }

    public void SetAction(EnCharacterAction action, Character targetCharacter)
    {
        SetAction(action, targetCharacter, null);
    }

    public void SetAction(EnCharacterAction action, CircleUnit targetCC)
    {
        SetAction(action, null, targetCC);
    }

    public void Reset()
    {
        this.Action = EnCharacterAction.None;
        TargetCharacter = null;
        TargetCC = null;
    }
}
public class Character : MonoBehaviour
{
    [SerializeField] private Transform spine;
    [SerializeField] private BattleHPBar hpBar;
    public int SpawnId { get; protected set; }
    public Character ClosestEnemy { get; protected set; }
    public int AttackValue { get; private set; }
    protected SkeletonAnimation skeletonAnimation;
    MeshRenderer meshRender;
    public Vector2 Size { get; private set; }
    public float SpineScale { get; private set; }
    public DTCharacter Data { get; private set; }
    public CircleUnit StandingBase { get; private set; }

    protected CharacterAction Action = new CharacterAction();
    public System.Action<Character> OnCharacterDie;
    protected float moveSpeed;
    private void Awake()
    {
        skeletonAnimation = spine.GetComponent<SkeletonAnimation>();
        meshRender = spine.GetComponent<MeshRenderer>();
    }

    public virtual void SetData(int spawnId, DTCharacter data)
    {
        this.Data = data;
        this.SpawnId = spawnId;
        skeletonAnimation.AnimationState.AddAnimation(0, AnimationAction.Idle, true, 0);
        hpBar.Init(this);
        Size = meshRender.bounds.size;
        SpineScale = spine.transform.localScale.x;
    }
    public virtual void UpdateClosestEnemy(Character character)
    {
        ClosestEnemy = character;
    }

    public virtual bool CheckAction()
    {
        Action.Reset();
        AttackValue = Random.Range(0, 3);
        return true;
    }
    public virtual void DoAction()
    {
        switch (Action.Action)
        {
            case EnCharacterAction.Move:
                {
                    MoveTo(Action.TargetCC);
                    break;
                }
            case EnCharacterAction.Attack:
                {
                    int attackLogicValue = (3 + AttackValue - Action.TargetCharacter.AttackValue)%3;
                    int damage = attackLogicValue == 0 ? 4 : attackLogicValue == 1 ? 5 : 3;
                    Attack(Action.TargetCharacter);
                    Action.TargetCharacter.GotHit(this, damage);
                    break;
                }
            case EnCharacterAction.Die:
                {
                    Die();
                    break;
                }
            default:
                break;
        }
    }
    public void UpdateStandingBase(CircleUnit unit)
    {
        StandingBase = unit;
    }
    public virtual void Attack(Character target)
    {
        bool isFlipX = target.transform.position.x > transform.position.x;
        Vector3 scale = spine.localScale;
        scale.x = Mathf.Abs(scale.x) * (isFlipX ? -1 : 1);
        spine.localScale = scale;
        skeletonAnimation.AnimationState.AddAnimation(0, AnimationAction.MoveBack, false, 0);
        skeletonAnimation.AnimationState.AddAnimation(0, AnimationAction.Idle, true, 0);
    }
    public virtual void GotHit(Character enemy, int damage)
    {
        //skeletonAnimation.AnimationState.SetAnimation(0, AnimationAction.GotHit, false);
        //skeletonAnimation.AnimationState.AddAnimation(0, AnimationAction.Idle, true, 0);
        Data.CurrentHP -= damage;
        if (Data.CurrentHP <= 0)
            Die();
    }

    public virtual void MoveTo(CircleUnit unit)
    {
        bool isFlipX = unit.Data.BasePosition.x > StandingBase.Data.BasePosition.x;
        Vector3 scale = spine.localScale;
        scale.x = Mathf.Abs(scale.x) * (isFlipX ? -1 : 1);
        spine.localScale = scale;
        skeletonAnimation.AnimationState.SetAnimation(0, AnimationAction.MoveBack, false);
        skeletonAnimation.AnimationState.AddAnimation(1, AnimationAction.Idle, true, 0);
    }
    public virtual void Die()
    {
        StandingBase.UpdateCharacter(null);
        if (OnCharacterDie != null)
            OnCharacterDie.Invoke(this);
        skeletonAnimation.AnimationState.SetAnimation(0, AnimationAction.Sleep, true);
        var color = meshRender.material.color;
        color.a = 0.5f;
        meshRender.material.color = color;
        hpBar.gameObject.SetActive(false);
    }
}
