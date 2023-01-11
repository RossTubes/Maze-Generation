using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Drawing;
using UnityEditor.Experimental.GraphView;

public class MazeGenerator : MonoBehaviour
{
    bool requestgenInstant;
    bool requestgenSlow;

    int generationint;
    [SerializeField] private int amountOfChecksPerFrame;

    [SerializeField] Nodes nodePrefab;
    [SerializeField] Vector2Int mazeSize;

    [SerializeField] float nodeSize;

    public GameObject Maze;
    public TMP_InputField inputWidth;
    public TMP_InputField inputHeight;

    public List<Nodes> node = new List<Nodes>();

    public List<Nodes> currentPath = new List<Nodes>();
    public List<Nodes> completedNodes = new List<Nodes>();

    public void Update()
    {
        //spreading the Generation over the frames
        SetSize();
        if (requestgenInstant)
        {
            for (int i = 0; i < amountOfChecksPerFrame; i++)
            {
                GenerateMazeInstant(mazeSize);
            }
        }
    }

    public void StartMazeInstant()
    {
        Create(mazeSize);
        GenerateMazeInstant(mazeSize);
    }

    public void StartCoroutine()
    {
        StartCoroutine(GenerateMaze(mazeSize));
    }

    public void Create(Vector2Int size)
    {
        if (node.Count > 0)
        {
            foreach (var node in node)
            {
                Destroy(node.gameObject);
            }
            node.Clear();
            currentPath.Clear();
            completedNodes.Clear();
        }

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector3 nodePos = new Vector3(x - (size.x / 2f), 0, y - (size.y / 2f));
                Nodes newNode = Instantiate(nodePrefab, nodePos, Quaternion.identity, transform);
                newNode.gameObject.transform.localScale = new Vector3(nodeSize, nodeSize, nodeSize);
                node.Add(newNode);
            }
        }

        currentPath.Add(node[Random.Range(0, node.Count)]);
        requestgenInstant = true;
        currentPath[0].SetState(NodeState.Current);
    }


    void GenerateMazeInstant(Vector2Int size)
    {
        List<int> possibleNextNodes = new List<int>();
        List<int> possibleDirections = new List<int>();
        //checks if there are nodes left
        if (completedNodes.Count < node.Count)
        {
            // check nodes next to the current node

            int currentNodeIndex = node.IndexOf(currentPath[currentPath.Count - 1]);
            int currentNodeX = currentNodeIndex / size.y;
            int currentNodeY = currentNodeIndex % size.y;

            if (currentNodeX < size.x - 1)
            {
                // checks if the nodes on the right to it is either in the current path or the completed path
                if (!completedNodes.Contains(node[currentNodeIndex + size.y]) &&
                    !currentPath.Contains(node[currentNodeIndex + size.y]))
                {
                    possibleDirections.Add(1); //1 because it shows which direction the node can move in which is always one
                    possibleNextNodes.Add(currentNodeIndex + size.y);
                }
            }

            if (currentNodeX > 0)
            {
                //checks node to the left of the current node
                if (!completedNodes.Contains(node[currentNodeIndex - size.y]) &&
                    !currentPath.Contains(node[currentNodeIndex - size.y]))
                {
                    possibleDirections.Add(2);
                    possibleNextNodes.Add(currentNodeIndex - size.y);
                }
            }

            if (currentNodeY < size.y - 1)
            {
                //checks node above the current node
                if (!completedNodes.Contains(node[currentNodeIndex + 1]) &&
                    !currentPath.Contains(node[currentNodeIndex + 1]))
                {
                    possibleDirections.Add(3);
                    possibleNextNodes.Add(currentNodeIndex + 1);
                }
            }

            if (currentNodeY > 0)
            {
                //check node below the current node
                if (!completedNodes.Contains(node[currentNodeIndex - 1]) &&
                    !currentPath.Contains(node[currentNodeIndex - 1]))
                {
                    possibleDirections.Add(4);
                    possibleNextNodes.Add(currentNodeIndex - 1);
                }
            }
            //choose next node
            if (possibleDirections.Count > 0)
            {
                int chosenDirection = Random.Range(0, possibleDirections.Count);
                Nodes chosenodes = node[possibleNextNodes[chosenDirection]];

                switch (possibleDirections[chosenDirection])
                {
                    case 1:
                        chosenodes.RemoveWall(1); // 1 because the first wall is the left wall 
                        currentPath[currentPath.Count - 1].RemoveWall(0);
                        break;
                    case 2:
                        chosenodes.RemoveWall(0);
                        currentPath[currentPath.Count - 1].RemoveWall(1);
                        break;
                    case 3:
                        chosenodes.RemoveWall(3);
                        currentPath[currentPath.Count - 1].RemoveWall(2);
                        break;
                    case 4:
                        chosenodes.RemoveWall(2);
                        currentPath[currentPath.Count - 1].RemoveWall(3);
                        break;
                }

                currentPath.Add(chosenodes);
                chosenodes.SetState(NodeState.Current);
            }
            else
            {
                completedNodes.Add(currentPath[currentPath.Count - 1]);

                currentPath[currentPath.Count - 1].SetState(NodeState.Completed);
                currentPath.RemoveAt(currentPath.Count - 1);
            }
        }
        else
        {
            requestgenInstant = false;
        }
    }
    IEnumerator GenerateMaze(Vector2Int size)
    {
        if (node.Count > 0)
        {
            foreach (var node in node)
            {
                Destroy(node.gameObject);
            }
            node.Clear();
            currentPath.Clear();
            completedNodes.Clear();
        }
        // Create nodes
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector3 nodePos = new Vector3(x - (size.x / 2f), 0, y - (size.y / 2f));
                Nodes newNode = Instantiate(nodePrefab, nodePos, Quaternion.identity, transform);
                node.Add(newNode);

                yield return null;
            }
        }

        // Choose starting node
        currentPath.Add(node[Random.Range(0, node.Count)]);
        currentPath[0].SetState(NodeState.Current);

        while (completedNodes.Count < node.Count)
        {
            // Check nodes next to the current node
            List<int> possibleNextNodes = new List<int>();
            List<int> possibleDirections = new List<int>();

            int currentNodeIndex = node.IndexOf(currentPath[currentPath.Count - 1]);
            int currentNodeX = currentNodeIndex / size.y;
            int currentNodeY = currentNodeIndex % size.y;

            if (currentNodeX < size.x - 1)
            {
                // Check node to the right of the current node
                if (!completedNodes.Contains(node[currentNodeIndex + size.y]) &&
                    !currentPath.Contains(node[currentNodeIndex + size.y]))
                {
                    possibleDirections.Add(1);
                    possibleNextNodes.Add(currentNodeIndex + size.y);
                }
            }
            if (currentNodeX > 0)
            {
                // Check node to the left of the current node
                if (!completedNodes.Contains(node[currentNodeIndex - size.y]) &&
                    !currentPath.Contains(node[currentNodeIndex - size.y]))
                {
                    possibleDirections.Add(2);
                    possibleNextNodes.Add(currentNodeIndex - size.y);
                }
            }
            if (currentNodeY < size.y - 1)
            {
                // Check node above the current node
                if (!completedNodes.Contains(node[currentNodeIndex + 1]) &&
                    !currentPath.Contains(node[currentNodeIndex + 1]))
                {
                    possibleDirections.Add(3);
                    possibleNextNodes.Add(currentNodeIndex + 1);
                }
            }
            if (currentNodeY > 0)
            {
                // Check node below the current node
                if (!completedNodes.Contains(node[currentNodeIndex - 1]) &&
                    !currentPath.Contains(node[currentNodeIndex - 1]))
                {
                    possibleDirections.Add(4);
                    possibleNextNodes.Add(currentNodeIndex - 1);
                }
            }

            // Choose next node
            if (possibleDirections.Count > 0)
            {
                int chosenDirection = Random.Range(0, possibleDirections.Count);
                Nodes chosenNode = node[possibleNextNodes[chosenDirection]];

                switch (possibleDirections[chosenDirection])
                {
                    case 1:
                        chosenNode.RemoveWall(1);
                        currentPath[currentPath.Count - 1].RemoveWall(0);
                        break;
                    case 2:
                        chosenNode.RemoveWall(0);
                        currentPath[currentPath.Count - 1].RemoveWall(1);
                        break;
                    case 3:
                        chosenNode.RemoveWall(3);
                        currentPath[currentPath.Count - 1].RemoveWall(2);
                        break;
                    case 4:
                        chosenNode.RemoveWall(2);
                        currentPath[currentPath.Count - 1].RemoveWall(3);
                        break;
                }

                currentPath.Add(chosenNode);
                chosenNode.SetState(NodeState.Current);
            }
            else
            {
                completedNodes.Add(currentPath[currentPath.Count - 1]);

                currentPath[currentPath.Count - 1].SetState(NodeState.Completed);
                currentPath.RemoveAt(currentPath.Count - 1);
            }

            yield return new WaitForSeconds(0.05f);
        }
    }

    public void SetSize()
    {
        int x = mazeSize.x;
        int y = mazeSize.y;
        if (int.TryParse(inputWidth.text, out x))
        {
            mazeSize.x = int.Parse(inputWidth.text);
        }
        if (int.TryParse(inputHeight.text, out y))
        {
            mazeSize.y = int.Parse(inputHeight.text);
        }

    }
}
