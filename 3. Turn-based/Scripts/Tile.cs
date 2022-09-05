using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game_Turn_Based
{
    public class Tile : MonoBehaviour
    {
        [SerializeField] private Color _baseColor, _offsetColor;
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private GameObject _highlight;
        public bool _isMoveable = true;
        public GameObject StandingUnit = null;
        public bool Moveable => _isMoveable && StandingUnit == null;

        public void Init(bool isOffset)
        {
            _renderer.color = isOffset ? _offsetColor : _baseColor;
        }

        public void SetUnit(GameObject unit)
        {
            StandingUnit = unit;
        }

        public void RemoveUnit()
        {
            StandingUnit = null;
        }

        void OnMouseEnter()
        {
            _highlight.SetActive(true);
        }

        void OnMouseExit()
        {
            _highlight.SetActive(false);
        }
    }
}