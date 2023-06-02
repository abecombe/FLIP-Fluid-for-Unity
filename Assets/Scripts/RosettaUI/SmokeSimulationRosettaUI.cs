using RosettaUI;
using UnityEngine;

[RequireComponent(typeof(RosettaUIRoot))]
public class SmokeSimulationRosettaUI : MonoBehaviour
{
    [SerializeField]
    private KeyCode _toggleUIKey = KeyCode.U;

    private RosettaUIRoot _root;

    private void Start()
    {
        _root = GetComponent<RosettaUIRoot>();
        _root.Build(FindObjectOfType<SmokeSimulation>().CreateElement());
    }

    private void Update()
    {
        if (Input.GetKeyDown(_toggleUIKey))
            _root.enabled = !_root.enabled;
    }
}