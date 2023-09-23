﻿using UnityEngine;

public class Pestleable : MonoBehaviour, IPestleable
{
    [SerializeField] private GameObject _objectToGive;
    [SerializeField] private float _resistance = 100f;
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private Collider2D _collider;
    private bool _canBeHit;
    public bool CanBeHit => _canBeHit;

    private IName _name;
    public IName Name => _name;

    private void Awake()
    {
        _name = GetComponent<IName>();
    }

    public void SetPosition(Vector2 position)
    {
        _canBeHit = true;
        _rb.velocity = Vector2.zero;
        _rb.isKinematic = true;
        _rb.freezeRotation = true;
        _rb.rotation = 0f;
        _collider.isTrigger = true;
        transform.position = position;
    }

    public void Destroy() => Destroy(gameObject);

    public void Hit(float damage)
    {
        if (!_canBeHit)
            return;

        _resistance -= damage;
        if (_resistance >= 0f)
            return;

        Instantiate(_objectToGive, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}

