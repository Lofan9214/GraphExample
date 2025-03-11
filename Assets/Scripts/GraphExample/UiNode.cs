using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UiNode : MonoBehaviour
{
    public Image image;
    public TextMeshProUGUI text;

    private Node node;

    public void SetNode(Node node)
    {
        this.node = node;

        SetColor(node.CanVisit ? Color.white : Color.gray);
        SetString($"ID: {node.id}\nWeight: {node.weight}");
    }

    public void SetColor(Color color)
    {
        image.color = color;
    }

    public void SetString(string text)
    {
        this.text.text = text;
    }
}
