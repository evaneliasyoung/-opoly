using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuickPlay : MonoBehaviour
{
    [SerializeField] private Toggle _toggle;
    // Start is called before the first frame update

    public void toggleQuickplay()
    {
        InitializeGame.Quickplay = _toggle;
    }

    // Update is called once per frame
}
