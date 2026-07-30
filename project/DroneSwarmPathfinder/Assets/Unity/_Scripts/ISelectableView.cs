using UnityEngine;

/// <summary>
/// Interface for any view in the scene that can be selected and worked with by the editor tools
/// </summary>
public interface ISelectableView
{
    int ID { get; }
    Transform transform { get; }
    GameObject gameObject { get; }
}