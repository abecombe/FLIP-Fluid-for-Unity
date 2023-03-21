using RosettaUI;
using UnityEngine;

[RequireComponent(typeof(RosettaUIRoot))]
public class RosettaUIController : MonoBehaviour
{
    [SerializeField] private KeyCode _toggleUIKey = KeyCode.U;

    private RosettaUIRoot _root;

    private void Start()
    {
        _root = GetComponent<RosettaUIRoot>();
        _root.Build(FindObjectOfType<FLIPSimulation>().CreateElement());
    }

    private void Update()
    {
        if (Input.GetKeyDown(_toggleUIKey))
            _root.enabled = !_root.enabled;
    }
}